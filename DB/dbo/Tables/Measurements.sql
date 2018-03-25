CREATE TABLE [dbo].[Measurements] (
    [Id]                INT IDENTITY,
	[MeasurementTypeId] TINYINT         NOT NULL,
    [UnitId]            INT             NOT NULL,
    [AquariumId]        INT             NOT NULL,
    [Value]             DECIMAL (10, 3) NOT NULL,
    [Taken]             DATETIMEOFFSET        CONSTRAINT [DF_Measurements_Taken] DEFAULT (getdate()) NOT NULL,
    [UserId] INT NOT NULL, 
    [Deleted] BIT NULL, 
    [RowVersion] ROWVERSION NOT NULL, 
    CONSTRAINT [PK_Measurements] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Measurements_Aquariums] FOREIGN KEY ([AquariumId]) REFERENCES [dbo].[Aquariums] ([Id]),
    CONSTRAINT [FK_Measurements_Units] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[Units] ([Id]), 
    CONSTRAINT [FK_Measurements_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]), 
    CONSTRAINT [FK_Measurements_MeasurementTypes] FOREIGN KEY ([MeasurementTypeId]) REFERENCES [MeasurementTypes]([Id])
);

