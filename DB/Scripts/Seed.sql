USE [Aquarium]
GO

DELETE FROM Measurements
DELETE FROM MeasurementTypes
DELETE FROM WaterChanges
DELETE FROM Aquariums
DELETE FROM AquariumTypes
DELETE FROM UserClaims
DELETE FROM Claims
DELETE FROM UserPasswords
DELETE FROM Users
DELETE FROM Units

IF NOT EXISTS (SELECT 1 FROM Users)
BEGIN
  SET IDENTITY_INSERT Users ON;

  INSERT INTO Users (Id, Username,Email,FirstName,LastName) 
  VALUES (1, 'andyb','andy@aqua.com', 'Andy', 'B'),
  (2, 'deans','dean@aqua.com', 'Dean', 'S'),
  (3, 'errolr','errol@aqua.com', 'Errol', 'R');

  SET IDENTITY_INSERT Users OFF;
END

IF NOT EXISTS (SELECT 1 FROM UserPasswords)
BEGIN

  INSERT INTO UserPasswords (UserId, PasswordHashAndSalt, Created) 
  VALUES (1, 'abc123', '20170105'),
   (1, 'pass', '20170110'),
   (1, 'hello', '20170123'),
   (1, '$7$C6..../....5325mdf/32DZIgq85iMWCOepzk0RorlsqT/usNY8X29$MYKIFvoPYSFoHyys0YNW40NY/ddrdoJIGdl3bTv3cg9', '20170207'),
   (2, '$7$C6..../....5325mdf/32DZIgq85iMWCOepzk0RorlsqT/usNY8X29$MYKIFvoPYSFoHyys0YNW40NY/ddrdoJIGdl3bTv3cg9', '20170207'),
   (3, '$7$C6..../....5325mdf/32DZIgq85iMWCOepzk0RorlsqT/usNY8X29$MYKIFvoPYSFoHyys0YNW40NY/ddrdoJIGdl3bTv3cg9', '20170207');
END


IF (SELECT COUNT(1) FROM MeasurementTypes) = 0
BEGIN
  SET IDENTITY_INSERT MeasurementTypes ON

  INSERT INTO MeasurementTypes (Id, Name)
     VALUES (1, 'NH4'),
     (2, 'NO2'),
     (3, 'PH'),
     (4, 'Temp');

  SET IDENTITY_INSERT MeasurementTypes OFF
END

IF (SELECT COUNT(1) FROM Units) = 0
BEGIN
  SET IDENTITY_INSERT Units ON

  INSERT INTO Units (Id, [Name], MeasurementUnit, VolumeUnit, DimesionUnit)
     VALUES (1, 'ppm', 1, NULL, NULL),
     (2, 'cm', NULL, NULL, 1),
     (3, 'l', NULL, 1, NULL);

  SET IDENTITY_INSERT Units OFF
END

IF (SELECT COUNT(1) FROM AquariumTypes) = 0
BEGIN
  SET IDENTITY_INSERT AquariumTypes ON

  INSERT INTO AquariumTypes (Id, Name)
     VALUES (1, 'Tropical'),
     (2, 'Coldwater'),
     (3, 'Marine');

  SET IDENTITY_INSERT AquariumTypes OFF
END

IF NOT EXISTS (SELECT 1 FROM Aquariums)
BEGIN
  SET IDENTITY_INSERT Aquariums ON;

  INSERT INTO Aquariums (Id, Name,UserId,AquariumTypeId,OfficalVolume,ActualVolume,VolumeUnitId,Width,Height,[Length],DimesionUnitId)
  SELECT 3, 'Lounge', u.Id, at.Id, 200, 174, vu.Id, 98, 38.5, 43, du.Id
  FROM Users u
  JOIN AquariumTypes at ON at.Name = 'Tropical'
  JOIN Units vu ON vu.Name = 'l'
  JOIN Units du ON du.Name = 'cm'
  WHERE u.Id = 1

  UNION 

  SELECT 4, 'Bedroom', u.Id, at.Id, 300, 260, vu.Id, 120, 45, 50, du.Id
  FROM Users u
  JOIN AquariumTypes at ON at.Name = 'Marine'
  JOIN Units vu ON vu.Name = 'l'
  JOIN Units du ON du.Name = 'cm'
  WHERE u.Id = 1

  UNION
   
  SELECT 5, 'Study', u.Id, at.Id, 400, 360, vu.Id, 180, 45, 50, du.Id
  FROM Users u
  JOIN AquariumTypes at ON at.Name = 'Marine'
  JOIN Units vu ON vu.Name = 'l'
  JOIN Units du ON du.Name = 'cm'
  WHERE u.Id = 2

  SET IDENTITY_INSERT Aquariums OFF;
END

IF (SELECT COUNT(1) FROM Measurements) = 0
BEGIN
  INSERT INTO dbo.Measurements (MeasurementTypeId, UnitId,AquariumId,Value,Taken,UserId)
  VALUES
    (1,1,3,0.05,'20180102 18:00:00', 1),
    (1,1,3,5,'20180110 18:00:00', 1),
    (1,1,3,6,'20180111 18:00:00', 1),
    (1,1,3,3,'20180115 18:00:00', 1),
    (1,1,3,0.3,'20180118 18:00:00', 1),
    (1,1,3,0.3,'20180119 18:00:00', 1),
    (1,1,3,0.3,'20180118 18:00:00', 1),

-- NO2
    (2,1,3,0.1,'20180102 18:00:00', 1),
    (2,1,3,1,'20180110 18:00:00', 1),
    (2,1,3,1,'20180111 18:00:00', 1),
    (2,1,3,1,'20180115 18:00:00', 1),
    (2,1,3,1,'20180118 18:00:00', 1),
    (2,1,3,1,'20180119 18:00:00', 1),
    (2,1,3,1,'20180121 18:00:00', 1),

-- PH
    (3,1,3,7,'20180102 18:00:00', 1),
    (3,1,3,6.5,'20180118 18:00:00', 1),

-- Temp
    (4,1,3,24.2,'20180102 18:00:00', 1),
    (4,1,3,25.2,'20180110 18:00:00', 1),
    (4,1,3,26.5,'20180111 18:00:00', 1),
    (4,1,3,27.6,'20180115 18:00:00', 1),
    (4,1,3,27.4,'20180118 18:00:00', 1),
    (4,1,3,27.4,'20180118 18:00:00', 1);
END

IF (SELECT COUNT(1) FROM WaterChanges) = 0
BEGIN
  SET IDENTITY_INSERT WaterChanges ON;

  INSERT INTO WaterChanges (Id, AquariumId,UserId,PercentageChanged,Changed)
     VALUES 
     (1, 3,1,10, '20180102'),
     (2, 3,1,15, '20180106'),
     (3, 3,1,12, '20180110'),
     (4, 4,1,10, '20180115');

  SET IDENTITY_INSERT WaterChanges OFF;
END

IF (SELECT COUNT(1) FROM Claims) = 0
BEGIN
	SET IDENTITY_INSERT Claims ON;

	INSERT INTO Claims (Id,[Name])
    VALUES (1, 'Admin'),
    (2, 'StandardUser'),
	 (3, 'RaspberryPi');

	SET IDENTITY_INSERT Claims OFF;
END

IF (SELECT COUNT(1) FROM UserClaims) = 0
BEGIN
  INSERT INTO UserClaims (UserId, ClaimId)
  VALUES 
  --(1,1),
  --(1,2),
  --(1,3),
  (2,1),
  (2,2),
  (2,3)
END


