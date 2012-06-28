/****** Object:  StoredProcedure [dbo].[agLoadGrantAppdata]    Script Date: 03/16/2012 19:14:00 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[agLoadGrantAppdata]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[agLoadGrantAppdata]
GO


Create PROCEDURE [dbo].[agLoadGrantAppdata]
AS
BEGIN
	SET NOCOUNT ON;
	
	declare @pid int,
	@personId int,
	@currentPersonId int,
	@title nvarchar(255),
	@fullprojectnum nvarchar(255),
	@FY int,
	@ApplicationId int,
	@GrantId nvarchar(255),
	@currentPID int,
	@grantCount int,
	@json nvarchar(4000),
	
	@shindigAppId int,
	@cnt int,
	
	@sFY nvarchar(255), 
	@sApplicationId nvarchar(255)
	
	set @currentPID = 0
	set @currentPersonId = 0
	set @grantCount = 0
	
	--set @shindigAppId =0
	select @shindigAppId = appId from shindig_apps where name = 'Awarded Grants'
	
	-- remove old grant appdata
	delete from shindig_appdata where appId = @shindigAppId and keyName != 'VISIBLE';
	
	declare investigator cursor FAST_FORWARD for 
	select distinct G.ProjectTitle, G.FullProjectNum, G.FY, G.ApplicationId, PrincipalInvestigatorId, p.PersonId
	  FROM agPrincipalInvestigator I
	  Join Person P on P.InternalUserName = I.EmployeeID
	  Join agGrantPrincipal GP on GP.PrincipalInvestigatorPK = I.PrincipalInvestigatorPK
	  Join [agGrant] G on G.GrantPK = GP.GrantPK
	where IsVerified = 1
	order by I.PrincipalInvestigatorId
	
	open investigator
	fetch next from investigator into @title, @fullprojectnum, @FY, @ApplicationID, @pid, @personId
	
	while @@fetch_status = 0 begin
		if(@currentPID != @pid) begin
			if  @currentPID != 0 and @grantCount > 0 begin
				print 'Insert grant, userId=' + cast(@currentPersonId as varchar) + ', appId=' + cast(@shindigAppId as varchar) + ', keyName=nih_n' + ', val='+ cast(@grantCount as varchar)
				
				insert shindig_appdata (userId, appId, keyName, value, createdDT, updatedDT)
				values(@currentPersonId, @shindigAppId, 'nih_n', @grantCount, GetDate(), GetDate())
			end
						
			set @currentPID = @pid
			set @currentPersonId = @personId
			set @grantCount = 0
		end
		
		select top 1 @GrantId = cast(GrantPK as nvarchar(255)) from [agGrant] where ApplicationID = @ApplicationId
		
		set @sApplicationId = cast(@ApplicationId as nvarchar(255));
		set @sFY = cast(@FY as nvarchar(255));

		--{"id":"GrantId", "t":"ProjectTitle", "fpn":" FullProjectNum", "fy":" FY", "aid":"ApplicationId"}				
		EXEC xp_sprintf @json OUTPUT, '{"id":"%s", "t":"%s", "fpn":"%s", "fy":"%s", "aid":"%s"}', 
			@GrantId, @title, @fullprojectnum, @sFY, @sApplicationId
			
		select @cnt = count(*) from shindig_appdata where appId = @shindigAppId and userId = @personID and keyName = 'nih_n'
		if(@cnt = 0) begin
			print 'Insert grant, userId=' + cast(@currentPersonId as varchar) + ', appId=' + cast(@shindigAppId as varchar) + ', keyName='+ 'nih_' + cast(@grantCount as varchar) + ', json='+ @json
			
			insert shindig_appdata (userId, appId, keyName, value, createdDT, updatedDT)
			values(@personID, @shindigAppId, 'nih_' + cast(@grantCount as varchar), @json, GetDate(), GetDate())
			
			set @grantCount = @grantCount + 1
		end
		
		fetch next from investigator into @title, @fullprojectnum, @FY, @ApplicationID, @pid, @personId
	end

	if  @grantCount > 0 begin
		print 'Insert grant, userId=' + cast(@currentPersonId as varchar) + ', appId=' + cast(@shindigAppId as varchar) + ', keyName=nih_n' + ', val='+ cast(@grantCount as varchar)
		
		insert shindig_appdata (userId, appId, keyName, value, createdDT, updatedDT)
		values(@currentPersonId, @shindigAppId, 'nih_n', @grantCount, GetDate(), GetDate())
	end
	
	close investigator
	deallocate investigator
	
	-- now add all new people with grants as VISIBLE
	insert shindig_appdata (userId, appId, keyName, value, createdDT, updatedDT)
		select distinct userId,  @shindigAppId, 'VISIBLE', 'Y', GetDate(), GetDate() from 
		shindig_appdata where appId = @shindigAppId and keyName != 'VISIBLE';
		
	insert shindig_app_registry (appId, personId, createdDT) 
		select @shindigAppId, userId, GetDate() from shindig_appdata
		where appId = @shindigAppId and keyName = 'VISIBLE' and [value] = 'Y' and 
		userId not in (select personId from shindig_app_registry where appId = @shindigAppId);
	
END