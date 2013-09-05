/****** Object:  Table [UCSF].[ChatterActivity]    Script Date: 11/09/2012 11:10:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [UCSF].[ChatterActivity](
	[activityLogId] [int] NOT NULL,
	[externalMessage] [bit] NOT NULL,
	[employeeId] [nvarchar](50) NULL,
	[url] [nvarchar](255) NULL,
	[title] [nvarchar](255) NULL,
	[body] [nvarchar](255) NULL,
	[chatterFlag] [char](1) NULL,
	[chatterAttempts] [int] NULL,
	[createdDT] [datetime] NOT NULL,
	[updatedDT] [datetime] NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [UCSF].[ChatterActivity]  WITH CHECK ADD  CONSTRAINT [FK_ChatterActivity_ActivityLog] FOREIGN KEY([activityLogId])
REFERENCES [UCSF].[ActivityLog] ([activityLogId])
GO

ALTER TABLE [UCSF].[ChatterActivity] CHECK CONSTRAINT [FK_ChatterActivity_ActivityLog]
GO

ALTER TABLE [UCSF].[ChatterActivity] ADD  CONSTRAINT [DF_chatterActivity_createdDT]  DEFAULT (getdate()) FOR [createdDT]
GO

CREATE PROCEDURE [UCSF].[ActivityLogToChatterActivity] @activityLogId int
AS   
/* Get the range of level for this job type from the jobs table. */
DECLARE 
   @privacyCode int,
   @employeeId nvarchar(50),
   @methodName nvarchar(255),
   @param1 nvarchar(255),
   @param2 nvarchar(255),
   @externalMessage bit,
   @url nvarchar(255),
   @title nvarchar(255),
   @body nvarchar(255),
   @journalTitle varchar(1000)
SELECT @privacyCode = i.privacyCode, @employeeId = ISNULL(p.InternalUserName, ip.internalusername),
	@methodName = i.methodName, @param1 = i.param1, @param2 = i.param2, @externalMessage = 0
FROM [UCSF].[ActivityLog] i LEFT OUTER JOIN [Profile.Data].[Person] p ON i.personId = p.personID 
LEFT OUTER JOIN [Profile.Import].[Person] ip on i.personId = UCSF.fnGeneratePersonID(ip.internalusername) WHERE i.activityLogId = @activityLogId
-- if we have a PMID, go ahead and grab that info
IF (@param1 = 'PMID')
   SELECT @url = 'http://www.ncbi.nlm.nih.gov/pubmed/' + @param2, @journalTitle = JournalTitle, @externalMessage = 1 FROM
		[Profile.Data].[Publication.PubMed.General] 
		WHERE PMID = cast(@param2 as int)
-- USER activities		
IF (@privacyCode = -1) 
BEGIN   
	IF (@methodName = 'Profiles.Edit.Utilities.DataIO.AddPublication')
		SELECT @title = 'added a PubMed publication', @body = 'added a publication from: ' + @journalTitle
	ELSE IF (@methodName = 'Profiles.Edit.Utilities.DataIO.AddCustomPublication')
		SELECT @title = 'added a custom publication', @body = 'added "'  + @param1 + '" to their ' + cp._propertyLabel + ' section : ' + @param2
			FROM [UCSF].[ActivityLog] al JOIN
			[Ontology.].[ClassProperty] cp ON cp.Property = al.property 		
			WHERE al.activityLogId = @activityLogId AND al.property IS NOT NULL
	ELSE IF (@methodName = 'Profiles.Edit.Utilities.DataIO.UpdateSecuritySetting')
		SELECT @title = 'made a section visible', @body = 'made "'  + cp._propertyLabel + '" visible'
			FROM [UCSF].[ActivityLog] al JOIN
			[Ontology.].[ClassProperty] cp ON cp.Property = al.property 		
			WHERE al.activityLogId = @activityLogId AND al.property IS NOT NULL
	ELSE IF (@methodName like 'Profiles.Edit.Utilities.DataIO.Add%')
		SELECT @title = 'added an item', @body = 'added '  + isnull('"' + @param1 + '"', 'an item') + ' to their ' + cp._propertyLabel + ' section'
			FROM [UCSF].[ActivityLog] al JOIN
			[Ontology.].[ClassProperty] cp ON cp.Property = al.property 		
			WHERE al.activityLogId = @activityLogId AND al.property IS NOT NULL
	ELSE IF (@methodName like 'Profiles.Edit.Utilities.DataIO.Update%')
		SELECT @title = 'updated an item', @body = 'updated "' + cp._propertyLabel + '"'
			FROM [UCSF].[ActivityLog] al JOIN
			[Ontology.].[ClassProperty] cp ON cp.Property = al.property 		
			WHERE al.activityLogId = @activityLogId AND al.property IS NOT NULL
END
ELSE IF (@methodName = 'ProfilesGetNewHRAndPubs.Disambiguation') 
	SELECT @title = 'has a new PubMed publication', @body = 'has a new publication listed from: ' + @journalTitle
ELSE IF (@methodName = 'ProfilesGetNewHRAndPubs.AddedToProfiles') 
	SELECT @title = 'added to Profiles', @body = 'now has a Profile page!'

-- if we have @title, then insert
IF (@title is not NULL)
	-- for now just set the body to be the same as the title !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	INSERT [UCSF].[ChatterActivity] (activityLogId, externalMessage, employeeId, url, title, body)
		VALUES (@activityLogId, @externalMessage, @employeeId, @url, @title, @body)

-- uncomment to help debut
--select @activityLogId, @methodName, @title, @privacyCode, @externalMessage, @employeeId, @url, @param1, @param2;
GO

CREATE TRIGGER [UCSF].[addChatterActivity]
ON [UCSF].[ActivityLog]
AFTER INSERT
AS
DECLARE 
   @activityLogId int
SELECT @activityLogId = i.activityLogId FROM inserted i 
EXEC [UCSF].[ActivityLogToChatterActivity] @activityLogId
GO