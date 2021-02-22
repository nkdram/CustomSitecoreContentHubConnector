using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Links.UrlBuilders;
using Sitecore.Resources.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMP.Connector.Pipelines.Media
{
    public class ExternalMediaManager : BaseMediaManager
    {
        private readonly BaseMediaManager _mediaManager;
        public ExternalMediaManager(BaseMediaManager baseMediaManager)
        {
            _mediaManager = baseMediaManager;
        }

        public override MediaCache Cache { get => _mediaManager.Cache; set => _mediaManager.Cache = value; }
        public override MediaConfig Config { get => _mediaManager.Config; set => _mediaManager.Config = value; }
        public override MediaCreator Creator { get => _mediaManager.Creator; set => _mediaManager.Creator = value; }
        public override ImageEffects Effects { get => _mediaManager.Effects; set => _mediaManager.Effects = value; }

        private string ThumbNailTransform
        {
            get { return Settings.GetSetting("ContentHub.ThumbNailTransform", "w320"); }
        }

        public override string MediaLinkPrefix => _mediaManager.MediaLinkPrefix;

        public override MimeResolver MimeResolver { get => _mediaManager.MimeResolver; set => _mediaManager.MimeResolver = value; }

        //This Method appends #zoom to all pdf links
        public override string GetMediaUrl(MediaItem item, MediaUrlOptions options)
        {
            var url = _mediaManager.GetMediaUrl(item, options);
            //If Requested Media is of DAM image type
            if (item.InnerItem.TemplateID == Constants.DamImageTemplateID)
            {
                var versionId = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.VersionID]?.Value ?? string.Empty;
                var mediaID = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.MediaID]?.Value ?? string.Empty;
                var transform = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.Transform]?.Value ?? string.Empty;
                //var rendition = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.Rendition];
                return AppendQuerytoUrl(url, new Dictionary<string, string> { { "v", versionId }, { "mid", mediaID }, { "t", transform } });
            }

            return url;
        }
        public override string GetMediaUrl(MediaItem item, MediaUrlBuilderOptions options)
        {
            var url = _mediaManager.GetMediaUrl(item, options);
            //If Requested Media is of DAM image type
            if (item.InnerItem.TemplateID == Constants.DamImageTemplateID)
            {
                var versionId = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.VersionID]?.Value ?? string.Empty;
                var mediaID = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.MediaID]?.Value ?? string.Empty;
                var transform = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.Transform]?.Value ?? string.Empty;
                //var rendition = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.Rendition];
                return AppendQuerytoUrl(url, new Dictionary<string, string> { { "v", versionId }, { "mid", mediaID }, { "t", transform } });
            }

            return url;
        }
        public override string GetMediaUrl(MediaItem item)
        {
            var url = _mediaManager.GetMediaUrl(item);
            //If Requested Media is of DAM image type
            if (item.InnerItem.TemplateID == Constants.DamImageTemplateID)
            {
                var versionId = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.VersionID]?.Value ?? string.Empty;
                var mediaID = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.MediaID]?.Value ?? string.Empty;
                var transform = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.Transform]?.Value ?? string.Empty;
                //var rendition = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.Rendition];
                return AppendQuerytoUrl(url, new Dictionary<string, string> { { "v", versionId }, { "mid", mediaID }, { "t", transform } });

            }

            return url;
        }

        public override Sitecore.Resources.Media.Media GetMedia(MediaItem item)
        {
            return _mediaManager.GetMedia(item);
        }

        public override Sitecore.Resources.Media.Media GetMedia(MediaUri mediaUri)
        {
            return _mediaManager.GetMedia(mediaUri);
        }

        public override bool HasMediaContent(Item item)
        {
            return _mediaManager.HasMediaContent(item);
        }

        public override string GetThumbnailUrl(MediaItem item)
        {
            var url = _mediaManager.GetThumbnailUrl(item);
            //If Requested Media is of DAM image type
            if (item.InnerItem.TemplateID == Constants.DamImageTemplateID)
            {
                var versionId = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.VersionID]?.Value ?? string.Empty;
                var mediaID = item.InnerItem.Fields[Constants.DAMImageTemplate.Fields.MediaID]?.Value ?? string.Empty;
                //get media Id, versionId and thumbnail transform
                return string.Format("{0}?v={1}&mid={2}&t={3}", url, versionId, mediaID, ThumbNailTransform);
            }

            return url;
        }

        public override bool IsMediaRequest(HttpRequest httpRequest)
        {
            return _mediaManager.IsMediaRequest(httpRequest);
        }

        public override bool IsMediaUrl(string url)
        {
            return _mediaManager.IsMediaUrl(url);
        }

        public override MediaRequest ParseMediaRequest(HttpRequest request)
        {
            return _mediaManager.ParseMediaRequest(request);
        }

        private string AppendQuerytoUrl(string Url, Dictionary<string, string> Values)
        {
            string longurl = Url;
            if (!longurl.StartsWith("http"))
                longurl = (!string.IsNullOrEmpty(Sitecore.Context.Site?.HostName) ? Sitecore.Context.Site.HostName :  "http://sitename/") + Url;

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