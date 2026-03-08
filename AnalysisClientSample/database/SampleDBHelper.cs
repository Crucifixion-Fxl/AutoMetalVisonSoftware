using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace AutoMetalDataBase
{
    public static class SampleDBHelper
    {
        private static string _connectionString;
        private static readonly object _lock = new object();

        public static void Initialize(string dbPath)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                lock(_lock)
                {
                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        _connectionString = $"Data Source={dbPath};Version=3;";
                        CreateDatabaseIfNotExists(dbPath);
                    }
                }
            }
        }

        public  static void CreateDatabaseIfNotExists(string dbPath)
        {
            if(!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                Console.WriteLine($"数据库已创建:{dbPath}");
                
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS Samples (
                            SampleId TEXT PRIMARY KEY,
                            BatchID INTEGER,
                            InternalNum INTEGER,
                            Coverage REAL DEFAULT 0.0,
                            OriginalImagePath TEXT,
                            ProcessedImagePath TEXT,
                            Uniformity REAL DEFAULT 0.0,
                            AbnormalitiesJson TEXT,
                            AbnormalImagePath TEXT,
                            UniformityAnalysisImagePath TEXT,
                            CoverageAnalysisImagePath TEXT,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";
                    ExecuteNonQuery(createTableSql);

                }

            }
        }

        public static void UpsertSample(SampleData sample)
        {
            string sql = @"
        INSERT OR REPLACE INTO Samples (
            SampleId,BatchID,InternalNum, Coverage, OriginalImagePath, ProcessedImagePath, 
            Uniformity, AbnormalitiesJson, AbnormalImagePath, 
            UniformityAnalysisImagePath, CoverageAnalysisImagePath
        ) VALUES (
            @SampleId, @BatchID, @InternalNum, @Coverage, @OriginalImagePath, @ProcessedImagePath, 
            @Uniformity, @AbnormalitiesJson, @AbnormalImagePath, 
            @UniformityAnalysisImagePath, @CoverageAnalysisImagePath
        )";

            var parameters = new[]
{
            new SQLiteParameter("@SampleId", sample.SampleId),
            new SQLiteParameter("@BatchID", sample.BatchID),
            new SQLiteParameter("@InternalNum", sample.InternalNum),
            new SQLiteParameter("@Coverage", sample.Coverage),
            new SQLiteParameter("@OriginalImagePath", sample.OriginalImagePath ?? (object)DBNull.Value),
            new SQLiteParameter("@ProcessedImagePath", sample.ProcessedImagePath ?? (object)DBNull.Value),
            new SQLiteParameter("@Uniformity", sample.Uniformity),
            new SQLiteParameter("@AbnormalitiesJson", JsonConvert.SerializeObject(sample.Abnormalities)),
            new SQLiteParameter("@AbnormalImagePath", sample.AbnormalImagePath ?? (object)DBNull.Value),
            new SQLiteParameter("@UniformityAnalysisImagePath", sample.UniformityAnalysisImagePath ?? (object)DBNull.Value),
            new SQLiteParameter("@CoverageAnalysisImagePath", sample.CoverageAnalysisImagePath ?? (object)DBNull.Value)
        };

            ExecuteNonQuery(sql, parameters);
        }


        public static List<SampleData> GetAllSamples()
        {
            var samples = new List<SampleData>();
            string sql = "SELECT * FROM Samples";

            using (var dt = ExecuteQuery(sql))
            {
                foreach (DataRow row in dt.Rows)
                {
                    samples.Add(MapDataRowToSampleData(row));
                }
            }

            return samples;
        }


        public static SampleData GetSampleById(string sampleId)
        {
            string sql = "SELECT * FROM Samples WHERE SampleId = @SampleId";
            var parameter = new SQLiteParameter("@SampleId", sampleId);

            using (var dt = ExecuteQuery(sql, parameter))
            {
                if (dt.Rows.Count > 0)
                {
                    return MapDataRowToSampleData(dt.Rows[0]);
                }
            }

            return null;
        }


        public static bool DeleteSample(string sampleId)
        {
            string sql = "DELETE FROM Samples WHERE SampleId = @SampleId";
            var parameter = new SQLiteParameter("@SampleId", sampleId);
            return ExecuteNonQuery(sql, parameter) > 0;
        }


        public static void UpdateSamplePartial(string sampleId, Action<SampleData> updateAction)
        {
            var sample = GetSampleById(sampleId);
            if (sample != null)
            {
                updateAction(sample);
                UpsertSample(sample);
            }
        }

        public static DataTable ExecuteQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }


        public static int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return command.ExecuteNonQuery();
                }
            }
        }

        public static List<SampleData> GetSamplesByDate(DateTime targetDate)
        {
            var samples = new List<SampleData>();

            // SQLite 使用 strftime 提取日期部分（兼容性写法）
            string sql = @"
        SELECT * FROM Samples 
        WHERE strftime('%Y', CreatedAt) = @Year 
          AND strftime('%m', CreatedAt) = @Month 
          AND strftime('%d', CreatedAt) = @Day";

            // 使用参数化查询防止SQL注入
            using (var dt = ExecuteQuery(sql,
                new SQLiteParameter("@Year", targetDate.Year.ToString("0000")),
                new SQLiteParameter("@Month", targetDate.Month.ToString("00")),
                new SQLiteParameter("@Day", targetDate.Day.ToString("00"))))
            {
                foreach (DataRow row in dt.Rows)
                {
                    samples.Add(MapDataRowToSampleData(row));
                }
            }

            return samples;
        }


        public static List<SampleData> GetSamplesByCoverageAndUniformity(double? coverage = null, double? uniformity = null)
        {
            var samples = new List<SampleData>();
            var sql = new StringBuilder("SELECT * FROM Samples WHERE 1=1");
            var parameters = new List<SQLiteParameter>();

            if (coverage.HasValue)
            {
                sql.Append(" AND Coverage >= @Coverage");
                parameters.Add(new SQLiteParameter("@Coverage", coverage.Value));
            }

            if (uniformity.HasValue)
            {
                sql.Append(" AND Uniformity >= @Uniformity");
                parameters.Add(new SQLiteParameter("@Uniformity", uniformity.Value));
            }

            using (var dt = ExecuteQuery(sql.ToString(), parameters.ToArray()))
            {
                foreach (DataRow row in dt.Rows)
                {
                    samples.Add(MapDataRowToSampleData(row));
                }
            }
            return samples;
        }

        private static SampleData MapDataRowToSampleData(DataRow row)
        {
            var abnormalitiesJson = row["AbnormalitiesJson"] as string;
            var abnormalities = !string.IsNullOrEmpty(abnormalitiesJson)
                ? JsonConvert.DeserializeObject<Dictionary<string, double>>(abnormalitiesJson)
                : new Dictionary<string, double>();

            return new SampleData
            {
                SampleId = row["SampleId"].ToString(),
                Coverage = Convert.ToDouble(row["Coverage"]),
                OriginalImagePath = row["OriginalImagePath"] as string,
                ProcessedImagePath = row["ProcessedImagePath"] as string,
                Uniformity = Convert.ToDouble(row["Uniformity"]),
                Abnormalities = abnormalities,
                AbnormalImagePath = row["AbnormalImagePath"] as string,
                UniformityAnalysisImagePath = row["UniformityAnalysisImagePath"] as string,
                CoverageAnalysisImagePath = row["CoverageAnalysisImagePath"] as string,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(row["UpdatedAt"])
            };
        }

        public static List<SampleData> GetSamplesByDateAndBatch(DateTime targetDate, int batchId)
        {
            var samples = new List<SampleData>();

            // SQLite 使用 strftime 提取日期部分，并添加批次号条件
            string sql = @"
        SELECT * FROM Samples 
        WHERE strftime('%Y', CreatedAt) = @Year 
          AND strftime('%m', CreatedAt) = @Month 
          AND strftime('%d', CreatedAt) = @Day
          AND BatchID = @BatchID";

            // 使用参数化查询防止SQL注入
            using (var dt = ExecuteQuery(sql,
                new SQLiteParameter("@Year", targetDate.Year.ToString("0000")),
                new SQLiteParameter("@Month", targetDate.Month.ToString("00")),
                new SQLiteParameter("@Day", targetDate.Day.ToString("00")),
                new SQLiteParameter("@BatchID", batchId)))
            {
                foreach (DataRow row in dt.Rows)
                {
                    samples.Add(MapDataRowToSampleData(row));
                }
            }

            return samples;
        }


        public class SampleData
        {
            public string SampleId { get; set; }
            public int BatchID { get; set; }
            public int InternalNum { get; set; }
            public double Coverage { get; set; } = 0.0;
            public string OriginalImagePath { get; set; }
            public string ProcessedImagePath { get; set; }
            public double Uniformity { get; set; } = 0.0;
            public Dictionary<string, double> Abnormalities { get; set; } = new Dictionary<string, double>();
            public string AbnormalImagePath { get; set; }
            public string UniformityAnalysisImagePath { get; set; }
            public string CoverageAnalysisImagePath { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }

            public string AbnormalitiesSummary
            {
                get
                {
                    if (Abnormalities == null || Abnormalities.Count == 0)
                        return "无异常";

                    return string.Join(", ", Abnormalities.Select(kv => $"{kv.Key}:{kv.Value}"));
                }
            }
        }

    }
}
