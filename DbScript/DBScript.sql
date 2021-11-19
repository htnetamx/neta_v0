CREATE TABLE [dbo].[Neta_Promotion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[PictureId] [int] NOT NULL,
	[StartDateUTC] [datetime] NOT NULL,
	[EndDateUTC] [datetime] NOT NULL,
	[Deleted] [bit] NOT NULL,
	[Published] [bit] NOT NULL,
 CONSTRAINT [PK_Neta_Promotion] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[Neta_Promotion_ProductMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Neta_PromotionId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[DisplayOrder] [int] NOT NULL,
 CONSTRAINT [PK_Neta_Promotion_ProductMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Neta_Promotion_ProductMapping] ADD  CONSTRAINT [DF_Neta_Promotion_ProductMapping_DisplayOrder]  DEFAULT ((0)) FOR [DisplayOrder]
GO


IF (NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'IsPromotionProduct'))
BEGIN
	ALTER TABLE Product ADD IsPromotionProduct bit NOT NULL DEFAULT 0;
END