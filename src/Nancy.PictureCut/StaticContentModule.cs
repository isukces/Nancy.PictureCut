using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nancy.PictureCut
{
    // ReSharper disable once UnusedMember.Global
    public class StaticContentModule : NancyModule
    {
        #region Constructors

        public StaticContentModule()
        {
            Get[BaseUrl + "{item}"] = o => Get1(o.item);
            Get[BaseUrl + "images/{item}"] = o => Get1("images/" + o.item);
            Post[BaseUrl + "{item}"] = o => Get1(o.item);
        }

        static StaticContentModule()
        {
            BaseUrl = "/ImageCut/";
            Lock = new object();
            lock (Lock)
            {
                Cache = new Dictionary<string, Item>();
            }
        }

        #endregion Constructors

        #region Static Methods

        // Private Methods 

        private static DateTime TruncateToSeconds(DateTime dateTime)
        {
            return new DateTime(dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Kind);
        }

        #endregion Static Methods

        #region Methods

        // Private Methods 

        private bool CanSendNotModifiedResponse(string eTag, DateTime lastModified)
        {
            var haveIfNoneMatchCondition = false;
            var ifNoneMatch = Request.Headers.IfNoneMatch;
            if (ifNoneMatch != null && ifNoneMatch.Any())
            {
                haveIfNoneMatchCondition = true;
                if (Request.Headers.IfNoneMatch.All(x => x != eTag))
                    return false;
            }
            var ifModifiedSince = Request.Headers.IfModifiedSince;
            if (!ifModifiedSince.HasValue) return haveIfNoneMatchCondition; // false if no 'IfNoneMatch' and no 'IfModifiedSince' present
            lastModified = TruncateToSeconds(lastModified);
            return lastModified <= ifModifiedSince.Value;
        }

        private Stream EmbeddedResourceStream(string streamName)
        {
            return GetType().Assembly.GetManifestResourceStream("Nancy.ImageCut." + streamName.Replace("/", "."));
        }

        private object FromStream(string streamName, string contentType)
        {
            Item item;
            MemoryStream ms;
            lock (Lock)
            {
                Cache.TryGetValue(streamName, out item);
            }
            if (item != null)
            {
                if (CanSendNotModifiedResponse(item.Etag, item.ContentTimestampUtc))
                    return HttpStatusCode.NotModified;
            }


            else
            {

                var stream = EmbeddedResourceStream(streamName);
                if (stream == null)
                    throw new FileNotFoundException(streamName);

                using (ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    item = new Item(ms.ToArray());
                }
                lock (Lock)
                {
                    Cache[streamName] = item;
                }
            }
            ms = new MemoryStream(item.Data);

            var response = Response.FromStream(ms, contentType);
            response.Headers["ETag"] = item.Etag;
            response.Headers["Last-Modified"] = item.ContentTimestampUtc.ToString("R");
            return response;
        }

        private object Get1(string item)
        {
            item = (item ?? "").Trim().ToLower();
            if (item.EndsWith(".png"))
                return FromStream("Resources." + item, MimePng);
            switch (item)
            {
                case "jquery.picture.cut.js":
                    return GetMainJavascript();
                case "window.pc.js":
                    return FromStream("Resources." + item, MimeJavascript);
                case "jquery-ui-1.10.0.custom.css":
                    return FromStream("Resources." + item, MimeCss);
                case WindowBootstrap:
                    return FromStream("Resources." + item, "text/html");
            }
            return "kuku";
        }

        private object GetMainJavascript()
        {
            using (var stream = EmbeddedResourceStream("Js.jquery.picture.cut.js"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    var dict = new Dictionary<string, string>
                    {
                        {"src/windows/window.bootstrap.htm",   WindowBootstrap},
                        {"src/img/icon_add_image2.png",   "icon_add_image2.png"},
                        {"src/windows/core/window.pc.js", "window.pc.js"},
                        {"src/windows/JanelaBootstrap/jquery-ui-1.10.0.custom.css","jquery-ui-1.10.0.custom.css"}
                    };
                    result = dict.Aggregate(result, (current, pair) => current.Replace(pair.Key, BaseUrl.Substring(1) + pair.Value));
                    return Response.AsText(result, MimeJavascript);
                }
            }
        }

        #endregion Methods

        #region Static Fields

        private static readonly Dictionary<string, Item> Cache;
        private static readonly object Lock;

        #endregion Static Fields

        #region Fields

        const string MimeCss = "text/css";
        const string MimeJavascript = "text/javascript";
        const string MimePng = "image/png";
        // public const string Root = "/ImageCutContent";
        const string WindowBootstrap = "window.bootstrap.htm";

        #endregion Fields

        #region Static Properties

        /// <summary>
        /// Base Url for static content used by image.cut. Default /ImageCut/
        /// </summary>
        public static string BaseUrl { get; set; }

        public static string MainJs
        {
            get { return BaseUrl + "jquery.picture.cut.js"; }
        }

        #endregion Static Properties

        #region Nested Classes


        class Item
        {
            #region Constructors

            public Item(byte[] data)
            {
                Data = data;
                Etag = Guid.NewGuid().ToString("N");
                ContentTimestampUtc = DateTime.UtcNow;
            }

            #endregion Constructors

            #region Properties

            public DateTime ContentTimestampUtc { get; private set; }

            public byte[] Data { get; private set; }

            public string Etag { get; private set; }

            #endregion Properties
        }
        #endregion Nested Classes
    }
}
