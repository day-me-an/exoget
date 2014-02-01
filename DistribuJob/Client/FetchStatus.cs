namespace DistribuJob.Client
{
    public enum FetchStatus : byte
    {
        NotFetched = 0,
        Success = 1,
        WarningHttpNotModified = 2,
        Disallowed = 3,
        ErrorHttpNotFound = 4,
        ErrorHttpOther = 5,
        ErrorIOTimedOut = 6,
        ErrorIOOther = 7,
        ErrorFormatUnknown = 8,
        ErrorMassReject = 9,
        ErrorUnknown = 10,
        ErrorIllegalRedirect = 11,
        ErrorIODnsResolutionFailed = 12,
        ErrorFormatNotImplemented = 13,
        ErrorFormatWrong = 14
    }
}
