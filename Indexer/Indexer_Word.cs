using System;
using System.Collections.Generic;
using System.Text;
using Exo.Web;
using Exo.Exoget.Model.Search;
using DistribuJob.Client.Extracts.Links;
using Exo.Misc;

namespace DistribuJob.Indexer
{
    partial class Indexer
    {
        const int MinWordLength = 1;
        const int MaxWordLength = 12;

        private static IndexWordType GetWordType(DocumentType docType, IndexPropertyType propertyType)
        {
            switch (docType)
            {
                case DocumentType.Page:
                    {
                        switch (propertyType)
                        {
                            case IndexPropertyType.Title:
                                return IndexWordType.PageTitle;

                            case IndexPropertyType.Description:
                                return IndexWordType.PageHeading;
                        }

                        break;
                    }

                case DocumentType.Feed:
                    {
                        switch (propertyType)
                        {
                            case IndexPropertyType.Title:
                                return IndexWordType.FeedTitle;

                            case IndexPropertyType.Description:
                                return IndexWordType.FeedDescription;

                            case IndexPropertyType.Keyword:
                                return IndexWordType.FeedKeyword;

                            case IndexPropertyType.Author:
                                return IndexWordType.FeedAuthor;

                            case IndexPropertyType.Copyright:
                                return IndexWordType.FeedCopyright;
                        }

                        break;
                    }

                case DocumentType.Media:
                    {
                        switch (propertyType)
                        {
                            case IndexPropertyType.Title:
                                return IndexWordType.MediaTitle;

                            case IndexPropertyType.Description:
                                return IndexWordType.MediaDescription;

                            case IndexPropertyType.Transcript:
                                return IndexWordType.MediaTranscript;

                            case IndexPropertyType.Keyword:
                                return IndexWordType.MediaKeyword;

                            case IndexPropertyType.Author:
                                return IndexWordType.MediaAuthor;

                            case IndexPropertyType.Copyright:
                                return IndexWordType.MediaCopyright;

                            case IndexPropertyType.Album:
                                return IndexWordType.MediaAlbum;

                            case IndexPropertyType.Genre:
                                return IndexWordType.MediaGenre;

                            case IndexPropertyType.Year:
                                return IndexWordType.MediaYear;
                        }

                        break;
                    }

                case DocumentType.MediaPlaylist:
                    {
                        switch (propertyType)
                        {
                            case IndexPropertyType.Title:
                                return IndexWordType.PlaylistTitle;

                            case IndexPropertyType.Description:
                                return IndexWordType.PlaylistDescription;
                        }

                        break;
                    }
            }

            return IndexWordType.None;
        }

        private static IndexWordType GetWordType(LinkType linkType, IndexPropertyType propertyType)
        {
            switch (linkType)
            {
                case LinkType.Feed:
                    {
                        switch (propertyType)
                        {
                            case IndexPropertyType.Title:
                                return IndexWordType.FeedLinkTitle;

                            case IndexPropertyType.Description:
                                return IndexWordType.FeedLinkDescription;

                            case IndexPropertyType.Keyword:
                                return IndexWordType.FeedLinkKeyword;

                            case IndexPropertyType.Author:
                                return IndexWordType.FeedLinkAuthor;

                            case IndexPropertyType.Copyright:
                                return IndexWordType.FeedLinkCopyright;
                        }

                        break;
                    }

                case LinkType.Page:
                case LinkType.Embed:
                    {
                        switch (propertyType)
                        {
                            case IndexPropertyType.Title:
                                return IndexWordType.PageLinkText;

                            case IndexPropertyType.Description:
                                return IndexWordType.PageLinkDescription;
                        }

                        break;
                    }

                case LinkType.MediaPlaylist:
                    {
                        switch (propertyType)
                        {
                            case IndexPropertyType.Title:
                                return IndexWordType.PlaylistLinkText;

                            case IndexPropertyType.Description:
                                return IndexWordType.PlaylistLinkDescription;
                        }

                        break;
                    }

                case LinkType.ArtificialMedia:
                    {
                        switch (propertyType)
                        {
                            case IndexPropertyType.Title:
                                return IndexWordType.ArtificialMediaLinkText;

                            case IndexPropertyType.Description:
                                return IndexWordType.ArtificialMediaLinkDescription;
                        }

                        break;
                    }
            }

            return IndexWordType.None;
        }

        private void AddWord(string sentence, IndexWordType type)
        {
            if (String.IsNullOrEmpty(sentence) || Convert.IsDBNull(sentence) || type == IndexWordType.None)
                return;

            sentence = TextUtil.RemoveUrlAndEmail(sentence);

            sentenceSet.Add(new SentenceInfo(sentence, type));
        }

        private void AddWord(string words, IndexPropertyType type, DocumentType docType)
        {
            AddWord(words, GetWordType(docType, type));
        }

        private void AddWord(string words, IndexPropertyType type, LinkType linkType)
        {
            AddWord(words, GetWordType(linkType, type));
        }

        private void AddKeyword(string keyword, IndexWordType wordType)
        {
            keywordSet.Add(keyword);
            AddWord(keyword, wordType);
        }

        private void AddKeyword(string keyword, DocumentType docType)
        {
            AddKeyword(keyword, GetWordType(docType, IndexPropertyType.Keyword));
        }

        private void AddKeyword(string keyword, LinkType linkType)
        {
            AddKeyword(keyword, GetWordType(linkType, IndexPropertyType.Keyword));
        }
    }
}
