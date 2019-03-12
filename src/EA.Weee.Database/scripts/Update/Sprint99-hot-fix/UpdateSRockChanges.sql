﻿ALTER TABLE [PCS].[MemberUpload]
ADD HasAnnualCharge bit NOT NULL DEFAULT 0
GO
ALTER TABLE [Lookup].[CompetentAuthority]
ADD AnnualChargeAmount [decimal](18, 2)
GO
UPDATE [Lookup].[CompetentAuthority]
SET AnnualChargeAmount = 12500.00
WHERE Id = 'A3C2D0DD-53A1-4F6A-99D0-1CCFC87611A8'