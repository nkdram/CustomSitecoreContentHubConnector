using System.Collections.Specialized;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;

namespace CMP.Connector.Request
{
    public class ExternalMediaRequest : MediaRequest
    {
        private HttpRequest innerRequest;
        private bool isRawUrlSafe;
        private bool isRawUrlSafeInitialized;
        private MediaUrlOptions mediaQueryString;
        private Sitecore.Resources.Media.MediaUri mediaUri;
        private MediaOptions options;

        protected override MediaOptions GetOptions()
        {
            NameValueCollection queryString = this.InnerRequest.QueryString;
            if ((queryString == null) || queryString.HasKeys())
            {
                options = new MediaOptions();
            }
            else
            {
                MediaUrlOptions mediaQueryString = this.GetMediaQueryString();
                options = new MediaOptions
                {
                    AllowStretch = mediaQueryString.AllowStretch,
                    BackgroundColor = mediaQueryString.BackgroundColor,
                    IgnoreAspectRatio = mediaQueryString.IgnoreAspectRatio,
                    Scale = mediaQueryString.Scale,
                    Width = mediaQueryString.Width,
                    Height = mediaQueryString.Height,
                    MaxWidth = mediaQueryString.MaxWidth,
                    MaxHeight = mediaQueryString.MaxHeight,
                    Thumbnail = mediaQueryString.Thumbnail
                };
                if (mediaQueryString.DisableMediaCache)
                {
                    options.UseMediaCache = false;
                }
                string[] strArray = queryString.AllKeys;
                for (int i = 0; i < strArray.Length; i = (int)(i + 1))
                {
                    string str = strArray[i];
                    if ((str != null) && (queryString.Get(str) != null))
                    {
                        options.CustomOptions[str] = queryString.Get(str);
                    }
                }
            }
            if (!this.IsRawUrlSafe)
            {
                if (Settings.Media.RequestProtection.LoggingEnabled)
                {
                    string urlReferrer = this.GetUrlReferrer();
                    Log.SingleError(string.Format("MediaRequestProtection: An invalid/missing hash value was encountered.The expected hash value: {0}. Media URL: {1}, Referring URL: {2}"
                        , HashingUtils.GetAssetUrlHash(this.InnerRequest.Path), this.InnerRequest.Path, string.IsNullOrEmpty(urlReferrer)
                        ? ((object)"(empty)") : ((object)urlReferrer)), this);
                }
                options = new MediaOptions();
            }

            this.ProcessCustomParameters(options);

            /// DAM Transforms
            if (!options.CustomOptions.ContainsKey("t"))
            {
                options.CustomOptions.Add("t", queryString.Get("t"));
            }
            else
            {
                options.CustomOptions["t"] = queryString.Get("t");
            }

            /// DAM Media Version
            if (!options.CustomOptions.ContainsKey("v"))
            {
                options.CustomOptions.Add("v", queryString.Get("v"));
            }
            else
            {
                options.CustomOptions["v"] = queryString.Get("v");
            }
            //DAM Unique Media Id
            if (!options.CustomOptions.ContainsKey("mid"))
            {
                options.CustomOptions.Add("mid", queryString.Get("mid"));
            }
            else
            {
                options.CustomOptions["mid"] = queryString.Get("mid");
            }

            return options;
        }

        public override MediaRequest Clone()
        {
            Assert.IsTrue((bool)(base.GetType() == typeof(ExternalMediaRequest)),
            "The Clone() method must be overridden to support prototyping.");
            return new ExternalMediaRequest
            {
                innerRequest = this.innerRequest,
                mediaUri = this.mediaUri,
                options = this.options,
                mediaQueryString = this.mediaQueryString
            };
        }
    }
}