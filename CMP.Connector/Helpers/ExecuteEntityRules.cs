using CMP.Connector.Rules;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Conversion;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data.Items;
using Sitecore.Rules;
using Stylelabs.M.Sdk.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CMP.Connector.Helpers
{
    public static class EntityRulesHelper
    {
        /// <summary>
        /// Gets folder item based on executed rule
        /// </summary>
        /// <param name="mappingItem"></param>
        /// <param name="entity"></param>
        /// <param name="entityDefinition"></param>
        /// <returns></returns>
        public static Item GetFolderBasedOnRule(Item mappingItem, ImportEntityPipelineArgs entityArgs, CmpSettings cmpSettings, CmpHelper cmpHelper, ICmpConverterMapper cmpMapper)
        {
            return GetResultItem(Constants.EntityMappingBucketFieldId, mappingItem, entityArgs, cmpSettings,cmpHelper, cmpMapper);
        }

        /// <summary>
        /// Gets Template item based on executed rule
        /// </summary>
        /// <param name="mappingItem"></param>
        /// <param name="entity"></param>
        /// <param name="entityDefinition"></param>
        /// <returns></returns>
        public static Item GetTemplateBasedOnRule(Item mappingItem, ImportEntityPipelineArgs entityArgs, CmpSettings cmpSettings, CmpHelper cmpHelper, ICmpConverterMapper cmpMapper)
        {
            return GetResultItem(Constants.EntityMappingTemplateFieldId, mappingItem, entityArgs, cmpSettings, cmpHelper, cmpMapper);
        }

        private static Item GetResultItem(Sitecore.Data.ID rulesFieldId,Item mappingItem, ImportEntityPipelineArgs entityArgs, CmpSettings cmpSettings, CmpHelper cmpHelper, ICmpConverterMapper cmpMapper)
        {
            String rule = mappingItem.Fields[rulesFieldId].Value;
            var rules = RuleFactory.ParseRules<EntityRuleContext>(mappingItem.Database, XElement.Parse(rule));

            var ruleContext = new EntityRuleContext()
            {
                CmpSettings = cmpSettings,
                CmpHelper=cmpHelper,
                CmpMapper= cmpMapper,
                EntityArgs = entityArgs,
                MappingItem = mappingItem
            };
            rules.Run(ruleContext);

            return ruleContext.ResulItem;
        }
    }
}