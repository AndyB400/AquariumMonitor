CREATE VIEW [dbo].[vw_Aquarium]
	AS 

SELECT a.Id,a.Name,a.Created,a.Notes,a.UserId,a.OfficalVolume,a.ActualVolume,a.Width,a.Height,a.[Length], a.[RowVersion]
,a.AquariumTypeId, at.Name AS AquariumTypeName
,a.DimesionUnitId,dm.Name AS DimesionUnitName
,a.VolumeUnitId,vm.Name AS VolumeUnitName
FROM Aquariums a
JOIN Users u ON u.Id = a.UserId
JOIN AquariumTypes at ON at.Id = a.AquariumTypeId
LEFT JOIN Units dm on dm.Id = a.DimesionUnitId
LEFT JOIN Units vm on vm.Id = a.VolumeUnitId
