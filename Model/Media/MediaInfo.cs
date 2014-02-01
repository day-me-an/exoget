using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Exo.Web;
using Exo.Exoget.Model.Search;
using Exo.Exoget.Model.User;
using System.Web.UI.HtmlControls;
using System.Linq;
using Exo.Exoget.Model.Feed;

namespace Exo.Exoget.Model.Media
{
    [Serializable]
    public class MediaInfo : PropertiesBase
    {
		private const string StaticPrefix = "http://static.exoget.com";
	
        public enum MediaQuality : byte
        {
            None = 0,
            Poor = 1,
            Ok = 2,
            Good = 3,
            Excellent = 4
        }

        private uint id, filesize, bitrate, duration, views;
        private ushort skey;
        private MediaType type;
        private DocumentType docType;
        private DocumentFormat docFormat;
        private string title, description, mediaUrl, sourceUrl, sourceHost, imageUrl;
        private Uri mediaUri;
        private MediaCommentInfo[] comments;
        private double rating;
        private FeedInfo feed;

        public MediaInfo(uint id, ushort auth)
        {
            this.id = id;
            this.skey = auth;
        }

        public MediaInfo()
        {
        }

        public uint Id
        {
            get { return id; }
            set { id = value; }
        }

        public ushort SKey
        {
            get { return skey; }
            set { skey = value; }
        }

        public string Title
        {
            get { return title ?? "Untitled"; }
            set { title = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public MediaType Type
        {
            get { return type; }
            set { type = value; }
        }

        public DocumentType DocType
        {
            get { return docType; }
            set { docType = value; }
        }

        public DocumentFormat DocFormat
        {
            get { return docFormat; }
            set { docFormat = value; }
        }

        /// <summary>
        /// If possible, returns a more commonly understood description of the format, instead of the enum name. eg: MP3 instead of MpegAudio3
        /// </summary>
        public string DocFormatFriendly
        {
            get
            {
                switch (docFormat)
                {
                    case DocumentFormat.MpegAudio3:
                        return "MP3";

                    case DocumentFormat.Realmedia:
                        return "RealMedia";

                    case DocumentFormat.MsMedia:
                        return "Windows";

                    case DocumentFormat.ThreeG:
                        return "3G";
                        
                    case DocumentFormat.MsAdvancedStreamRedirector:
                        return "ASX";

                    default:
                        return docFormat.ToString();
                }
            }
        }

        public string MediaUrl
        {
            get { return mediaUrl; }
            set { mediaUrl = value; }
        }

        public Uri MediaUri
        {
            get { return mediaUri ?? (mediaUri = new Uri(mediaUrl)); }
        }

        public uint MediaFileSize
        {
            get { return filesize; }
            set { filesize = value; }
        }

        public string SourceUrl
        {
            get { return sourceUrl; }
            set { sourceUrl = value; }
        }

        public string SourceHost
        {
            get
            {
                if (sourceUrl != null)
                {
                    if (sourceHost == null)
                        sourceHost = new Uri(sourceUrl, UriKind.Absolute).Host;

                    return sourceHost;
                }
                else
                    return MediaUri.Host;
            }
        }

        public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; }
        }

        public string ImagePath
        {
            get
            {
                if (imageUrl != null)
				{
					if(imageUrl[0] == '/')
						return StaticPrefix + imageUrl;
					
					else
						return imageUrl;
				}
                else if (feed != null)
                    return StaticPrefix + "/images/podcast.png";

                else
                {
                    switch (type)
                    {
                        case MediaType.Audio:
                            return StaticPrefix + "/images/audio.png";

                        case MediaType.Video:
                            return StaticPrefix + "/images/video.png";

                        default:
                            throw new ArgumentException();
                    }
                }
            }
        }

        public uint Bitrate
        {
            get { return bitrate; }
            set { bitrate = value; }
        }

        public MediaQuality Quality
        {
            get
            {
                if (MediaUri.Host == "lads.myspace.com" || MediaUri.Host == "www.youtube.com")
                    return MediaQuality.Good;

                else
                    return GetBitrateQuality(bitrate);
            }
        }

        public static MediaQuality GetBitrateQuality(uint bitrate)
        {
            if (bitrate == 0)
                return MediaQuality.None;

            else if (bitrate < 64000)
                return MediaQuality.Poor;

            else if (bitrate < 128000)
                return MediaQuality.Ok;

            else if (bitrate < 200000)
                return MediaQuality.Good;

            else
                return MediaQuality.Excellent;
        }

        public uint Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public TimeSpan DurationTimeSpan
        {
            get { return TimeSpan.FromSeconds((double)duration); }
        }

        public MediaCommentInfo[] Comments
        {
            get { return comments; }
            set { comments = value; }
        }

        public FeedInfo Feed
        {
            get { return feed; }
            set { feed = value; }
        }

        #region Statistics

        public uint Views
        {
            get { return views; }
            set { views = value; }
        }

        public double Rating
        {
            get { return rating; }
            set { rating = value; }
        }

        /// <summary>
        /// The rating expressed as a percentage
        /// </summary>
        public double RatingPercentage
        {
            get { return Math.Round(rating * 20); }
        }

        #endregion
    }
}