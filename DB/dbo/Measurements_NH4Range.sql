ALTER TABLE [dbo].[Measurements]
	ADD CONSTRAINT [Measurements_NH4Range]
	CHECK (MeasurementTypeId = 1 AND [Value] >= 0 AND [Value] <= 10 OR MeasurementTypeId != 1)
