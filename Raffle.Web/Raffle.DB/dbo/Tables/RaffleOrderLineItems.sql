CREATE TABLE [dbo].[RaffleOrderLineItems]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [RaffleOrderId] INT NOT NULL, 
    [RaffleItemId] INT NOT NULL, 
    [Name] NVARCHAR(250) NOT NULL, 
    [Price] INT NOT NULL, 
    [Count] INT NOT NULL, 
    CONSTRAINT [FK_RaffleOrderLineItems_RaffleOrders] FOREIGN KEY (RaffleOrderId) REFERENCES [RaffleOrders]([Id])
)

GO

CREATE INDEX [IX_RaffleOrderLineItems_RaffleOrderId] ON [dbo].[RaffleOrderLineItems] ([RaffleOrderId])
