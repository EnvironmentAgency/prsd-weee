BEGIN TRAN

DECLARE @TestUserEmail nvarchar(63) = 'sfw@environment-agency.gov.uk'

IF NOT EXISTS (SELECT * FROM [Identity].[AspNetUsers] WHERE Email = @TestUserEmail)
BEGIN
	INSERT INTO [Identity].[AspNetUsers]
	(
		Id
		, Email
		, EmailConfirmed
		, PasswordHash
		, SecurityStamp
		, PhoneNumber
		, PhoneNumberConfirmed
		, TwoFactorEnabled
		, LockoutEndDateUtc
		, LockoutEnabled
		, AccessFailedCount
		, UserName
		, FirstName
		, Surname
	)
	VALUES 
	(
		NEWID()
		, @TestUserEmail	
		, 1
		, 'AJhPg4wmEurKbuAzwC+2FUJp/Yz3skfF48ixS0Mtygoe3GePtziFQyss+OoryroZZw=='	
		, '32e86b84-f1b3-4413-baab-e2243e501432'
		, NULL
		, 0
		, 0
		, NULL
		, 1
		, 0
		, @TestUserEmail
		, 'SFW'
		, 'User'
	)

	INSERT INTO [Identity].[AspNetUserClaims]
	(
		UserId,
		ClaimType,
		ClaimValue
	)
	VALUES 
	(
		(SELECT TOP 1 Id FROM [Identity].[AspNetUsers] WHERE Email = 'sfw@environment-agency.gov.uk')
		, 'http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod'
		, 'can_access_internal_area'
	)
END

COMMIT