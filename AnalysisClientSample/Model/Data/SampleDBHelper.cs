using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoMetal.Data
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
            }

            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Samples (
                    SampleId TEXT PRIMARY KEY,
                    SampleType TEXT,
                    IterationNo INTEGER DEFAULT 0,
                    BatchNo INTEGER DEFAULT 0,
                    Coverage REAL DEFAULT 0.0,
                    Uniformity REAL DEFAULT 0.0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    OriImagePath TEXT,
                    CroppedImagePath TEXT,
                    HeatmapImagePath TEXT,
                    MaskImagePath TEXT,
                    OutputImagePath TEXT,
                    StandardImagePath TEXT
                )";
            ExecuteNonQuery(createTableSql);
            EnsureSchema();
        }

        private static void EnsureSchema()
        {
            EnsureColumnExists("SampleType", "TEXT");
            EnsureColumnExists("IterationNo", "INTEGER DEFAULT 0");
            EnsureColumnExists("BatchNo", "INTEGER DEFAULT 0");
            EnsureColumnExists("Coverage", "REAL DEFAULT 0.0");
            EnsureColumnExists("Uniformity", "REAL DEFAULT 0.0");
            EnsureColumnExists("CreatedAt", "DATETIME DEFAULT CURRENT_TIMESTAMP");
            EnsureColumnExists("UpdatedAt", "DATETIME DEFAULT CURRENT_TIMESTAMP");
            EnsureColumnExists("OriImagePath", "TEXT");
            EnsureColumnExists("CroppedImagePath", "TEXT");
            EnsureColumnExists("HeatmapImagePath", "TEXT");
            EnsureColumnExists("MaskImagePath", "TEXT");
            EnsureColumnExists("OutputImagePath", "TEXT");
            EnsureColumnExists("StandardImagePath", "TEXT");
        }

        private static void EnsureColumnExists(string columnName, string columnTypeDefinition)
        {
            string checkSql = "PRAGMA table_info(Samples)";
            using (var dt = ExecuteQuery(checkSql))
            {
                foreach (DataRow row in dt.Rows)
                {
                    var name = row["name"]?.ToString();
                    if (string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }
            }

            ExecuteNonQuery($"ALTER TABLE Samples ADD COLUMN {columnName} {columnTypeDefinition}");
        }

        public static void UpsertSample(SampleData sample)
        {
            string sql = @"
        INSERT OR REPLACE INTO Samples (
            SampleId, SampleType, IterationNo, BatchNo, Coverage, Uniformity,
            CreatedAt, UpdatedAt, OriImagePath, CroppedImagePath,
            HeatmapImagePath, MaskImagePath, OutputImagePath, StandardImagePath
        ) VALUES (
            @SampleId, @SampleType, @IterationNo, @BatchNo, @Coverage, @Uniformity,
            @CreatedAt, @UpdatedAt, @OriImagePath, @CroppedImagePath,
            @HeatmapImagePath, @MaskImagePath, @OutputImagePath, @StandardImagePath
        )";

            var parameters = new[]
            {
                new SQLiteParameter("@SampleId", sample.SampleId),
                new SQLiteParameter("@SampleType", sample.SampleType ?? (object)DBNull.Value),
                new SQLiteParameter("@IterationNo", sample.IterationNo),
                new SQLiteParameter("@BatchNo", sample.BatchNo),
                new SQLiteParameter("@Coverage", sample.Coverage),
                new SQLiteParameter("@Uniformity", sample.Uniformity),
                new SQLiteParameter("@CreatedAt", sample.CreatedAt == default(DateTime) ? DateTime.Now : sample.CreatedAt),
                new SQLiteParameter("@UpdatedAt", sample.UpdatedAt == default(DateTime) ? DateTime.Now : sample.UpdatedAt),
                new SQLiteParameter("@OriImagePath", sample.OriImagePath ?? (object)DBNull.Value),
                new SQLiteParameter("@CroppedImagePath", sample.CroppedImagePath ?? (object)DBNull.Value),
                new SQLiteParameter("@HeatmapImagePath", sample.HeatmapImagePath ?? (object)DBNull.Value),
                new SQLiteParameter("@MaskImagePath", sample.MaskImagePath ?? (object)DBNull.Value),
                new SQLiteParameter("@OutputImagePath", sample.OutputImagePath ?? (object)DBNull.Value),
                new SQLiteParameter("@StandardImagePath", sample.StandardImagePath ?? (object)DBNull.Value)
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
            return new SampleData
            {
                SampleId = GetString(row, "SampleId"),
                SampleType = GetString(row, "SampleType"),
                IterationNo = GetInt(row, "IterationNo"),
                BatchNo = GetInt(row, "BatchNo"),
                Coverage = GetDouble(row, "Coverage"),
                Uniformity = GetDouble(row, "Uniformity"),
                CreatedAt = GetDateTime(row, "CreatedAt"),
                UpdatedAt = GetDateTime(row, "UpdatedAt"),
                OriImagePath = GetString(row, "OriImagePath", "OriginalImagePath"),
                CroppedImagePath = GetString(row, "CroppedImagePath"),
                HeatmapImagePath = GetString(row, "HeatmapImagePath", "UniformityAnalysisImagePath"),
                MaskImagePath = GetString(row, "MaskImagePath", "CoverageAnalysisImagePath"),
                OutputImagePath = GetString(row, "OutputImagePath"),
                StandardImagePath = GetString(row, "StandardImagePath")
            };
        }

        private static string GetString(DataRow row, params string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                if (row.Table.Columns.Contains(name) && row[name] != DBNull.Value)
                {
                    return row[name].ToString();
                }
            }
            return string.Empty;
        }

        private static double GetDouble(DataRow row, string name)
        {
            if (!row.Table.Columns.Contains(name) || row[name] == DBNull.Value)
            {
                return 0.0;
            }
            return Convert.ToDouble(row[name]);
        }

        private static int GetInt(DataRow row, string name)
        {
            if (!row.Table.Columns.Contains(name) || row[name] == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToInt32(row[name]);
        }

        private static DateTime GetDateTime(DataRow row, string name)
        {
            if (!row.Table.Columns.Contains(name) || row[name] == DBNull.Value)
            {
                return DateTime.MinValue;
            }
            return Convert.ToDateTime(row[name]);
        }

        public static List<SampleData> GetSamplesByDateAndBatch(DateTime targetDate, int batchId)
        {
            var samples = new List<SampleData>();

            // SQLite 使用 strftime 提取日期部分（与 AutoMetal 创建字段一致，表无 BatchID 时仅按日期查询）
            string sql = @"
        SELECT * FROM Samples 
        WHERE strftime('%Y', CreatedAt) = @Year 
          AND strftime('%m', CreatedAt) = @Month 
          AND strftime('%d', CreatedAt) = @Day";

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


        public class SampleData
        {
            public string SampleId { get; set; }
            public string SampleType { get; set; }
            public int IterationNo { get; set; }
            public int BatchNo { get; set; }
            public double Coverage { get; set; } = 0.0;
            public string OriImagePath { get; set; }
            public string CroppedImagePath { get; set; }
            public double Uniformity { get; set; } = 0.0;
            public string HeatmapImagePath { get; set; }
            public string MaskImagePath { get; set; }
            public string OutputImagePath { get; set; }
            public string StandardImagePath { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }

            // 向后兼容旧字段命名（已有调用无需立即改动）
            public string OriginalImagePath { get => OriImagePath; set => OriImagePath = value; }
            public string UniformityAnalysisImagePath { get => HeatmapImagePath; set => HeatmapImagePath = value; }
            public string CoverageAnalysisImagePath { get => MaskImagePath; set => MaskImagePath = value; }
        }

    }
}
