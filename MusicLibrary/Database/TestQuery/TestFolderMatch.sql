WITH RECURSIVE
--查询的目录树
PathName (Dirname) AS(
VALUES ('dir3'),('dir2'),('dir7')),
Path(Depth, Dirname) AS(
SELECT row_number() OVER(), Dirname From PathName
),
--当前的递归文件夹位置
Track(TrackDepth, AncestorId, AncestorName) AS(
SELECT 1,  Id, Dirname
FROM FolderNodes
WHERE Id = 1
UNION ALL
SELECT Track.TrackDepth + 1, D.Descendant, D.DescendantName FROM DirectSubFolder AS D, Track
JOIN Path
ON Track.TrackDepth = Path.Depth
WHERE D.Ancestor = Track.AncestorId
AND D.DescendantName = Path.Dirname)
SELECT AncestorId As ID, AncestorName AS Dirname FROM Track;
SELECT Max(Path.Depth) < Max(Track.TrackDepth) AS Success FROM Path, Track;

UPDATE FolderNodes SET Dirname = 'dir4' WHERE Id = 4;

--勉强能冲版
/*WITH RECURSIVE
--父文件夹，子文件夹，子文件夹名
DirectSubFolderInfo(Ancestor, Descendant, DescendantName) AS(
SELECT F1.Id, F2.Id, F2.Dirname FROM FolderPaths
JOIN FolderNodes AS F1
ON FolderPaths.Ancestor = F1.Id
JOIN FolderNodes AS F2
ON FolderPaths.Descendant = F2.Id
WHERE Length = 1),
--查询的目录树
Path(Depth, Dirname) AS(
VALUES (1,'dir3'),(2,'dir4'),(3,'dir7')),
--当前的递归文件夹位置
Track(TrackDepth, AncestorId) AS(
VALUES (1, 1)
UNION ALL
SELECT Track.TrackDepth + 1, D.Descendant FROM DirectSubFolderInfo AS D, Track
JOIN Path
ON Track.TrackDepth = Path.Depth
WHERE D.Ancestor = Track.AncestorId
AND D.DescendantName = Path.Dirname)
SELECT Max(Path.Depth) < Max(Track.TrackDepth) AS Success FROM Path, Track;*/


SELECT * FROM FolderNodes;
SELECT F1.Id AS Ancestor,
F1.Dirname AS AncestorName,
F2.Id AS Descendant,
F2.Dirname AS DescendantName FROM FolderPaths
JOIN FolderNodes AS F1
ON FolderPaths.Ancestor = F1.Id
JOIN FolderNodes AS F2
ON FolderPaths.Descendant = F2.Id
WHERE Length = 1 AND Ancestor = 2 AND DescendantName = 'dir2'; 
SELECT * FROM DirectSubFolder WHERE Ancestor = 2 AND Dirname = 'Dir5';
