CREATE TABLE [dbo].[Aquariums] (
    [Id]      INT           IDENTITY (1, 1) NOT NULL,
    [Name]    VARCHAR (100) NOT NULL,
    [Created] DATETIMEOFFSET      CONSTRAINT [DF_Aquariums_Created] DEFAULT (getdate()) NOT NULL,
    [Notes] VARCHAR(MAX) NULL, 
    [UserId] INT NOT NULL, 
    [AquariumTypeId] INT NULL, 
    [OfficalVolume] DECIMAL(10, 3) NULL, 
    [ActualVolume] DECIMAL(10, 3) NULL, 
    [VolumeUnitId] INT NULL, 
    [Width] DECIMAL(10, 3) NULL, 
    [Height] DECIMAL(10, 3) NULL, 
    [Length] DECIMAL(10, 3) NULL, 
    [DimesionUnitId] INT NULL, 
    [Deleted] BIT NULL, 
    [RowVersion] ROWVERSION NOT NULL, 
    CONSTRAINT [PK_Aquariums] PRIMARY KEY CLUSTERED ([Id] ASC), 
    CONSTRAINT [FK_Aquariums_Users] FOREIGN KEY (UserId) REFERENCES [Users]([Id]), 
    CONSTRAINT [FK_Aquariums_AquariumTypes] FOREIGN KEY ([AquariumTypeId]) REFERENCES [AquariumTypes]([Id]), 
    CONSTRAINT [FK_Aquariums_VolumeUnits] FOREIGN KEY ([VolumeUnitId]) REFERENCES [Units]([Id]), 
    CONSTRAINT [FK_Aquariums_DimesionUnits] FOREIGN KEY ([DimesionUnitId]) REFERENCES [Units]([Id])
);

