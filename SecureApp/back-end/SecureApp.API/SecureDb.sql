CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL
);

INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('admin', 'TEMP_HASH', 'Admin');

INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('user', 'USER_HASH', 'User');


UPDATE Users
SET PasswordHash = '$2a$11$ji5mMbiyWalomKZM4CLWpepApqFKOJqMsGA6kJrqhcPt4sYTYleUa'
WHERE Username = 'admin';

CREATE TABLE Files (
    Id INT IDENTITY PRIMARY KEY,
    FileName NVARCHAR(255),
    FilePath NVARCHAR(500),
    UploadedBy NVARCHAR(50),
    UploadDate DATETIME2 DEFAULT GETDATE()
);

UPDATE Users
SET PasswordHash = '$2a$11$ji5mMbiyWalomKZM4CLWpepApqFKOJqMsGA6kJrqhcPt4sYTYleUa'
WHERE Username = 'user';

ALTER TABLE Files
ALTER COLUMN UploadDate DATETIME2;