CREATE TABLE [dbo].[shindig_appdata](
	[userId] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[appId] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[keyname] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[value] [nvarchar](4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[createdDT] [datetime] NULL CONSTRAINT [DF_shindig_appdata_createdDT]  DEFAULT (getdate()),
	[updatedDT] [datetime] NULL CONSTRAINT [DF_shindig_appdata_updatedDT]  DEFAULT (getdate())
) ON [PRIMARY]

CREATE TABLE [dbo].[person](
	[PersonID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[CtscID] [int] NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[MiddleName] [nvarchar](50) NULL,
	[DisplayName] [nvarchar](255) NULL,
	[DegreeList] [nvarchar](50) NULL,
	[Suffix] [nvarchar](50) NULL,
	[IsActive] [bit] NULL,
	[EmailAddr] [nvarchar](255) NULL,
	[Phone] [nvarchar](35) NULL,
	[Fax] [nvarchar](25) NULL,
	[AddressLine1] [nvarchar](55) NULL,
	[AddressLine2] [nvarchar](55) NULL,
	[AddressLine3] [nvarchar](55) NULL,
	[AddressLine4] [nvarchar](55) NULL,
	[City] [varchar](100) NULL,
	[State] [varchar](2) NULL,
	[Zip] [varchar](10) NULL,
	[Building] [nvarchar](255) NULL,
	[Floor] [int] NULL,
	[Room] [nvarchar](255) NULL,
	[AddressString] [nvarchar](1000) NULL,
	[Latitude] [decimal](18, 14) NULL,
	[Longitude] [decimal](18, 14) NULL,
	[GeoScore] [tinyint] NULL,
	[AssistantName] [nvarchar](255) NULL,
	[AssistantPhone] [nvarchar](35) NULL,
	[AssistantEmail] [nvarchar](255) NULL,
	[AssistantAddress] [nvarchar](1000) NULL,
	[AssistantFax] [nvarchar](25) NULL,
	[Title] [nvarchar](255) NULL,
	[FacultyRankID] [int] NULL,
	[JobCode] [varchar](50) NULL,
	[InternalUsername] [nvarchar](50) NULL,
	[InternalID] [nvarchar](50) NULL,
	[InternalLDAPUserName] [nvarchar](50) NULL,
	[InternalFacultyID] [int] NULL,
	[InternalPersonType] [varchar](50) NULL,
	[InternalFullPartTime] [nvarchar](50) NULL,
	[InternalAdminTitle] [nvarchar](200) NULL,
	[InternalDeptType] [varchar](50) NULL,
	[Visible] [bit] NULL,
 CONSTRAINT [PK__person] PRIMARY KEY CLUSTERED 
(
	[PersonID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


