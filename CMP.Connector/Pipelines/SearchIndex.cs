using System;
using System.Linq;
using CMP.Connector.Helpers;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Conversion;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace CMP.Connector.Pipelines
{
    public class SearchIndex : ImportEntityProcessor
    {
        private static CmpSettings _settings;

        private readonly BaseFactory _factory;

        private readonly CmpHelper _cmpHelper;

        public SearchIndex(BaseFactory factory, BaseLog logger, CmpSettings cmpSettings, CmpHelper cmpHelper)
            : base(logger, cmpSettings)
        {
            _factory = factory;
            _settings = cmpSettings;
            _cmpHelper = cmpHelper;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            if (args.Item == null)
            {
                if (args.EntityMappingItem == null)
                {
                    args.EntityMappingItem = _cmpHelper.GetEntityMappingItem(args);
                }
                Assert.IsNotNull(args.EntityMappingItem, "Could not find any Entity Mapping item for the content type " + args.ContentTypeIdentifier);
                Database database = _factory.GetDatabase(_settings.DatabaseName);
                Assert.IsNotNull(database, "Could not get the master database.");
                //Item cmpItemBucket = _cmpHelper.GetCmpItemBucket(args, database);
                Item cmpItemBucket = EntityRulesHelper.GetFolderBasedOnRule(args.EntityMappingItem, args, _settings, _cmpHelper, new CmpConverterMapper(_factory, _settings));
                Assert.IsNotNull(cmpItemBucket, "Could not find the item bucket. Check this field value in the configuration item.");
                if (TryGetFromSearchIndex(args, cmpItemBucket, out var itemId))
                {
                    args.Item = database.GetItem(itemId, args.Language);
                }
            }
        }

        internal virtual bool TryGetFromSearchIndex(ImportEntityPipelineArgs args, Item itemBucket, out ID itemId)
        {
            Assert.IsNotNull(args.EntityIdentifier, "Could not get entity identifier.");
            using (IProviderSearchContext providerSearchContext = ContentSearchManager.GetIndex(new SitecoreIndexableItem(itemBucket)).CreateSearchContext())
            {
                SearchResultItem searchResultItem2 = providerSearchContext.GetQueryable<SearchResultItem>().FirstOrDefault((SearchResultItem searchResultItem) => string.Equals(searchResultItem["entityIdentifier_t"], $"\"{args.EntityIdentifier}\"", StringComparison.Ordinal) && searchResultItem.Paths.Contains(itemBucket.ID));
                if (searchResultItem2 == null)
                {
                    itemId = ID.Null;
                    return false;
                }
                itemId = searchResultItem2.ItemId;
                return true;
            }
        }
    }
}