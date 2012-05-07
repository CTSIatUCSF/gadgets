
IF NOT EXISTS(SELECT [name] FROM syscolumns WHERE [name] = 'IsVerified' AND id = OBJECT_ID('agGrant')) BEGIN
	ALTER TABLE agGrant ADD [IsVerified] [bit] NULL
END
GO
