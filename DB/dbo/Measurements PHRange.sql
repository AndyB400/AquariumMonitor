ALTER TABLE [dbo].[Measurements]
	ADD CONSTRAINT [Measurements_PHRange]
	CHECK (MeasurementTypeId = 3 AND [Value] >= 0 AND [Value] <= 14 OR MeasurementTypeId != 3)
