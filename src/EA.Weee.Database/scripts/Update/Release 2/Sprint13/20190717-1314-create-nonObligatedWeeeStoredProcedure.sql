GO
/****** Object:  StoredProcedure [AATF].[UkNonObligatedWeeeReceivedByComplianceYear]    Script Date: 24/07/2019 08:48:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Will Andrews>
-- Create date: <18-07-2019>
-- =============================================
CREATE PROCEDURE [AATF].[UkNonObligatedWeeeReceivedByComplianceYear] 
	@ComplianceYear INT
AS
BEGIN

-- This table will be all the returns we want to get weee data for
CREATE TABLE #FinalReturns(
 Id uniqueidentifier,
 [Quarter] int,
 [Year] int)

-- Insert all returns we want to try an use into a temp table
Select *
Into   #Returns
From   [Aatf].[Return]
WHERE FacilityType = 1
AND ComplianceYear = @ComplianceYear
AND ReturnStatus = 2

DECLARE @Id uniqueidentifier
DECLARE @parentId uniqueidentifier

-- Loop through the temp table, if there is no parent id, see if that row is the parent of another row, then repeat until we find the youngest child row. 
-- We then insert the youngest child row into a temp table so that we can get that return data
WHILE EXISTS(SELECT * FROM #Returns)
BEGIN

    SELECT TOP 1 @Id = Id, @parentId = ParentId FROM #Returns

    WHILE EXISTS(SELECT * FROM #Returns WHERE ParentId = @Id)
	BEGIN
		DELETE #Returns WHERE Id = @Id
		SELECT @Id = Id FROM #Returns WHERE ParentId = @Id
	END
	
	INSERT INTO #FinalReturns (Id, [Quarter], [Year]) SELECT Id, [Quarter], ComplianceYear from #Returns WHERE Id = @Id

    DELETE #Returns WHERE Id = @Id

END

  -- Weee category
CREATE TABLE #WeeeCategory(
 ID int,
 Name nvarchar(250))

 INSERT INTO #WeeeCategory (ID, Name) values(1,'1. Large Household Appliances')
 INSERT INTO #WeeeCategory (ID, Name) values(2,'2. Small Household Appliances')
 INSERT INTO #WeeeCategory (ID, Name) values(3,'3. IT and Telecomms Equipment')
 INSERT INTO #WeeeCategory (ID, Name) values(4,'4. Consumer Equipment')
 INSERT INTO #WeeeCategory (ID, Name) values(5,'5. Lighting Equipment')
 INSERT INTO #WeeeCategory (ID, Name) values(6,'6. Electrical and Electronic Tools')
 INSERT INTO #WeeeCategory (ID, Name) values(7,'7. Toys Leisure and Sports')
 INSERT INTO #WeeeCategory (ID, Name) values(8,'8. Medical Devices')
 INSERT INTO #WeeeCategory (ID, Name) values(9,'9. Monitoring and Control Instruments')
 INSERT INTO #WeeeCategory (ID, Name) values(10,'10. Automatic Dispensers')
 INSERT INTO #WeeeCategory (ID, Name) values(11,'11. Display Equipment')
 INSERT INTO #WeeeCategory (ID, Name) values(12,'12. Cooling Appliances Containing Refrigerants')
 INSERT INTO #WeeeCategory (ID, Name) values(13,'13. Gas Discharge Lamps and LED Light Sources')
 INSERT INTO #WeeeCategory (ID, Name) values(14,'14. Photovoltaic Panels')

 CREATE TABLE #Results(
 [Quarter] nvarchar(5),
 [Category] nvarchar(100),
 [TotalNonObligatedWeeeReceived] decimal(23,3),
 [TotalNonObligatedWeeeReceivedFromDcf] decimal(23,3))

 -- Insert Quarter data
 INSERT INTO #Results
 SELECT CONCAT('Q',fr.Quarter) ,
  w.Name, 
 CONVERT(decimal(28,3), CASE WHEN SUM(CASE WHEN non.Dcf = 0 THEN non.Tonnage ELSE 0 END) IS NULL THEN 0 ELSE SUM(CASE WHEN non.Dcf = 0 THEN non.Tonnage ELSE 0 END) END), 
 CONVERT(decimal(28,3), CASE WHEN SUM(CASE WHEN non.Dcf = 1 THEN non.Tonnage ELSE 0 END) IS NULL THEN 0 ELSE SUM(CASE WHEN non.Dcf = 1 THEN non.Tonnage ELSE 0 END) END)
 FROM #WeeeCategory w, [AATF].[NonObligatedWeee] non 
 INNER JOIN #FinalReturns fr
 ON non.ReturnId = fr.Id 
 WHERE w.Id = non.CategoryId
 GROUP BY fr.[Quarter], W.ID, w.Name 

 -- Insert year data
 INSERT INTO #Results
 SELECT @ComplianceYear,
 w.Name, 
 CONVERT(decimal(28,3), CASE WHEN SUM(CASE WHEN non.Dcf = 0 THEN non.Tonnage ELSE 0 END) IS NULL THEN 0 ELSE SUM(CASE WHEN non.Dcf = 0 THEN non.Tonnage ELSE 0 END) END), 
 CONVERT(decimal(28,3), CASE WHEN SUM(CASE WHEN non.Dcf = 1 THEN non.Tonnage ELSE 0 END) IS NULL THEN 0 ELSE SUM(CASE WHEN non.Dcf = 1 THEN non.Tonnage ELSE 0 END) END)
 FROM #WeeeCategory w, [AATF].[NonObligatedWeee] non 
 INNER JOIN #FinalReturns fr
 ON non.ReturnId = fr.Id 
 WHERE w.Id = non.CategoryId
 GROUP BY fr.[Year], W.ID,  w.Name

 
SELECT * from #Results

DROP TABLE #Results
DROP TABLE #Returns
DROP TABLE #WeeeCategory
DROP TABLE #FinalReturns
END