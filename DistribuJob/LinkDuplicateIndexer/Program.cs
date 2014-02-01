using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using Exo.Misc;
using System.IO;

namespace LinkDuplicateIndexer
{
    class Program
    {
        private const string DatabaseConnectionString = "Pipe=mysql; database=spider; uid=root; password=z090690sin; pooling=false;";

        private static MySqlConnection DatabaseConnection
        {
            get
            {
                MySqlConnection dbConn = new MySqlConnection(DatabaseConnectionString);
                dbConn.Open();

                return dbConn;
            }
        }

        static void Main(string[] args)
        {
            uint currentSourceDocId = 0;
            Dictionary<uint, string[]> linkIdToDesc = new Dictionary<uint, string[]>();

            using (MySqlCommand command = new MySqlCommand(
@"SELECT `id`,`sourceDoc`,`description`
FROM `links`
WHERE `type` IN (1,2)
AND `id` > 2285544
LIMIT 100", DatabaseConnection))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (currentSourceDocId != reader.GetUInt32(1))
                    {
                        if (linkIdToDesc.Count > 1)
                        {
                            StringBuilder sql = new StringBuilder();

                            foreach (KeyValuePair<uint, string[]> pair in linkIdToDesc)
                            {
                                int[] scores = new int[linkIdToDesc.Count - 1];
                                int scoresIndex = 0;

                                foreach (KeyValuePair<uint, string[]> cpair in linkIdToDesc)
                                {
                                    if (cpair.Key == pair.Key)
                                        continue;

                                    int lev = TextUtil.GetLevenshteinWordDistance(pair.Value, cpair.Value);

                                    if (lev == 0)
                                        scores[scoresIndex++] = 0;

                                    else
                                        scores[scoresIndex++] = (int)(((double)pair.Value.Length / (double)lev) * 100d);

                                    if (scores[scoresIndex - 1] < 0)
                                    {
                                        Console.WriteLine("problem (" + scores[scoresIndex - 1] + "): \r\ns={0}\r\nt={1}", String.Join(",", pair.Value), String.Join(",", cpair.Value));
                                    }
                                }

                                int scoresTotal = 0;

                                foreach (int score in scores)
                                {
                                    scoresTotal += score;
                                }

                                int overall = scoresTotal / scores.Length;

                                sql.Append("UPDATE links SET descRelativeDuplicate=" + overall + " WHERE id=" + pair.Key + ";" + Environment.NewLine);
                            }

                            sql = sql.Remove(sql.Length - 1, 1);

                            File.WriteAllText("sql/" + DateTime.Now.Ticks + ".sql", sql.ToString());
                        }

                        linkIdToDesc.Clear();

                        currentSourceDocId = reader.GetUInt32(1);
                    }

                    if (reader.IsDBNull(2) || reader.GetString(2) == String.Empty)
                        continue;

                    linkIdToDesc[reader.GetUInt32(0)] = TextUtil.ExtractWords(reader.GetString(2), true);
                }
            }
        }
    }
}
