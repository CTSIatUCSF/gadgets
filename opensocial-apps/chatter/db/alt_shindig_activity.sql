
IF NOT EXISTS(SELECT [name] FROM syscolumns WHERE [name] = 'chatterFlag' AND id = OBJECT_ID('shindig_activity')) BEGIN
	ALTER TABLE shindig_activity ADD chatterFlag bit NULL DEFAULT 0
END
GO

UPDATE shindig_activity SET chatterFlag = 0 where chatterFlag is null
ALTER TABLE shindig_activity ALTER COLUMN chatterFlag bit NOT NULL
