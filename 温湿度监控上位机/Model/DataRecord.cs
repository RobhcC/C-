using System;

namespace WinformPractise.Model
{
    // 数据记录实体类 - 用于存储单条温湿度采集记录
    public class DataRecord
    {
        public DateTime CollectTime { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public string Status { get; set; }
        public DataRecord()
        {
            CollectTime = DateTime.Now;
            Status = "正常";
        }
        // 带参数的构造函数
        public DataRecord(double temperature, double humidity, string status)
        {
            CollectTime = DateTime.Now;
            Temperature = temperature;
            Humidity = humidity;
            Status = status;
        }
    }
}
