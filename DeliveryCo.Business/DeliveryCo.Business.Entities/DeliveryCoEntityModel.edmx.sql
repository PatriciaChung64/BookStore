
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 04/18/2019 16:59:17
-- Generated from EDMX file: C:\Users\gre403\Documents\Basser\COMP5348 2019\Group Project\GroupProject\DeliveryCo.Business\DeliveryCo.Business.Entities\DeliveryCoEntityModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [DeliveryCo];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[DeliveryInfo]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DeliveryInfo];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'DeliveryInfo'
CREATE TABLE [dbo].[DeliveryInfo] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SourceAddress] nvarchar(max)  NOT NULL,
    [DestinationAddress] nvarchar(max)  NOT NULL,
    [OrderNumber] nvarchar(max)  NOT NULL,
    [DeliveryIdentifier] uniqueidentifier  NOT NULL,
    [DeliveryNotificationAddress] nvarchar(max)  NOT NULL,
    [Status] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'DeliveryInfo'
ALTER TABLE [dbo].[DeliveryInfo]
ADD CONSTRAINT [PK_DeliveryInfo]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------