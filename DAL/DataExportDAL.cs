using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ModbusRTU_TCP.Model;

namespace ModbusRTU_TCP.DAL
{
    // 数据导出数据访问层 - 负责数据的文件导出操作
    public class DataExportDAL
    {
        // 历史记录最大保存条数
        private const int MaxRecordCount = 1000;
        private readonly string _dbFilePath;
        private readonly string _connectionString;
        private readonly object _dbLock = new object();

        public DataExportDAL()
        {
            _dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.db");
            _connectionString = $"Data Source={_dbFilePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            lock (_dbLock)
            {
                if (!File.Exists(_dbFilePath))
                {
                    SQLiteConnection.CreateFile(_dbFilePath);
                }

                using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    const string createSql = @"
                    CREATE TABLE IF NOT EXISTS DataRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CollectTime TEXT NOT NULL,
                        Temperature REAL NOT NULL,
                        Humidity REAL NOT NULL,
                        Status TEXT NOT NULL
                    );";
                    using (SQLiteCommand cmd = new SQLiteCommand(createSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        // Create
        public bool SaveRecordToSqlite(DataRecord record)
        {
            if (record == null) return false;

            try
            {
                lock (_dbLock)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();
                        const string insertSql = @"
                        INSERT INTO DataRecords (CollectTime, Temperature, Humidity, Status)
                        VALUES (@CollectTime, @Temperature, @Humidity, @Status);";

                        using (SQLiteCommand cmd = new SQLiteCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@CollectTime", record.CollectTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@Temperature", record.Temperature);
                            cmd.Parameters.AddWithValue("@Humidity", record.Humidity);
                            cmd.Parameters.AddWithValue("@Status", record.Status ?? "正常");
                            cmd.ExecuteNonQuery();
                            record.Id = conn.LastInsertRowId;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Read
        public List<DataRecord> GetAllRecordsFromSqlite()
        {
            var result = new List<DataRecord>();
            try
            {
                lock (_dbLock)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();
                        const string querySql = "SELECT Id, CollectTime, Temperature, Humidity, Status FROM DataRecords ORDER BY CollectTime DESC;";
                        using (SQLiteCommand cmd = new SQLiteCommand(querySql, conn))
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(MapRecord(reader));
                            }
                        }
                    }
                }
            }
            catch
            {
                // Keep silent and return existing results.
            }

            return result;
        }

        public DataRecord GetRecordById(long id)
        {
            if (id <= 0) return null;

            try
            {
                lock (_dbLock)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();
                        const string querySql = "SELECT Id, CollectTime, Temperature, Humidity, Status FROM DataRecords WHERE Id = @Id LIMIT 1;";
                        using (SQLiteCommand cmd = new SQLiteCommand(querySql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return MapRecord(reader);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public List<DataRecord> QueryRecordsByTimeRange(DateTime startTime, DateTime endTime)
        {
            var result = new List<DataRecord>();
            if (startTime > endTime) return result;

            try
            {
                lock (_dbLock)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();
                        const string querySql = @"
                        SELECT Id, CollectTime, Temperature, Humidity, Status
                        FROM DataRecords
                        WHERE CollectTime >= @StartTime AND CollectTime <= @EndTime
                        ORDER BY CollectTime DESC;";

                        using (SQLiteCommand cmd = new SQLiteCommand(querySql, conn))
                        {
                            cmd.Parameters.AddWithValue("@StartTime", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@EndTime", endTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    result.Add(MapRecord(reader));
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Keep silent and return existing results.
            }

            return result;
        }

        // Update
        public bool UpdateRecord(DataRecord record)
        {
            if (record == null || record.Id <= 0) return false;

            try
            {
                lock (_dbLock)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();
                        const string updateSql = @"
                        UPDATE DataRecords
                        SET CollectTime = @CollectTime, Temperature = @Temperature, Humidity = @Humidity, Status = @Status
                        WHERE Id = @Id;";

                        using (SQLiteCommand cmd = new SQLiteCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@CollectTime", record.CollectTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@Temperature", record.Temperature);
                            cmd.Parameters.AddWithValue("@Humidity", record.Humidity);
                            cmd.Parameters.AddWithValue("@Status", record.Status ?? "正常");
                            cmd.Parameters.AddWithValue("@Id", record.Id);
                            return cmd.ExecuteNonQuery() > 0;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // Delete
        public bool DeleteRecordById(long id)
        {
            if (id <= 0) return false;

            try
            {
                lock (_dbLock)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();
                        const string deleteSql = "DELETE FROM DataRecords WHERE Id = @Id;";
                        using (SQLiteCommand cmd = new SQLiteCommand(deleteSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            return cmd.ExecuteNonQuery() > 0;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public int DeleteRecordsByTimeRange(DateTime startTime, DateTime endTime)
        {
            if (startTime > endTime) return 0;

            try
            {
                lock (_dbLock)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();
                        const string deleteSql = @"
                        DELETE FROM DataRecords
                        WHERE CollectTime >= @StartTime AND CollectTime <= @EndTime;";

                        using (SQLiteCommand cmd = new SQLiteCommand(deleteSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@StartTime", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@EndTime", endTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            return cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        public int ClearAllRecords()
        {
            try
            {
                lock (_dbLock)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();
                        const string clearSql = "DELETE FROM DataRecords;";
                        using (SQLiteCommand cmd = new SQLiteCommand(clearSql, conn))
                        {
                            return cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        private static DataRecord MapRecord(SQLiteDataReader reader)
        {
            DateTime collectTime;
            DateTime.TryParse(reader["CollectTime"].ToString(), out collectTime);

            return new DataRecord
            {
                Id = Convert.ToInt64(reader["Id"]),
                CollectTime = collectTime,
                Temperature = Convert.ToDouble(reader["Temperature"]),
                Humidity = Convert.ToDouble(reader["Humidity"]),
                Status = reader["Status"].ToString()
            };
        }

        // 导出数据到TXT文本文件（异步）
        public async Task<bool> ExportToTxtAsync(List<DataRecord> records, string filePath)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    await sw.WriteLineAsync("===== 温湿度采集历史记录 =====");
                    await sw.WriteLineAsync($"导出时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    await sw.WriteLineAsync($"记录总数：{records.Count}");
                    await sw.WriteLineAsync("==============================");
                    
                    foreach (var record in records)
                    {
                        await sw.WriteLineAsync($"采集时间：{record.CollectTime:yyyy-MM-dd HH:mm:ss}");
                        await sw.WriteLineAsync($"温度：{record.Temperature:0.0} ℃");
                        await sw.WriteLineAsync($"湿度：{record.Humidity:0.0} %");
                        await sw.WriteLineAsync($"设备状态：{record.Status}");
                        await sw.WriteLineAsync("------------------------------");
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 导出数据到CSV表格文件（异步）
        public async Task<bool> ExportToCsvAsync(List<DataRecord> records, string filePath)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    await sw.WriteLineAsync("采集时间,温度(℃),湿度(%),设备状态");
                    
                    foreach (var record in records)
                    {
                        await sw.WriteLineAsync($"{record.CollectTime:yyyy-MM-dd HH:mm:ss},{record.Temperature:0.0},{record.Humidity:0.0},{record.Status}");
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 获取历史记录最大保存条数
        public int GetMaxRecordCount()
        {
            return MaxRecordCount;
        }
    }
}
