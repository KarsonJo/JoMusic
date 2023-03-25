--视图：Ancestor的直接子树
CREATE VIEW IF NOT EXISTS FolderCreate
AS
SELECT DISTINCT Ancestor, Dirname FROM FolderNodes, FolderPaths
WHERE Descendant = Id AND Length = 1;


--视图：Ancestor的直接子树和名称
CREATE VIEW IF NOT EXISTS  DirectSubFolder
AS
SELECT Ancestor, Descendant, Dirname AS DescendantName FROM FolderPaths
JOIN FolderNodes
ON FolderPaths.Descendant = Id
WHERE Length = 1;

--视图：链接结点
CREATE VIEW IF NOT EXISTS ConnectPath
AS
SELECT Ancestor, Descendant FROM FolderPaths;

--视图：复制助手
CREATE VIEW IF NOT EXISTS FolderCopy
AS
SELECT DISTINCT Ancestor FROM FolderPaths;

--视图：剪切助手
CREATE VIEW IF NOT EXISTS FolderCut
AS
SELECT DISTINCT Ancestor FROM FolderPaths;



--触发器：处理结点的链接
CREATE TRIGGER IF NOT EXISTS ConnectPath_Instead_Insert
INSTEAD OF INSERT
ON ConnectPath
BEGIN
SELECT CASE
	--0.重复将回滚
	WHEN (SELECT 1 FROM ConnectPath WHERE Ancestor = NEW.Ancestor AND Descendant = NEW.Descendant) IS NOT NULL
		THEN RAISE(ROLLBACK, 'Duplicate path in this field')
	END;
	--1.链接文件路径
	INSERT INTO FolderPaths (ancestor, descendant, length)
	SELECT t.ancestor, NEW.Descendant, t.length+1
	FROM FolderPaths AS t
	WHERE t.descendant = /*locationID*/NEW.Ancestor
	UNION ALL
	SELECT NEW.Descendant, NEW.Descendant, 0;
END;



--触发器：处理直接子树的插入
CREATE TRIGGER IF NOT EXISTS FolderCreate_Instead_Insert
INSTEAD OF INSERT
ON FolderCreate
BEGIN
SELECT CASE
	--0.同目录同名将回滚
	WHEN (SELECT 1 FROM FolderCreate WHERE Ancestor = NEW.Ancestor AND Dirname = NEW.Dirname) IS NOT NULL
		THEN RAISE(ROLLBACK, 'Duplicate folder name in this field')
	END;
	--1.创建文件夹
	INSERT INTO FolderNodes(Dirname) VALUES(NEW.Dirname);
	--2.链接文件路径
	INSERT INTO ConnectPath (ancestor, descendant)
	VALUES(NEW.Ancestor ,last_insert_rowid());
END;
SELECT DISTINCT FileName
FROM FolderPaths, FileNodes, SongFileMeta 
WHERE Ancestor = 32 AND FolderId = Descendant AND SongId = Id;
SELECT * FROM DirectSubFolder WHERE Ancestor = 32;
SELECT Descendant FROM FolderPaths WHERE Ancestor = 32;
SELECT * FROM FolderNodes;
SELECT * FROM FolderPaths WHERE Ancestor = 32;


--触发器：处理子树的递归删除
CREATE TRIGGER IF NOT EXISTS DirectSubFolder_Instead_Delete
INSTEAD OF DELETE
ON DirectSubFolder
BEGIN
DELETE FROM FolderNodes
WHERE Id IN (SELECT Descendant FROM FolderPaths WHERE Ancestor = OLD.Descendant);
END;



--触发器：处理子树的复制粘贴
CREATE TRIGGER IF NOT EXISTS FolderCopy_Instead_Update
INSTEAD OF UPDATE
ON FolderCopy
BEGIN
SELECT CASE
	--0.新目录不存在将回滚
	WHEN (SELECT 1 FROM FolderPaths WHERE Ancestor = NEW.Ancestor) IS NULL
		THEN RAISE(ROLLBACK, 'Target folder not exist')
	END;
	--1.为要复制的文件夹插入未定义的结点
	INSERT INTO FolderNodes (Dirname)
	SELECT NULL FROM (
		SELECT * FROM FolderPaths 
		WHERE Ancestor IN(
			SELECT Descendant 
			FROM FolderPaths 
			WHERE Ancestor = /*复制子树根*/Old.Ancestor) 
		AND length = 0);

	--2.创建临时表
	INSERT INTO FolderIdMap
	SELECT Descendant AS OldId, Id AS NewId FROM
		(SELECT Id,row_number() OVER (order by Id) as RowNum
		FROM FolderNodes 
		WHERE Dirname IS NULL) AS A
	JOIN
		(SELECT Descendant, row_number() OVER (order by Descendant) AS RowNum FROM FolderPaths 
		WHERE Ancestor IN(
			SELECT Descendant 
			FROM FolderPaths 
			WHERE Ancestor = /*复制子树根*/Old.Ancestor) 
		AND length = 0) AS B
	ON A.RowNum = B.RowNum;
	--3.按层级插入表（连带根结点）
	--1)根节点
	INSERT INTO ConnectPath
	SELECT /*目标位置*/New.Ancestor, NewId FROM FolderIdMap
	WHERE OldId = /*复制子树根*/Old.Ancestor;
	--2)其它子结点（按层级顺序对距离为1的连接排序）
	INSERT INTO ConnectPath
	SELECT Ancestor, Descendant
	FROM
		--3.1子节点层级顺序
		(SELECT Descendant AS SortOrder FROM FolderPaths 
		WHERE Ancestor = /*复制子树根*/Old.Ancestor AND Length > 0
		ORDER BY Length) AS T1
		JOIN
		--3.2筛选出距离为1的连接
		(SELECT Descendant AS SortOrder, IdMap1.NewId AS Ancestor, IdMap2.NewId As Descendant
		FROM(
			SELECT * FROM FolderPaths 
			WHERE Ancestor 
			IN(
				SELECT Descendant 
				FROM FolderPaths 
				WHERE Ancestor = /*复制子树根*/Old.Ancestor)
			AND Length = 1) AS A
			JOIN FolderIdMap AS IdMap1
			ON A.Ancestor = IdMap1.OldId
			JOIN FolderIdMap AS IdMap2
			ON A.Descendant = IdMap2.OldId) AS T2
		ON T1.SortOrder = T2.SortOrder;

	--4复制文件
	INSERT INTO FileNodes
	SELECT SongId, NewId 
	FROM FileNodes, FolderIdMap
	WHERE FolderId = OldId;
	--5 重命名文件夹
	UPDATE FolderNodes SET Dirname = Source.Dirname
	FROM FolderNodes AS Source, FolderIdMap
	WHERE Source.Id = FolderIdMap.OldId AND FolderNodes.Id = FolderIdMap.NewId;
	--6根节点唯一重命名
	UPDATE FolderNodes SET Dirname = FolderNodes.Dirname || '@!Copy' || FolderIdMap.NewId || '#@'
	FROM FolderNodes AS Source, FolderIdMap
	WHERE Old.Ancestor = FolderIdMap.OldId AND FolderNodes.Id = FolderIdMap.NewId;
	--6 删除id表中的项
	DELETE FROM FolderIdMap;
END;



--触发器：处理子树的剪切
CREATE TRIGGER IF NOT EXISTS FolderCut_Instead_Update
INSTEAD OF UPDATE
ON FolderCut
BEGIN
SELECT CASE
	--0.新目录不存在将回滚
	WHEN (SELECT 1 FROM FolderPaths WHERE Ancestor = NEW.Ancestor) IS NULL
		THEN RAISE(ROLLBACK, 'Target folder not exist')
	END;
	--1.删除外部联系
	DELETE FROM FolderPaths
	WHERE descendant IN (SELECT descendant FROM FolderPaths WHERE ancestor = Old.Ancestor)
	AND ancestor NOT IN (SELECT descendant FROM FolderPaths WHERE ancestor = Old.Ancestor);
	--2.建立新联系
	INSERT INTO FolderPaths (ancestor, descendant, length)
	SELECT supertree.ancestor, subtree.descendant,
	supertree.length+subtree.length+1
	FROM FolderPaths AS supertree JOIN FolderPaths AS subtree
	WHERE subtree.ancestor = Old.Ancestor
	AND supertree.descendant = New.Ancestor;
	--3.根结点唯一重命名
	UPDATE FolderNodes SET Dirname = Dirname || '@!Copy' || Id || '#@'
	WHERE FolderNodes.Id = Old.Ancestor;
END;

--触发器：子结点重命名
CREATE TRIGGER IF NOT EXISTS DirectSubFolder_Instead_Update
INSTEAD OF UPDATE
OF DescendantName
ON DirectSubFolder
BEGIN
	UPDATE FolderNodes
	SET Dirname = New.DescendantName
	WHERE Id = New.Descendant;
END;