begin transaction
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

 CREATE TABLE #NonDcfData(
 CategoryId int,
 Tonnage decimal(23,3))

 INSERT INTO #NonDcfData
 SELECT w.ID, CONVERT(decimal(28,3), SUM(non.Tonnage)) as Tonnage
 FROM #WeeeCategory, [AATF].[NonObligatedWeee] non
 LEFT JOIN #WeeeCategory w ON w.ID = non.CategoryId
 WHERE non.Dcf = 0
 GROUP BY  w.Id
 
 SELECT * from #NonDcfData



  rollback transaction
