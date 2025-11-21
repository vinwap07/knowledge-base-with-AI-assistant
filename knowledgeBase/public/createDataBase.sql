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

-- Создание таблицы категорий
CREATE TABLE IF NOT EXISTS Category (
    slug VARCHAR(100) NOT NULL UNIQUE,
    "name" VARCHAR(100) NOT NULL,
    description VARCHAR(200) NOT NULL, 
    articlesCount INTEGER DEFAULT 0,
    icon VARCHAR(15),
    PRIMARY KEY ("slug")
);

INSERT INTO Category (slug, "name", description, icon) VALUES 
('guides', 'Руководства', 'Пошаговые инструкции и руководства для начинающих и опытных пользователей', '📚'),
('support', 'Техническая поддержка', 'Решение технических проблем и ответы на вопросы по настройке',  '🔧')
ON CONFLICT (slug) DO NOTHING;

-- Создание таблицы статей
CREATE TABLE IF NOT EXISTS Article (
    Id SERIAL PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Summary TEXT,
    Content TEXT NOT NULL,
    Description TEXT NOT NULL,
    Author VARCHAR(100) NOT NULL,
    Category VARCHAR(100) NOT NULL,
    PublishDate DATE NOT NULL, 
    ReadingTime INTEGER NOT NULL,
    LikesCount INTEGER DEFAULT 0,
    FOREIGN KEY (Author) REFERENCES "User"(Email) ON DELETE CASCADE,
    FOREIGN KEY (Category) REFERENCES Category(slug) 
);

CREATE TABLE IF NOT EXISTS UserArticle (
    "User" VARCHAR(100),
    Article INTEGER,
    PRIMARY KEY ("User", Article),
    FOREIGN KEY ("User") REFERENCES "User"(Email) ON DELETE CASCADE,
    FOREIGN KEY (Article) REFERENCES Article(id) ON DELETE CASCADE
);


