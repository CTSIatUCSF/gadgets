IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UCSF].[agGrantData]') AND type in (N'U'))
BEGIN
CREATE TABLE [UCSF].[agGrantData](
	[APPLICATION_ID] [varchar](100) NULL,
	[ACTIVITY] [varchar](10) NULL,
	[ADMINISTERING_IC] [varchar](1000) NULL,
	[APPLICATION_TYPE] [varchar](1000) NULL,
	[ARRA_FUNDED] [varchar](1000) NULL,
	[AWARD_NOTICE_DATE] [varchar](1000) NULL,
	[BUDGET_START] [varchar](1000) NULL,
	[BUDGET_END] [varchar](1000) NULL,
	[CFDA_CODE] [varchar](1000) NULL,
	[CORE_PROJECT_NUM] [varchar](1000) NULL,
	[ED_INST_TYPE] [varchar](1000) NULL,
	[FOA_NUMBER] [varchar](1000) NULL,
	[FULL_PROJECT_NUM] [varchar](1000) NULL,
	[FUNDING_ICs] [varchar](1000) NULL,
	[FY] [varchar](1000) NULL,
	[IC_NAME] [varchar](1000) NULL,
	[NIH_SPENDING_CATS] [varchar](1000) NULL,
	[ORG_CITY] [varchar](1000) NULL,
	[ORG_COUNTRY] [varchar](1000) NULL,
	[ORG_DEPT] [varchar](1000) NULL,
	[ORG_DISTRICT] [varchar](1000) NULL,
	[ORG_DUNS] [varchar](100) NULL,
	[ORG_FIPS] [varchar](1000) NULL,
	[ORG_NAME] [varchar](200) NULL,
	[ORG_STATE] [varchar](1000) NULL,
	[ORG_ZIPCODE] [varchar](1000) NULL,
	[PHR] [varchar](1000) NULL,
	[PI_IDS] [varchar](1000) NULL,
	[PI_NAMEs] [varchar](1000) NULL,
	[PROGRAM_OFFICER_NAME] [varchar](1000) NULL,
	[PROJECT_END] [varchar](1000) NULL,
	[Project_Terms] [varchar](8000) NULL,
	[PROJECT_TITLE] [varchar](8000) NULL,
	[SERIAL_NUMBER] [varchar](8000) NULL,
	[STUDY_SECTION] [varchar](8000) NULL,
	[STUDY_SECTION_NAME] [varchar](8000) NULL,
	[SUBPROJECT_ID] [varchar](8000) NULL,
	[SUFFIX] [varchar](8000) NULL,
	[SUPPORT_YEAR] [varchar](8000) NULL,
	[TOTAL_COST] [varchar](8000) NULL,
	[TOTAL_COST_SUB_PROJECT] [varchar](8000) NULL
) ON [PRIMARY]
END
GO


/****** Object:  Index [IX_RawData]    Script Date: 02/24/2012 18:22:52 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[UCSF].[GrantData]') AND name = N'IX_RawData')
CREATE NONCLUSTERED INDEX [IX_RawData] ON [UCSF].[agGrantData] 
(
	[APPLICATION_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO


/****** Object:  Index [IX_RawData_1]    Script Date: 02/24/2012 18:22:52 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[UCSF].[GrantData]') AND name = N'IX_RawData_1')
CREATE NONCLUSTERED INDEX [IX_RawData_1] ON [UCSF].[agGrantData] 
(
	[ORG_NAME] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO