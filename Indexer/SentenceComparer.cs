using System;
using System.Collections.Generic;
using System.Text;
using Exo.Exoget.Model.Search;

namespace DistribuJob.Indexer
{
    public class SentenceComparer : IComparer<SentenceInfo>
    {
        private readonly SentenceComparerType type;
        private IndexWordType exludeWordType;

        private bool hasSentencesOfType = false;

        public SentenceComparer(SentenceComparerType type)
        {
            this.type = type;
        }

        #region IComparer<SentenceInfo> Members

        public int Compare(SentenceInfo x, SentenceInfo y)
        {
            if (x.Type == y.Type)
                return (Score(x) + (x.Sentence.Length > y.Sentence.Length ? 0 : 1)) - ((y.Sentence.Length > x.Sentence.Length ? 0 : 1) + Score(y));

            else
                return Score(x) - Score(y);
        }

        #endregion

        private int Score(SentenceInfo sentence)
        {
            int score = Int32.MaxValue;

            switch (type)
            {
                case SentenceComparerType.Title:
                    {
                        switch (sentence.Type)
                        {
                            case IndexWordType.ArtificialMediaLinkText:
                                score = 0;
                                break;

                            case IndexWordType.FeedLinkTitle:
                                score = 1;
                                break;

                            case IndexWordType.MediaTitle:
                                score = 2;
                                break;

                            case IndexWordType.FeedLinkDescription:
                                score = 3;
                                break;

                            case IndexWordType.PlaylistLinkText:
                                score = 4;
                                break;

                            case IndexWordType.MediaFilenameTitle:
                                score = 5;
                                break;

                            case IndexWordType.MediaFilename:
                                score = 6;
                                break;

                            case IndexWordType.PageLinkText:
                                score = 7;
                                break;

                            case IndexWordType.PageTitle:
                                score = 8;
                                break;
                        }

                        break;
                    }

                case SentenceComparerType.Description:
                    {
                        switch (sentence.Type)
                        {
                            case IndexWordType.ArtificialMediaLinkDescription:
                                {
                                    if (exludeWordType == IndexWordType.ArtificialMediaLinkDescription)
                                        goto default;

                                    score = 0;
                                    break;
                                }

                            case IndexWordType.FeedLinkDescription:
                                {
                                    if (exludeWordType == IndexWordType.FeedLinkDescription)
                                        goto default;

                                    score = 1;
                                    break;
                                }

                            case IndexWordType.MediaTranscript:
                                {
                                    if (exludeWordType == IndexWordType.MediaTranscript)
                                        goto default;

                                    score = 2;
                                    break;
                                }

                            case IndexWordType.MediaDescription:
                                {
                                    if (exludeWordType == IndexWordType.MediaDescription)
                                        goto default;

                                    score = 3;
                                    break;
                                }

                            case IndexWordType.FeedDescription:
                                {
                                    if (exludeWordType == IndexWordType.FeedDescription)
                                        goto default;

                                    score = 4;
                                    break;
                                }

                            case IndexWordType.PlaylistLinkDescription:
                                {
                                    if (exludeWordType == IndexWordType.PlaylistLinkDescription)
                                        goto default;

                                    score = 5;
                                    break;
                                }

                            case IndexWordType.PageLinkText:
                                {
                                    if (exludeWordType == IndexWordType.PageLinkText)
                                        goto default;

                                    score = 6;
                                    break;
                                }

                            case IndexWordType.PageLinkDescription:
                                {
                                    if (exludeWordType == IndexWordType.PageLinkDescription)
                                        goto default;

                                    score = 7;
                                    break;
                                }

                            case IndexWordType.PageTitle:
                                {
                                    if (exludeWordType == IndexWordType.PageTitle)
                                        goto default;

                                    score = 8;
                                    break;
                                }

                            default:
                                {
                                    score = Int32.MaxValue;
                                    break;
                                }
                        }

                        break;
                    }

                case SentenceComparerType.Author:
                    {
                        switch (sentence.Type)
                        {
                            case IndexWordType.MediaAuthor:
                                score = 0;
                                break;

                            case IndexWordType.FeedLinkAuthor:
                                score = 1;
                                break;

                            case IndexWordType.FeedAuthor:
                                score = 2;
                                break;

                            case IndexWordType.MediaFilenameAuthor:
                                score = 3;
                                break;

                            case IndexWordType.MediaLinkTextAuthor:
                                score = 4;
                                break;
                        }

                        break;
                    }
            }

            if (score != Int32.MaxValue)
                hasSentencesOfType = true;

            return score;
        }

        public IndexWordType ExludeWordType
        {
            set { exludeWordType = value; }
            get { return exludeWordType; }
        }

        public bool HasSentencesOfType
        {
            get { return hasSentencesOfType; }
        }
    }

    public enum SentenceComparerType
    {
        Title,
        Description,
        Author
    }
}
