CREATE TABLE [dbo].[Users]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Username] VARCHAR(50) NOT NULL, 
    [Email] VARCHAR(100) NOT NULL, 
    [Deleted] BIT NULL, 
    [Created] DATETIMEOFFSET NOT NULL DEFAULT (GetDate()), 
    [FirstName] VARCHAR(150) NULL, 
    [LastName] VARCHAR(150) NULL, 
    [LastLogin] DATETIMEOFFSET NULL, 
    [RowVersion] ROWVERSION NOT NULL
)
