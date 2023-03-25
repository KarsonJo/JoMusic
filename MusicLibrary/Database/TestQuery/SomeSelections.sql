SELECT * FROM FileNodes;
SELECT * FROM DirectSubFolderInfo WHERE Ancestor = 2 AND DescendantName = 'dir2';
SELECT * FROM FolderPaths;
SELECT * FROM FolderNodes;
--查询路径
SELECT Ancestor, Length, FolderNodes.Dirname FROM FolderPaths 
JOIN FolderNodes
ON Ancestor = Id
WHERE Descendant = 2 ORDER BY Length DESC;
SELECT * FROM SongFileMeta;
SELECT * FROM SongArtists;
SELECT * FROM NeteaseData;


--重命名冲突检测
SELECT B.* FROM DirectSubFolder AS A, DirectSubFolder AS B
WHERE A.Descendant = /*目标*/25
AND B.Ancestor = A.Ancestor
AND B.DescendantName = /*新名*/'dir7';
--另一种
SELECT * FROM DirectSubFolder
WHERE Ancestor = 1
AND DescendantName = /*新名*/'dir2';

SELECT * FROM SongFileMeta WHERE Id IN (SELECT DISTINCT SongId FROM FileNodes) LIMIT 3;

SELECT * FROM PlayList;
--待重命名的文件夹
SELECT * FROM DirectSubFolder WHERE DescendantName REGEXP '.*@!Copy\d*#@';
--匹配重名
SELECT * FROM DirectSubFolder WHERE Ancestor = 1 AND DescendantName = 'dir3';
SELECT 1 FROM DirectSubFolder WHERE Ancestor = 1 AND DescendantName = 'dir3';
SELECT * FROM DirectSubFolder WHERE DescendantName='单曲' GROUP BY Ancestor HAVING Count(Ancestor) > 1;
SELECT * FROM DirectSubFolder;
--重命名
UPDATE DirectSubFolder SET DescendantName = 'dir7@!Copy16#@' WHERE DescendantName='dir7' AND Ancestor = 5;


DELETE FROM SongFileMeta;

--查询子树
SELECT Descendant AS Id, FolderNodes.Dirname  FROM FolderPaths 
JOIN FolderNodes
ON Descendant = Id
WHERE Ancestor = 8 ORDER BY Length DESC;

SELECT * FROM DirectSubFolder WHERE Ancestor = 1;
SELECT * FROM DirectSubFolder WHERE Descendant = 7;
DELETE FROM DirectSubFolder WHERE Descendant = 7;
DELETE FROM DirectSubFolder WHERE Descendant IN (SELECT Descendant FROM DirectSubFolder WHERE Ancestor = 1);


DELETE FROM FolderNodes
WHERE Id IN (SELECT Descendant FROM FolderPaths WHERE Ancestor = 1);