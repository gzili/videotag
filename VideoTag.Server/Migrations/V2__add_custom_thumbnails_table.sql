ALTER TABLE Videos
    RENAME TO VideosOld;

ALTER TABLE VideoTags
    RENAME TO VideoTagsOld;

CREATE TABLE Videos
(
    VideoId             TEXT,
    FullPath            TEXT    NOT NULL,
    Width               INTEGER NOT NULL,
    Height              INTEGER NOT NULL,
    Framerate           REAL    NOT NULL,
    DurationInSeconds   REAL    NOT NULL,
    Bitrate             INTEGER NOT NULL,
    Size                INTEGER NOT NULL,
    LastModifiedTimeUtc TEXT    NOT NULL,
    ThumbnailSeek       REAL    NOT NULL,

    CONSTRAINT PK_Videos PRIMARY KEY (VideoId),
    CONSTRAINT UQ_Videos_FullPath UNIQUE (FullPath)
);

CREATE TABLE VideoTags
(
    VideoId TEXT,
    TagId   TEXT,
    
    CONSTRAINT PK_VideoTags PRIMARY KEY (VideoId, TagId),
    CONSTRAINT FK_Videos
        FOREIGN KEY (VideoId)
            REFERENCES Videos (VideoId)
            ON DELETE CASCADE,
    CONSTRAINT FK_Tags
        FOREIGN KEY (TagId)
            REFERENCES Tags (TagId)
            ON DELETE CASCADE
);

INSERT INTO Videos(VideoId, FullPath, Width, Height, Framerate, Bitrate, DurationInSeconds, Size, LastModifiedTimeUtc,
                   ThumbnailSeek)
SELECT VideoId,
       FullPath,
       0,
       0,
       0,
       0,
       Duration,
       Size,
       LastModifiedTimeUtc,
       ThumbnailSeek
FROM VideosOld;

INSERT INTO VideoTags
SELECT *
FROM VideoTagsOld;

CREATE TABLE CustomThumbnails
(
    VideoId   TEXT,
    Thumbnail BLOB NOT NULL,
    
    CONSTRAINT PK_CustomThumbnails PRIMARY KEY (VideoId),
    CONSTRAINT FK_Videos
        FOREIGN KEY (VideoId)
            REFERENCES Videos (VideoId)
);

CREATE TABLE Meta
(
    Name  TEXT,
    Value TEXT,

    CONSTRAINT PK_Meta PRIMARY KEY (Name)
);

INSERT INTO Meta(Name, Value)
VALUES ('RebuildNeeded', '1');

DROP TABLE VideoTagsOld;

DROP TABLE VideosOld;