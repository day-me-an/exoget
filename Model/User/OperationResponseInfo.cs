using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Runtime.Serialization;

namespace Exo.Exoget.Model.User
{
    [DataContract]
    public class OperationResponseInfo
    {
        private OperationStatus status;
        private string message;

        public OperationResponseInfo(OperationStatus status, string message)
        {
            this.status = status;
            this.message = message;
        }

        public OperationResponseInfo()
        {
        }

        public OperationStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        /// <summary>
        /// The localized message
        /// </summary>
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        /// <summary>
        /// Easy conversion between a OperationStatus and its default OperationResponseInfo (containing the localized message)
        /// </summary>
        public static explicit operator OperationResponseInfo(OperationStatus status)
        {
            switch (status)
            {
                case OperationStatus.Success:
                    return new OperationResponseInfo(status, UserResources.Success);

                case OperationStatus.NotAuthenticated:
                    return new OperationResponseInfo(status, UserResources.NotAuthenticated);

                case OperationStatus.UnknownError:
                    return new OperationResponseInfo(status, UserResources.UnknownError);

                case OperationStatus.Duplicate:
                    return new OperationResponseInfo(status, UserResources.Duplicate);

                case OperationStatus.UnknownReference:
                    return new OperationResponseInfo(status, UserResources.UnknownReference);

                case OperationStatus.IntervalNotElapsed:
                    return new OperationResponseInfo(status, UserResources.IntervalNotElapsed);

                default:
                    throw new ArgumentOutOfRangeException("status", status, "No default available for");
            }
        }
    }
}
