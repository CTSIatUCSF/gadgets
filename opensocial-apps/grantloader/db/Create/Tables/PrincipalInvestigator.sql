IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrincipalInvestigator]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PrincipalInvestigator](
	[PrincipalInvestigatorId] [uniqueidentifier] NOT NULL,
	[PrincipalInvestigator_Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
        [EmployeeId] nvarchar(50),
 CONSTRAINT [PK_PrincipalInvestigator] PRIMARY KEY CLUSTERED 
(
	[PrincipalInvestigatorId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


/****** Object:  Index [IX_PrincipalInvestigator]    Script Date: 02/15/2012 18:26:40 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PrincipalInvestigator]') AND name = N'IX_PrincipalInvestigator')
CREATE NONCLUSTERED INDEX [IX_PrincipalInvestigator] ON [dbo].[PrincipalInvestigator] 
(
	[PrincipalInvestigator_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO


