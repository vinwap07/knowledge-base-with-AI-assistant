-- Создание таблицы пользователей
CREATE TABLE IF NOT EXISTS "Role"(
    id SERIAL PRIMARY KEY, 
    name VARCHAR(15) 
);

INSERT INTO "Role" (Id, Name)
VALUES
    (1, 'User'),
    (2, 'Admin'),
    (3, 'Moderator')
ON CONFLICT (Id) DO NOTHING;

CREATE TABLE IF NOT EXISTS "User" (
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    "name" VARCHAR(100) NOT NULL,
    RoleId INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY (Email),
    FOREIGN KEY (RoleId) REFERENCES "Role"(id) ON DELETE CASCADE                           
);

INSERT INTO "User" (Email, Password, name, RoleId)
VALUES
    ('admin@gmail.com', 'adminpassword', 'admin', 2)
ON CONFLICT (Email) DO NOTHING;

-- Создание таблицы сессий
CREATE TABLE IF NOT EXISTS "Session" (
    SessionId VARCHAR(100) NOT NULL UNIQUE,
    UserEmail VARCHAR(100) NOT NULL,
    EndTime TIMESTAMP NOT NULL,
    PRIMARY KEY (SessionId),
    FOREIGN KEY (UserEmail) REFERENCES "User"(Email) ON DELETE CASCADE
);

-- Создание таблицы логов вопросов
CREATE TABLE IF NOT EXISTS QuestionLog (
    Id SERIAL PRIMARY KEY,
    Question TEXT NOT NULL,
    Answer TEXT NOT NULL,
    Assessment INTEGER NOT NULL,
    UserComment TEXT
);

-- Создание таблицы категорий
CREATE TABLE IF NOT EXISTS Category (
    "name" VARCHAR(100) NOT NULL,
    Slug VARCHAR(100) NOT NULL,
    PRIMARY KEY ("name")
);

-- Создание таблицы статей
CREATE TABLE IF NOT EXISTS Article (
    Id SERIAL PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Summary VARCHAR(300) NOT NULL,
    Content TEXT NOT NULL,
    Author VARCHAR(100) NOT NULL,
    Category VARCHAR(100) NOT NULL,
    PublishDate DATE NOT NULL, 
    ReadingTime INTEGER NOT NULL,
    FOREIGN KEY (Author) REFERENCES "User"(Email) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS UserArticle (
    "User" VARCHAR(100),
    Article INTEGER,
    PRIMARY KEY ("User", Article)
);

INSERT INTO UserArticle ("User", Article)
VALUES 
    ('admin@gmail.com', 1),
    ('admin@gmail.com', 2),
    ('admin@gmail.com', 3)
ON CONFLICT ("User", Article) DO NOTHING; 

