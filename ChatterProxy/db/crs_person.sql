/****** Object:  Table [dbo].[person]    Script Date: 06/21/2011 14:57:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[person](
                [PersonID] [int] IDENTITY(1,1) NOT NULL,
                [UserID] [int] NULL,
                [CtscID] [int] NULL,
                [FirstName] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [LastName] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [MiddleName] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [DisplayName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [DegreeList] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Suffix] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [IsActive] [bit] NULL,
                [EmailAddr] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Phone] [nvarchar](35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Fax] [nvarchar](25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [AddressLine1] [nvarchar](55) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [AddressLine2] [nvarchar](55) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [AddressLine3] [nvarchar](55) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [AddressLine4] [nvarchar](55) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [City] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [State] [varchar](2) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Zip] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Building] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Floor] [int] NULL,
                [Room] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [AddressString] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Latitude] [decimal](18, 14) NULL,
                [Longitude] [decimal](18, 14) NULL,
                [GeoScore] [tinyint] NULL,
                [AssistantName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [AssistantPhone] [nvarchar](35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [AssistantEmail] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [AssistantAddress] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [AssistantFax] [nvarchar](25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Title] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [FacultyRankID] [int] NULL,
                [JobCode] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InternalUsername] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InternalID] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InternalLDAPUserName] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InternalFacultyID] [int] NULL,
                [InternalPersonType] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InternalFullPartTime] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InternalAdminTitle] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InternalDeptType] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Visible] [bit] NULL,
CONSTRAINT [PK__person] PRIMARY KEY CLUSTERED 
(
                [PersonID] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO


--ALTER TABLE [dbo].[person]  WITH CHECK ADD  CONSTRAINT [FK_person_user] FOREIGN KEY([UserID])
--REFERENCES [dbo].[user] ([UserID])

