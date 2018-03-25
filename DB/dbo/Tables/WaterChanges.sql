CREATE TABLE [dbo].[WaterChanges]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [AquariumId] INT NOT NULL, 
	[UserId] INT NOT NULL,
    [PercentageChanged] TINYINT NOT NULL, 
    [Changed] DATETIMEOFFSET NOT NULL, 
    [Deleted] BIT NULL, 
    [RowVersion] ROWVERSION NOT NULL, 
    CONSTRAINT [FK_WaterChange_Aquarium] FOREIGN KEY (AquariumId) REFERENCES [Aquariums]([Id]), 
    CONSTRAINT [FK_WaterChanges_Users] FOREIGN KEY (UserId) REFERENCES [Users](Id)
)
