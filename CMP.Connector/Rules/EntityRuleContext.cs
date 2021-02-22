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

namespace CMP.Connector.Rules
{
    /// <summary>
    /// Entity Rule Context
    /// </summary>
    public class EntityRuleContext : RuleContext
    {
        public ImportEntityPipelineArgs EntityArgs { get; set; }

        public CmpHelper CmpHelper { get; set; }
        public CmpSettings CmpSettings { get; set; }

        public ICmpConverterMapper CmpMapper { get; set; }
        public Item MappingItem { get; set; }

        /// <summary>
        /// Property used to map Folder/Templatee based on executed Action
        /// </summary>
        public Item ResulItem { get; set; }
    }
}