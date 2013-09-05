IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UCSF].[agGrantPrincipal]') AND type in (N'U'))
BEGIN
CREATE TABLE [UCSF].[agGrantPrincipal](
	[GrantPrincipalPK] [uniqueidentifier] NOT NULL,
	[GrantPK] [uniqueidentifier] NOT NULL,
	[PrincipalInvestigatorPK] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_GrantPrincipalPK] PRIMARY KEY CLUSTERED 
(
	[GrantPrincipalPK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO


/****** Object:  Index [IX_GrantPrincipal]    Script Date: 02/15/2012 18:26:33 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[UCSF].[agGrantPrincipal]') AND name = N'IX_GrantPrincipal')
CREATE NONCLUSTERED INDEX [IX_GrantPrincipal] ON [UCSF].[agGrantPrincipal] 
(
	[GrantPK] ASC,
	[PrincipalInvestigatorPK] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[UCSF].[FK_GrantPrincipal_Grant]') AND parent_object_id = OBJECT_ID(N'[UCSF].[agGrantPrincipal]'))
ALTER TABLE [UCSF].[agGrantPrincipal]  WITH CHECK ADD  CONSTRAINT [FK_GrantPrincipal_Grant] FOREIGN KEY([GrantPK])
REFERENCES [UCSF].[agGrant] ([GrantPK])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[UCSF].[FK_GrantPrincipal_Grant]') AND parent_object_id = OBJECT_ID(N'[UCSF].[agGrantPrincipal]'))
ALTER TABLE [UCSF].[agGrantPrincipal] CHECK CONSTRAINT [FK_GrantPrincipal_Grant]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[UCSF].[FK_GrantPrincipal_PrincipalInvestigator]') AND parent_object_id = OBJECT_ID(N'[UCSF].[agGrantPrincipal]'))
ALTER TABLE [UCSF].[agGrantPrincipal]  WITH CHECK ADD  CONSTRAINT [FK_GrantPrincipal_PrincipalInvestigator] FOREIGN KEY([PrincipalInvestigatorPK])
REFERENCES [UCSF].[agPrincipalInvestigator] ([PrincipalInvestigatorPK])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[UCSF].[FK_GrantPrincipal_PrincipalInvestigator]') AND parent_object_id = OBJECT_ID(N'[UCSF].[agGrantPrincipal]'))
ALTER TABLE [UCSF].[agGrantPrincipal] CHECK CONSTRAINT [FK_GrantPrincipal_PrincipalInvestigator]
GO