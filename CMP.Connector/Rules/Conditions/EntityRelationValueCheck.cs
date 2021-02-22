using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Conversion;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Diagnostics;
using Sitecore.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMP.Connector.Rules
{
    public class EntityRelationValueCheck<T> : Sitecore.Rules.Conditions.StringOperatorCondition<T> where T : EntityRuleContext
    {
        public string Value { get; set; }
        public string Fieldname { get; set; }
        public string Relationname { get; set; }

        protected override bool Execute(T ruleContext)
        {
            Assert.ArgumentNotNull(ruleContext, "CMP Entity Value Rule Context not null");
           
            string entityValue = "";
            ///Creating an importy entity arg and assigning entity to manupulate relationship field TODO: Needs to updated in future
            var list = ruleContext.CmpHelper.TryMapRelationPropertyValues(ruleContext.EntityArgs, Relationname, Fieldname);

            if (list?.Count > 0)
                entityValue  = ((list.Count != 0) ? string.Join(ruleContext.CmpSettings.RelationFieldMappingSeparator, list) : string.Empty);

            //Compares with the value provided
            return Compare(entityValue, Value);
        }
    }
}