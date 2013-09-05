
IF EXISTS(SELECT [name] FROM syscolumns WHERE [name] = 'chatterFlag' AND id = OBJECT_ID('shindig_activity')) BEGIN
	EXEC sp_rename 'shindig_activity.chatterFlag', 'chatterFlagOld', 'COLUMN'
END 
GO

IF NOT EXISTS(SELECT [name] FROM syscolumns WHERE [name] = 'chatterFlag' AND id = OBJECT_ID('shindig_activity')) BEGIN
	ALTER TABLE shindig_activity ADD chatterFlag char NULL
END 
GO

UPDATE shindig_activity SET chatterFlag = 'S' where chatterFlagOld = 1 and chatterFlag is null

IF NOT EXISTS(SELECT [name] FROM syscolumns WHERE [name] = 'chatterAttempts' AND id = OBJECT_ID('shindig_activity')) BEGIN
	ALTER TABLE shindig_activity ADD chatterAttempts int NULL
END 
GO

IF NOT EXISTS(SELECT [name] FROM syscolumns WHERE [name] = 'updatedDT' AND id = OBJECT_ID('shindig_activity')) BEGIN
	ALTER TABLE shindig_activity ADD updatedDT datetime NULL
END 
GO

