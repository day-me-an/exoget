namespace DistribuJob.Client.Extracts.Links
{
    public enum LinkType : byte
    {
        None = 0,

        /// <summary>
        /// Normal hyperlink on a HTML web page
        /// </summary>
        Page = 1,

        /// <summary>
        /// Same as LinkType.Page except its specifically for an embed, object or bgsound element
        /// </summary>
        Embed = 2,

        /// <summary>
        /// text, description, author, tags, duration - information that can be used to instantly create a record in `medias`
        /// </summary>
        ArtificialMedia = 3,

        /// <summary>
        /// Playlist item to media, {playlistitem} -> http://www.example.com/mp3s/public_transport.mp3 
        /// </summary>
        MediaPlaylist = 4,

        /// <summary>
        /// Feed item, eg {feeditem} -> http://www.example.com/mp3s/public_transport.mp3
        /// </summary>
        Feed = 5,

        /// <summary>
        /// eg: http://www.example.com/podcast/public_transport.html -> http://www.example.com/mp3s/public_transport.mp3
        /// </summary>
        FeedItemPageToMedia = 6,

        /// <summary>
        /// eg: http://www.example.com/podcast/index.html -> http://www.example.com/podcast.xml
        /// </summary>
        FeedChannelPageToFeed = 7
    }
}