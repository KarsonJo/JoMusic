-- ROLLBACK TRANSACTION;
UPDATE FolderCopy
SET Ancestor = /*目标父结点*/3
WHERE Ancestor = /*复制结点*/2;
UPDATE FolderCut
SET Ancestor = /*目标父结点*/3
WHERE Ancestor = /*复制结点*/2;
VALUES ('dir1'),('dir2');
SELECT * FROM FolderNodes;
SELECT last_insert_rowid();
SELECT * FROM FolderPaths ORDER BY Ancestor;
SELECT * FROM FolderPaths WHERE Length = 1 ORDER BY Ancestor;

INSERT INTO DirectSubFolder
VALUES (0, 'root')

----操作：查询一个子文件夹ID
--主要用于刚插入的文件夹
SELECT Descendant, DescendantName FROM DirectSubFolderInfo 
WHERE Ancestor = 1 AND DescendantName='dir3';


----操作：新建一个文件夹
BEGIN TRANSACTION;
--参数：在哪？新名字？
--操作全部交给视图了
INSERT INTO FolderCreate
--VALUES (1, 'dir2');
VALUES (0, 'root'),(1, 'dir2'),(1, 'dir3'), (2, 'dir4'),(2, 'dir5'),(3, 'dir6'),(4, 'dir7');
COMMIT TRANSACTION;

----操作：递归删除文件夹
BEGIN TRANSACTION;
DELETE FROM DirectSubFolder WHERE Descendant = 1;
COMMIT TRANSACTION;

----操作：复制文件夹
--参数：复制结点？目标父结点？
BEGIN TRANSACTION;
UPDATE FolderCopy
SET Ancestor = /*目标父结点*/3
WHERE Ancestor = /*复制结点*/2;
COMMIT TRANSACTION;

----操作：剪切文件夹
--参数：剪切结点？目标父结点？
BEGIN TRANSACTION;
UPDATE FolderCut
SET Ancestor = /*目标父结点*/3
WHERE Ancestor = /*复制结点*/2;
COMMIT TRANSACTION;