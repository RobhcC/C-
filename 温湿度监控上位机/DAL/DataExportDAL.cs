using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WinformPractise.Model;

namespace WinformPractise.DAL
{
    // 数据导出数据访问层 - 负责数据的文件导出操作
    public class DataExportDAL
    {
        // 历史记录最大保存条数
        private const int MaxRecordCount = 1000;

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
