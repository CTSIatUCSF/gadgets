IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UCSF].[agGrant]') AND type in (N'U'))
BEGIN
CREATE TABLE [UCSF].[agGrant](
	[GrantPK] [uniqueidentifier] NOT NULL,
	[ApplicationId] [int] NOT NULL,
	[Activity] [varchar](50) NOT NULL,
	[AdministeringIC] [varchar](50) NOT NULL,
	[ApplicationType] [int] NOT NULL,
	[ARRAFunded] [bit] NULL,
	[BudgetStart] [datetime] NULL,
	[BudgetEnd] [datetime] NULL,
	[FOANumber] [varchar](255) NULL,
	[FullProjectNum] [varchar](255) NULL,
	[FundingICS] [varchar](400) NULL,
	[FY] [int] NOT NULL,
	[OrgCity] [varchar](255) NULL,
	[OrgCountry] [varchar](255) NULL,
	[OrgDistrict] [varchar](255) NULL,
	[OrgDUNS] [int] NOT NULL,
	[OrgDept] [varchar](255) NULL,
	[OrgFIPS] [varchar](255) NULL,
	[OrgState] [varchar](2) NULL,
	[OrgZip] [varchar](9) NULL,
	[ICName] [varchar](255) NULL,
	[OrgName] [varchar](255) NULL,
	[ProjectTitle] [varchar](255) NULL,
	[ProjectStart] [datetime] NULL,
	[ProjectEnd] [datetime] NULL,
	[TotalCost] [int] NULL,
	[CoreProjectNumber] [varchar](50) NULL,
	[XML] [xml] NULL,
	[IsVerified] [bit] NULL,
 CONSTRAINT [PK_Grant] PRIMARY KEY CLUSTERED 
(
	[GrantPK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


/****** Object:  Index [IX_Grant]    Script Date: 05/04/2012 18:27:38 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[UCSF].[agGrant]') AND name = N'IX_Grant')
CREATE NONCLUSTERED INDEX [IX_Grant] ON [UCSF].[agGrant] 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO