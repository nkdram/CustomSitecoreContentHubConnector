using Sitecore.Rules.Actions;
using Sitecore.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;

namespace CMP.Connector.Rules
{
    public class EntityTemplateAction<T> : RuleAction<T> where T : EntityRuleContext
    {
        public ID Templateid { get; set; }
        public override void Apply(T ruleContext)
        {
            var db = ruleContext.MappingItem.Database;
            //sets Reultitem based on FolderId
            ruleContext.ResulItem = db.GetItem(Templateid);
        }
    }
}