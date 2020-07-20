CREATE TABLE [dbo].[RaffleItemImages]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [RaffleItemId] INT NOT NULL, 
    [ImageRoute] NVARCHAR(500) NOT NULL, 
    [Title] NVARCHAR(250) NULL, 
    CONSTRAINT [FK_RaffleItemImages_RaffleItems] FOREIGN KEY ([RaffleItemId]) REFERENCES [dbo].[RaffleItems]([Id])
)
