CREATE TABLE CustomThumbnailsNew
(
    VideoId   TEXT,
    Thumbnail BLOB NOT NULL,

    CONSTRAINT PK_CustomThumbnails PRIMARY KEY (VideoId),
    CONSTRAINT FK_Videos
        FOREIGN KEY (VideoId)
            REFERENCES Videos (VideoId) ON DELETE CASCADE
);

INSERT INTO CustomThumbnailsNew SELECT * FROM CustomThumbnails;

DROP TABLE CustomThumbnails;

ALTER TABLE CustomThumbnailsNew RENAME TO CustomThumbnails;