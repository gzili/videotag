INSERT INTO Meta (Name, Value)
VALUES ('VacuumNeeded', '1');

UPDATE Videos
SET LastModifiedTimeUtc = replace(LastModifiedTimeUtc, ' ', 'T') || 'Z';

CREATE TABLE WatchLogs
(
    Id      INTEGER PRIMARY KEY,
    VideoId TEXT NOT NULL,
    TimeUtc TEXT NOT NULL,

    FOREIGN KEY (VideoId) REFERENCES Videos (VideoId) ON DELETE CASCADE
);
