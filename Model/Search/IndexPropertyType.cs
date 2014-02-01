namespace Exo.Exoget.Model.Search
{
    public enum IndexPropertyType : byte
    {
        None = 0,

        Title = 1,
        Description = 2,
        Transcript = 3,
        Keyword = 9,

        Author = 4,
        Album = 5,
        Genre = 6,
        Copyright = 7,
        Year = 8,

        /// <summary>
        /// Publication date and time
        /// </summary>
        Pubdate = 10
    }
}