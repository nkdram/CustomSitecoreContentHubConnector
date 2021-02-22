using Sitecore.Data;
using Sitecore.Rules;
using Sitecore.Rules.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMP.Connector.Rules
{
    public class EntityFolderAction<T> : RuleAction<T> where T : EntityRuleContext
    {
        public ID Folderid { get; set; }
        public override void Apply(T ruleContext)
        {
            var db = ruleContext.MappingItem.Database;
            //sets Reultitem based on FolderId
            ruleContext.ResulItem = db.GetItem(Folderid);
        }
    }
}