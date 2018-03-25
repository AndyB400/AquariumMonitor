CREATE TABLE [dbo].[UserClaims]
(
	[UserId] INT NOT NULL , 
    [ClaimId] INT NOT NULL, 
    PRIMARY KEY ([ClaimId], [UserId]), 
    CONSTRAINT [FK_UserClaims_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]), 
    CONSTRAINT [FK_UserClaims_Claims] FOREIGN KEY ([ClaimId]) REFERENCES [Claims]([Id])
)
