/****** Object:  Table [dbo].[shindig_activity]    Script Date: 06/17/2011 15:34:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[shindig_activity](
 [activityId] [int] IDENTITY(1,1) NOT NULL,
 [userId] [int] NULL,
 [appId] [int] NULL,
 [createdDT] [datetime] NULL CONSTRAINT [DF_shindig_activity_createdDT]  DEFAULT (getdate()),
 [activity] [xml] NULL,
 CONSTRAINT [PK__activity] PRIMARY KEY CLUSTERED 
(
 [activityId] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]