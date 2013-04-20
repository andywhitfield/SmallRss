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
/****** Object:  Table [dbo].[Log]    Script Date: 04/13/2013 11:57:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log] (
    [Id] [int] IDENTITY (1, 1) NOT NULL,
	[Application] [nvarchar] (255) NOT NULL,
    [Date] [datetime] NOT NULL,
    [Thread] [nvarchar] (255) NOT NULL,
    [Level] [nvarchar] (50) NOT NULL,
    [Logger] [nvarchar] (255) NOT NULL,
    [Message] [nvarchar] (4000) NOT NULL,
    [Exception] [nvarchar] (2000) NULL
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

/*
 
   ELMAH - Error Logging Modules and Handlers for ASP.NET
   Copyright (c) 2004-9 Atif Aziz. All rights reserved.
 
    Author(s):
 
        Atif Aziz, http://www.raboof.com
        Phil Haacked, http://haacked.com
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
      http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
*/

-- ELMAH DDL script for Microsoft SQL Server 2000 or later.

-- $Id: SQLServer.sql 776 2011-01-12 21:09:24Z azizatif $

/* ------------------------------------------------------------------------
        TABLES
   ------------------------------------------------------------------------ */

CREATE TABLE [dbo].[ELMAH_Error]
(
    [ErrorId]     UNIQUEIDENTIFIER NOT NULL,
    [Application] NVARCHAR(60)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Host]        NVARCHAR(50)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Type]        NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Source]      NVARCHAR(60)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [Message]     NVARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [User]        NVARCHAR(50)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [StatusCode]  INT NOT NULL,
    [TimeUtc]     DATETIME NOT NULL,
    [Sequence]    INT IDENTITY (1, 1) NOT NULL,
    [AllXml]      NVARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[ELMAH_Error] WITH NOCHECK ADD
    CONSTRAINT [PK_ELMAH_Error] PRIMARY KEY NONCLUSTERED ([ErrorId]) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ELMAH_Error] ADD
    CONSTRAINT [DF_ELMAH_Error_ErrorId] DEFAULT (NEWID()) FOR [ErrorId]
GO

CREATE NONCLUSTERED INDEX [IX_ELMAH_Error_App_Time_Seq] ON [dbo].[ELMAH_Error]
(
    [Application]   ASC,
    [TimeUtc]       DESC,
    [Sequence]      DESC
)
ON [PRIMARY]
GO

/* ------------------------------------------------------------------------
        STORED PROCEDURES                                                      
   ------------------------------------------------------------------------ */

SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE PROCEDURE [dbo].[ELMAH_GetErrorXml]
(
    @Application NVARCHAR(60),
    @ErrorId UNIQUEIDENTIFIER
)
AS

    SET NOCOUNT ON

    SELECT
        [AllXml]
    FROM
        [ELMAH_Error]
    WHERE
        [ErrorId] = @ErrorId
    AND
        [Application] = @Application

GO
SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE PROCEDURE [dbo].[ELMAH_GetErrorsXml]
(
    @Application NVARCHAR(60),
    @PageIndex INT = 0,
    @PageSize INT = 15,
    @TotalCount INT OUTPUT
)
AS

    SET NOCOUNT ON

    DECLARE @FirstTimeUTC DATETIME
    DECLARE @FirstSequence INT
    DECLARE @StartRow INT
    DECLARE @StartRowIndex INT

    SELECT
        @TotalCount = COUNT(1)
    FROM
        [ELMAH_Error]
    WHERE
        [Application] = @Application

    -- Get the ID of the first error for the requested page

    SET @StartRowIndex = @PageIndex * @PageSize + 1

    IF @StartRowIndex <= @TotalCount
    BEGIN

        SET ROWCOUNT @StartRowIndex

        SELECT  
            @FirstTimeUTC = [TimeUtc],
            @FirstSequence = [Sequence]
        FROM
            [ELMAH_Error]
        WHERE  
            [Application] = @Application
        ORDER BY
            [TimeUtc] DESC,
            [Sequence] DESC

    END
    ELSE
    BEGIN

        SET @PageSize = 0

    END

    -- Now set the row count to the requested page size and get
    -- all records below it for the pertaining application.

    SET ROWCOUNT @PageSize

    SELECT
        errorId     = [ErrorId],
        application = [Application],
        host        = [Host],
        type        = [Type],
        source      = [Source],
        message     = [Message],
        [user]      = [User],
        statusCode  = [StatusCode],
        time        = CONVERT(VARCHAR(50), [TimeUtc], 126) + 'Z'
    FROM
        [ELMAH_Error] error
    WHERE
        [Application] = @Application
    AND
        [TimeUtc] <= @FirstTimeUTC
    AND
        [Sequence] <= @FirstSequence
    ORDER BY
        [TimeUtc] DESC,
        [Sequence] DESC
    FOR
        XML AUTO

GO
SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE PROCEDURE [dbo].[ELMAH_LogError]
(
    @ErrorId UNIQUEIDENTIFIER,
    @Application NVARCHAR(60),
    @Host NVARCHAR(30),
    @Type NVARCHAR(100),
    @Source NVARCHAR(60),
    @Message NVARCHAR(500),
    @User NVARCHAR(50),
    @AllXml NVARCHAR(MAX),
    @StatusCode INT,
    @TimeUtc DATETIME
)
AS

    SET NOCOUNT ON

    INSERT
    INTO
        [ELMAH_Error]
        (
            [ErrorId],
            [Application],
            [Host],
            [Type],
            [Source],
            [Message],
            [User],
            [AllXml],
            [StatusCode],
            [TimeUtc]
        )
    VALUES
        (
            @ErrorId,
            @Application,
            @Host,
            @Type,
            @Source,
            @Message,
            @User,
            @AllXml,
            @StatusCode,
            @TimeUtc
        )

GO
SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO