CREATE TABLE [dbo].[Units] (
    [Id]   INT          IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (50) NOT NULL,
    [MeasurementUnit] BIT NULL, 
    [VolumeUnit] BIT NULL, 
    [DimesionUnit] BIT NULL, 
    CONSTRAINT [PK_Units] PRIMARY KEY CLUSTERED ([Id] ASC)
);

