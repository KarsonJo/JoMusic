--SQL中定义变量： https://stackoverflow.com/questions/7739444/declare-variable-in-sqlite-and-use-it
-- ROLLBACK TRANSACTION;
-- BEGIN TRANSACTION;
-- COMMIT TRANSACTION;

SELECT * FROM FolderNodes;
SELECT last_insert_rowid();
SELECT * FROM FolderPaths ORDER BY Ancestor;
SELECT * FROM FolderPaths WHERE Length = 1 ORDER BY Ancestor;

SELECT DISTINCT Ancestor, Dirname FROM FolderNodes, FolderPaths
WHERE Descendant = Id AND Ancestor = 2 AND length = 1;

SELECT * FROM FolderPaths 
WHERE Ancestor IN(
	SELECT Descendant 
	FROM FolderPaths 
	WHERE Ancestor = 2) 
AND length = 0
ORDER BY Descendant

--1.为要复制的文件夹插入未定义的结点
INSERT INTO FolderNodes (Dirname)
SELECT NULL FROM (
	SELECT * FROM FolderPaths 
	WHERE Ancestor IN(
		SELECT Descendant 
		FROM FolderPaths 
		WHERE Ancestor = 2) 
	AND length = 0)

--2.1查找刚插的文件夹，并映射至旧结点
SELECT Id AS NewId, Descendant AS OldId FROM
	(SELECT Id,row_number() OVER (order by Id) as RowNum
	FROM FolderNodes 
	WHERE Dirname IS NULL) AS A
JOIN
	(SELECT Descendant, row_number() OVER (order by Descendant) AS RowNum FROM FolderPaths 
	WHERE Ancestor IN(
		SELECT Descendant 
		FROM FolderPaths 
		WHERE Ancestor = 2) 
	AND length = 0) AS B
ON A.RowNum = B.RowNum

--2.2子树所有结点
SELECT * FROM FolderPaths 
WHERE Ancestor IN(
	SELECT Descendant 
	FROM FolderPaths 
	WHERE Ancestor = 2)

--2.3映射后的表
WITH IdMap AS(
SELECT Id AS NewId, Descendant AS OldId FROM
	(SELECT Id,row_number() OVER (order by Id) as RowNum
	FROM FolderNodes 
	WHERE Dirname IS NULL) AS A
JOIN
	(SELECT Descendant, row_number() OVER (order by Descendant) AS RowNum FROM FolderPaths 
	WHERE Ancestor IN(
		SELECT Descendant 
		FROM FolderPaths 
		WHERE Ancestor = 2) 
	AND length = 0) AS B
ON A.RowNum = B.RowNum)
SELECT IdMap2.NewId AS Ancestor, IdMap1.NewId As Descendant, Length
FROM(
	SELECT * FROM FolderPaths 
	WHERE Ancestor IN(
		SELECT Descendant 
		FROM FolderPaths 
		WHERE Ancestor = 2)) AS A
	JOIN IdMap AS IdMap1
	ON A.Descendant = IdMap1.OldId
	JOIN IdMap AS IdMap2
	ON A.Ancestor = IdMap2.OldId
ORDER BY Ancestor /*排序可选*/


--2将映射的新结点按旧结点规则插入Path中
--SELECT * FROM IdMap;
--创建临时表
CREATE TEMP TABLE IdMap AS 
SELECT Id AS NewId, Descendant AS OldId FROM
	(SELECT Id,row_number() OVER (order by Id) as RowNum
	FROM FolderNodes 
	WHERE Dirname IS NULL) AS A
JOIN
	(SELECT Descendant, row_number() OVER (order by Descendant) AS RowNum FROM FolderPaths 
	WHERE Ancestor IN(
		SELECT Descendant 
		FROM FolderPaths 
		WHERE Ancestor = 2) 
	AND length = 0) AS B
ON A.RowNum = B.RowNum;
--复制子树
INSERT INTO FolderPaths
SELECT IdMap2.NewId AS Ancestor, IdMap1.NewId As Descendant, Length
FROM(
	SELECT * FROM FolderPaths 
	WHERE Ancestor IN(
		SELECT Descendant 
		FROM FolderPaths 
		WHERE Ancestor = 2)) AS A
	JOIN IdMap AS IdMap1
	ON A.Descendant = IdMap1.OldId
	JOIN IdMap AS IdMap2
	ON A.Ancestor = IdMap2.OldId;
DROP TABLE IdMap;

SELECT *
FROM(
	SELECT * FROM FolderPaths 
	WHERE Ancestor IN(
		SELECT Descendant 
		FROM FolderPaths 
		WHERE Ancestor = 2)) AS A
	JOIN IdMap AS IdMap1
	ON A.Descendant = IdMap1.OldId
	JOIN IdMap AS IdMap2
	ON A.Ancestor = IdMap2.OldId
	ORDER BY length;
	
SELECT * FROM FolderPaths WHERE Ancestor = 2;

--3.1子节点层级顺序
SELECT Descendant AS SortOrder FROM FolderPaths 
WHERE Ancestor = 2 AND Length > 0
ORDER BY Length;

--3.2筛选出距离为1的连接
SELECT Descendant AS SortOrder, IdMap1.NewId AS Ancestor, IdMap2.NewId As Descendant
FROM(
	SELECT * FROM FolderPaths 
	WHERE Ancestor 
	IN(
		SELECT Descendant 
		FROM FolderPaths 
		WHERE Ancestor = 2)
	AND Length = 1) AS A
	JOIN IdMap AS IdMap1
	ON A.Ancestor = IdMap1.OldId
	JOIN IdMap AS IdMap2
	ON A.Descendant = IdMap2.OldId;
	
--3.3按层级顺序对距离为1的连接排序
SELECT Ancestor, Descendant
FROM
	(SELECT Descendant AS SortOrder FROM FolderPaths 
	WHERE Ancestor = 2 AND Length > 0
	ORDER BY Length) AS T1
	JOIN
	(SELECT Descendant AS SortOrder, IdMap1.NewId AS Ancestor, IdMap2.NewId As Descendant
	FROM(
		SELECT * FROM FolderPaths 
		WHERE Ancestor 
		IN(
			SELECT Descendant 
			FROM FolderPaths 
			WHERE Ancestor = 2)
		AND Length = 1) AS A
		JOIN IdMap AS IdMap1
		ON A.Ancestor = IdMap1.OldId
		JOIN IdMap AS IdMap2
		ON A.Descendant = IdMap2.OldId) AS T2
	ON T1.SortOrder = T2.SortOrder;
	

	
--3.插入，更新版
--创建临时表
CREATE TEMP TABLE IdMap AS 
SELECT Id AS NewId, Descendant AS OldId FROM
	(SELECT Id,row_number() OVER (order by Id) as RowNum
	FROM FolderNodes 
	WHERE Dirname IS NULL) AS A
JOIN
	(SELECT Descendant, row_number() OVER (order by Descendant) AS RowNum FROM FolderPaths 
	WHERE Ancestor IN(
		SELECT Descendant 
		FROM FolderPaths 
		WHERE Ancestor = /*复制子树根*/2) 
	AND length = 0) AS B
ON A.RowNum = B.RowNum;
--按层级插入表（连带根结点）
--1根节点
INSERT INTO ConnectPath
SELECT /*目标位置*/3, NewId FROM IdMap
WHERE OldId = /*复制子树根*/2;
--2其它子结点（按层级顺序对距离为1的连接排序）
INSERT INTO ConnectPath
SELECT Ancestor, Descendant
FROM
	--3.1子节点层级顺序
	(SELECT Descendant AS SortOrder FROM FolderPaths 
	WHERE Ancestor = /*复制子树根*/2 AND Length > 0
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
			WHERE Ancestor = /*复制子树根*/2)
		AND Length = 1) AS A
		JOIN IdMap AS IdMap1
		ON A.Ancestor = IdMap1.OldId
		JOIN IdMap AS IdMap2
		ON A.Descendant = IdMap2.OldId) AS T2
	ON T1.SortOrder = T2.SortOrder;

SELECT * FROM IdMap;
	
--4 重命名文件夹
UPDATE FolderNodes SET Dirname = Source.Dirname
FROM FolderNodes AS Source, IdMap
WHERE Source.Id = IdMap.OldId AND FolderNodes.Id = IdMap.NewId; 



----操作：新建一个文件夹
BEGIN TRANSACTION;
--参数：在哪？新名字？
--操作全部交给视图了
INSERT INTO DirectSubFolder
--VALUES (1, 'dir2');
VALUES (0, 'root'),(1, 'dir2'),(1, 'dir3'), (2, 'dir4'),(2, 'dir5'),(3, 'dir6'),(4, 'dir7');
COMMIT TRANSACTION;

----操作：递归删除文件夹
BEGIN TRANSACTION;
DELETE FROM DirectSubFolder WHERE Ancestor = 1;
COMMIT TRANSACTION;

----操作：复制文件夹
--参数：复制结点？目标父结点？
BEGIN TRANSACTION;
UPDATE FolderCopy
--VALUES (1, 'dir2');
SET Ancestor = /*目标父结点*/3
WHERE Ancestor = /*复制结点*/2;
COMMIT TRANSACTION;

----操作：复制文件夹
--参数：复制结点？目标父结点？
BEGIN TRANSACTION;
INSERT INTO AllSubFolder
--VALUES (1, 'dir2');
SELECT ()
COMMIT TRANSACTION;