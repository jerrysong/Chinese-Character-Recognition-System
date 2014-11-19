using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseCharacterRecognitionSystem
{
    class DatabaseProcessor
    {
        public static List<Character> LoadReferenceCharacterDatabase(string directory)
        {
            directory += @"Characters\";
            List<Character> referenceCharacters = new List<Character>();
            int referenceCharacterIndex = 0;
            List<char> fontList;
            List<Radical> radicalList;
            LoadCharacterFonts(out fontList, out radicalList);

            while (File.Exists(directory + referenceCharacterIndex.ToString() + ".csv"))
            {
                string fileName = directory + referenceCharacterIndex.ToString() + ".csv";
                Character referenceCharacter = new Character();
                using (CsvFileReader reader = new CsvFileReader(fileName))
                {
                    Stroke stroke = new Stroke(StrokeType.Online);
                    CsvRow row = new CsvRow();
                    while (reader.ReadRow(row))
                    {
                        if (IsIndexRow(row))
                        {
                            if (stroke.Count() != 0)
                            {
                                referenceCharacter.Add(stroke);
                            }
                            stroke = new Stroke(StrokeType.Online);
                            continue;
                        }
                        else if (IsPointsCountRow(row))
                        {
                            referenceCharacter.PointsCount = Convert.ToInt32(row[1].ToString());
                            break;
                        }

                        Point p = new Point();
                        p.X = Convert.ToInt32(row[0].ToString());
                        p.Y = Convert.ToInt32(row[1].ToString());
                        p.oritentationAngle = Convert.ToDouble(row[2].ToString());
                        p.type = StrokeType.Online;
                        stroke.Add(p);
                    }
                    referenceCharacter.Add(stroke);
                    referenceCharacter.Font = fontList[referenceCharacterIndex];
                    referenceCharacter.Radical = radicalList[referenceCharacterIndex];
                    referenceCharacters.Add(referenceCharacter);
                }
                referenceCharacterIndex++;
            }

            return referenceCharacters;
        }

        public static List<Character> LoadRadicalDatabase(string directory)
        {
            directory += @"Radicals\";
            List<Character> radicals = new List<Character>();
            int radicalIndex = 0;
            List<char> fontList;
            LoadRadicalFonts(out fontList);

            while (File.Exists(directory + radicalIndex.ToString() + ".csv"))
            {
                string fileName = directory + radicalIndex.ToString() + ".csv";
                Character referenceCharacter = new Character();
                using (CsvFileReader reader = new CsvFileReader(fileName))
                {
                    Stroke stroke = new Stroke(StrokeType.Online);
                    CsvRow row = new CsvRow();
                    while (reader.ReadRow(row))
                    {
                        if (IsIndexRow(row))
                        {
                            if (stroke.Count() != 0)
                            {
                                referenceCharacter.Add(stroke);
                            }
                            stroke = new Stroke(StrokeType.Online);
                            continue;
                        }
                        else if (IsPointsCountRow(row))
                        {
                            referenceCharacter.PointsCount = Convert.ToInt32(row[1].ToString());
                            break;
                        }

                        Point p = new Point();
                        p.X = Convert.ToInt32(row[0].ToString());
                        p.Y = Convert.ToInt32(row[1].ToString());
                        p.oritentationAngle = Convert.ToDouble(row[2].ToString());
                        p.type = StrokeType.Online;
                        stroke.Add(p);
                    }
                    referenceCharacter.Add(stroke);
                    referenceCharacter.Font = fontList[radicalIndex];
                    radicals.Add(referenceCharacter);
                }
                radicalIndex++;
            }

            return radicals;
        }

        public static List<Character> LoadTestCharacters(string direcotry, TestSampleBatch sampleBatch)
        {
            switch (sampleBatch)
            {
                case TestSampleBatch.TidyBatch:
                    direcotry += @"Samples\Tidy\";
                    break;
                case TestSampleBatch.CursiveBatch:
                    direcotry += @"Samples\Cursive\";
                    break;
                case TestSampleBatch.LigatureBatch:
                    direcotry += @"Samples\Ligature\";
                    break;
            }
            List<Character> testCharacters = new List<Character>();
            int testCharacterIndex = 0;
            List<char> fontList;
            LoadSampleFonts(out fontList);

            while (File.Exists(direcotry + testCharacterIndex.ToString() + ".csv"))
            {
                string fileName = direcotry + testCharacterIndex.ToString() + ".csv";
                Character testCharacter = new Character();
                using (CsvFileReader reader = new CsvFileReader(fileName))
                {
                    Stroke stroke = new Stroke(StrokeType.Online);
                    CsvRow row = new CsvRow();
                    while (reader.ReadRow(row))
                    {
                        if (IsIndexRow(row))
                        {
                            if (stroke.Count() != 0)
                            {
                                testCharacter.Add(stroke);
                            }
                            stroke = new Stroke(StrokeType.Online);
                            continue;
                        }
                        else if (IsPointsCountRow(row))
                        {
                            testCharacter.PointsCount = Convert.ToInt32(row[1].ToString());
                            break;
                        }

                        Point p = new Point();
                        p.X = Convert.ToInt32(row[0].ToString());
                        p.Y = Convert.ToInt32(row[1].ToString());
                        p.oritentationAngle = Convert.ToDouble(row[2].ToString());
                        p.type = StrokeType.Online;
                        stroke.Add(p);
                    }
                    testCharacter.Add(stroke);
                    testCharacter.Font = fontList[testCharacterIndex];
                    testCharacters.Add(testCharacter);
                }
                testCharacterIndex++;
            }

            return testCharacters;
        }

        public static void SaveToCharacterDatabase(string characterIndex, Character character, string directory)
        {
            using (CsvFileWriter writer = new CsvFileWriter(directory + characterIndex + ".csv"))
            {
                CsvRow row;
                int pointsCount = 0;
                for (int j = 0; j < character.StrokeCount(); j++)
                {
                    Stroke featureStroke = character[j];
                    pointsCount += featureStroke.Count();
                    if (j != 0)
                    {
                        int offLinePointsCount = Algorithms.GetOffLinePointsCount(character[j].First(), character[j - 1].Last());
                        pointsCount += offLinePointsCount;
                    }

                    row = new CsvRow();
                    row.Add(j.ToString());
                    writer.WriteRow(row);
                    for (int i = 0; i < featureStroke.Count(); i++)
                    {
                        row = new CsvRow();
                        row.Add(featureStroke[i].X.ToString());
                        row.Add(featureStroke[i].Y.ToString());
                        row.Add(featureStroke[i].oritentationAngle.ToString());
                        writer.WriteRow(row);
                    }
                }

                row = new CsvRow();
                row.Add("Points Count:");
                row.Add(pointsCount.ToString());
                writer.WriteRow(row);
            }
        }

        private static void LoadCharacterFonts(out List<char> fontList, out List<Radical> radicalList)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ChineseCharacterRecognitionSystem.Properties.Settings.DatabaseConnectionString"].ConnectionString;
            string sql = String.Format("select * from CD");
            OleDbConnection oleConnection = new OleDbConnection();
            oleConnection.ConnectionString = connectionString;
            oleConnection.Open();

            DataSet dataSet = new DataSet();
            OleDbDataAdapter adapter = new OleDbDataAdapter(sql, oleConnection);
            adapter.Fill(dataSet);
            oleConnection.Close();

            fontList = new List<char>();
            radicalList = new List<Radical>();
            DataTable dataTable = dataSet.Tables[0];
            foreach (DataRow row in dataTable.Rows)
            {
                DataColumn fontColumn = dataTable.Columns[1];
                string font = row[fontColumn].ToString();
                fontList.Add(font[0]);

                DataColumn radicalColumn = dataTable.Columns[3];
                string radical = row[radicalColumn].ToString();
                if (radical.Count() == 0)
                {
                    radicalList.Add(Radical.None);
                }
                else
                {
                    radicalList.Add(Algorithms.CharToRadical(radical[0]));
                }
            }
        }

        private static void LoadRadicalFonts(out List<char> radicalFontList)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ChineseCharacterRecognitionSystem.Properties.Settings.DatabaseConnectionString"].ConnectionString;
            string sql = String.Format("select * from RD");
            OleDbConnection oleConnection = new OleDbConnection();
            oleConnection.ConnectionString = connectionString;
            oleConnection.Open();

            DataSet dataSet = new DataSet();
            OleDbDataAdapter adapter = new OleDbDataAdapter(sql, oleConnection);
            adapter.Fill(dataSet);
            oleConnection.Close();

            radicalFontList = new List<char>();
            DataTable dataTable = dataSet.Tables[0];
            foreach (DataRow row in dataTable.Rows)
            {
                DataColumn fontColumn = dataTable.Columns[1];
                string font = row[fontColumn].ToString();
                radicalFontList.Add(font[0]);
            }
        }

        private static void LoadSampleFonts(out List<char> sampleFointList)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ChineseCharacterRecognitionSystem.Properties.Settings.DatabaseConnectionString"].ConnectionString;
            string sql = String.Format("select * from Samples");
            OleDbConnection oleConnection = new OleDbConnection();
            oleConnection.ConnectionString = connectionString;
            oleConnection.Open();

            DataSet dataSet = new DataSet();
            OleDbDataAdapter adapter = new OleDbDataAdapter(sql, oleConnection);
            adapter.Fill(dataSet);
            oleConnection.Close();

            sampleFointList = new List<char>();
            DataTable dataTable = dataSet.Tables[0];
            foreach (DataRow row in dataTable.Rows)
            {
                DataColumn fontColumn = dataTable.Columns[1];
                string font = row[fontColumn].ToString();
                sampleFointList.Add(font[0]);
            }
        }

        private static bool IsIndexRow(CsvRow row)
        {
            if (row.Count == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsPointsCountRow(CsvRow row)
        {
            if (row.Count == 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
