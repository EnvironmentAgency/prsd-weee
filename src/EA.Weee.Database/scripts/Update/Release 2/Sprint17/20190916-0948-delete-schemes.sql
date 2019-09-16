﻿BEGIN TRANSACTION
 
SELECT Id INTO #OrganisationsToDelete FROM [Organisation].[Organisation] WHERE Id IN (

'54DB84EA-0306-4606-AD62-AA5300F591C8',
'94151B31-F8D5-4C8A-8D39-A86900BB9AAF',
'8B14EF05-1740-4005-A9F8-A89400F1AAF1',
'60A9EBC3-8DCD-4190-8B71-A95200AAEEF1',
'B3BE1D44-7DAF-48E7-B55B-A88E010D297C',
'B3E04063-5A55-4F48-9233-A6E4009A6B3B',
'6B060EF1-83CD-40B2-93B4-A94200940BA7',
'87BB35BD-B503-4D2A-805F-A76001423DAF',
'A08FDC2F-533D-48DD-BDB2-AA32009D390A',
'04689E17-8C7B-4D3F-9CBF-A9AD0009197C',
'521139BF-01E2-48F3-8091-A64201261ECE',
'260C8FEB-5214-4290-BD3F-AA2900F1D02B',
'3717B135-91D4-435A-83F8-AA0A0114571A',
'CA309EBD-936A-4527-9A5B-A9810062A3B9',
'CF9FA8BB-4C01-426E-9D8F-A7A500A5F4CD',
'BF480162-7960-4AFD-81F3-A7D00179B0DB',
'4A10B47E-305B-417D-947C-A886000E4B2E',
'9BF29C7B-8AC8-4430-9CD3-A6D300F74037',
'E52D45F4-8933-4C86-8B45-AA3F010772AF',
'A18C6946-469E-4467-9CF5-AA3100FE9683',
'ADCA5C9E-0E7C-4572-B501-A88000D6DB5B',
'56BECDD3-AAC1-40BE-A2E7-A65700063C9C'
)
 
IF EXISTS (
SELECT * FROM 
	#OrganisationsToDelete o 
	INNER JOIN [PCS].Scheme s ON s.OrganisationId = o.Id
	INNER JOIN [PCS].DataReturnUpload m ON m.SchemeId = s.Id) BEGIN
	GOTO rollback_transaction
END

IF EXISTS (
SELECT * FROM 
	#OrganisationsToDelete o 
	INNER JOIN [AATF].AATF a ON a.OrganisationId = o.Id) BEGIN
	GOTO rollback_transaction
END
	   
DELETE 
	ou
FROM
	[Organisation].OrganisationUser ou
	INNER JOIN #OrganisationsToDelete o ON ou.OrganisationId = o.Id

DELETE
	s
FROM
	#OrganisationsToDelete o 
	INNER JOIN [PCS].Scheme s ON s.OrganisationId = o.Id

DELETE
	org
FROM
	#OrganisationsToDelete o
	INNER JOIN [Organisation].Organisation org ON o.Id = org.Id

DROP TABLE #OrganisationsToDelete

COMMIT TRANSACTION
RETURN

rollback_transaction:
	ROLLBACK TRANSACTION