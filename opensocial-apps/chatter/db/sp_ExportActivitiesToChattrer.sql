
/****** Object:  StoredProcedure [dbo].[sp_test]    Script Date: 06/29/2011 22:07:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
	declare @employeeId nvarchar(50)
	
	DECLARE activityCursor CURSOR FAST_FORWARD FOR 
	SELECT TOP 100 a.activityId, a.userId, a.createdDT, a.activity, u.InternalUserName as employeeId
	FROM [shindig_activity] a INNER JOIN [dbo].[user] u ON a.[userId] = u.[UserID]
	WHERE chatterFlag = 0
	ORDER BY userId

	OPEN activityCursor

	FETCH NEXT FROM activityCursor 
	INTO @activityId, @userId, @createdDT, @activity, @employeeId

	WHILE @@FETCH_STATUS = 0
	BEGIN

		exec dbo.usp_CreateChatterActivity @url, @username, @password, @token, @employeeId, @activity
		UPDATE shindig_activity SET chatterFlag = 1 WHERE activityId = @activityId
		
		FETCH NEXT FROM activityCursor 
		INTO @activityId, @userId, @createdDT, @activity, @employeeId
	END
	
	CLOSE activityCursor
	DEALLOCATE activityCursor
END
