CREATE TABLE [dbo].[RaffleItems] (
    [Id]          INT            NOT NULL IDENTITY,
    [ItemNumber]  INT            NOT NULL DEFAULT 0,
    [Title]       NVARCHAR (150)  NOT NULL,
    [Description] NVARCHAR (800) NOT NULL,
    [Category]    NVARCHAR (50)  NOT NULL,
    [Sponsor]     NVARCHAR (200)  NOT NULL,
    [ImageUrl]    NVARCHAR (500) NOT NULL,
    [ItemValue]   NVARCHAR (120)  NOT NULL,
    [Cost]        INT            NOT NULL,
    [IsAvailable] BIT            NOT NULL,
    [ForOver21]    BIT           NOT NULL DEFAULT 0,
    [LocalPickupOnly] BIT        NOT NULL DEFAULT 0,
    [NumberOfDraws] INT          NOT NULL DEFAULT 1,
    [WinningTickets] NVARCHAR(200) NOT NULL DEFAULT '',
    [Order] INT NOT NULL DEFAULT 0, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

