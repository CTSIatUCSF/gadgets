DECLARE @nodeid bigint
DECLARE @applicationId int

select @applicationId = appId from [ORNG.].[Apps] where name = 'Global Health'

DECLARE person_cursor CURSOR FOR select nodeid from [UCSF.].vwPerson

OPEN person_cursor
	
FETCH NEXT FROM person_cursor INTO @nodeid

WHILE @@FETCH_STATUS = 0 BEGIN
	exec [ORNG.].RemoveAppFromPerson @SubjectID = @nodeId, @appId = @applicationId
	FETCH NEXT FROM person_cursor INTO @nodeid
END

CLOSE person_cursor
DEALLOCATE person_cursor

