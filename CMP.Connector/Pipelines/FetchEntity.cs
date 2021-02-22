using CMP.Connector.Helpers;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;
using System;

namespace CMP.Connector.Pipelines
{
    public class FetchEntity : ImportEntityProcessor
    {
        private static CmpSettings _settings;

        private readonly BaseFactory _factory;

        private readonly IWebMClient _mClient;

        private readonly ID DefaultSitecoreLanguage = new ID("{21CA0142-2D9E-46CE-99E6-12ADEEBCD635}");

        public FetchEntity(BaseFactory factory, BaseLog logger, IWebMClient mClient, CmpSettings settings)
            : base(logger, settings)
        {
            _factory = factory;
            _mClient = mClient;
            _settings = settings;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            if (!args.BusMessage.UserProperties.ContainsKey("target_id") || !long.TryParse(args.BusMessage.UserProperties["target_id"].ToString(), out var entityId))
            {
                throw new ArgumentException("The message contains no valid target id.", "args");
            }
            args.ConfigItem = BaseHelper.GetConfigItem(_factory, _settings);
            int attemptsCount = 0;
            args.Entity = Retryer.Retry(delegate
            {
                logger.Info(BaseHelper.GetLogMessageText(_settings.LogMessageTitle, $"Getting an entity by id: {entityId}. Attempt #{++attemptsCount}."), this);
                return _mClient.Entities.GetAsync(entityId, EntityLoadConfiguration.Full);
            }, 5, 1000).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
                .GetResult();
            Assert.IsNotNull(args.Entity, $"The M-Entity (id: {entityId}) was not found in M.");
            args.EntityDefinition = _mClient.EntityDefinitions.GetAsync(args.Entity.DefinitionName).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
                .GetResult();
            Assert.IsNotNull(args.EntityDefinition, $"The entity definition '{args.Entity.DefinitionName}' of M-Entity (id: {entityId}) was not found in M.");
            args.EntityIdentifier = GetEntityIdentifier(args.Entity, _mClient);
            Assert.IsNotNullOrEmpty(args.EntityIdentifier, "Failed to get the M-Entity's identifier.");
            args.Language = GetLanguage(args.Entity, _mClient);
            if (args.Language == null && Language.TryParse(args.ConfigItem[DefaultSitecoreLanguage], out var result))
            {
                args.Language = result;
            }
            Assert.IsNotNull(args.Language, "Failed to get the language.");
            long? num = args.Entity.GetRelation<IChildToOneParentRelation>("ContentTypeToContent")?.Parent;
            if (num == null && args.Entity.Identifier.StartsWith("asset")) //if type asset
            {
                //Setting Content Type Identifier as M.Asset so that it can retrieve asset and map assset type in Sitecore
                args.ContentTypeIdentifier = Stylelabs.M.Sdk.Constants.Asset.DefinitionName;
            }
            else
            {
                Assert.IsNotNull(num, "Failed to get a parent ID of the ContentTypeToContent / AssetTypeToAsset relation (it is needed to detect the content type or asset type).");
                IEntity result2 = _mClient.Entities.GetAsync(num.Value, EntityLoadConfiguration.Default).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
                    .GetResult();
                Assert.IsNotNull(result2, "Failed to get a parent of the ContentTypeToContent / AssetTypeToAsset relation (it is needed to detect the content type or asset type).");
                args.ContentTypeIdentifier = result2.Identifier;
            }
        }

        private static string GetEntityIdentifier(IEntity entity, IWebMClient client)
        {
            long? num = entity.GetRelation<IChildToOneParentRelation>("ContentToContentLocalization")?.Parent;
            if (!num.HasValue)
            {
                return entity.Identifier;
            }
            IEntity result = client.Entities.GetAsync(num.Value, EntityLoadConfiguration.Default).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
                .GetResult();
            Assert.IsNotNull(result, string.Format("There is a {0} relation, but its parent (id: {1}) is null.", "ContentToContentLocalization", num.Value));
            return result.Identifier;
        }

        private static Language GetLanguage(IEntity entity, IWebMClient client)
        {
            long? num = entity.GetRelation<IChildToOneParentRelation>("LocalizationToContent")?.Parent;
            if (!num.HasValue)
            {
                return null;
            }
            IEntity result = client.Entities.GetAsync(num.Value, EntityLoadConfiguration.Full).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
                .GetResult();
            Assert.IsNotNull(result, string.Format("Could not fetch the {0} parent (id: {1}) (the language).", "LocalizationToContent", num.Value));
            string propertyValue = result.GetPropertyValue<string>("ValueName");
            Assert.IsNotNullOrEmpty(propertyValue, string.Format("There is a {0} parent (id: {1}), but it's ValueName property is null or empty.", "LocalizationToContent", num.Value));
            Assert.IsTrue(Language.TryParse(propertyValue, out var result2), "Failed to parse the language (" + propertyValue + ").");
            return result2;
        }
    }
}