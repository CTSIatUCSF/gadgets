
/****** Object:  StoredProcedure [UCSF].[agLoadGrantAppdata]    Script Date: 03/28/2013 11:37:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE PROCEDURE [UCSF].[agLoadGrantAppdata]
AS
BEGIN
	SET NOCOUNT ON;
	
	declare @pid int,
	@personId int,
	@nodeid int,
	@currentPersonId int,
	@currentPersonURI nvarchar(255),
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
	select @orngAppId = appId from [ORNG].[Apps] where name = 'Awarded Grants'

	-- set @baseURI	
	SELECT @baseURI = [Value] from [Framework.].[Parameter] where [parameterID]= 'baseURI';

	-- remove old grant appdata
	delete from [ORNG].[AppData] where appId = @orngAppId;
	
	declare investigator cursor FAST_FORWARD for 
	select distinct G.ProjectTitle, G.FullProjectNum, G.FY, G.ApplicationId, PrincipalInvestigatorId, p.PersonId, n.nodeid
	  FROM [UCSF].agPrincipalInvestigator I
	  Join [Profile.Data].Person P on P.InternalUserName = I.EmployeeID
	  Join [UCSF].agGrantPrincipal GP on GP.PrincipalInvestigatorPK = I.PrincipalInvestigatorPK
	  Join [UCSF].[agGrant] G on G.GrantPK = GP.GrantPK
	  Join [RDF.Stage].internalnodemap N on N.internalid = p.personid 

	where IsVerified = 1 AND  n.class = 'http://xmlns.com/foaf/0.1/Person'
	order by p.personId
	
	open investigator
	fetch next from investigator into @title, @fullprojectnum, @FY, @ApplicationID, @pid, @personId, @nodeid
	
	while @@fetch_status = 0 begin
		if(@currentPersonId != @personId) 
			begin
			if  @currentPersonId != 0 and @grantCount > 0 
				begin
					print 'Insert grant, userId=' + cast(@currentPersonId as varchar) + ', appId=' + cast(@orngAppId as varchar) + ', keyName=nih_n' + ', val='+ cast(@grantCount as varchar)
				
					insert [ORNG].[AppData] (userId, appId, keyName, value, createdDT, updatedDT)
					values(@currentPersonURI, @orngAppId, 'nih_n', @grantCount, GetDate(), GetDate())
				end
						
			set @currentPID = @pid
			set @currentPersonId = @personId
			set @currentPersonURI = @baseURI + cast(@nodeid as varchar)
			set @grantCount = 0
		end
		
		select top 1 @GrantId = cast(GrantPK as nvarchar(255)) from [UCSF].[agGrant] where ApplicationID = @ApplicationId
		
		set @sApplicationId = cast(@ApplicationId as nvarchar(255));
		set @sFY = cast(@FY as nvarchar(255));

		--{"id":"GrantId", "t":"ProjectTitle", "fpn":" FullProjectNum", "fy":" FY", "aid":"ApplicationId"}				
		EXEC xp_sprintf @json OUTPUT, '{"id":"%s", "t":"%s", "fpn":"%s", "fy":"%s", "aid":"%s"}', 
			@GrantId, @title, @fullprojectnum, @sFY, @sApplicationId
			
		select @cnt = count(*) from [ORNG].[AppData] where appId = @orngAppId and userId = @baseURI + cast(@nodeid as varchar) and keyName = 'nih_n'
		if(@cnt = 0) begin
			print 'Insert grant, userId=' + cast(@currentPersonId as varchar) + ', appId=' + cast(@orngAppId as varchar) + ', keyName='+ 'nih_' + cast(@grantCount as varchar) + ', json='+ @json
			
			insert [ORNG].[AppData] (userId, appId, keyName, value, createdDT, updatedDT)
			values(@baseURI + cast(@nodeid as varchar), @orngAppId, 'nih_' + cast(@grantCount as varchar), replace(@json, '""', '"'), GetDate(), GetDate())
			
			set @grantCount = @grantCount + 1
		end
		
		fetch next from investigator into @title, @fullprojectnum, @FY, @ApplicationID, @pid, @personId, @nodeid
	end

	if  @grantCount > 0 begin
		print 'Insert grant, userId=' + cast(@currentPersonId as varchar) + ', appId=' + cast(@orngAppId as varchar) + ', keyName=nih_n' + ', val='+ cast(@grantCount as varchar)
		
		insert [ORNG].[AppData] (userId, appId, keyName, value, createdDT, updatedDT)
		values(@currentPersonURI, @orngAppId, 'nih_n', @grantCount, GetDate(), GetDate())
	end
	
	close investigator
	deallocate investigator
	
	-- now add all new people with grants to registry
	insert [ORNG].[AppRegistry] (appId, personId, createdDT) 
		select distinct @orngAppId, userId, GetDate() from [ORNG].[AppData]
		where appId = @orngAppId and 
		userId not in (select personId from [ORNG].[AppRegistry] where appId = @orngAppId);
	
END


GO


