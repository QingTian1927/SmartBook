/*
CREATE DATABASE SmartBook;
USE SmartBook;
*/

CREATE TABLE Users
(
    Id       INT PRIMARY KEY IDENTITY (1,1),
    Username NVARCHAR(100) NOT NULL,
    Email    NVARCHAR(255) NOT NULL UNIQUE,
    Password NVARCHAR(512) NOT NULL
);

CREATE TABLE Authors
(
    Id   INT PRIMARY KEY IDENTITY (1,1),
    Name NVARCHAR(100) NOT NULL,
    Bio  NVARCHAR(MAX) NULL
);

CREATE TABLE Categories
(
    Id   INT PRIMARY KEY IDENTITY (1,1),
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Books
(
    Id         INT PRIMARY KEY IDENTITY (1,1),
    Title      NVARCHAR(255) NOT NULL,
    AuthorId   INT           NOT NULL,
    CategoryId INT           NOT NULL,

    CONSTRAINT FK_Books_Authors FOREIGN KEY (AuthorId) REFERENCES Authors (Id),
    CONSTRAINT FK_Books_Categories FOREIGN KEY (CategoryId) REFERENCES Categories (Id)
);
CREATE UNIQUE INDEX IX_Books_TitleAuthor ON Books (Title, AuthorId);

CREATE TABLE UserBooks
(
    Id     INT PRIMARY KEY IDENTITY (1,1),
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    Rating INT CHECK (Rating BETWEEN 0 AND 5),

    CONSTRAINT FK_UserBooks_Users FOREIGN KEY (UserId) REFERENCES Users (Id),
    CONSTRAINT FK_UserBooks_Books FOREIGN KEY (BookId) REFERENCES Books (Id),
    CONSTRAINT UX_UserBooks_User_Book UNIQUE (UserId, BookId)
);
