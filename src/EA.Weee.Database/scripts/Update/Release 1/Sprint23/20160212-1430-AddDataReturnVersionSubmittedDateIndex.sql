GO

CREATE NONCLUSTERED INDEX [IX_DataReturnVersion_SubmittedDate] ON [PCS].[DataReturnVersion]
(
	[SubmittedDate] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO


