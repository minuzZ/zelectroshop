﻿--upgrade scripts from nopCommerce 2.20 to nopCommerce 2.30

--new locale resources
declare @resources xml
--a resource will be delete if its value is empty
set @resources='
<Language>
  <LocaleResource Name="Admin.System.SystemInfo.ASPNETInfo.Hint">
        <Value>ASP.NET info</Value>
    </LocaleResource>
    <LocaleResource Name="Admin.System.SystemInfo.IsFullTrust.Hint">
        <Value>Is full trust level</Value>
    </LocaleResource>
    <LocaleResource Name="Admin.System.SystemInfo.NopVersion.Hint">
        <Value>nopCommerce version</Value>
    </LocaleResource>
    <LocaleResource Name="Admin.System.SystemInfo.OperatingSystem.Hint">
        <Value>Operating system</Value>
    </LocaleResource>
    <LocaleResource Name="Admin.System.SystemInfo.ServerLocalTime.Hint">
        <Value>Server local time</Value>
    </LocaleResource>
    <LocaleResource Name="Admin.System.SystemInfo.ServerTimeZone.Hint">
        <Value>Server time zone</Value>
    </LocaleResource>
    <LocaleResource Name="Admin.System.SystemInfo.UTCTime.Hint">
        <Value>Greenwich mean time (GMT/UTC)</Value>
    </LocaleResource>
    <LocaleResource Name="Admin.Common.ConfigurationNotRequired">
        <Value>Configuration is not required</Value>
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
				[LanguageID],
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

