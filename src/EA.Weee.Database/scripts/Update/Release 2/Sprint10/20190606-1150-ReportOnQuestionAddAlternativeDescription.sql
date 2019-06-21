ALTER TABLE [AATF].ReportOnQuestion ADD AlternativeDescription nvarchar(1000) NULL

GO

UPDATE [AATF].ReportOnQuestion
SET AlternativeDescription = 'Non-obligated WEEE received on behalf of a DCF'
WHERE Question = 'NonObligatedDCF'