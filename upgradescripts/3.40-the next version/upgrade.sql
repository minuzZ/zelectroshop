﻿--upgrade scripts from nopCommerce 3.40 to 3.50

--new locale resources
declare @resources xml
--a resource will be deleted if its value is empty
set @resources='
<Language>
  <LocaleResource Name="Admin.Promotions.Campaigns.Fields.Store">
    <Value>Limited to store</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Promotions.Campaigns.Fields.Store.Hint">
    <Value>Choose a store which subscribers will get this email.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Plugins.Saved">
    <Value>The plugin has been updated successfully.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Vendors.List.SearchName">
    <Value>Vendor name</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Vendors.List.SearchName.Hint">
    <Value>A vendor name.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.SalesReport.Incomplete.View">
    <Value>view all</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.IncludeInTopMenu">
    <Value>Include in top menu</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.IncludeInTopMenu.Hint">
    <Value>Check to include this topic in the top menu.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Blog.BlogPosts.Fields.Tags.NoDots">
    <Value>Dots are not supported by tags.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Countries.ExportToCsv">
    <Value>Export states to CSV</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Countries.ImportFromCsv">
    <Value>Import states from CSV</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Countries.ImportSuccess">
    <Value>{0} states have been successfully imported</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.RewardPoints.DisplayHowMuchWillBeEarned">
    <Value>Display how much will be earned</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.RewardPoints.DisplayHowMuchWillBeEarned.Hint">
    <Value>Check to display how much point will be earned before checkout.</Value>
  </LocaleResource>
  <LocaleResource Name="ShoppingCart.Totals.RewardPoints.WillEarn">
    <Value>You will earn</Value>
  </LocaleResource>
  <LocaleResource Name="ShoppingCart.Totals.RewardPoints.WillEarn.Point">
    <Value>{0} points</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.Media.MultipleThumbDirectories">
    <Value>Multiple thumb directories</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.Media.MultipleThumbDirectories.Hint">
    <Value>Check to enable multiple thumb directories. It can be helpful if your hosting company has some limitations to the number of allowed files per directory.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Affiliates.Fields.AdminComment">
    <Value>Admin comment</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Affiliates.Fields.AdminComment.Hint">
    <Value>Admin comment. For internal use.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.CacheProductPrices.Hint">
    <Value>Check to cache product prices. It can significantly improve performance. But you not should enable it if you use some complex discounts, discount requirement rules, or coupon codes.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Promotions.Discounts.Fields.MaximumDiscountedQuantity">
    <Value>Maximum discounted quantity</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Promotions.Discounts.Fields.MaximumDiscountedQuantity.Hint">
    <Value>Maximum product quantity which could be discounted. For example, you can have two products (the same) in the cart but only one of them will be discounted. It can be used for scenarios like "buy 2 get 1 free". Leave empty if any quantity could be discounted.</Value>
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


--new column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Campaign]') and NAME='StoreId')
BEGIN
	ALTER TABLE [Campaign]
	ADD [StoreId] int NULL
END
GO

     
UPDATE [Campaign]
SET [StoreId] = 0
WHERE [StoreId] IS NULL
GO

ALTER TABLE [Campaign] ALTER COLUMN [StoreId] int NOT NULL
GO


--new column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Topic]') and NAME='IncludeInTopMenu')
BEGIN
	ALTER TABLE [Topic]
	ADD [IncludeInTopMenu] bit NULL
END
GO

UPDATE [Topic]
SET [IncludeInTopMenu] = 0
WHERE [IncludeInTopMenu] IS NULL
GO

ALTER TABLE [Topic] ALTER COLUMN [IncludeInTopMenu] bit NOT NULL
GO

--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'commonsettings.ignorelogwordlist')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'commonsettings.ignorelogwordlist', N'', 0)
END
GO



--new setting
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'rewardpointssettings.displayhowmuchwillbeearned')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'rewardpointssettings.displayhowmuchwillbeearned', N'true', 0)
END
GO


--new column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Affiliate]') and NAME='AdminComment')
BEGIN
	ALTER TABLE [Affiliate]
	ADD [AdminComment] nvarchar(MAX) NULL
END
GO

--new column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Discount]') and NAME='MaximumDiscountedQuantity')
BEGIN
	ALTER TABLE [Discount]
	ADD [MaximumDiscountedQuantity] int NULL
END
GO