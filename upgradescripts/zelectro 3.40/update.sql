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