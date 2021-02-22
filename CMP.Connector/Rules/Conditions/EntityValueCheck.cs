using Newtonsoft.Json.Linq;
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
    public class EntityValueCheck<T> : Sitecore.Rules.Conditions.StringOperatorCondition<T> where T : EntityRuleContext
    {
        public string Value { get; set; }
        public string Fieldname { get; set; }
        protected override bool Execute(T ruleContext)
        {
            Assert.ArgumentNotNull(ruleContext, "CMP Entity Value Rule Context not null");

            string entityValue = "";
            //Mapping inner fields using JObject select tokens
            if (Fieldname.Contains("<"))
            {
                var propertyField = ruleContext.EntityArgs.Entity.GetPropertyValue(Fieldname.Split("<".ToCharArray()).FirstOrDefault()).ToString();
                var token = Fieldname.Split('<', '>')[1];

                if (propertyField != null && token != null)
                {
                    JObject propertyObj = JObject.Parse(propertyField);
                    entityValue = (string)propertyObj.SelectToken(token) ?? "";
                }
            }
            else
                ///Creating an importy entity arg and assigning entity to manupulate relationship field TODO: Needs to updated in future
                entityValue = ruleContext.CmpMapper.Convert(ruleContext.EntityArgs.EntityDefinition, Fieldname, ruleContext.EntityArgs.Entity.GetPropertyValue(Fieldname));

            //Compares with the value provided
            return Compare(entityValue, Value);
        }
    }
}