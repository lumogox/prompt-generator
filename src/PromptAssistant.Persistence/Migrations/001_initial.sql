CREATE TABLE IF NOT EXISTS schema_version (
    Version INTEGER PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS Templates (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT NOT NULL,
    CreatedUtc  TEXT NOT NULL,
    Json        TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Runs (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    TemplateId      INTEGER NOT NULL,
    Provider        TEXT NOT NULL,
    RanUtc          TEXT NOT NULL,
    RenderedPrompt  TEXT NOT NULL,
    Response        TEXT,
    ExitCode        INTEGER,
    StdErr          TEXT,
    DurationMs      INTEGER NOT NULL,
    FOREIGN KEY (TemplateId) REFERENCES Templates(Id)
);

CREATE INDEX IF NOT EXISTS IX_Runs_TemplateId ON Runs(TemplateId);

INSERT OR IGNORE INTO schema_version (Version) VALUES (1);
