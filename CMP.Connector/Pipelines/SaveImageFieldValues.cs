using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stylelabs.M.Sdk;
using Sitecore.Abstractions;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Stylelabs.M.Sdk.WebClient;
using Stylelabs.M.Sdk.WebClient.Authentication;
using System.Threading.Tasks;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using CMP.Connector.Models;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using CMP.Connector.Helpers;
using Sitecore.Connector.CMP;
using CMP.Connector.Pipelines.Media;
using Sitecore.Configuration;

namespace CMP.Connector.Pipelines
{
    public class SaveImageFieldValues : ImportEntityProcessor
    {
        private ImportEntityPipelineArgs Args
        {
            get;
            set;
        }
        private static CmpSettings _settings;

        private readonly BaseFactory _factory;

        private readonly IWebMClient _mClient;
        /// <summary>
        /// Load Connection String keys from CMP.ContentHub Connection
        /// </summary>
        private Dictionary<string, string> ConnectionStringKeys
        {
            get
            {
                string connectionString = Sitecore.Configuration.Settings.GetConnectionString("CMP.ContentHub");
                return connectionString.Split(';')
                        .Select(s => s.Split('='))
                        .ToDictionary(s => s.First(), s => s.Last());
            }
        }

        private string IconTransform
        {
            get { return Settings.GetSetting("ContentHub.IconTransform", "scIcon"); }
        }

        public SaveImageFieldValues(BaseFactory factory, BaseLog logger, IWebMClient mClient, CmpSettings settings) : base(logger, settings)
        {
            _factory = factory;
            _mClient = mClient;
            _settings = settings;
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            Assert.IsNotNull(args.Item, "The item is null.");
            Assert.IsNotNull(args.Language, "The language is null.");
            Args = args;
            var contentHubHost = ConnectionStringKeys["URI"];
            using (new SecurityDisabler())
            {
                using (new LanguageSwitcher(args.Language))
                {
                    try
                    {
                        int assetIndexField = 1;
                        args.Item.Editing.BeginEdit();
                        foreach (Item item in from i in args.EntityMappingItem.Children
                                              where i.TemplateID == Constants.ImageFieldMappingTypeID
                                              select i)
                        {
                            //var cmpFieldName = item[Sitecore.Connector.CMP.Constants.FieldMappingCmpFieldNameFieldId];
                            var sitecoreFieldName = item[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId];

                            if (!int.TryParse(item[Constants.FieldMappingSitecoreAssetIndexFieldID], out assetIndexField))
                            {
                                assetIndexField = 1;
                            }

                            var renditionField = item[Constants.FieldMappingSitecoreRenditionFieldID];
                            var publicLink = GetPublicLinkData(contentHubHost, args.Entity, renditionField, assetIndexField).GetAwaiter().GetResult();

                            if (publicLink != null && !string.IsNullOrEmpty(publicLink.URL))
                            {
                                //TODO : Providing empty URL on purpose - if not media id is not getting resolved
                                var imgElement = GetContentHubDamImageElement("", publicLink);
                                args.Item.Fields[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId].Value = imgElement;
                                //Having it static
                                if (!publicLink.URL.StartsWith("http"))
                                    publicLink.URL = (!string.IsNullOrEmpty(contentHubHost) ? contentHubHost : "http://sitename/") + publicLink.URL;
                                UriBuilder uri = new UriBuilder(publicLink.URL);
                                var query = HttpUtility.ParseQueryString(uri.Query);
                                args.Item.Fields[Constants.DAMImageTemplate.Fields.MediaID].Value = uri.Path.Split("/".ToCharArray()).Last();
                                args.Item.Fields[Constants.DAMImageTemplate.Fields.VersionID].Value = query["v"];

                                var mediaUrl = Sitecore.Resources.Media.MediaManager.GetMediaUrl(new MediaItem(args.Item));
                                // Generate Icon Url and set it 
                                args.Item.Fields[Constants.DAMImageTemplate.Fields.IconField].Value = GenerateIconUrl(mediaUrl, new Dictionary<string, string> { { "mid", uri.Path.Split("/".ToCharArray()).Last() }, { "v", query["v"] }, { "t", IconTransform } });
                            }
                        }
                    }
                    finally
                    {
                        args.Item.Editing.EndEdit();
                    }
                }
            }
        }

        private async Task<PublicLinkData> GetPublicLinkData(string host, IEntity entity, string rendition, int assetIndex)
        {
            var assetPublicLink = await PublicLinkHelper.GetorCreatePublicLink(_mClient, rendition, entity.Id.Value);
            Assert.IsNotNull(assetPublicLink, $"Could not get or create public link to asset id: {entity.Id.Value}.");

            var publicLink = PublicLinkHelper.GetPublicLinkData(host, entity, assetPublicLink, rendition);
            Assert.IsNotNull(publicLink, $"public link data is null.");

            return publicLink;
        }


        private string GetContentHubDamImageElement(string host, PublicLinkData publicLink)
        {
            return $"<image stylelabs-content-id=\"{publicLink.AssetId}\" thumbnailsrc=\"{host}/api/gateway/{publicLink.AssetId}/thumbnail\" src=\"{publicLink.URL}\" mediaid =\"\" stylelabs-content-type=\"{publicLink.ContentType.ToString()}\" alt=\"{publicLink.AltText}\" height=\"{publicLink.Height ?? ""}\" width=\"{publicLink.Width ?? ""}\" />";
        }

        private string GenerateIconUrl(string Url, Dictionary<string, string> Values)
        {
            string longurl = Url;
            if (!longurl.StartsWith("http"))
                longurl = (!string.IsNullOrEmpty(Sitecore.Context.Site?.HostName) ? Sitecore.Context.Site.HostName : "http://sitename/") + Url;

            var uriBuilder = new UriBuilder(longurl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            bool isTransformApplied = false;
            foreach (var key in Values.Keys)
            {
                if (!string.IsNullOrEmpty(Values[key]))
                {
                    isTransformApplied = !isTransformApplied ? key == "t" : isTransformApplied;
                    query[key] = Values[key];
                }
            }
            if (isTransformApplied)
            {
                query.Remove("w");
                query.Remove("h");
            }

            uriBuilder.Query = query.ToString();
            return Url.Split(new Char[] { '?' })[0] + uriBuilder.Query;
        }

    }
}