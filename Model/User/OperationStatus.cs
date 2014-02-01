namespace Exo.Exoget.Model.User
{
    /// <summary>
    /// Represents the status of a user operation, not to be confused with an exception
    /// </summary>
    public enum OperationStatus : byte
    {
        None = 0,

        /// <summary>
        /// Indicates the operation was a success
        /// </summary>
        Success = 1,

        /// <summary>
        /// Indicates an operation attempted to write data which would cause a duplicate (e.g: a comment being posted twice)
        /// </summary>
        Duplicate = 2,

        /// <summary>
        /// Indicates the user is not authenticated
        /// </summary>
        NotAuthenticated = 3,

        /// <summary>
        /// Indicates a parameter passed to an operation was invalid (e.g: an invalid user ids)
        /// </summary>
        UnknownReference = 4,

        /// <summary>
        /// An unknown error occured, usually an exception (e.g: the mysql server not responding)
        /// </summary>
        UnknownError = 5,

        /// <summary>
        /// Indicates a specified time span between an operation has not passed, e.g: prevent comment spam by limiting x comments per x
        /// </summary>
        IntervalNotElapsed = 6,

        /// <summary>
        /// Indicates the specified limit of something has been reached (e.g: comments per day)
        /// </summary>
        QuotaExceeded = 7,

        /// <summary>
        /// Indicates input validation failed
        /// </summary>
        NotValid = 8
    }
}