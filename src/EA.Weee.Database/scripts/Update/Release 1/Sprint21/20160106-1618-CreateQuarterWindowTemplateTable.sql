﻿CREATE TABLE [Lookup].[QuarterWindowTemplate] 
(
	Id UNIQUEIDENTIFIER NOT NULL, 
	Quarter int NOT NULL,
	AddStartYears int NOT NULL,
	StartMonth int NOT NULL,
	StartDay int NOT NULL,
	AddEndYears int NOT NULL,
	EndMonth int NOT NULL,
	EndDay int NOT NULL
	CONSTRAINT [PK_QuarterWindowTemplate] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)	
	WITH 
	(
		PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON
	) 
	ON [PRIMARY]
) ON [PRIMARY]