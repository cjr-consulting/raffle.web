﻿CREATE TABLE [dbo].[RaffleOrders]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TicketNumber] NVARCHAR(150) NULL, 
    [Confirmed21] BIT NOT NULL DEFAULT 0,
    [Customer_Email] NVARCHAR(500) NULL, 
    [Customer_FirstName] NVARCHAR(200) NULL, 
    [Customer_LastName] NVARCHAR(200) NULL, 
    [Customer_PhoneNumber] NVARCHAR(25) NULL,
    [Customer_AddressLine1] NVARCHAR(500) NULL, 
    [Customer_AddressLine2] NVARCHAR(150) NULL, 
    [Customer_Address_City] NVARCHAR(150) NULL, 
    [Customer_Address_State] NVARCHAR(150) NULL, 
    [Customer_Address_Zip] NVARCHAR(11) NULL,
    [Customer_IsInternational] BIT NOT NULL DEFAULT 0,
    [Customer_IAddressText] NVARCHAR(500) NULL,
    [IsOrderConfirmed] BIT NULL DEFAULT 0, 
    [StartDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
    [CompletedDate] DATETIME2 NULL, 
    [UpdatedDate] DATETIME2 NULL
)
