/* Make Name column nullable */

ALTER TABLE Organisation.Organisation 
ALTER COLUMN Name nvarchar(2048) null