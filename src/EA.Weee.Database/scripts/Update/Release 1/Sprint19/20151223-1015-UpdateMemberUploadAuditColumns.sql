SET NOCOUNT ON

-- Add new audit columns
ALTER TABLE [PCS].[MemberUpload]
ADD CreatedDate DATETIME NULL

ALTER TABLE [PCS].[MemberUpload]
ADD UpdatedDate DATETIME NULL

ALTER TABLE [PCS].[MemberUpload]
ADD CreatedById NVARCHAR(128) NULL

ALTER TABLE [PCS].[MemberUpload]
ADD UpdatedById NVARCHAR(128) NULL

GO

-- Copy existing auditing data to new columns
UPDATE [PCS].[MemberUpload]
SET CreatedById = UserId

UPDATE [PCS].[MemberUpload]
SET [CreatedDate] = [Date]

-- Drop existing audit columns
ALTER TABLE [PCS].[MemberUpload]
DROP COLUMN [Date]

ALTER TABLE [PCS].[MemberUpload]
DROP COLUMN [UserId]

-- Make the new columns non-nullable
ALTER TABLE [PCS].[MemberUpload]
ALTER COLUMN CreatedDate DATETIME NOT NULL

ALTER TABLE [PCS].[MemberUpload]
ALTER COLUMN CreatedById NVARCHAR(128) NOT NULL

-- Add foreign keys
ALTER TABLE [PCS].[MemberUpload]
ADD FOREIGN KEY (CreatedById)
REFERENCES [Identity].[AspNetUsers](Id)

ALTER TABLE [PCS].[MemberUpload]
ADD FOREIGN KEY (UpdatedById)
REFERENCES [Identity].[AspNetUsers](Id)