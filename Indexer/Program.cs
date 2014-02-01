using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using Exo.Misc;
using System.IO;
using Lucene.Net.Index;
using Exo.Exoget.Model.Search;
using System.Configuration;
using Lucene.Net.Store;
using SpellChecker.Net.Search.Spell;

namespace DistribuJob.Indexer
{
    class Program
    {
        internal static IndexWriter indexWriter;
        private static StringBuilder insertSql = new StringBuilder();
        private static List<string> doneMediaIds = new List<string>();
        private static uint done = 1;

        static MySqlConnection DatabaseConnection
        {
            get
            {
                MySqlConnection dbConn = new MySqlConnection(ConfigurationManager.ConnectionStrings["exoget"].ConnectionString);
                dbConn.Open();

                return dbConn;
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "Indexer " + System.Windows.Forms.Application.ProductVersion;

            indexWriter = new IndexWriter(new FileInfo(args[0]), new ExoLuceneAnalyzer(), true);
            indexWriter.SetMaxBufferedDocs(5000);
            indexWriter.SetMergeFactor(50);

            uint maxMediaId = 0;

            using (MySqlConnection conn = DatabaseConnection)
            using (MySqlCommand command = new MySqlCommand("SELECT AUTO_INCREMENT FROM information_schema.tables WHERE table_name = 'media'", conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                reader.Read();

                maxMediaId = reader.GetUInt32(0);
            }

            DateTime start = DateTime.Now;

            Export();

            using (MySqlConnection conn2 = DatabaseConnection)
            {
                for (uint i = maxMediaId - 1; i >= 0; i--)
                {
                    if (done % Settings.Default.CommitRegularity == 0)
                        Export();

                    Indexer indexer = new Indexer(i, insertSql, conn2);

                    if (indexer.Index())
                    {
                        doneMediaIds.Add(i.ToString());
                        done++;
                    }
                }
            }

            Export();

            Console.WriteLine("done indexing, now optimizing");

            indexWriter.Optimize();
            indexWriter.Close();

            Console.WriteLine("done search index {0} in {1} @ {2}",
                done,
                DateTime.Now - start,
                DateTime.Now);
        }

        /*
        private static void CreateSpellIndex(string path)
        {
            IndexReader indexReader = IndexReader.Open(indexWriter.GetDirectory());
            Lucene.Net.Store.Directory spellIndex = FSDirectory.GetDirectory(path, true);

            foreach (string field in SearchManager.SearchFields)
            {
                Dictionary dictionary = new LuceneDictionary(indexReader, field);
                SpellChecker.Net.Search.Spell.SpellChecker spellChecker = new SpellChecker.Net.Search.Spell.SpellChecker(spellIndex);
                spellChecker.IndexDictionary(dictionary);
            }

            spellIndex.Close();
        }
        */

        private static void Export()
        {
            if (doneMediaIds.Count > 0)
            {
                insertSql.AppendLine("UPDATE media SET status = 1 WHERE id IN (" + String.Join(",", doneMediaIds.ToArray()) + ");");
                insertSql.Append("COMMIT;");

                File.WriteAllText("sql/" + DateTime.Now.Ticks + ".sql", insertSql.ToString(), new UTF8Encoding(false));

                insertSql.Length = 0;
                doneMediaIds.Clear();

                Console.WriteLine("imported {0:#,##0} @ {1}",
                    done - 1,
                    DateTime.Now);
            }
            
            insertSql.AppendLine("SET AUTOCOMMIT=0;SET FOREIGN_KEY_CHECKS=0;");
        }
    }
}
