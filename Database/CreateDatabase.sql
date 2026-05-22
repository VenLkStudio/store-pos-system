USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'RetailShop')
    DROP DATABASE RetailShop;
GO
CREATE DATABASE RetailShop;
GO
USE RetailShop;
GO

CREATE TABLE Сотрудники (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    ФИО         NVARCHAR(200) NOT NULL,
    должность   NVARCHAR(100),
    роль        NVARCHAR(50)  NOT NULL,  -- Оператор | Администратор | Товаровед | СтаршийКассир
    логин       NVARCHAR(50)  UNIQUE NOT NULL,
    пароль      NVARCHAR(100) NOT NULL
);

CREATE TABLE Поставщики (
    id       INT IDENTITY(1,1) PRIMARY KEY,
    название NVARCHAR(200) NOT NULL,
    ИНН      NVARCHAR(12),
    контакт  NVARCHAR(200),
    телефон  NVARCHAR(20)
);

CREATE TABLE Товары (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    название         NVARCHAR(200) NOT NULL,
    штрихкод         NVARCHAR(50),
    единицаИзмерения NVARCHAR(20) DEFAULT 'шт'
);

CREATE TABLE Склад (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    товарId     INT NOT NULL REFERENCES Товары(id),
    количество  INT NOT NULL DEFAULT 0,
    секция      NVARCHAR(50)
);

CREATE TABLE РозничныеЦены (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    товарId          INT NOT NULL REFERENCES Товары(id),
    закупочнаяЦена   DECIMAL(10,2) NOT NULL DEFAULT 0,
    наценка          DECIMAL(5,2)  NOT NULL DEFAULT 0,
    розничнаяЦена    DECIMAL(10,2) NOT NULL DEFAULT 0,
    дата             DATETIME DEFAULT GETDATE()
);

CREATE TABLE ПартииТовара (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    датаПоступления  DATETIME NOT NULL DEFAULT GETDATE(),
    поставщикId      INT NOT NULL REFERENCES Поставщики(id),
    операторId       INT NOT NULL REFERENCES Сотрудники(id),
    статус           NVARCHAR(50) DEFAULT 'Создана'  -- Создана | ОтправленаНаСклад | Отклонена
);

CREATE TABLE СтрокиПартии (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    партияId    INT NOT NULL REFERENCES ПартииТовара(id),
    товарId     INT NOT NULL REFERENCES Товары(id),
    количество  INT NOT NULL,
    цена        DECIMAL(10,2) NOT NULL
);

CREATE TABLE Документы (
    id        INT IDENTITY(1,1) PRIMARY KEY,
    партияId  INT NOT NULL REFERENCES ПартииТовара(id),
    тип       NVARCHAR(50) NOT NULL,  -- Накладная | СчётФактуры | СертификатКачества
    номер     NVARCHAR(50),
    дата      DATE,
    сумма     DECIMAL(10,2),
    проверен  BIT DEFAULT 0
);

CREATE TABLE ЗаявкиВЗал (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    товарId          INT NOT NULL REFERENCES Товары(id),
    количество       INT NOT NULL,
    администраторId  INT NOT NULL REFERENCES Сотрудники(id),
    дата             DATETIME DEFAULT GETDATE(),
    статус           NVARCHAR(50) DEFAULT 'Новая'  -- Новая | Выполнена
);

CREATE TABLE ВозвратыПоставщику (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    партияId         INT NOT NULL REFERENCES ПартииТовара(id),
    товарId          INT NOT NULL REFERENCES Товары(id),
    товаровЕдId      INT NOT NULL REFERENCES Сотрудники(id),
    количество       INT NOT NULL,
    причина          NVARCHAR(500),
    дата             DATETIME DEFAULT GETDATE()
);

CREATE TABLE Кассы (
    id      INT IDENTITY(1,1) PRIMARY KEY,
    номер   NVARCHAR(20) NOT NULL,
    статус  NVARCHAR(20) DEFAULT 'Закрыта'  -- Открыта | Закрыта
);

CREATE TABLE Чеки (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    кассаId     INT NOT NULL REFERENCES Кассы(id),
    дата        DATETIME DEFAULT GETDATE(),
    итого       DECIMAL(10,2) NOT NULL DEFAULT 0,
    аннулирован BIT DEFAULT 0
);

CREATE TABLE СтрокиЧека (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    чекId       INT NOT NULL REFERENCES Чеки(id),
    товарId     INT NOT NULL REFERENCES Товары(id),
    количество  INT NOT NULL,
    цена        DECIMAL(10,2) NOT NULL
);

CREATE TABLE КассовыеОтчёты (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    кассаId         INT NOT NULL REFERENCES Кассы(id),
    кассирId        INT NOT NULL REFERENCES Сотрудники(id),
    дата            DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    суммаЗаСмену    DECIMAL(10,2) DEFAULT 0,
    количествоЧеков INT DEFAULT 0
);

CREATE TABLE Инвентаризация (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    дата            DATETIME DEFAULT GETDATE(),
    ответственный   NVARCHAR(200),
    статус          NVARCHAR(50) DEFAULT 'В процессе'
);

CREATE TABLE СтрокиИнвентаризации (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    инвентаризацияId    INT NOT NULL REFERENCES Инвентаризация(id),
    товарId             INT NOT NULL REFERENCES Товары(id),
    остатокПоБазе       INT NOT NULL DEFAULT 0,
    фактическийОстаток  INT NOT NULL DEFAULT 0
);

-- ─── Тестовые данные ────────────────────────────────────────────────────────
INSERT INTO Сотрудники (ФИО, должность, роль, логин, пароль) VALUES
('Иванов Иван Иванович',       'Оператор',        'Оператор',        'operator',  '1234'),
('Петрова Мария Сергеевна',    'Администратор',   'Администратор',   'admin',     '1234'),
('Сидоров Алексей Николаевич', 'Товаровед',       'Товаровед',       'tovaroved', '1234'),
('Козлова Елена Викторовна',   'Старший кассир',  'СтаршийКассир',   'kassir',    '1234');

INSERT INTO Поставщики (название, ИНН, контакт, телефон) VALUES
('ООО «ПродСнаб»',  '1234567890', 'Смирнов А.А.',  '+7-999-111-22-33'),
('АО «Фреш»',       '0987654321', 'Кузнецов Б.Б.', '+7-999-444-55-66'),
('ИП Зайцев В.В.',  '1122334455', 'Зайцев В.В.',   '+7-999-777-88-99');

INSERT INTO Товары (название, штрихкод, единицаИзмерения) VALUES
('Молоко 3.2% 1л',         '4600001111111', 'шт'),
('Хлеб белый нарезной',    '4600002222222', 'шт'),
('Масло сливочное 200г',   '4600003333333', 'шт'),
('Сыр российский 300г',    '4600004444444', 'шт'),
('Яйцо куриное С1 10шт',   '4600005555555', 'упак'),
('Сахар-песок 1кг',        '4600006666666', 'шт'),
('Мука пшеничная 1кг',     '4600007777777', 'шт'),
('Чай Lipton 25пак',       '4600008888888', 'шт');

INSERT INTO Склад (товарId, количество, секция) VALUES
(1,150,'A-1'),(2,200,'A-2'),(3,80,'Б-1'),
(4,60,'Б-2'),(5,120,'В-1'),(6,300,'В-2'),
(7,250,'Г-1'),(8,100,'Г-2');

INSERT INTO РозничныеЦены (товарId, закупочнаяЦена, наценка, розничнаяЦена) VALUES
(1,55,45,79.9),(2,30,66,49.9),(3,120,42,169.9),
(4,180,39,249.9),(5,90,44,129.9),(6,65,38,89.9),
(7,55,45,79.9),(8,75,47,109.9);

INSERT INTO Кассы (номер, статус) VALUES
('Касса №1','Закрыта'),('Касса №2','Закрыта'),('Касса №3','Закрыта');
GO
PRINT 'База данных RetailShop успешно создана.';
