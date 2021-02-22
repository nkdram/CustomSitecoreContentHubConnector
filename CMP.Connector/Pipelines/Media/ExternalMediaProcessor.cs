using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace CMP.Connector.Pipelines.Media
{
    public class ExternalMediaProcessor
    {
        private string Transform
        {
            get { return Settings.GetSetting("ContentHub.Transform", "t"); }
        }

        private string Version
        {
            get { return Settings.GetSetting("ContentHub.Version", "v"); }
        }

        private string MediaId
        {
            get { return Settings.GetSetting("ContentHub.MediaId", "mid"); }
        }

        private string ThumbNailTransform
        {
            get { return Settings.GetSetting("ContentHub.ThumbNailTransform", "w320"); }
        }

        private string ContentHubUri
        {
            get
            {
                var connectionSettings = Settings.GetConnectionString("CMP.ContentHub");
                ToConnectionDictionary(connectionSettings).TryGetValue("URI", out string contentHuburl);
                return contentHuburl;
            }
        }

        public IEnumerable<string> ValidMimeTypes
        {
            get
            {
                var validMimetypes = Settings.GetSetting("ContentHub.ValidMimeTypes", "image/jpeg|image/pjpeg|image/png|image/gif|image/tiff|image/bmp");
                return validMimetypes.Split(new[] { ",", "|", ";" }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public void Process(GetMediaStreamPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            if (!IsValidImageRequest(args.MediaData.MimeType))
                return;
            var transForm = GetQueryOrCustomOption(Transform, args.Options.CustomOptions);
            var version = GetQueryOrCustomOption(Version, args.Options.CustomOptions);
            var mediaId = GetQueryOrCustomOption(MediaId, args.Options.CustomOptions);

            //Return in case the version or media id is missing
            if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(mediaId))
                return;

            if (args.Options.Thumbnail)
                transForm = ThumbNailTransform;

            //if (args.OutputStream == null || !args.OutputStream.AllowMemoryLoading)
            //    return;

            try
            {
                var client = new WebClient();
                // Read image from ContentHub
                var result = client.OpenRead(string.Format(ContentHubUri + "/api/public/content/{0}?v={1}&t={2}", mediaId, version, transForm));

                if (result == null)
                    return;
                var stream = new MemoryStream();
                result.CopyTo(stream);
                args.OutputStream = new MediaStream(stream, args.MediaData.Extension, args.MediaData.MediaItem);
                args.AbortPipeline();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, this);
                return;
            }
        }

        private ImageFormat GetImageFormat(string mimeType)
        {
            switch (mimeType)
            {
                case "image/jpeg":
                    return ImageFormat.Jpeg;
                case "image/pjpeg":
                    return ImageFormat.Jpeg;
                case "image/png":
                    return ImageFormat.Png;
                case "image/gif":
                    return ImageFormat.Gif;
                case "image/tiff":
                    return ImageFormat.Tiff;
                    ;
                case "image/bmp":
                    return ImageFormat.Bmp;
                default:
                    return ImageFormat.Jpeg;
            }
        }

        protected bool IsValidImageRequest(string mimeType)
        {
            return ValidMimeTypes.Any(v => v.Equals(mimeType, StringComparison.InvariantCultureIgnoreCase));
        }

        protected string GetQueryOrCustomOption(string key, StringDictionary customOptions)
        {
            var value = WebUtil.GetQueryString(key);
            return string.IsNullOrEmpty(value) ? customOptions[key] : value;
        }

        private Dictionary<string, string> ToConnectionDictionary(string connectionString)
        {
            string[] source = connectionString.Split(';');
            return (from pair in source
                    select pair.Split('=') into kv
                    where kv.Length == 2
                    select kv).ToDictionary((string[] kv) => kv[0], (string[] kv) => kv[1]);
        }
    }
}