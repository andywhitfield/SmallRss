/****** Object:  Table [dbo].[UserAccount]    Script Date: 04/13/2013 11:57:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAccount](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LastLogin] [datetime] NULL,
 CONSTRAINT [PK_UserAccount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RssFeed]    Script Date: 04/13/2013 11:57:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RssFeed](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Uri] [nvarchar](500) NOT NULL,
	[LastUpdated] [datetime] NULL,
	[LastRefreshed] [datetime] NULL,
 CONSTRAINT [PK_RssFeed] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Article]    Script Date: 04/13/2013 11:57:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Article](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RssFeedId] [int] NOT NULL,
	[ArticleGuid] [nvarchar](max) NOT NULL,
	[Heading] [nvarchar](max) NULL,
	[Body] [nvarchar](max) NULL,
	[Url] [nvarchar](500) NULL,
	[Published] [datetime] NULL,
 CONSTRAINT [PK_Artile] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserAccountSetting]    Script Date: 04/13/2013 11:57:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAccountSetting](
	[UserAccountId] [int] NOT NULL,
	[SettingType] [nvarchar](50) NOT NULL,
	[SettingName] [nvarchar](50) NOT NULL,
	[SettingValue] [nvarchar](2000) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserFeed]    Script Date: 04/13/2013 11:57:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserFeed](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserAccountId] [int] NOT NULL,
	[RssFeedId] [int] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[GroupName] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_UserFeed] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserArticlesRead]    Script Date: 04/13/2013 11:57:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserArticlesRead](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserAccountId] [int] NOT NULL,
	[UserFeedId] [int] NOT NULL,
	[ArticleId] [int] NOT NULL,
 CONSTRAINT [PK_UserArticlesRead] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  ForeignKey [FK_Article_RssFeed]    Script Date: 04/13/2013 11:57:46 ******/
ALTER TABLE [dbo].[Article]  WITH CHECK ADD  CONSTRAINT [FK_Article_RssFeed] FOREIGN KEY([RssFeedId])
REFERENCES [dbo].[RssFeed] ([Id])
GO
ALTER TABLE [dbo].[Article] CHECK CONSTRAINT [FK_Article_RssFeed]
GO
/****** Object:  ForeignKey [FK_UserAccountSetting_UserAccount]    Script Date: 04/13/2013 11:57:46 ******/
ALTER TABLE [dbo].[UserAccountSetting]  WITH CHECK ADD  CONSTRAINT [FK_UserAccountSetting_UserAccount] FOREIGN KEY([UserAccountId])
REFERENCES [dbo].[UserAccount] ([Id])
GO
ALTER TABLE [dbo].[UserAccountSetting] CHECK CONSTRAINT [FK_UserAccountSetting_UserAccount]
GO
/****** Object:  ForeignKey [FK_UserArticlesRead_Article]    Script Date: 04/13/2013 11:57:46 ******/
ALTER TABLE [dbo].[UserArticlesRead]  WITH CHECK ADD  CONSTRAINT [FK_UserArticlesRead_Article] FOREIGN KEY([ArticleId])
REFERENCES [dbo].[Article] ([Id])
GO
ALTER TABLE [dbo].[UserArticlesRead] CHECK CONSTRAINT [FK_UserArticlesRead_Article]
GO
/****** Object:  ForeignKey [FK_UserArticlesRead_UserAccount]    Script Date: 04/13/2013 11:57:46 ******/
ALTER TABLE [dbo].[UserArticlesRead]  WITH CHECK ADD  CONSTRAINT [FK_UserArticlesRead_UserAccount] FOREIGN KEY([UserAccountId])
REFERENCES [dbo].[UserAccount] ([Id])
GO
ALTER TABLE [dbo].[UserArticlesRead] CHECK CONSTRAINT [FK_UserArticlesRead_UserAccount]
GO
/****** Object:  ForeignKey [FK_UserArticlesRead_UserFeed]    Script Date: 04/13/2013 11:57:46 ******/
ALTER TABLE [dbo].[UserArticlesRead]  WITH CHECK ADD  CONSTRAINT [FK_UserArticlesRead_UserFeed] FOREIGN KEY([UserFeedId])
REFERENCES [dbo].[UserFeed] ([Id])
GO
ALTER TABLE [dbo].[UserArticlesRead] CHECK CONSTRAINT [FK_UserArticlesRead_UserFeed]
GO
/****** Object:  ForeignKey [FK_UserFeed_RssFeed]    Script Date: 04/13/2013 11:57:46 ******/
ALTER TABLE [dbo].[UserFeed]  WITH CHECK ADD  CONSTRAINT [FK_UserFeed_RssFeed] FOREIGN KEY([RssFeedId])
REFERENCES [dbo].[RssFeed] ([Id])
GO
ALTER TABLE [dbo].[UserFeed] CHECK CONSTRAINT [FK_UserFeed_RssFeed]
GO
/****** Object:  ForeignKey [FK_UserFeed_UserAccount]    Script Date: 04/13/2013 11:57:46 ******/
ALTER TABLE [dbo].[UserFeed]  WITH CHECK ADD  CONSTRAINT [FK_UserFeed_UserAccount] FOREIGN KEY([UserAccountId])
REFERENCES [dbo].[UserAccount] ([Id])
GO
ALTER TABLE [dbo].[UserFeed] CHECK CONSTRAINT [FK_UserFeed_UserAccount]
GO
