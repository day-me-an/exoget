using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exo.Web;
using DistribuJob.Client.Properties;
using DistribuJob.Client.Net;

namespace DistribuJob.Client.Processors
{
    partial class Fetcher
    {
        public override void QueueControl(Job job)
        {
            Console.WriteLine(job.FetchStatus);
            Console.WriteLine(job.Type);
            Console.WriteLine(job.Format);

            if (job.FetchStatus == FetchStatus.Success)
            {
                job.Server.consecutiveTimeouts = 0;
                job.Server.consecutiveLookupFailures = 0;

                /*if (!isSlowFetcher
                    && processDur > Settings.Default.Processor_Fetcher_SlowTime
                    && job.Server.consecutiveSlowFetches++ > Settings.Default.Processor_Fetcher_MaxSlow)
                {
                    Dj.Queues.fetcher.MoveAllJobs(job.Server, Dj.Queues.fetcherSlow);
                }
                else*/
                job.Server.consecutiveSlowFetches = 0;

                if (job.Type != DocumentType.Image)
                {
                    switch (job.Format)
                    {
                        case DocumentFormat.Html:
                        case DocumentFormat.Xml:
                        case DocumentFormat.MpegAudio3:
                        case DocumentFormat.Mpeg:
                        case DocumentFormat.Realmedia:
                        case DocumentFormat.MsMedia:
                            break;

                            /*
                        case DocumentFormat.Html:
                            {
                                if (job.ServerId != 653474)
                                    goto default;

                                else
                                    break;
                            }
                             */

                        default:
                            {
                                job.FetchStatus = FetchStatus.ErrorFormatNotImplemented;
                                Dj.Queues.exports.Enqueue(job);

                                return;
                            }
                    }
                }

                switch (job.Type)
                {
                    case DocumentType.Page:
                        {
                            Dj.Queues.htmlExtractor.Enqueue(job);
                            break;
                        }

                    case DocumentType.Feed:
                        {
                            Dj.Queues.feedExtractor.Enqueue(job);
                            break;
                        }

                    case DocumentType.MediaPlaylist:
                        {
                            if (job.Format != DocumentFormat.MpegPlaylist)
                            {
                                job.FetchStatus = FetchStatus.ErrorFormatNotImplemented;
                                Dj.Queues.exports.Enqueue(job);

                                return;
                            }
                            else
                                Dj.Queues.playlistExtractor.Enqueue(job);

                            break;
                        }

                    case DocumentType.Media:
                        {
                            switch (job.Format)
                            {
                                case DocumentFormat.MpegAudio3:
                                    Dj.Queues.wmfExtractor.Enqueue(job);
                                    break;

                                /*case DocumentFormat.MpegAudio3:
                                    Dj.Queues.id3Extractor.Enqueue(job);
                                    break;*/

                                //case DocumentFormat.Avi:
                                case DocumentFormat.Mpeg:
                                case DocumentFormat.FlashVideo:
                                    Dj.Queues.directshowExtractor.Enqueue(job);
                                    break;

                                case DocumentFormat.Realmedia:
                                case DocumentFormat.MsMedia:
                                    {
                                        if (job.ContentLength > 8192)
                                        {
                                            if (job.Format == DocumentFormat.MsMedia)
                                                Dj.Queues.wmfExtractor.Enqueue(job);

                                            else
                                                Dj.Queues.directshowExtractor.Enqueue(job);
                                        }
                                        else
                                            Dj.Queues.exports.Enqueue(job);

                                        break;
                                    }

                                case DocumentFormat.Aiff:
                                case DocumentFormat.Wave:
                                case DocumentFormat.Midi:
                                case DocumentFormat.Avi:
                                case DocumentFormat.Quicktime:
                                case DocumentFormat.Mpeg4:
                                case DocumentFormat.OggVorbis:
                                case DocumentFormat.ThreeG:
                                    Dj.Queues.quicktimeExtractor.Enqueue(job);
                                    break;

                                case DocumentFormat.Flash:
                                    Dj.Queues.swfExtractor.Enqueue(job);
                                    break;
                            }

                            break;
                        }

                    case DocumentType.Image:
                        Dj.Queues.imageManipulator.Enqueue(job);
                        break;

                    case DocumentType.Unknown:
                        Dj.Queues.exports.Enqueue(job);
                        break;
                }

                job.Server.IncrementSuccessfullRequests();
            }
            else
            {
                job.Extract = null;

                /*
                 * hack for artificial media sites that redirect when an item has been removed
                 */
                if (job.ServerId != 653474)
                    job.Server.IncrementUnsuccessfulRequests();
                /*
                 * end hack
                 */

                if (job.Server.consecutiveUnsuccessfulRequests > Settings.Default.Server_MaxConsecutiveUnsuccessfulRequests)
                {
                    Dj.Queues.fetcher.MoveAllJobs(job.Server, Dj.Queues.exports, FetchStatus.ErrorMassReject);
                    Dj.Servers.SetServerStatus(job.ServerId, ServerStatus.ErrorMassReject);
                }

                Dj.Queues.exports.Enqueue(job);
            }
        }

    }
}