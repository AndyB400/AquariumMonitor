ALTER TABLE [dbo].[Measurements]
	ADD CONSTRAINT [Measurements_NO2Range]
	CHECK (MeasurementTypeId = 2 AND [Value] >= 0 AND [Value] <= 1 OR MeasurementTypeId != 2)
