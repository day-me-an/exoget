namespace Exo.Exoget.Model.Search
{
    public enum IndexWordType : byte
    {
        None = 0,

        #region Media
        [IndexWordAttribute("title")]
        MediaTitle = 1,

        [IndexWordAttribute("desc")]
        MediaDescription = 2,

        [IndexWordAttribute("speech")]
        MediaTranscript = 3,

        [IndexWordAttribute("author")]
        MediaAuthor = 4,

        [IndexWordAttribute("album")]
        MediaAlbum = 5,

        [IndexWordAttribute("genre")]
        MediaGenre = 6,

        [IndexWordAttribute("copyright")]
        MediaCopyright = 7,

        [IndexWordAttribute("year")]
        MediaYear = 8,

        [IndexWordAttribute("keyword")]
        MediaKeyword = 9,

        [IndexWordAttribute("url")]
        MediaFilename = 32,

        /// <summary>
        /// The title part of a filename, eg: {author} - {title}.mp3
        /// </summary>
        [IndexWordAttribute("title")]
        MediaFilenameTitle = 33,

        /// <summary>
        /// The author part of a filename, eg: {author} - {title}.mp3
        /// </summary>
        [IndexWordAttribute("author")]
        MediaFilenameAuthor = 34,

        /// <summary>
        /// The author part of text in link that links to the media, eg: "{author} - Stan"
        /// </summary>
        [IndexWordAttribute("author")]
        MediaLinkTextAuthor = 35,
        #endregion

        #region Page
        [IndexWordAttribute("pagetitle")]
        PageTitle = 10,

        [IndexWordAttribute("pagetitle")]
        PageHeading = 11,

        [IndexWordAttribute("linktext")]
        PageLinkText = 12,

        [IndexWordAttribute("linktext")]
        PageLinkDescription = 13,

        [IndexWordAttribute("linktext")]
        EmbedLinkText = 14,

        [IndexWordAttribute("linktext")]
        EmbedLinkDescription = 15,
        #endregion


        #region Feed
        [IndexWordAttribute("channel")]
        FeedTitle = 16,

        [IndexWordAttribute("channel")]
        FeedDescription = 17,

        [IndexWordAttribute("channel")]
        FeedCopyright = 18,

        [IndexWordAttribute("channel")]
        FeedKeyword = 19,

        [IndexWordAttribute("channel")]
        FeedAuthor = 20,
        #endregion


        #region Feed Link
        [IndexWordAttribute("feed")]
        FeedLinkTitle = 21,

        [IndexWordAttribute("feed")]
        FeedLinkDescription = 22,

        [IndexWordAttribute("feed")]
        FeedLinkKeyword = 23,

        [IndexWordAttribute("author")]
        FeedLinkAuthor = 24,

        [IndexWordAttribute("feed")]
        FeedLinkCopyright = 25,
        #endregion

        #region Playlist
        [IndexWordAttribute("desc")]
        PlaylistTitle = 26,

        [IndexWordAttribute("desc")]
        PlaylistDescription = 27,

        [IndexWordAttribute("desc")]
        PlaylistLinkText = 28,

        [IndexWordAttribute("desc")]
        PlaylistLinkDescription = 29,
        #endregion

        #region Artficial Media
        [IndexWordAttribute("desc")]
        ArtificialMediaLinkText = 30,

        [IndexWordAttribute("desc")]
        ArtificialMediaLinkDescription = 31,
        #endregion
    }
}
