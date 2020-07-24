CREATE TABLE [dbo].[RaffleEvents]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Title] NVARCHAR(150) NOT NULL, 
    [VisibleDate] DATETIME2 NOT NULL, 
    [StartDate] DATETIME2 NOT NULL, 
    [CloseDate] DATETIME2 NULL
)
