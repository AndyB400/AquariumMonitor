ALTER TABLE [dbo].[WaterChanges]
	ADD CONSTRAINT [WaterChange_Date_Range]
	CHECK ([Changed] >= '20160101' AND [Changed] <= '20320101')
