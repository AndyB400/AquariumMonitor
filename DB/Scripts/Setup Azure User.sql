-- Use master database

CREATE LOGIN aquarium_api WITH PASSWORD = *********
GO

-- Swap to aquarium database

CREATE USER aquarium_api FOR LOGIN aquarium_api	WITH DEFAULT_SCHEMA = dbo
GO

-- Add user to the database owner role
EXEC sp_addrolemember N'db_datareader', N'aquarium_api'
GO

EXEC sp_addrolemember N'db_datawriter', N'aquarium_api'
GO