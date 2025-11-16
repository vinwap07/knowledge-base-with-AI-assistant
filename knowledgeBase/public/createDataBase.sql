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
    ('admin@gmail.com', 'X3sFq3xWxxUE2K1KWANwGg==;wAKDVZBX4E4Uk2Ry9hA4OBrWwQ5q9FAijbU+Ov0HwtQ=', 'admin', 2)
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
    description VARCHAR(200) NOT NULL, 
    articlesCount INTEGER DEFAULT 0,
    slug VARCHAR(100) NOT NULL,
    icon VARCHAR(15),
    PRIMARY KEY ("name")
);

INSERT INTO Category ("name", description, slug, icon) VALUES 
('Руководства', 'Пошаговые инструкции и руководства для начинающих и опытных пользователей', 'guides', '📚'),
('Техническая поддержка', 'Решение технических проблем и ответы на вопросы по настройке', 'support', '🔧')
ON CONFLICT ("name") DO NOTHING;

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
    LikesCount INTEGER DEFAULT 0,
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
    ('admin@gmail.com', 3),
    ('admin@gmail.com', 4)
ON CONFLICT ("User", Article) DO NOTHING; 

