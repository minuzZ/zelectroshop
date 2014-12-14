--new locale resources
declare @resources xml
--a resource will be deleted if its value is empty
set @resources='
<Language>
  <LocaleResource Name="Admin.Configuration.Settings.SMS">
    <Value>SMS</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.SMS.Sender">
    <Value>Sender</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.SMS.CountryCode">
    <Value>Country code</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.SMS.NumberLength">
    <Value>Number length without country code</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.SMS.MessageTemplate">
    <Value>Message template</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.SMS.ServiceId">
    <Value>Bytehand service id</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.SMS.ServiceKey">
    <Value>Bytehand service key</Value>
  </LocaleResource>
  <LocaleResource Name="ShoppingCart.NotInStock">
    <Value>Not in stock</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.DynamicPrice">
    <Value>Dynamic price</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.DynamicPrice.EuroRate">
    <Value>Euro rate</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.DynamicPrice.EuroRate.Hint">
    <Value>Set euro rate</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.DynamicPrice.DollarRate">
    <Value>Dollar rate</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.DynamicPrice.DollarRate.Hint">
    <Value>Set dollar rate</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.DynamicPrice.Recalculate">
    <Value>Recalculate</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.DynamicPrice.Recalculated">
    <Value>Prices were successfully recalculated!</Value>
  </LocaleResource>
</Language>
'

CREATE TABLE #LocaleStringResourceTmp
	(
		[ResourceName] [nvarchar](200) NOT NULL,
		[ResourceValue] [nvarchar](max) NOT NULL
	)

INSERT INTO #LocaleStringResourceTmp (ResourceName, ResourceValue)
SELECT	nref.value('@Name', 'nvarchar(200)'), nref.value('Value[1]', 'nvarchar(MAX)')
FROM	@resources.nodes('//Language/LocaleResource') AS R(nref)

--do it for each existing language
DECLARE @ExistingLanguageID int
DECLARE cur_existinglanguage CURSOR FOR
SELECT [ID]
FROM [Language]
OPEN cur_existinglanguage
FETCH NEXT FROM cur_existinglanguage INTO @ExistingLanguageID
WHILE @@FETCH_STATUS = 0
BEGIN
	DECLARE @ResourceName nvarchar(200)
	DECLARE @ResourceValue nvarchar(MAX)
	DECLARE cur_localeresource CURSOR FOR
	SELECT ResourceName, ResourceValue
	FROM #LocaleStringResourceTmp
	OPEN cur_localeresource
	FETCH NEXT FROM cur_localeresource INTO @ResourceName, @ResourceValue
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF (EXISTS (SELECT 1 FROM [LocaleStringResource] WHERE LanguageID=@ExistingLanguageID AND ResourceName=@ResourceName))
		BEGIN
			UPDATE [LocaleStringResource]
			SET [ResourceValue]=@ResourceValue
			WHERE LanguageID=@ExistingLanguageID AND ResourceName=@ResourceName
		END
		ELSE 
		BEGIN
			INSERT INTO [LocaleStringResource]
			(
				[LanguageId],
				[ResourceName],
				[ResourceValue]
			)
			VALUES
			(
				@ExistingLanguageID,
				@ResourceName,
				@ResourceValue
			)
		END
		
		IF (@ResourceValue is null or @ResourceValue = '')
		BEGIN
			DELETE [LocaleStringResource]
			WHERE LanguageID=@ExistingLanguageID AND ResourceName=@ResourceName
		END
		
		FETCH NEXT FROM cur_localeresource INTO @ResourceName, @ResourceValue
	END
	CLOSE cur_localeresource
	DEALLOCATE cur_localeresource


	--fetch next language identifier
	FETCH NEXT FROM cur_existinglanguage INTO @ExistingLanguageID
END
CLOSE cur_existinglanguage
DEALLOCATE cur_existinglanguage

DROP TABLE #LocaleStringResourceTmp
GO

--END NEW RESOURCES


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

--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'smssettings.numberlength')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'smssettings.numberlength', N'', 0)
END
GO

--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'dynamicpricesetting.eurorate')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'dynamicpricesetting.eurorate', N'', 0)
END
GO

--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'dynamicpricesetting.dollarrate')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'dynamicpricesetting.dollarrate', N'', 0)
END
GO

--new column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='FirstCost')
BEGIN
	ALTER TABLE [Product]
	ADD [FirstCost] [decimal](18, 4) NULL
END
GO

UPDATE [Product]
SET [FirstCost] = 0
WHERE [FirstCost] IS NULL
GO

ALTER TABLE [Product] ALTER COLUMN [FirstCost] [decimal](18, 4) NOT NULL
GO

--new column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='FirstCostCurrencyTypeId')
BEGIN
	ALTER TABLE [Product]
	ADD [FirstCostCurrencyTypeId] int NULL
END
GO

UPDATE [Product]
SET [FirstCostCurrencyTypeId] = 0
WHERE [FirstCostCurrencyTypeId] IS NULL
GO

ALTER TABLE [Product] ALTER COLUMN [FirstCostCurrencyTypeId] int NOT NULL
GO

--new column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DesiredProfit')
BEGIN
	ALTER TABLE [Product]
	ADD [DesiredProfit] int NULL
END
GO

UPDATE [Product]
SET [DesiredProfit] = 0
WHERE [DesiredProfit] IS NULL
GO

ALTER TABLE [Product] ALTER COLUMN [DesiredProfit] int NOT NULL
GO

--new column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Product]') and NAME='DollarPrice')
BEGIN
	ALTER TABLE [Product]
	ADD [DollarPrice] [decimal](18, 4) NULL
END
GO

UPDATE [Product]
SET [DollarPrice] = 0
WHERE [DollarPrice] IS NULL
GO

ALTER TABLE [Product] ALTER COLUMN [DollarPrice] [decimal](18, 4) NOT NULL
GO