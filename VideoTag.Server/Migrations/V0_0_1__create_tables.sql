CREATE TABLE Videos (
    VideoId TEXT CONSTRAINT PK_Videos PRIMARY KEY,
    FullPath TEXT NOT NULL
        CONSTRAINT UQ_Videos_FullPath UNIQUE,
    Duration INTEGER NOT NULL,
    Resolution TEXT NOT NULL,
    Size INTEGER NOT NULL,
    LastModifiedTimeUtc TEXT NOT NULL,
    ThumbnailSeek INTEGER NOT NULL
);

CREATE TABLE Categories (
    CategoryId TEXT CONSTRAINT PK_Categories PRIMARY KEY,
    Label TEXT NOT NULL
);

INSERT INTO Categories(CategoryId, Label) VALUES ('00000000-0000-0000-0000-000000000000', 'Uncategorized');

CREATE TABLE Tags (
    TagId TEXT CONSTRAINT PK_Tags PRIMARY KEY,
    Label TEXT NOT NULL,
    CategoryId TEXT NOT NULL,
    CONSTRAINT FK_Categories
        FOREIGN KEY (CategoryId)
            REFERENCES Categories(CategoryId)
);

CREATE TABLE VideoTags (
    VideoId TEXT,
    TagId TEXT,
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