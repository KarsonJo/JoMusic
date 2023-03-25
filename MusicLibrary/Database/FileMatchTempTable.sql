CREATE TEMP TABLE Paths(
	StringName TEXT
);

VALUES ('1'),('2'),('3');

INSERT INTO Paths
VALUES ('Eminem - The Ringer.m4a');

CREATE INDEX IX_Paths_StringName ON Paths(StringName);

DROP TABLE Paths;

SELECT StringName FROM Paths, SongFileMeta WHERE StringName = FileName;

SELECT FileName FROM SongFileMeta;