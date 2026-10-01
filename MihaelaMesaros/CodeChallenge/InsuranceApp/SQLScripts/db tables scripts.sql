USE [Insurances]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[FeeConfigs] DROP CONSTRAINT IF EXISTS [CK_FeeConfigs_Percentage]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[FeeConfigs] DROP CONSTRAINT IF EXISTS [CK_FeeConfigs_EffectivePeriod]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Currencies]') AND type in (N'U'))
ALTER TABLE [dbo].[Currencies] DROP CONSTRAINT IF EXISTS [CK_Currencies_ExchangeRate]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Counties]') AND type in (N'U'))
ALTER TABLE [dbo].[Counties] DROP CONSTRAINT IF EXISTS [FK_Counties_Countries]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cities]') AND type in (N'U'))
ALTER TABLE [dbo].[Cities] DROP CONSTRAINT IF EXISTS [FK_Cities_Counties]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Buildings]') AND type in (N'U'))
ALTER TABLE [dbo].[Buildings] DROP CONSTRAINT IF EXISTS [FK_Buildings_Clients]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Buildings]') AND type in (N'U'))
ALTER TABLE [dbo].[Buildings] DROP CONSTRAINT IF EXISTS [FK_Buildings_Cities]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Buildings]') AND type in (N'U'))
ALTER TABLE [dbo].[Buildings] DROP CONSTRAINT IF EXISTS [FK_Buildings_BuildingTypes]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RiskFactorConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[RiskFactorConfigs] DROP CONSTRAINT IF EXISTS [DF_RiskFactorConfigs_CreatedAt]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RiskFactorConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[RiskFactorConfigs] DROP CONSTRAINT IF EXISTS [DF_RiskFactorConfigs_IsActive]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RiskFactorConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[RiskFactorConfigs] DROP CONSTRAINT IF EXISTS [DF_RiskFactorConfigs_RiskFactorConfigId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[FeeConfigs] DROP CONSTRAINT IF EXISTS [DF_FeeConfigs_CreatedAt]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[FeeConfigs] DROP CONSTRAINT IF EXISTS [DF_FeeConfigs_IsActive]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeeConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[FeeConfigs] DROP CONSTRAINT IF EXISTS [DF_FeeConfigs_FeeConfigId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Currencies]') AND type in (N'U'))
ALTER TABLE [dbo].[Currencies] DROP CONSTRAINT IF EXISTS [DF_Currencies_CreatedAt]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Currencies]') AND type in (N'U'))
ALTER TABLE [dbo].[Currencies] DROP CONSTRAINT IF EXISTS [DF_Currencies_IsActive]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Currencies]') AND type in (N'U'))
ALTER TABLE [dbo].[Currencies] DROP CONSTRAINT IF EXISTS [DF_Currencies_CurrencyId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Countries]') AND type in (N'U'))
ALTER TABLE [dbo].[Countries] DROP CONSTRAINT IF EXISTS [DF_Countries_CountryId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Counties]') AND type in (N'U'))
ALTER TABLE [dbo].[Counties] DROP CONSTRAINT IF EXISTS [DF_Counties_CountyId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
ALTER TABLE [dbo].[Clients] DROP CONSTRAINT IF EXISTS [DF_Clients_CreatedAt]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
ALTER TABLE [dbo].[Clients] DROP CONSTRAINT IF EXISTS [DF_Clients_ClientId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cities]') AND type in (N'U'))
ALTER TABLE [dbo].[Cities] DROP CONSTRAINT IF EXISTS [DF_Cities_CityId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BuildingTypes]') AND type in (N'U'))
ALTER TABLE [dbo].[BuildingTypes] DROP CONSTRAINT IF EXISTS [DF_BuildingTypes_BuildingTypeId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Buildings]') AND type in (N'U'))
ALTER TABLE [dbo].[Buildings] DROP CONSTRAINT IF EXISTS [DF_Buildings_CreatedAt]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Buildings]') AND type in (N'U'))
ALTER TABLE [dbo].[Buildings] DROP CONSTRAINT IF EXISTS [DF_Buildings_BuildingId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Brokers]') AND type in (N'U'))
ALTER TABLE [dbo].[Brokers] DROP CONSTRAINT IF EXISTS [DF_Brokers_CreatedAt]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Brokers]') AND type in (N'U'))
ALTER TABLE [dbo].[Brokers] DROP CONSTRAINT IF EXISTS [DF_Brokers_IsActive]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Brokers]') AND type in (N'U'))
ALTER TABLE [dbo].[Brokers] DROP CONSTRAINT IF EXISTS [DF_Brokers_BrokerId]
GO
/****** Object:  Index [UQ_RiskFactorConfigs_Level_ReferenceId]    Script Date: 09/30/2026 11:02:40 ******/
DROP INDEX IF EXISTS [UQ_RiskFactorConfigs_Level_ReferenceId] ON [dbo].[RiskFactorConfigs]
GO
/****** Object:  Index [IX_RiskFactorConfigs_Level_ReferenceId]    Script Date: 09/30/2026 11:02:40 ******/
DROP INDEX IF EXISTS [IX_RiskFactorConfigs_Level_ReferenceId] ON [dbo].[RiskFactorConfigs]
GO
/****** Object:  Index [UQ_Currencies_Code]    Script Date: 09/30/2026 11:02:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Currencies]') AND type in (N'U'))
ALTER TABLE [dbo].[Currencies] DROP CONSTRAINT IF EXISTS [UQ_Currencies_Code]
GO
/****** Object:  Index [UQ_Countries_Name]    Script Date: 09/30/2026 11:02:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Countries]') AND type in (N'U'))
ALTER TABLE [dbo].[Countries] DROP CONSTRAINT IF EXISTS [UQ_Countries_Name]
GO
/****** Object:  Index [IX_Counties_CountryId]    Script Date: 09/30/2026 11:02:40 ******/
DROP INDEX IF EXISTS [IX_Counties_CountryId] ON [dbo].[Counties]
GO
/****** Object:  Index [UQ_Counties_CountryId_Name]    Script Date: 09/30/2026 11:02:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Counties]') AND type in (N'U'))
ALTER TABLE [dbo].[Counties] DROP CONSTRAINT IF EXISTS [UQ_Counties_CountryId_Name]
GO
/****** Object:  Index [UQ_Clients_IdentificationNumber]    Script Date: 09/30/2026 11:02:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
ALTER TABLE [dbo].[Clients] DROP CONSTRAINT IF EXISTS [UQ_Clients_IdentificationNumber]
GO
/****** Object:  Index [IX_Cities_CountyId]    Script Date: 09/30/2026 11:02:40 ******/
DROP INDEX IF EXISTS [IX_Cities_CountyId] ON [dbo].[Cities]
GO
/****** Object:  Index [UQ_Cities_CountyId_Name]    Script Date: 09/30/2026 11:02:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cities]') AND type in (N'U'))
ALTER TABLE [dbo].[Cities] DROP CONSTRAINT IF EXISTS [UQ_Cities_CountyId_Name]
GO
/****** Object:  Index [UQ_BuildingTypes_Name]    Script Date: 09/30/2026 11:02:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BuildingTypes]') AND type in (N'U'))
ALTER TABLE [dbo].[BuildingTypes] DROP CONSTRAINT IF EXISTS [UQ_BuildingTypes_Name]
GO
/****** Object:  Index [IX_Buildings_ClientId]    Script Date: 09/30/2026 11:02:40 ******/
DROP INDEX IF EXISTS [IX_Buildings_ClientId] ON [dbo].[Buildings]
GO
/****** Object:  Index [IX_Buildings_CityId]    Script Date: 09/30/2026 11:02:40 ******/
DROP INDEX IF EXISTS [IX_Buildings_CityId] ON [dbo].[Buildings]
GO
/****** Object:  Index [IX_Buildings_BuildingTypeId]    Script Date: 09/30/2026 11:02:40 ******/
DROP INDEX IF EXISTS [IX_Buildings_BuildingTypeId] ON [dbo].[Buildings]
GO
/****** Object:  Index [UQ_Brokers_BrokerCode]    Script Date: 09/30/2026 11:02:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Brokers]') AND type in (N'U'))
ALTER TABLE [dbo].[Brokers] DROP CONSTRAINT IF EXISTS [UQ_Brokers_BrokerCode]
GO
/****** Object:  Table [dbo].[RiskFactorConfigs]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[RiskFactorConfigs]
GO
/****** Object:  Table [dbo].[FeeConfigs]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[FeeConfigs]
GO
/****** Object:  Table [dbo].[Currencies]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[Currencies]
GO
/****** Object:  Table [dbo].[Countries]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[Countries]
GO
/****** Object:  Table [dbo].[Counties]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[Counties]
GO
/****** Object:  Table [dbo].[Clients]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[Clients]
GO
/****** Object:  Table [dbo].[Cities]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[Cities]
GO
/****** Object:  Table [dbo].[BuildingTypes]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[BuildingTypes]
GO
/****** Object:  Table [dbo].[Buildings]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[Buildings]
GO
/****** Object:  Table [dbo].[Brokers]    Script Date: 09/30/2026 11:02:40 ******/
DROP TABLE IF EXISTS [dbo].[Brokers]
GO
/****** Object:  Table [dbo].[Brokers]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Brokers](
	[BrokerId] [uniqueidentifier] NOT NULL,
	[BrokerCode] [varchar](50) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Email] [varchar](100) NULL,
	[Phone] [varchar](20) NULL,
	[CommissionPercentage] [decimal](5, 2) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ModifiedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Brokers] PRIMARY KEY CLUSTERED 
(
	[BrokerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Buildings]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Buildings](
	[BuildingId] [uniqueidentifier] NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[CityId] [uniqueidentifier] NOT NULL,
	[BuildingTypeId] [uniqueidentifier] NOT NULL,
	[AddressStreet] [nvarchar](200) NOT NULL,
	[AddressStreetNumber] [nvarchar](20) NOT NULL,
	[ConstructionYear] [int] NOT NULL,
	[NumberOfFloors] [int] NOT NULL,
	[SurfaceArea] [decimal](18, 2) NOT NULL,
	[InsuredValue] [decimal](18, 2) NOT NULL,
	[RiskIndicators] [nvarchar](1000) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ModifiedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Buildings] PRIMARY KEY CLUSTERED 
(
	[BuildingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BuildingTypes]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BuildingTypes](
	[BuildingTypeId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_BuildingTypes] PRIMARY KEY CLUSTERED 
(
	[BuildingTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Cities]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cities](
	[CityId] [uniqueidentifier] NOT NULL,
	[CountyId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Cities] PRIMARY KEY CLUSTERED 
(
	[CityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Clients]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clients](
	[ClientId] [uniqueidentifier] NOT NULL,
	[ClientType] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IdentificationNumber] [varchar](20) NOT NULL,
	[Email] [varchar](100) NULL,
	[Phone] [varchar](20) NULL,
	[Address] [nvarchar](200) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ModifiedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED 
(
	[ClientId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Counties]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Counties](
	[CountyId] [uniqueidentifier] NOT NULL,
	[CountryId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Counties] PRIMARY KEY CLUSTERED 
(
	[CountyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Countries]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Countries](
	[CountryId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED 
(
	[CountryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Currencies]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Currencies](
	[CurrencyId] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](3) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[ExchangeRateToBase] [decimal](18, 4) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ModifiedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Currencies] PRIMARY KEY CLUSTERED 
(
	[CurrencyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FeeConfigs]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FeeConfigs](
	[FeeConfigId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[FeeType] [int] NOT NULL,
	[Percentage] [decimal](5, 2) NOT NULL,
	[EffectiveFrom] [date] NOT NULL,
	[EffectiveTo] [date] NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ModifiedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_FeeConfigs] PRIMARY KEY CLUSTERED 
(
	[FeeConfigId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RiskFactorConfigs]    Script Date: 09/30/2026 11:02:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RiskFactorConfigs](
	[RiskFactorConfigId] [uniqueidentifier] NOT NULL,
	[Level] [int] NOT NULL,
	[ReferenceId] [uniqueidentifier] NOT NULL,
	[AdjustmentPercentage] [decimal](5, 2) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ModifiedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_RiskFactorConfigs] PRIMARY KEY CLUSTERED 
(
	[RiskFactorConfigId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[BuildingTypes] ([BuildingTypeId], [Name]) VALUES (N'594239f9-60b7-f111-bc3c-bcf1054861b1', N'Industrial')
GO
INSERT [dbo].[BuildingTypes] ([BuildingTypeId], [Name]) VALUES (N'584239f9-60b7-f111-bc3c-bcf1054861b1', N'Office')
GO
INSERT [dbo].[BuildingTypes] ([BuildingTypeId], [Name]) VALUES (N'574239f9-60b7-f111-bc3c-bcf1054861b1', N'Residential')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'ebdd7dad-60b7-f111-bc3c-bcf1054861b1', N'e6dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Alba Iulia')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'ecdd7dad-60b7-f111-bc3c-bcf1054861b1', N'e6dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Sebeș')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'eddd7dad-60b7-f111-bc3c-bcf1054861b1', N'e7dd7dad-60b7-f111-bc3c-bcf1054861b1', N'București')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'eedd7dad-60b7-f111-bc3c-bcf1054861b1', N'e8dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Cluj-Napoca')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'f0dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e8dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Dej')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'efdd7dad-60b7-f111-bc3c-bcf1054861b1', N'e8dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Turda')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'f2dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e9dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Reghin')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'f3dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e9dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Sighișoara')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'f1dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e9dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Târgu Mureș')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'f5dd7dad-60b7-f111-bc3c-bcf1054861b1', N'eadd7dad-60b7-f111-bc3c-bcf1054861b1', N'Călimănești')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'f4dd7dad-60b7-f111-bc3c-bcf1054861b1', N'eadd7dad-60b7-f111-bc3c-bcf1054861b1', N'Râmnicu Vâlcea')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'f7dd7dad-60b7-f111-bc3c-bcf1054861b1', N'f6dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Budapest')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'fbdd7dad-60b7-f111-bc3c-bcf1054861b1', N'f8dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Bergamo')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'fcdd7dad-60b7-f111-bc3c-bcf1054861b1', N'f8dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Brescia')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'fadd7dad-60b7-f111-bc3c-bcf1054861b1', N'f8dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Milan')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'fddd7dad-60b7-f111-bc3c-bcf1054861b1', N'f9dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Florence')
GO
INSERT [dbo].[Cities] ([CityId], [CountyId], [Name]) VALUES (N'fedd7dad-60b7-f111-bc3c-bcf1054861b1', N'f9dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Pisa')
GO
INSERT [dbo].[Counties] ([CountyId], [CountryId], [Name]) VALUES (N'e6dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e3dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Alba')
GO
INSERT [dbo].[Counties] ([CountyId], [CountryId], [Name]) VALUES (N'e7dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e3dd7dad-60b7-f111-bc3c-bcf1054861b1', N'București')
GO
INSERT [dbo].[Counties] ([CountyId], [CountryId], [Name]) VALUES (N'e8dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e3dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Cluj')
GO
INSERT [dbo].[Counties] ([CountyId], [CountryId], [Name]) VALUES (N'e9dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e3dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Mureș')
GO
INSERT [dbo].[Counties] ([CountyId], [CountryId], [Name]) VALUES (N'eadd7dad-60b7-f111-bc3c-bcf1054861b1', N'e3dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Vâlcea')
GO
INSERT [dbo].[Counties] ([CountyId], [CountryId], [Name]) VALUES (N'f6dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e4dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Budapest')
GO
INSERT [dbo].[Counties] ([CountyId], [CountryId], [Name]) VALUES (N'f8dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e5dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Lombardy')
GO
INSERT [dbo].[Counties] ([CountyId], [CountryId], [Name]) VALUES (N'f9dd7dad-60b7-f111-bc3c-bcf1054861b1', N'e5dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Tuscany')
GO
INSERT [dbo].[Countries] ([CountryId], [Name]) VALUES (N'e4dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Hungary')
GO
INSERT [dbo].[Countries] ([CountryId], [Name]) VALUES (N'e5dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Italy')
GO
INSERT [dbo].[Countries] ([CountryId], [Name]) VALUES (N'e3dd7dad-60b7-f111-bc3c-bcf1054861b1', N'Romania')
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Brokers_BrokerCode]    Script Date: 09/30/2026 11:02:40 ******/
ALTER TABLE [dbo].[Brokers] ADD  CONSTRAINT [UQ_Brokers_BrokerCode] UNIQUE NONCLUSTERED 
(
	[BrokerCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Buildings_BuildingTypeId]    Script Date: 09/30/2026 11:02:40 ******/
CREATE NONCLUSTERED INDEX [IX_Buildings_BuildingTypeId] ON [dbo].[Buildings]
(
	[BuildingTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Buildings_CityId]    Script Date: 09/30/2026 11:02:40 ******/
CREATE NONCLUSTERED INDEX [IX_Buildings_CityId] ON [dbo].[Buildings]
(
	[CityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Buildings_ClientId]    Script Date: 09/30/2026 11:02:40 ******/
CREATE NONCLUSTERED INDEX [IX_Buildings_ClientId] ON [dbo].[Buildings]
(
	[ClientId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_BuildingTypes_Name]    Script Date: 09/30/2026 11:02:40 ******/
ALTER TABLE [dbo].[BuildingTypes] ADD  CONSTRAINT [UQ_BuildingTypes_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Cities_CountyId_Name]    Script Date: 09/30/2026 11:02:40 ******/
ALTER TABLE [dbo].[Cities] ADD  CONSTRAINT [UQ_Cities_CountyId_Name] UNIQUE NONCLUSTERED 
(
	[CountyId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Cities_CountyId]    Script Date: 09/30/2026 11:02:40 ******/
CREATE NONCLUSTERED INDEX [IX_Cities_CountyId] ON [dbo].[Cities]
(
	[CountyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Clients_IdentificationNumber]    Script Date: 09/30/2026 11:02:40 ******/
ALTER TABLE [dbo].[Clients] ADD  CONSTRAINT [UQ_Clients_IdentificationNumber] UNIQUE NONCLUSTERED 
(
	[IdentificationNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Counties_CountryId_Name]    Script Date: 09/30/2026 11:02:40 ******/
ALTER TABLE [dbo].[Counties] ADD  CONSTRAINT [UQ_Counties_CountryId_Name] UNIQUE NONCLUSTERED 
(
	[CountryId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Counties_CountryId]    Script Date: 09/30/2026 11:02:40 ******/
CREATE NONCLUSTERED INDEX [IX_Counties_CountryId] ON [dbo].[Counties]
(
	[CountryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Countries_Name]    Script Date: 09/30/2026 11:02:40 ******/
ALTER TABLE [dbo].[Countries] ADD  CONSTRAINT [UQ_Countries_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Currencies_Code]    Script Date: 09/30/2026 11:02:40 ******/
ALTER TABLE [dbo].[Currencies] ADD  CONSTRAINT [UQ_Currencies_Code] UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RiskFactorConfigs_Level_ReferenceId]    Script Date: 09/30/2026 11:02:40 ******/
CREATE NONCLUSTERED INDEX [IX_RiskFactorConfigs_Level_ReferenceId] ON [dbo].[RiskFactorConfigs]
(
	[Level] ASC,
	[ReferenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ_RiskFactorConfigs_Level_ReferenceId]    Script Date: 09/30/2026 11:02:40 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_RiskFactorConfigs_Level_ReferenceId] ON [dbo].[RiskFactorConfigs]
(
	[Level] ASC,
	[ReferenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Brokers] ADD  CONSTRAINT [DF_Brokers_BrokerId]  DEFAULT (newsequentialid()) FOR [BrokerId]
GO
ALTER TABLE [dbo].[Brokers] ADD  CONSTRAINT [DF_Brokers_IsActive]  DEFAULT ((0)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Brokers] ADD  CONSTRAINT [DF_Brokers_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Buildings] ADD  CONSTRAINT [DF_Buildings_BuildingId]  DEFAULT (newsequentialid()) FOR [BuildingId]
GO
ALTER TABLE [dbo].[Buildings] ADD  CONSTRAINT [DF_Buildings_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[BuildingTypes] ADD  CONSTRAINT [DF_BuildingTypes_BuildingTypeId]  DEFAULT (newsequentialid()) FOR [BuildingTypeId]
GO
ALTER TABLE [dbo].[Cities] ADD  CONSTRAINT [DF_Cities_CityId]  DEFAULT (newsequentialid()) FOR [CityId]
GO
ALTER TABLE [dbo].[Clients] ADD  CONSTRAINT [DF_Clients_ClientId]  DEFAULT (newsequentialid()) FOR [ClientId]
GO
ALTER TABLE [dbo].[Clients] ADD  CONSTRAINT [DF_Clients_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Counties] ADD  CONSTRAINT [DF_Counties_CountyId]  DEFAULT (newsequentialid()) FOR [CountyId]
GO
ALTER TABLE [dbo].[Countries] ADD  CONSTRAINT [DF_Countries_CountryId]  DEFAULT (newsequentialid()) FOR [CountryId]
GO
ALTER TABLE [dbo].[Currencies] ADD  CONSTRAINT [DF_Currencies_CurrencyId]  DEFAULT (newsequentialid()) FOR [CurrencyId]
GO
ALTER TABLE [dbo].[Currencies] ADD  CONSTRAINT [DF_Currencies_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Currencies] ADD  CONSTRAINT [DF_Currencies_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[FeeConfigs] ADD  CONSTRAINT [DF_FeeConfigs_FeeConfigId]  DEFAULT (newsequentialid()) FOR [FeeConfigId]
GO
ALTER TABLE [dbo].[FeeConfigs] ADD  CONSTRAINT [DF_FeeConfigs_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[FeeConfigs] ADD  CONSTRAINT [DF_FeeConfigs_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RiskFactorConfigs] ADD  CONSTRAINT [DF_RiskFactorConfigs_RiskFactorConfigId]  DEFAULT (newsequentialid()) FOR [RiskFactorConfigId]
GO
ALTER TABLE [dbo].[RiskFactorConfigs] ADD  CONSTRAINT [DF_RiskFactorConfigs_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[RiskFactorConfigs] ADD  CONSTRAINT [DF_RiskFactorConfigs_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Buildings]  WITH CHECK ADD  CONSTRAINT [FK_Buildings_BuildingTypes] FOREIGN KEY([BuildingTypeId])
REFERENCES [dbo].[BuildingTypes] ([BuildingTypeId])
GO
ALTER TABLE [dbo].[Buildings] CHECK CONSTRAINT [FK_Buildings_BuildingTypes]
GO
ALTER TABLE [dbo].[Buildings]  WITH CHECK ADD  CONSTRAINT [FK_Buildings_Cities] FOREIGN KEY([CityId])
REFERENCES [dbo].[Cities] ([CityId])
GO
ALTER TABLE [dbo].[Buildings] CHECK CONSTRAINT [FK_Buildings_Cities]
GO
ALTER TABLE [dbo].[Buildings]  WITH CHECK ADD  CONSTRAINT [FK_Buildings_Clients] FOREIGN KEY([ClientId])
REFERENCES [dbo].[Clients] ([ClientId])
GO
ALTER TABLE [dbo].[Buildings] CHECK CONSTRAINT [FK_Buildings_Clients]
GO
ALTER TABLE [dbo].[Cities]  WITH CHECK ADD  CONSTRAINT [FK_Cities_Counties] FOREIGN KEY([CountyId])
REFERENCES [dbo].[Counties] ([CountyId])
GO
ALTER TABLE [dbo].[Cities] CHECK CONSTRAINT [FK_Cities_Counties]
GO
ALTER TABLE [dbo].[Counties]  WITH CHECK ADD  CONSTRAINT [FK_Counties_Countries] FOREIGN KEY([CountryId])
REFERENCES [dbo].[Countries] ([CountryId])
GO
ALTER TABLE [dbo].[Counties] CHECK CONSTRAINT [FK_Counties_Countries]
GO
ALTER TABLE [dbo].[Currencies]  WITH CHECK ADD  CONSTRAINT [CK_Currencies_ExchangeRate] CHECK  (([ExchangeRateToBase]>(0)))
GO
ALTER TABLE [dbo].[Currencies] CHECK CONSTRAINT [CK_Currencies_ExchangeRate]
GO
ALTER TABLE [dbo].[FeeConfigs]  WITH CHECK ADD  CONSTRAINT [CK_FeeConfigs_EffectivePeriod] CHECK  (([EffectiveTo] IS NULL OR [EffectiveTo]>=[EffectiveFrom]))
GO
ALTER TABLE [dbo].[FeeConfigs] CHECK CONSTRAINT [CK_FeeConfigs_EffectivePeriod]
GO
ALTER TABLE [dbo].[FeeConfigs]  WITH CHECK ADD  CONSTRAINT [CK_FeeConfigs_Percentage] CHECK  (([Percentage]>=(0) AND [Percentage]<=(100)))
GO
ALTER TABLE [dbo].[FeeConfigs] CHECK CONSTRAINT [CK_FeeConfigs_Percentage]
GO
