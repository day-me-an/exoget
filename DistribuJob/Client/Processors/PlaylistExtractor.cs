using System;
using System.Collections.Generic;
using System.Text;
using Exo.Collections;
using Exo.Web;
using DistribuJob.Client.Extracts;
using Exo.Media;
using DistribuJob.Client.Extracts.Links;
using Exo.Misc;

namespace DistribuJob.Client.Processors
{
    class PlaylistExtractor : Processor
    {
        public PlaylistExtractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            //asx  HIGHER   DONE
            //wvx  EASY     DONE
            //m3u  MODERATE DONE
            //pls  MODERATE DONE
            //smil HIGHER   NEARLY

            switch (job.Format)
            {
                case DocumentFormat.MsAdvancedStreamRedirector:
                    {
                        AsxParser parser = new AsxParser(job.FilePath);

                        AsxDocument doc;

                        if (!parser.TryParse(out doc))
                            goto case DocumentFormat.Pls;

                        else if (doc.Entries.Length == 0)
                            break;

                        foreach (AsxEntry entry in doc.Entries)
                        {
                            Uri uri = entry.Ref;

                            if (!UriUtil.TryValidateUri(ref uri))
                                continue;

                            ArtificialMediaLink mediaLink = new ArtificialMediaLink(LinkType.MediaPlaylist, uri);

                            mediaLink.Text = entry.Title;
                            mediaLink.Description = entry.Abstract;
                            mediaLink.MediaExtract.Author = entry.Author;
                            mediaLink.MediaExtract.Copyright = entry.Copyright;

                            job.MediaPlaylistExtract.LinkList.Add(mediaLink);
                        }

                        break;
                    }

                case DocumentFormat.Pls:
                    {
                        PlsParser parser = new PlsParser(job.FilePath);
                        PlsEntry[] entries = parser.Parse();

                        if (entries.Length == 0)
                            goto case DocumentFormat.MpegPlaylist;

                        foreach (PlsEntry entry in entries)
                        {
                            Uri uri = entry.Uri;

                            if (!UriUtil.TryValidateUri(ref uri))
                                continue;

                            ArtificialMediaLink mediaLink = new ArtificialMediaLink(LinkType.MediaPlaylist, uri);
                            mediaLink.Text = entry.Title;

                            job.MediaPlaylistExtract.LinkList.Add(mediaLink);
                        }

                        break;
                    }

                case DocumentFormat.MpegPlaylist:
                    {
                        MpegPlaylistParser parser = new MpegPlaylistParser(job.FilePath);
                        MpegPlaylistEntry[] entries = parser.Parse();

                        foreach (MpegPlaylistEntry entry in entries)
                        {
                            ArtificialMediaLink mediaLink = new ArtificialMediaLink(LinkType.MediaPlaylist, entry.Uri);
                            mediaLink.Text = entry.Title;

                            job.MediaPlaylistExtract.LinkList.Add(mediaLink);
                        }

                        break;
                    }
            }
        }

        public override void QueueControl(Job job)
        {
            Console.WriteLine(job.Extract);

            Dj.Queues.exports.Enqueue(job);
        }
    }
}