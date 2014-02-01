using System;

namespace Exo.Exoget.Model.Search
{
    [Flags]
    public enum SearchOptions : uint
    {
        None = 0,

        QualityPoor = 1,
        QualityOk = 2,
        QualityGood = 4,
        QualityExcellent = 8,
        QualityAll = QualityPoor | QualityOk | QualityGood | QualityExcellent,

        FormatMp3 = 16,
        FormatMsMedia = 32,
        FormatRealmedia = 64,
        FormatQuicktime = 128,
        FormatMp4 = 256,
        FormatOther = 512,
        FormatAllExceptMp3 = FormatMsMedia | FormatRealmedia | FormatQuicktime | FormatMp4 | FormatOther,

        DurationOne = 1024,
        DurationTwo = 2048,
        DurationThree = 4096,
        DurationFour = 8192,
        DurationAll = DurationOne | DurationTwo | DurationThree | DurationFour,

        /// <summary>
        /// Uses the OR operator for query terms instead of the default AND
        /// </summary>
        OperatorOR = 16384,

        /// <summary>
        /// Represents all search options enabled, no extra criteria is used with this setting
        /// </summary>
        All = QualityAll | FormatMp3 | FormatMsMedia | FormatRealmedia | FormatQuicktime | FormatMp4 | DurationOne
    }
}