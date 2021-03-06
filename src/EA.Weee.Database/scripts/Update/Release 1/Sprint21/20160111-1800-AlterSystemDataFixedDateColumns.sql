DECLARE @newLine as varchar(1) = char(13);
DECLARE @sqlCmd as varchar(max) = '';
SELECT @sqlCmd = @sqlCmd + 'alter table ' + OBJECT_NAME(parent_object_id) + ' drop constraint [' + OBJECT_NAME(OBJECT_ID) + '];' + @newLine
FROM sys.objects
WHERE type = 'd'
AND name like 'DF__SystemDat__Fixed%' or name like 'DF__SystemDat__UseFi%'
ORDER BY CASE WHEN left(OBJECT_NAME(OBJECT_ID),2) = 'PK' then 1 else 0 end
EXEC (@sqlCmd)

ALTER TABLE [SystemData]
DROP COLUMN UseFixedComplianceYearAndQuarter, FixedComplianceYear, FixedQuarter

ALTER TABLE [SystemData]
ADD UseFixedCurrentDate BIT NOT NULL CONSTRAINT DF_SystemData_UseFixedCurrentDate DEFAULT(0),
FixedCurrentDate DATE NOT NULL CONSTRAINT DF_SystemData_FixedCurrentDate DEFAULT('2016-01-01')