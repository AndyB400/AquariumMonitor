CREATE VIEW [dbo].[vw_UserClaims]
	AS 
	
SELECT [Name], u.Id AS UserId
FROM Claims c
JOIN UserClaims uc ON c.Id = uc.ClaimId	
JOIN Users u ON uc.UserId = u.Id	
