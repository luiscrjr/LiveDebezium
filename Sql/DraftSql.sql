CREATE DATABASE poc_debezium
CREATE DATABASE poc_debezium_destino

use poc_debezium

EXEC sys.sp_cdc_enable_db;

CREATE TABLE userdata (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    BirthDate DATE,
    Email NVARCHAR(255) UNIQUE,
    Salary DECIMAL(18, 2),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    ProfileImage VARBINARY(MAX),
    Comments TEXT,
    Rating FLOAT
);


EXEC sys.sp_cdc_enable_table

     @source_schema = 'dbo',

     @source_name   = 'userdata',

     @role_name     = 'sa';


SELECT * FROM sys.tables WHERE is_tracked_by_cdc = 1;


--loop para inserir registros na userdata

DECLARE @i INT = 12;
WHILE @i <= 14
BEGIN
    INSERT INTO userdata (Name, BirthDate, Email, Salary, IsActive, ProfileImage, Comments, Rating)
    VALUES (
        CONCAT('Name_', @i), -- Nome fictício
        DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 10000), GETDATE()), -- Data de nascimento aleatória nos últimos 10000 dias
        CONCAT('email_', @i, '@example.com'), -- Email fictício
        ROUND(CAST(5000 + (RAND(CHECKSUM(NEWID())) * 95000) AS DECIMAL(18, 2)), 2), -- Salário aleatório entre 5000 e 100000
        CAST(RAND(CHECKSUM(NEWID())) * 2 AS INT), -- IsActive aleatório (0 ou 1)
        NULL, -- ProfileImage como NULL (pode ser ajustado para dados binários)
        CONCAT('Comment_', @i), -- Comentário fictício
        CAST(RAND(CHECKSUM(NEWID())) * 5 AS FLOAT) -- Rating aleatório entre 0 e 5
    );
    SET @i = @i + 1;
END

