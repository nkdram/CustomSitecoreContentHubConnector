using Newtonsoft.Json.Linq;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Conversion;
using Sitecore.Connector.CMP.Helpers;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMP.Connector.Pipelines
{
    public class SaveFieldValues : ImportEntityProcessor
	{
		private static CmpSettings _settings;

		private readonly ICmpConverterMapper _mapper;

		private readonly CmpHelper _cmpHelper;

		public SaveFieldValues(ICmpConverterMapper mapper, BaseLog logger, CmpHelper cmpHelper, CmpSettings settings)
			: base(logger, settings)
		{
			_mapper = mapper;
			_settings = settings;
			_cmpHelper = cmpHelper;
		}

		public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
		{
			Assert.IsNotNull(args.Item, "The item is null.");
			Assert.IsNotNull(args.Language, "The language is null.");
			using (new SecurityDisabler())
			{
				using (new LanguageSwitcher(args.Language))
				{
					bool flag = false;
					try
					{
						args.Item.Editing.BeginEdit();
						args.Item[Constants.EntityIdentifierFieldId] = args.EntityIdentifier;
						flag = TryMapConfiguredFields(args);
					}
					catch
					{
						flag = false;
						throw;
					}
					finally
					{
						if (flag)
						{
							args.Item.Editing.EndEdit();
						}
						else
						{
							args.Item.Editing.CancelEdit();
							args.Item.Editing.BeginEdit();
							args.Item[Constants.EntityIdentifierFieldId] = args.EntityIdentifier;
							args.Item.Editing.EndEdit();
						}
					}
				}
			}
		}

		internal virtual bool TryMapConfiguredFields(ImportEntityPipelineArgs args)
		{
			if (args.EntityMappingItem == null)
			{
				args.EntityMappingItem = _cmpHelper.GetEntityMappingItem(args);
			}
			Assert.IsNotNull(args.EntityMappingItem, "Could not find any Entity Mapping item for the content type " + args.ContentTypeIdentifier);
			bool flag = false;
			foreach (Item item in args.EntityMappingItem.Children.Where((Item i) => i.TemplateID == Constants.FieldMappingTemplateId || i.TemplateID == Constants.RelationFieldMappingTemplateId))
			{
				string text = item[Constants.FieldMappingSitecoreFieldNameFieldId];
				string text2 = item[Constants.FieldMappingCmpFieldNameFieldId];
				if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
				{
					base.Logger.Error(BaseHelper.GetLogMessageText(_settings.LogMessageTitle, $"Configuration of the field mapping '{item.ID}' is incorrect. Required fields are not specified."), this);
					flag = true;
					continue;
				}
				try
				{
					if (item.TemplateID == Constants.RelationFieldMappingTemplateId)
					{
						string text3 = item[Constants.RelationFieldMappingCmpRelationFieldNameFieldId];
						if (string.IsNullOrEmpty(text3))
						{
							base.Logger.Error(BaseHelper.GetLogMessageText(_settings.LogMessageTitle, $"Configuration of the field mapping '{item.ID}' is incorrect. Required fields are not specified."), this);
							flag = true;
						}
						else
						{
							List<string> list = _cmpHelper.TryMapRelationPropertyValues(args, text3, text2);
							args.Item[text] = ((list.Count != 0) ? string.Join(_settings.RelationFieldMappingSeparator, list) : string.Empty);
						}
					}
					else if (text2.Contains("<")) // Option to filter entity by token value
                    {
						var propertyField = args.Entity.GetPropertyValue(text2.Split("<".ToCharArray()).FirstOrDefault()).ToString();
						var token = text2.Split('<', '>')[1];

						if(propertyField!=null && token != null)
                        {
							JObject propertyObj = JObject.Parse(propertyField);
							args.Item[text] = (string)propertyObj.SelectToken(token) ?? "";
						}
                    }
					else
					{
						args.Item[text] = _mapper.Convert(args.EntityDefinition, text2, args.Entity.GetPropertyValue(text2));
					}
				}
				catch (Exception exception)
				{
					base.Logger.Error(BaseHelper.GetLogMessageText(_settings.LogMessageTitle, $"An error occured during converting '{text2}' field to '{text}' field. Field mapping ID: '{item.ID}'."), exception, this);
					flag = true;
					args.Exception = exception;
				}
			}
			return !flag;
		}
	}
}