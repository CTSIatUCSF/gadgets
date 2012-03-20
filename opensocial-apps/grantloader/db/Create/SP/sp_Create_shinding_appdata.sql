/****** Object:  StoredProcedure [dbo].[sp_Create_shinding_appdata]    Script Date: 03/16/2012 19:14:00 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Create_shinding_appdata]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_Create_shinding_appdata]
GO


Create PROCEDURE [dbo].[sp_Create_shinding_appdata]
AS
BEGIN
	SET NOCOUNT ON;
	
	declare @pid int,
	@personId int,
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
	
	set @currentPID = 0;
	
	select @shindigAppId = appId from shindig_apps where name = 'Awarded Grants'
	
	declare investigator cursor FAST_FORWARD for 
	select distinct G.ProjectTitle, G.FullProjectNum, G.FY, G.ApplicationId, PrincipalInvestigator_Id, p.PersonId
	  FROM PrincipalInvestigator I
	  Join Person P on P.InternalUserName = I.EmployeeID
	  Join GrantPrincipal GP on GP.PrincipalInvestigatorId = I.PrincipalInvestigatorId
	  Join [Grant] G on G.GrantId = GP.GrantId
	order by I.PrincipalInvestigator_Id
	
	open investigator
	fetch next from investigator into @title, @fullprojectnum, @FY, @ApplicationID, @pid, @personId
	
	while @@fetch_status = 0 begin
		if(@currentPID != @pid) begin
			if  @currentPID != 0 begin
				insert shindig_appdata (userId, appId, keyName, value, createdDT, updatedDT)
				values(@personID, @shindigAppId, 'nih_n', @grantCount, GetDate(), GetDate())
			end
						
			set @currentPID = @pid
			set @grantCount = 0
		end
		
		select top 1 @GrantId = cast(GrantId as nvarchar(255)) from [Grant] where ApplicationID = @ApplicationId
		
		set @sApplicationId = cast(@ApplicationId as nvarchar(255));
		set @sFY = cast(@FY as nvarchar(255));

		--{"id":"GrantId", "t":"ProjectTitle", "fpn":" FullProjectNum", "fy":" FY", "aid":"ApplicationId"}				
		EXEC xp_sprintf @json OUTPUT, '{"id":"%s", "t":"%s", "fpn":"%s", "fy":"%s", "aid":"%s"}', 
			@GrantId, @title, @fullprojectnum, @sFY, @sApplicationId
			
		--print @json
		select @cnt = count(*) from shindig_appdata where appId = @shindigAppId and userId = @personID and keyName = 'nih_n'
		if(@cnt = 0) begin
			insert shindig_appdata (userId, appId, keyName, value, createdDT, updatedDT)
			values(@personID, @shindigAppId, 'nih_' + cast(@grantCount as varchar), @json, GetDate(), GetDate())
			
			set @grantCount = @grantCount + 1
		end
		
		fetch next from investigator into @title, @fullprojectnum, @FY, @ApplicationID, @pid, @personId
	end

	if  @grantCount != 0 begin
		insert shindig_appdata (userId, appId, keyName, value, createdDT, updatedDT)
		values(@personID, @shindigAppId, 'nih_n', @grantCount, GetDate(), GetDate())
	end
	
	close investigator
	deallocate investigator
END