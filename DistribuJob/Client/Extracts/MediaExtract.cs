using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Exo.Web;
using Exo.Exoget.Model.Search;
using DistribuJob.Client.Processors;

namespace DistribuJob.Client.Extracts
{
    [Serializable]
    public class MediaExtract : Extract
    {
        private MediaType mediaType;
        private string title, description, transcript, author, album, genre, copyright, year;
        private List<string> keywords;
        private uint bitrate, duration;
        internal int width, height;
        internal ImageInfo image;

        [NonSerialized]
        internal Id3Type id3Type;
        [NonSerialized]
        internal bool isVbr;

        public MediaExtract()
            : base()
        {
        }

        public override void AddIndexProperties()
        {
            TryAddIndexProperty(IndexPropertyType.Author, Author);
            TryAddIndexProperty(IndexPropertyType.Copyright, Copyright);
            TryAddIndexProperty(IndexPropertyType.Album, Album);
            TryAddIndexProperty(IndexPropertyType.Genre, Genre);
            TryAddIndexProperty(IndexPropertyType.Year, Year);
            TryAddIndexProperty(IndexPropertyType.Keyword, keywords);
        }

        public void SetImageFromFile(string filepath)
        {
            ImageManipulator.CreateImage(filepath, out image);

            File.Delete(filepath);
        }

        public override string ToString()
        {
            return String.Format("\n------------\nMediaType: {0} \nBitrate: {1}bps \nDuration: {2} \nTitle: {3} \nAuthor: {4} \nAlbum: {5} \nYear: {6} \nGenre: {7} \nDescription: {8} \nTranscript: {9} \nCopyright: {10} \nHasImage: {11} \nWidth: {12} \nHeight: {13} \n------------\n",
                mediaType,
                bitrate,
                TimeSpan.FromSeconds(duration),
                title,
                author,
                album,
                year,
                genre,
                description,
                transcript,
                copyright,
                HasImage,
                width,
                height);
        }

        public bool HasKeywords
        {
            get { return keywords != null && keywords.Count > 0; }
        }

        #region Properties

        public MediaType MediaType
        {
            get { return mediaType; }
            set { mediaType = value; }
        }

        internal Id3Type Id3Type
        {
            get { return id3Type; }
            set { id3Type = value; }
        }

        public bool IsVbr
        {
            get { return isVbr; }
            set { isVbr = value; }
        }

        public ImageInfo Image
        {
            get { return image; }
            set { image = value; }
        }

        public bool HasImage
        {
            get { return image != null; }
        }

        public uint Bitrate
        {
            get { return bitrate; }
            set { bitrate = value; }
        }

        public uint Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Description
        {
            get { return description; }

            set
            {
                if (value == null)
                    return;

                string[] descSegs = value.Split(' ');

                if (mediaType == MediaType.Audio && descSegs.Length == 10)
                {
                    foreach (string descSeg in descSegs)
                    {
                        if (descSeg.Length != 8)
                        {
                            description = value;
                            return;
                        }
                    }

                    description = null;
                }
                else
                    description = value;
            }
        }

        public string Transcript
        {
            get { return transcript; }
            set { transcript = value; }
        }

        public List<string> Keywords
        {
            get { return keywords ?? (keywords = new List<string>()); }
        }

        public string Author
        {
            get { return author; }
            set { author = value; }
        }

        public string Album
        {
            get { return album; }
            set { album = value; }
        }

        public string Genre
        {
            get { return genre; }
            set { genre = value; }
        }

        public string Year
        {
            get { return year; }
            set { year = value; }
        }

        public string Copyright
        {
            get { return copyright; }
            set { copyright = value; }
        }

        #endregion
    }

    internal enum Id3Type
    {
        Id3_1,
        Id3_2,
    }
}