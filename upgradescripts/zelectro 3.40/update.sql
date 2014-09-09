--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'smssettings.from')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'smssettings.from', N'', 0)
END
GO

--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'smssettings.countrycode')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'smssettings.countrycode', N'', 0)
END
GO

--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'smssettings.messagetemplate')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'smssettings.messagetemplate', N'', 0)
END
GO

--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'smssettings.serviceid')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'smssettings.serviceid', N'', 0)
END
GO

--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'smssettings.servicekey')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'smssettings.servicekey', N'', 0)
END
GO