
/****** Object:  StoredProcedure [dbo].[sp_test]    Script Date: 06/29/2011 22:07:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UCSF.].[ExportActivitiesToChatter]') AND type in (N'P', N'PC')) BEGIN
	DROP PROCEDURE [UCSF.].[ExportActivitiesToChatter]
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [UCSF].[ExportActivitiesToChatter] 
	@url nvarchar(256),
	@username nvarchar(50),
	@password nvarchar(50),
	@token nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @activityLogId int
	declare @createdDT datetime
	declare @externalMessage bit
	declare @employeeId nvarchar(50)
	declare @actUrl nvarchar(255)
	declare @actTitle nvarchar(255)
	declare @actBody nvarchar(255)
	declare @attempts int
	declare @errorCount int
	declare @errorMsg nvarchar(255)
	
	set @errorCount = 0
	
	DECLARE activityCursor CURSOR FAST_FORWARD FOR 
	SELECT TOP 100 activityLogId, createdDT, externalMessage, employeeId, url, title, body, chatterAttempts
	FROM [UCSF].[ChatterActivity] 
	WHERE chatterFlag is null
	ORDER BY activityLogId

	OPEN activityCursor

	FETCH NEXT FROM activityCursor 
	INTO @activityLogId, @createdDT, @externalMessage, @employeeId, @actUrl, @actTitle, @actBody, @attempts

	WHILE @@FETCH_STATUS = 0
	BEGIN
		BEGIN TRY
			set @attempts = ISNULL(@attempts, 0) + 1
			
			exec [UCSF].[CreateChatterActivity] @url, @username, @password, @token, @createdDT, @externalMessage, @employeeId, @actUrl, @actTitle, @actBody
			UPDATE [UCSF].[ChatterActivity] SET chatterFlag = 'S', chatterAttempts = @attempts, updatedDT = GETDATE() WHERE activityLogId = @activityLogId
		END TRY
		BEGIN CATCH
			set @errorCount = @errorCount + 1
			select @errorMsg = ERROR_MESSAGE()
			RAISERROR (@errorMsg, 10, 1)
			
			IF @attempts > 0 BEGIN
				UPDATE [UCSF].[ChatterActivity] SET chatterFlag = 'F', chatterAttempts = @attempts, updatedDT = GETDATE() WHERE activityLogId = @activityLogId
			END	
			ELSE BEGIN
				UPDATE [UCSF].[ChatterActivity] SET chatterAttempts = @attempts, updatedDT = GETDATE() WHERE activityLogId = @activityLogId
			END
						 
			IF @errorCount > 100 BEGIN
				RAISERROR ('Too many errors', 11,1)
				CLOSE activityCursor
				DEALLOCATE activityCursor
				RETURN
			END
		END CATCH
		
		FETCH NEXT FROM activityCursor 
		INTO @activityLogId, @createdDT, @externalMessage, @employeeId, @actUrl, @actTitle, @actBody, @attempts
	END
	
	CLOSE activityCursor
	DEALLOCATE activityCursor
END
GO