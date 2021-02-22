using CMP.Connector.Helpers;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Conversion;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;

namespace CMP.Connector.Pipelines
{
    public class EnsureItem : ImportEntityProcessor
    {
        private readonly BaseFactory _factory;

        private static CmpSettings _settings;

        private readonly CmpHelper _cmpHelper;

        private readonly ID EntityMappingTemplateFieldId = new ID("{E9C95E69-0D3D-4AE3-B893-D1FF4BA36B03}");

        public EnsureItem(BaseFactory factory, BaseLog logger, CmpSettings cmpSettings, CmpHelper cmpHelper)
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
                using (new SecurityDisabler())
                {
                    Database database = _factory.GetDatabase(_settings.DatabaseName);
                    Assert.IsNotNull(database, "Could not get the master database.");
                    var _mapper = new CmpConverterMapper(_factory, _settings);
                    //Item cmpItemBucket = _cmpHelper.GetCmpItemBucket(args, database);
                    Item cmpItemBucket = EntityRulesHelper.GetFolderBasedOnRule(args.EntityMappingItem, args, _settings, _cmpHelper, _mapper);
                    Assert.IsNotNull(cmpItemBucket, "Could not find the item bucket. Check this field value in the configuration item.");
                    string propertyValue = args.Entity.GetPropertyValue<string>("Content.Name");
                    if (string.IsNullOrEmpty(propertyValue))
                        propertyValue = args.Entity.GetPropertyValue<string>("FileName");
                    string name = ItemUtil.ProposeValidItemName(propertyValue);
                    // TemplateItem templateItem = database.GetItem(new ID(args.EntityMappingItem[EntityMappingTemplateFieldId]), args.Language);
                    TemplateItem templateItem = EntityRulesHelper.GetTemplateBasedOnRule(args.EntityMappingItem, args, _settings, _cmpHelper, _mapper);
                    Assert.IsNotNull(templateItem, "Could not get template item. Check this field value in the configuration item.");
                    args.Item = cmpItemBucket.Add(name, templateItem);
                }
            }
        }
    }
}