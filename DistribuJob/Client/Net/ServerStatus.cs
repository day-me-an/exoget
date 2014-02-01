namespace DistribuJob.Client.Net
{
    public enum ServerStatus : byte
    {
        None = 0,
        Locked = 1,
        Disabled = 2,
        ErrorIODnsResolutionFailed = 3,
        ErrorIOTimedout = 4,
        ErrorMassReject = 5
    }
}
