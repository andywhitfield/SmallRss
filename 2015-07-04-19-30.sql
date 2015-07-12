SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[Article] ADD [Author] [nvarchar](500) NULL
GO

ALTER TABLE [dbo].[ArticleArchive] ADD [Author] [nvarchar](500) NULL
GO
