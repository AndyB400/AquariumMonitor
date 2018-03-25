CREATE TABLE [dbo].[UserPasswords]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NOT NULL, 
    [PasswordHashAndSalt] VARCHAR(250) NOT NULL, 
    [Created] DATETIME2 NOT NULL
)
