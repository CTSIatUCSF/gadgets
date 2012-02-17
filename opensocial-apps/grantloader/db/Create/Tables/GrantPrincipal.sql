IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GrantPrincipal]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[GrantPrincipal](
	[GrantPrincipalId] [uniqueidentifier] NOT NULL,
	[GrantId] [uniqueidentifier] NOT NULL,
	[PrincipalInvestigatorId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_GrantPrincipalId] PRIMARY KEY CLUSTERED 
(
	[GrantPrincipalId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


/****** Object:  Index [IX_GrantPrincipal]    Script Date: 02/15/2012 18:26:33 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GrantPrincipal]') AND name = N'IX_GrantPrincipal')
CREATE NONCLUSTERED INDEX [IX_GrantPrincipal] ON [dbo].[GrantPrincipal] 
(
	[GrantId] ASC,
	[PrincipalInvestigatorId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GrantPrincipal_Grant]') AND parent_object_id = OBJECT_ID(N'[dbo].[GrantPrincipal]'))
ALTER TABLE [dbo].[GrantPrincipal]  WITH CHECK ADD  CONSTRAINT [FK_GrantPrincipal_Grant] FOREIGN KEY([GrantId])
REFERENCES [dbo].[Grant] ([GrantId])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GrantPrincipal_Grant]') AND parent_object_id = OBJECT_ID(N'[dbo].[GrantPrincipal]'))
ALTER TABLE [dbo].[GrantPrincipal] CHECK CONSTRAINT [FK_GrantPrincipal_Grant]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GrantPrincipal_PrincipalInvestigator]') AND parent_object_id = OBJECT_ID(N'[dbo].[GrantPrincipal]'))
ALTER TABLE [dbo].[GrantPrincipal]  WITH CHECK ADD  CONSTRAINT [FK_GrantPrincipal_PrincipalInvestigator] FOREIGN KEY([PrincipalInvestigatorId])
REFERENCES [dbo].[PrincipalInvestigator] ([PrincipalInvestigatorId])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GrantPrincipal_PrincipalInvestigator]') AND parent_object_id = OBJECT_ID(N'[dbo].[GrantPrincipal]'))
ALTER TABLE [dbo].[GrantPrincipal] CHECK CONSTRAINT [FK_GrantPrincipal_PrincipalInvestigator]
GO


