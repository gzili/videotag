ALTER TABLE Videos
    RENAME TO VideosOld;

ALTER TABLE VideoTags
    RENAME TO VideoTagsOld;

CREATE TABLE Videos
(
    VideoId             TEXT
        CONSTRAINT PK_Videos PRIMARY KEY,
    FullPath            TEXT    NOT NULL
        CONSTRAINT UQ_Videos_FullPath UNIQUE,
    Width               INTEGER NOT NULL,
    Height              INTEGER NOT NULL,
    Framerate           REAL    NOT NULL,
    DurationInSeconds   REAL    NOT NULL,
    Bitrate             INTEGER NOT NULL,
    Size                INTEGER NOT NULL,
    LastModifiedTimeUtc TEXT    NOT NULL,
    ThumbnailSeek       REAL    NOT NULL
);

UPDATE Videos
SET FullPath = @fullPath,
    Width = @width,
    Height = @height,
    Framerate = @framerate,
    DurationInSeconds = @durationInSeconds,
    Bitrate = @bitrate,
    Size = @size,
    LastModifiedTimeUtc = @lastModifiedTimeUtc,
    ThumbnailSeek = @thumbnailSeek
WHERE VideoId = @videoId;

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
       DurationInSeconds,
       Size,
       LastModifiedTimeUtc,
       ThumbnailSeek
FROM VideosOld;

INSERT INTO VideoTags
SELECT *
FROM VideoTagsOld;

CREATE TABLE CustomThumbnails
(
    VideoId   TEXT
        CONSTRAINT PK_CustomThumbnails PRIMARY KEY
        CONSTRAINT FK_Videos FOREIGN KEY
            REFERENCES Videos (VideoId),
    Thumbnail BLOB NOT NULL
);

CREATE TABLE Meta
(
    Name  TEXT
        CONSTRAINT PK_Meta PRIMARY KEY,
    Value TEXT
);

INSERT INTO Meta(Name, Value)
VALUES ('RebuildNeeded', '1');

DROP TABLE VideoTagsOld;

DROP TABLE VideosOld;