/****** Object:  StoredProcedure [ORNG.Grant].[agLoadGrantAppdata]    Script Date: 06/05/2015 09:47:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [ORNG.Grant].[agLoadGrantAppdata]
AS
BEGIN
	SET NOCOUNT ON;
	
	declare @pid int,
	@personId int,
	@nodeid int,
	@currentPersonId int,
	@currentNodeId int,
	@title nvarchar(255),
	@fullprojectnum nvarchar(255),
	@FY int,
	@ApplicationId int,
	@GrantId nvarchar(255),
	@currentPID int,
	@grantCount int,
	@json nvarchar(4000),
	@baseURI nvarchar(255),
	
	@orngAppId int,
	@cnt int,
	
	@sFY nvarchar(255), 
	@sApplicationId nvarchar(255)
	
	set @currentPersonId = 0
	set @grantCount = 0
	
	--set @orngAppId =0
	select @orngAppId = appId from [ORNG.].[Apps] where name = 'Awarded Grants'

	-- set @baseURI	
	SELECT @baseURI = [Value] from [Framework.].[Parameter] where [parameterID]= 'baseURI';

	CREATE TABLE #n (
		NodeID BIGINT PRIMARY KEY
	)	
	
	-- add existing people with grants into table
	INSERT #n(NodeID) SELECT DISTINCT NodeID FROM [ORNG.].[AppData] WHERE AppID = @orngAppId;
	
	-- remove old grant appdata
	delete from [ORNG.].[AppData] where appId = @orngAppId;
	
	declare investigator cursor FAST_FORWARD for 
	select distinct G.ProjectTitle, G.FullProjectNum, G.FY, G.ApplicationId, PrincipalInvestigatorId, p.PersonId, n.nodeid
	  FROM [ORNG.Grant].agPrincipalInvestigator I
	  Join [Profile.Data].Person P on P.InternalUserName = I.EmployeeID
	  Join [ORNG.Grant].agGrantPrincipal GP on GP.PrincipalInvestigatorPK = I.PrincipalInvestigatorPK
	  Join [ORNG.Grant].[agGrant] G on G.GrantPK = GP.GrantPK
	  Join [RDF.Stage].internalnodemap N on N.internalid = p.personid 

	where IsVerified = 1 AND  n.class = 'http://xmlns.com/foaf/0.1/Person'
	order by p.personId
	
	open investigator
	fetch next from investigator into @title, @fullprojectnum, @FY, @ApplicationID, @pid, @personId, @nodeid
	
	WHILE @@fetch_status = 0 BEGIN
		if(@currentPersonId != @personId) 
			BEGIN
			if  @currentPersonId != 0 and @grantCount > 0 
				BEGIN
					print 'Insert grant, userId=' + cast(@currentPersonId as varchar) + ', appId=' + cast(@orngAppId as varchar) + ', keyName=nih_n' + ', val='+ cast(@grantCount as varchar)
				
					INSERT [ORNG.].[AppData] (nodeId, appId, keyName, value, createdDT, updatedDT)
						VALUES(@currentNodeId, @orngAppId, 'nih_n', @grantCount, GETDATE(), GETDATE())
				END
						
			set @currentPID = @pid
			set @currentPersonId = @personId
			set @currentNodeId = @nodeid
			SET @grantCount = 0
			END
		
		SELECT TOP 1 @GrantId = CAST(GrantPK AS NVARCHAR(255)) FROM [ORNG.Grant].[agGrant] WHERE ApplicationID = @ApplicationId
		
		SET @sApplicationId = CAST(@ApplicationId AS NVARCHAR(255));
		SET @sFY = CAST(@FY AS NVARCHAR(255));

		--{"id":"GrantId", "t":"ProjectTitle", "fpn":" FullProjectNum", "fy":" FY", "aid":"ApplicationId"}				
		EXEC xp_sprintf @json OUTPUT, '{"id":"%s", "t":"%s", "fpn":"%s", "fy":"%s", "aid":"%s"}', 
			@GrantId, @title, @fullprojectnum, @sFY, @sApplicationId
			
		SELECT @cnt = COUNT(*) FROM [ORNG.].[AppData] WHERE nodeId = @nodeId AND appId = @orngAppId AND keyName = 'nih_n'
		IF(@cnt = 0) BEGIN
			PRINT 'Insert grant, userId=' + CAST(@currentPersonId AS VARCHAR) + ', appId=' + CAST(@orngAppId AS VARCHAR) + ', keyName='+ 'nih_' + CAST(@grantCount AS VARCHAR) + ', json='+ @json
			
			INSERT [ORNG.].[AppData] (nodeId, appId, keyName, value, createdDT, updatedDT)
			VALUES(@nodeid, @orngAppId, 'nih_' + CAST(@grantCount AS VARCHAR), REPLACE(@json, '""', '"'), GETDATE(), GETDATE())
			
			SET @grantCount = @grantCount + 1
		END
		
		FETCH NEXT FROM investigator INTO @title, @fullprojectnum, @FY, @ApplicationID, @pid, @personId, @nodeid
	END

	IF  @grantCount > 0 BEGIN
		PRINT 'Insert grant, userId=' + CAST(@currentPersonId AS VARCHAR) + ', appId=' + CAST(@orngAppId AS VARCHAR) + ', keyName=nih_n' + ', val='+ CAST(@grantCount AS VARCHAR)
		
		INSERT [ORNG.].[AppData] (nodeId, appId, keyName, value, createdDT, updatedDT)
			VALUES(@currentNodeId, @orngAppId, 'nih_n', @grantCount, GETDATE(), GETDATE())
	END
	
	CLOSE investigator
	DEALLOCATE investigator

	-- now add the app for new people, 
	declare addAppToPerson cursor FAST_FORWARD for 
	select DISTINCT NodeID FROM [ORNG.].[AppData] WHERE AppID = @orngAppId AND
		NodeID NOT IN (SELECT NodeID FROM #n)
		
	open addAppToPerson
	fetch next from addAppToPerson into @nodeid
	
	WHILE @@fetch_status = 0 BEGIN
		-- trying it by Eric' advice           
		EXEC [ORNG.].[AddAppToPerson] @SubjectID = @nodeid, @AppID = @orngAppId
	END		
	CLOSE addAppToPerson
	DEALLOCATE addAppToPerson
	
	-- now remove the app from those that no longer have it 
	declare removeAppFromPerson cursor FAST_FORWARD for 
	select DISTINCT NodeID FROM #n WHERE NodeID NOT IN
		(SELECT NodeID FROM [ORNG.].[AppData] WHERE AppID = @orngAppId)
		
	open removeAppFromPerson
	fetch next from removeAppFromPerson into @nodeid
	
	WHILE @@fetch_status = 0 BEGIN
		-- trying it by Eric' advice           
		EXEC [ORNG.].[RemoveAppFromPerson] @SubjectID = @nodeid, @AppID = @orngAppId
	END		
	CLOSE removeAppFromPerson
	DEALLOCATE removeAppFromPerson
	
	DROP TABLE #n
END











GO


