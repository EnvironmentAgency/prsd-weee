GO
/****** Object:  StoredProcedure [Producer].[spgCSVDataBySchemeComplianceYearAndAuthorisedAuthority]    Script Date: 05/11/2015 14:31:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Priety Mahajan
-- Create date: 03 Nov 2015
-- Description:	Returns data about all producers currently registered
--				with the specified scheme in the specified year and AA for Scheme.
--				This data is suitable for populating the CSV file
--				which may be downloaded by AA to see the members associated with scheme in compliance year
-- =============================================
CREATE PROCEDURE [Producer].[spgCSVDataBySchemeComplianceYearAndAuthorisedAuthority]
		@ComplianceYear INT,
		@SchemeId uniqueidentifier = null,
		@CompetentAuthorityId uniqueidentifier = null
AS
BEGIN

	SET NOCOUNT ON;

SELECT
	S.SchemeName,
	S.ApprovalNumber,
	P.TradingName,
    
	CASE O.OrganisationType
		WHEN 1 THEN 'Registered Company'
		WHEN 2 THEN 'Partnership'
		WHEN 3 THEN 'Sole trader or individual'
		ELSE ''
	END AS 'OrganisationType',

	COALESCE(PBC.Name, PBP.Name, '') AS 'OrganisationName',

	 P.RegistrationNumber AS 'PRN',
	 
	 P_First.UpdatedDate AS 'DateRegistered',

 	 P.UpdatedDate AS 'DateAmended',

	 SICCODES.SICCode as 'SICCODES',

     P.VATRegistered,

     P.AnnualTurnover,

	  CASE P.AnnualTurnoverBandType
			WHEN 0 THEN 'Less than or equal to one million pounds'
			WHEN 1 THEN 'Greater than one million pounds'
			ELSE ''
		END AS 'AnnualTurnoverBandType',
      
	  CASE P.EEEPlacedOnMarketBandType
			WHEN 0 THEN 'More than or equal to 5T EEE placed on market'
			WHEN 1 THEN 'Less than 5T EEE placed on market'
			WHEN 2 THEN 'Both'
			ELSE ''
		END AS 'EEEPlacedOnMarketBandType',

	CASE P.ObligationType
			WHEN 1 THEN 'B2B'
			WHEN 2 THEN 'B2C'
			WHEN 3 THEN 'Both'
			ELSE ''
		END AS 'ObligationType',
  
  CASE P.ChargeBandType
			WHEN 0 THEN 'A'
			WHEN 1 THEN 'B'
			WHEN 2 THEN 'C'
			WHEN 3 THEN 'D'
			WHEN 4 THEN 'E'
			ELSE ''
		END AS 'ChargeBand',
		
	CASE P.SellingTechniqueType
			WHEN 0 THEN 'Direct Selling to End User'
			WHEN 1 THEN 'Indirect Selling to End User'
			WHEN 2 THEN 'Both'
			ELSE ''
		END AS 'SellingTechniqueType',

     P.CeaseToExist,	
	  
	   --Correspondence of Notices details
      CFNC.Title,
      CFNC.Forename,
      CFNC.Surname,
      CFNC.Telephone,
      CFNC.Mobile,
      CFNC.Fax,
      CFNC.Email,
      CFNC_A.PrimaryName,
      CFNC_A.SecondaryName,
      CFNC_A.Street,
      CFNC_A.Town,
      CFNC_A.Locality,
      CFNC_A.AdministrativeArea,
      CFNC_A.PostCode,
      CFNC_A_C.Name as 'Country',
   
     --Registered Office details
      PBC.CompanyNumber,
      ROC.Title,
      ROC.Forename,
      ROC.Surname,
      ROC.Telephone,
      ROC.Mobile,
      ROC.Fax,
      ROC.Email,
      ROC_A.PrimaryName,
      ROC_A.SecondaryName,
      ROC_A.Street,
      ROC_A.Town,
      ROC_A.Locality,
      ROC_A.AdministrativeArea,
      ROC_A.PostCode,
      ROC_A_C.Name as 'Country',
   
     --Principal place of business details
	  Partners.Partners,
      PPOB.Title,
      PPOB.Forename,
      PPOB.Surname,
      PPOB.Telephone,
      PPOB.Mobile,
      PPOB.Fax,
      PPOB.Email,
      PPOB_A.PrimaryName,
      PPOB_A.SecondaryName,
      PPOB_A.Street,
      PPOB_A.Town,
      PPOB_A.Locality,
      PPOB_A.AdministrativeArea,
      PPOB_A.PostCode,
      PPOB_A_C.Name as 'Country',
	  
	   --Overseas contact details
      AR.OverseasProducerName,
      OC.Title,
      OC.Forename,
      OC.Surname,
      OC.Telephone,
      OC.Mobile,
      OC.Fax,
      OC.Email,
      OC_A.PrimaryName,
      OC_A.SecondaryName,
      OC_A.Street,
      OC_A.Town,
      OC_A.Locality,
      OC_A.AdministrativeArea,
      OC_A.PostCode,
      OC_A_C.Name as 'Country'

FROM
      [Producer].[Producer] P
INNER JOIN
      [PCS].[MemberUpload] MU
            ON P.MemberUploadId = MU.Id
INNER JOIN
      [Pcs].[Scheme] S
            ON MU.SchemeId = S.Id

INNER JOIN  [Organisation].[Organisation] O
		on S.OrganisationId = O.Id

LEFT JOIN
      [Producer].[AuthorisedRepresentative] AR
            ON P.AuthorisedRepresentativeId = AR.Id
      LEFT JOIN
            [Producer].[Contact] OC
            INNER JOIN
                  [Producer].[Address] OC_A
                  INNER JOIN
                        [Lookup].[Country] OC_A_C
                              ON OC_A.CountryId = OC_A_C.Id
                        ON OC.AddressId = OC_A.Id
                  ON AR.OverseasContactId = OC.Id
LEFT JOIN
      [Producer].[Business] PB
            ON P.ProducerBusinessId = PB.Id
      LEFT JOIN
            [Producer].[Contact] CFNC
            INNER JOIN
                  [Producer].[Address] CFNC_A
                  INNER JOIN
                        [Lookup].[Country] CFNC_A_C
                              ON CFNC_A.CountryId = CFNC_A_C.Id   
                        ON CFNC.AddressId = CFNC_A.Id
                  ON PB.CorrespondentForNoticesContactId = CFNC.Id
      LEFT JOIN
            [Producer].[Company] PBC
                  ON PB.CompanyId = PBC.Id
            LEFT JOIN
                  [Producer].[Contact] ROC
                  INNER JOIN
                        [Producer].[Address] ROC_A
                        INNER JOIN
                              [Lookup].[Country] ROC_A_C
                                    ON ROC_A.CountryId = ROC_A_C.Id     
                              ON ROC.AddressId = ROC_A.Id
                        ON PBC.RegisteredOfficeContactId = ROC.Id
      LEFT JOIN
            [Producer].[Partnership] PBP
                  ON PB.PartnershipId = PBP.Id
            LEFT JOIN
                  [Producer].[Contact] PPOB
                  INNER JOIN
                        [Producer].[Address] PPOB_A
                        INNER JOIN
                              [Lookup].[Country] PPOB_A_C
                                    ON PPOB_A.CountryId = PPOB_A_C.Id   
                              ON PPOB.AddressId = PPOB_A.Id
                        ON PBP.PrincipalPlaceOfBusinessId = PPOB.Id
			INNER JOIN
			(
			SELECT
				MU.ComplianceYear,
				P.RegistrationNumber,
				P.UpdatedDate,
				ROW_NUMBER() OVER
				(
					PARTITION BY
						MU.ComplianceYear,
						P.RegistrationNumber
					ORDER BY P.UpdatedDate
				) AS RowNumber
			FROM
				Producer.Producer P
			INNER JOIN
				PCS.MemberUpload MU
					ON P.MemberUploadId = MU.Id
			WHERE
				MU.IsSubmitted = 1
		) P_First
			ON P.RegistrationNumber = P_First.RegistrationNumber
			AND MU.ComplianceYear = P_First.ComplianceYear
			AND P_First.RowNumber = 1


LEFT JOIN
	(
	select distinct P.Id, STUFF((SELECT distinct '; ' + SIC.Name
         from [Producer].[SICCode] SIC 
         where P.Id = SIC.ProducerId 
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,2,'') SICCode
		from [Producer].[Producer] P
	
	)SICCODES on P.Id = SICCODES.Id

LEFT JOIN
	(
	select distinct P.Id, STUFF((SELECT distinct '; ' + PP.Name
         from [Producer].[Partner] PP 
         where P.Id = PP.PartnershipId
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,2,'') Partners
		from [Producer].[Partnership] P
	
	)Partners on PBP.Id = Partners.Id

WHERE
      MU.ComplianceYear = @ComplianceYear
AND

	  (S.Id = @SchemeId or @SchemeId is null )
AND
	 (@CompetentAuthorityId is null or  S.CompetentAuthorityId = @CompetentAuthorityId)
AND
      MU.IsSubmitted = 1
AND
      P.IsCurrentForComplianceYear = 1
ORDER BY
	S.SchemeName,
     COALESCE(PBC.Name, PBP.Name, '')
END
GO
