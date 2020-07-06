﻿CREATE TABLE [dbo].[RaffleOrders]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [TicketNumber] NVARCHAR(30) NULL, 
    [Customer_Email] NVARCHAR(500) NULL, 
    [Customer_FirstName] NVARCHAR(200) NULL, 
    [Customer_LastName] NVARCHAR(200) NULL, 
    [Customer_PhoneNumber] NVARCHAR(12) NULL,
    [Customer_AddressLine1] NVARCHAR(500) NULL, 
    [Customer_AddressLine2] NVARCHAR(150) NULL, 
    [Customer_Address_City] NVARCHAR(150) NULL, 
    [Customer_Address_State] NVARCHAR(150) NULL, 
    [Customer_Address_Zip] NVARCHAR(11) NULL
)
