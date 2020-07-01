-- Create a new table called '[RaffleItems]' in schema '[dbo]'
-- Drop the table if it already exists
IF OBJECT_ID('[dbo].[RaffleItems]', 'U') IS NOT NULL
DROP TABLE [dbo].[RaffleItems]
GO
-- Create the table in the specified schema
CREATE TABLE [dbo].[RaffleItems]
(
    [Id] INT NOT NULL PRIMARY KEY, -- Primary Key column
    [Title] NVARCHAR(80) NOT NULL,
    [Description] NVARCHAR(150) NOT NULL,
    [Category] NVARCHAR(50) NOT NULL,
    [Sponsor] NVARCHAR(50) NOT NULL,
    [ItemValue] NVARCHAR(50) NOT NULL,
    [Cost] INT NOT NULL,
    [IsAvailable] BIT NOT NULL
);
GO