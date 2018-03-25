ALTER TABLE [dbo].[Measurements]
	ADD CONSTRAINT [Measurements_TempRange]
	CHECK (MeasurementTypeId = 4 AND [Value] >= 0 AND [Value] <= 50 OR MeasurementTypeId != 4)
