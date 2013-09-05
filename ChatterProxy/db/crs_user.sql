/****** Object:  Table [dbo].[user]    Script Date: 06/21/2011 14:57:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[user](
                [UserID] [int] IDENTITY(1,1) NOT NULL,
                [PersonID] [int] NULL,
                [CtscID] [int] NULL,
                [IsActive] [bit] NULL,
                [CanBeProxy] [bit] NULL,
                [FirstName] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [LastName] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [DisplayName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [OfficePhone] [varchar](35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [EmailAddr] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InstitutionFullname] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [DepartmentFullname] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [DivisionFullname] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [UserName] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Password] [varchar](128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [PasswordFormat] [int] NULL,
                [PasswordQuestion] [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [PasswordAnswer] [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [LastActivityDate] [datetime] NULL,
                [LastLoginDate] [datetime] NULL,
                [LastPasswordChangedDate] [datetime] NULL,
                [CreateDate] [datetime] NULL,
                [IsLockedOut] [bit] NULL,
                [LastLockOutDate] [datetime] NULL,
                [FailedPasswordAttemptCount] [int] NULL,
                [FailedPasswordAttemptWindowStart] [datetime] NULL,
                [FailedPasswordAnswerAttemptCount] [int] NULL,
                [FailedPasswordAnswerAttemptWindowstart] [datetime] NULL,
                [ApplicationName] [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [Comment] [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [IsApproved] [bit] NULL,
                [IsOnline] [bit] NULL,
                [InternalLdapUsername] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InternalFacultyID] [int] NULL,
                [InternalUserName] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
                [InternalID] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CONSTRAINT [PK__user] PRIMARY KEY CLUSTERED 
(
                [UserID] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
--ALTER TABLE [dbo].[user]  WITH CHECK ADD  CONSTRAINT [FK_user_person] FOREIGN KEY([PersonID])
--REFERENCES [dbo].[person] ([PersonID])
