
/****** Object:  StoredProcedure [dbo].[sp_test]    Script Date: 06/29/2011 22:07:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_ExportActivitiesToChattrer]') AND type in (N'P', N'PC')) BEGIN
	DROP PROCEDURE [dbo].[sp_ExportActivitiesToChattrer]
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_ExportActivitiesToChattrer] 
	@url nvarchar(256),
	@username nvarchar(50),
	@password nvarchar(50),
	@token nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @activityId int
	declare @userId int
	declare @createdDT datetime
	declare @activity xml
	declare @emploeeyId nvarchar(50)
	declare @attempts int
	declare @errorCount int
	declare @errorMsg nvarchar(max)
	
	set @errorCount = 0
	
	DECLARE activityCursor CURSOR FAST_FORWARD FOR 
	SELECT TOP 100 a.activityId, a.userId, a.createdDT, a.activity, a.chatterAttempts ,u.InternalUserName as emploeeyId
	FROM [shindig_activity] a INNER JOIN [dbo].[user] u ON a.[userId] = u.[UserID]
	WHERE chatterFlag is null
	ORDER BY userId

	OPEN activityCursor

	FETCH NEXT FROM activityCursor 
	INTO @activityId, @userId, @createdDT, @activity, @attempts, @emploeeyId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		BEGIN TRY
			set @attempts = ISNULL(@attempts, 0) + 1
			
			exec dbo.usp_CreateChatterActivity @url, @username, @password, @token, @emploeeyId, @activity
			UPDATE shindig_activity SET chatterFlag = 'S', chatterAttempts = @attempts, updatedDT = GETDATE() WHERE activityId = @activityId
		END TRY
		BEGIN CATCH
			set @errorCount = @errorCount + 1
			select @errorMsg = ERROR_MESSAGE()
			RAISERROR (@errorMsg, 10, 1)
			
			IF @attempts > 10 BEGIN
				UPDATE shindig_activity SET chatterFlag = 'F', chatterAttempts = @attempts, updatedDT = GETDATE() WHERE activityId = @activityId
			END	
			ELSE BEGIN
				UPDATE shindig_activity SET chatterAttempts = @attempts, updatedDT = GETDATE() WHERE activityId = @activityId
			END
						 
			IF @errorCount > 10 BEGIN
				RAISERROR ('Too many errors', 11,1)
				CLOSE activityCursor
				DEALLOCATE activityCursor
				RETURN
			END
		END CATCH
		
		FETCH NEXT FROM activityCursor 
		INTO @activityId, @userId, @createdDT, @activity, @attempts, @emploeeyId
	END
	
	CLOSE activityCursor
	DEALLOCATE activityCursor
END
GO