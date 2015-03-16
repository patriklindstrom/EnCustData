USE [db1]
GO

/****** Object:  Table [dbo].[EncryptVal_TB]    Script Date: 03/16/2015 15:30:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EncryptVal_TB](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DbName] [nvarchar](50) NOT NULL,
	[TblName] [nvarchar](50) NOT NULL,
	[FieldName] [nvarchar](50) NOT NULL,
	[ValueKey] [uniqueidentifier] NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL,
	[EncryptedValue] [nvarchar](512) NOT NULL,
 CONSTRAINT [PK_EncryptVal_TB] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[EncryptVal_TB]  WITH CHECK ADD  CONSTRAINT [UniqueValueKey] FOREIGN KEY([Id])
REFERENCES [dbo].[EncryptVal_TB] ([Id])
GO

ALTER TABLE [dbo].[EncryptVal_TB] CHECK CONSTRAINT [UniqueValueKey]
GO

ALTER TABLE [dbo].[EncryptVal_TB] ADD  CONSTRAINT [DF_EncryptVal_TB_InsertDate]  DEFAULT (getdate()) FOR [InsertDate]
GO


