using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModbusRTU_TCP.BLL;
using ModbusRTU_TCP.Model;

namespace ModbusRTU_TCP
{
    public partial class Form1 : Form
    {
        private ModbusBLL modbusBll = new ModbusBLL();
        private System.Windows.Forms.Timer timerAutoSend;
        private bool isConnecting = false;
        private bool _isUserClosing = false;
        
        // 当前生效的读取参数（点击批量读取按钮后才更新）
        private ushort _currentStartAddr = 0;
        private ushort _currentReadCount = 2;

       public Form1()
        {
            InitializeComponent();
            InitModbusBllEvents();
            timerAutoSend = new System.Windows.Forms.Timer();
            timerAutoSend.Interval = 3000;
            timerAutoSend.Tick += TimerAutoSend_Tick;
        }

        // 初始化BLL层事件订阅
        private void InitModbusBllEvents()
        {
            modbusBll.OnLogAdded += AddLog;
            modbusBll.OnDataUpdated += UpdateModbusTable;
            modbusBll.OnDataCleared += ClearModbusTable;
            modbusBll.OnDataReceived += OnDataReceived;
            // 订阅批量数据接收事件
            modbusBll.OnBatchDataReceived += OnBatchDataReceived;
            // 订阅写入完成事件
            modbusBll.OnWriteCompleted += OnWriteCompleted;
        }

        // 批量数据接收事件处理
        private void OnBatchDataReceived(ushort startAddr, ushort[] values)
        {
            if (dgvModbus.InvokeRequired)
            {
                dgvModbus.Invoke(new Action<ushort, ushort[]>(OnBatchDataReceived), startAddr, values);
                return;
            }      
            // 更新表格显示批量数据
            UpdateBatchDataTable(startAddr, values);
        }

        // 写入完成事件处理
        private void OnWriteCompleted(ushort addr, ushort valueOrCount)
        {
            AddLog($"【写入确认】地址：{addr}，值/数量：{valueOrCount}");
        }

        // 更新批量数据表格
        private void UpdateBatchDataTable(ushort startAddr, ushort[] values)
        {
            // 清空现有行
            dgvModbus.Rows.Clear();
            
            // 添加批量数据行
            for (int i = 0; i < values.Length; i++)
            {
                ushort addr = (ushort)(startAddr + i);
                ushort value = values[i];
                
                // 根据地址判断数据类型
                string name = GetRegisterName(addr);
                string displayValue = FormatRegisterValue(addr, value);
                string status = GetRegisterStatus(addr, value);
                
                int rowIndex = dgvModbus.Rows.Add($"0x{addr:X4}", name, displayValue, status);
                
                // 设置状态列的颜色
                var statusCell = dgvModbus.Rows[rowIndex].Cells[3];
                SetStatusCellColor(statusCell, status);
            }
            
            // 调整行高
            if (dgvModbus.Rows.Count > 0)
            {
                int availableHeight = dgvModbus.Height - dgvModbus.ColumnHeadersHeight;
                int rowHeight = availableHeight / dgvModbus.Rows.Count;
                foreach (DataGridViewRow row in dgvModbus.Rows)
                {
                    row.Height = Math.Max(rowHeight, 25);
                }
            }
        }
        
        // 设置状态单元格颜色
        private void SetStatusCellColor(DataGridViewCell cell, string status)
        {
            switch (status)
            {
                case "高温预警":
                    cell.Style.BackColor = Color.Red;
                    cell.Style.ForeColor = Color.White;
                    break;
                case "低温预警":
                    cell.Style.BackColor = Color.Blue;
                    cell.Style.ForeColor = Color.White;
                    break;
                case "正常":
                    cell.Style.BackColor = Color.Green;
                    cell.Style.ForeColor = Color.White;
                    break;
                default:
                    cell.Style.BackColor = Color.LightGray;
                    cell.Style.ForeColor = Color.Black;
                    break;
            }
        }

        // 获取寄存器名称
        private string GetRegisterName(ushort addr)
        {
            switch (addr)
            {
                case 0: return "温度值";
                case 1: return "湿度值";
                case 2: return "运行状态";
                case 3: return "设定温度";
                case 4: return "设定湿度";
                default: return $"寄存器{addr}";
            }
        }

        // 格式化寄存器值
        private string FormatRegisterValue(ushort addr, ushort value)
        {
            switch (addr)
            {
                case 0:  // 温度（需要除以10）
                case 1:  // 湿度（需要除以10）
                    return (value / 10.0).ToString("0.0");
                default:
                    return value.ToString();
            }
        }

        // 获取寄存器状态
        private string GetRegisterStatus(ushort addr, ushort value)
        {
            if (addr == 0)  // 温度
            {
                double temp = value / 10.0;
                if (temp > 35) return "高温预警";
                if (temp < 10) return "低温预警";
                return "正常";
            }
            if (addr == 1)  // 湿度
            {
                double humidity = value / 10.0;
                if (humidity > 80) return "湿度偏高";
                if (humidity < 30) return "湿度偏低";
                return "正常";
            }
            if (addr == 2)  // 运行状态
            {
                return value == 1 ? "运行中" : "已停止";
            }
            return "-";
        }

        private void OnDataReceived()
        {
            timerTimeOut.Stop();
            modbusBll.ResetRetry();
            timerAutoSend.Start();
        }

        private void TimerAutoSend_Tick(object sender, EventArgs e)
        {
            if (modbusBll.CanSend)
            {
                btnSendData_Click(null, null);
            }
        }

        private void cboPortName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string Name = cboPortName.Text;
            AddLog($"将串口更改为{Name}");
        }

        private void cboBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            string BaudRate = cboBaudRate.Text;
            AddLog($"将波特率改为{BaudRate}");
        }
        //表格初始化
        private void InitModbusTable()
        {
            dgvModbus.Columns.Clear();
            dgvModbus.Rows.Clear();

            dgvModbus.Columns.Add("Addr", "寄存器地址");
            dgvModbus.Columns.Add("Name", "寄存器名称");
            dgvModbus.Columns.Add("Value", "数据值");
            dgvModbus.Columns.Add("Status", "设备状态");

            dgvModbus.Rows.Add("0x00", "温度值", "--", "正常");
            dgvModbus.Rows.Add("0x01", "湿度值", "--", "正常");
            dgvModbus.Rows.Add("0x02", "运行状态", "--", "Run");

            dgvModbus.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvModbus.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvModbus.BackgroundColor = this.BackColor;

            if (dgvModbus.Rows.Count > 0)
            {
                int available_height = dgvModbus.Height - dgvModbus.ColumnHeadersHeight;
                int row_height = available_height / dgvModbus.Rows.Count;

                foreach (DataGridViewRow row in dgvModbus.Rows)
                {
                    row.Height = row_height;
                }
                dgvModbus.RowTemplate.Height = row_height;
            }
        }
        //更新表格数据
        private void UpdateModbusTable(double temp, double humi, string status, Color color)
        {
            if (dgvModbus.InvokeRequired)
            {
                dgvModbus.Invoke(new Action(() => UpdateModbusTable(temp, humi, status, color)));
                return;
            }
            if (dgvModbus.Rows.Count < 3) return;
            dgvModbus.Rows[0].Cells["Value"].Value = temp.ToString("0.0");
            dgvModbus.Rows[1].Cells["Value"].Value = humi.ToString("0.0");
            dgvModbus.Rows[2].Cells["Value"].Value = status;
            dgvModbus.Rows[2].DefaultCellStyle.BackColor = color;
            dgvModbus.Rows[2].DefaultCellStyle.ForeColor = Color.White;
            UpdateExportButtons();
        }
        //清空表格数据
        private void ClearModbusTable()
        {
            if (dgvModbus.InvokeRequired)
            {
                dgvModbus.Invoke(new Action(() => ClearModbusTable()));
                return;
            }
            if (dgvModbus.Rows.Count < 3) return;
            dgvModbus.Rows[0].Cells["Value"].Value = "--";
            dgvModbus.Rows[1].Cells["Value"].Value = "--";
            dgvModbus.Rows[2].Cells["Value"].Value = "Run";
            dgvModbus.Rows[2].DefaultCellStyle.BackColor = this.BackColor;
            dgvModbus.Rows[2].DefaultCellStyle.ForeColor = Color.Black;
        }
        //协议选择
        private void cboProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProtocol.SelectedIndex == 0)
            {
                groupBoxSerial.Enabled = true;
                groupBoxTcp.Enabled = false;
                modbusBll.CloseTcp();
            }
            else
            {
                groupBoxSerial.Enabled = false;
                groupBoxTcp.Enabled = true;
                if (modbusBll.IsSerialPortOpen())
                {
                    modbusBll.CloseSerialPort();
                    timerReceive.Stop();
                }
            }
            btnSendData.Enabled = false;
            btnOpenSerial.Enabled = true;
            btnCloseSerial.Enabled = false;

            UpdateExportButtons();
        }
        //日志
        private void AddLog(string msg)
        {
            string time = DateTime.Now.ToString("yyyy/MM/dd:HH:mm:ss");
            string logMsg = $"{time}{msg}";
            
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action(() => 
                {
                    lstLog.Items.Add(logMsg);
                    lstLog.TopIndex = lstLog.Items.Count - 1;
                }));
            }
            else
            {
                lstLog.Items.Add(logMsg);
                lstLog.TopIndex = lstLog.Items.Count - 1;
            }
        }
        //窗体自加载
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cboPortName.Items.AddRange(ports);
            cboBaudRate.Items.AddRange(new string[] { "9600", "19200", "38400" });
            cboBaudRate.Text = "9600";

            btnCloseSerial.Enabled = false;
            btnSendData.Enabled = false;
            btnOpenSerial.Enabled = true;
            btnExportCsv.Enabled = false;
            btnExportTxt.Enabled = false;
            
            InitModbusTable();
        }
        //打开连接
        private void btnOpenSerial_Click(object sender, EventArgs e)
        {
            if (isConnecting)
            {
                AddLog("【提示】正在连接中，请稍候...");
                return;
            }

            try
            {
                if (cboProtocol.SelectedIndex == 0)
                {
                    if (string.IsNullOrEmpty(cboPortName.Text))
                    {
                        AddLog("【提示】请选择串口号");
                        return;
                    }
                    if (modbusBll.IsSerialPortOpen())
                    {
                        AddLog("【提示】串口已打开，请勿重复操作");
                        return;
                    }
                    modbusBll.InitSerialPort(cboPortName.Text, Convert.ToInt32(cboBaudRate.Text));
                    if (!modbusBll.OpenSerialPort())
                    {
                        return;
                    }
                    StartCommunication();
                }
                else
                {
                    if (string.IsNullOrEmpty(txtIP.Text) || string.IsNullOrEmpty(txtPort.Text))
                    {
                        AddLog("【错误】请填写IP地址和端口");
                        return;
                    }
                    
                    AddLog("【TCP】正在连接，请稍候...");
                    isConnecting = true;
                    btnOpenSerial.Enabled = false;
                    
                    string ip = txtIP.Text.Trim();
                    int port = int.Parse(txtPort.Text.Trim());
                    
                    Task.Run(async () =>
                    {
                        try
                        {
                            await ConnectTcpAsync(ip, port);
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() =>
                            {
                                AddLog($"【TCP连接异常】{ex.Message}");
                                isConnecting = false;
                                btnOpenSerial.Enabled = true;
                            }));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                if (cboProtocol.SelectedIndex == 0)
                {
                    AddLog($"【错误】串口打开失败{ex.Message}");
                }
                else
                {
                    AddLog($"【错误】TCP连接失败{ex.Message}");
                }
                isConnecting = false;
                btnOpenSerial.Enabled = true;
            }
        }

        private async Task ConnectTcpAsync(string ip, int port)
        {
            bool success = false;
            try
            {
                success = await modbusBll.ConnectTcpAsync(ip, port);
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() => AddLog($"【TCP】连接异常：{ex.Message}")));
            }

            this.Invoke(new Action(() =>
            {
                isConnecting = false;
                if (success)
                {
                    AddLog("【TCP】连接流程完成，准备启动接收...");
                    StartCommunication();
                }
                else
                {
                    AddLog("【错误】TCP连接失败，无法启动");
                    btnOpenSerial.Enabled = true;
                }
            }));
        }
         //连接成功后启动通信
        private void StartCommunication()
        {
            modbusBll.CanSend = true;
            modbusBll.ResetRetry();
            timerReceive.Start();
            timerAutoSend.Start();
            AddLog("【自动采集已启动】每3秒自动获取数据");

            UpdateExportButtons();
            cboPortName.Enabled = false;
            cboBaudRate.Enabled = false;
            btnCloseSerial.Enabled = true;
            btnOpenSerial.Enabled = false;
            btnSendData.Enabled = true;
        }

        private void btnCloseSerial_Click(object sender, EventArgs e)
        {
            try
            {
                _isUserClosing = true;
                timerReceive.Stop();
                timerAutoSend.Stop();
                timerTimeOut.Stop();
                AddLog("【自动采集已停止】");
                modbusBll.ResetRetry();

                if (modbusBll.IsSerialPortOpen())
                {
                    modbusBll.CloseSerialPort();
                }
                modbusBll.CloseTcp();
            }
            catch (Exception ex)
            {
                AddLog($"【错误】关闭失败{ex.Message}");
            }
            finally
            {
                UpdateExportButtons();
                cboPortName.Enabled = true;
                cboBaudRate.Enabled = true;
                btnOpenSerial.Enabled = true;
                btnCloseSerial.Enabled = false;
                btnSendData.Enabled = false;
                isConnecting = false;
                _isUserClosing = false;
            }
        }

        private async void btnSendData_Click(object sender, EventArgs e)
        {
            if (!modbusBll.CanSend)
            {
                AddLog("【禁止】设备离线，停止自动发送");
                return;
            }

            if (!modbusBll.IsConnected)
            {
                AddLog("【提示】连接已断开");
                timerTimeOut.Stop();
                modbusBll.IncrementRetry();
                if (modbusBll.RetryCount > modbusBll.MaxRetry)
                {
                    modbusBll.SetDeviceOffline();
                    AddLog($"【断线】重发{modbusBll.MaxRetry}次无响应，设备已断开！");
                    ClearModbusTable();
                    TryReconnect();
                }
                else
                {
                    AddLog($"【超时】第{modbusBll.RetryCount}次自动重发指令....");
                    timerTimeOut.Start();
                }
                return;
            }
            try
            {
                timerTimeOut.Stop();
                timerAutoSend.Stop();
                
                byte[] modbusCmd = modbusBll.BuildBatchReadCommand(_currentStartAddr, _currentReadCount);
                await modbusBll.SendByProtocolAsync(modbusCmd, cboProtocol.SelectedIndex);
                
                timerTimeOut.Start();
            }
            catch (Exception ex)
            {
                AddLog($"【错误】发送失败：{ex.Message}");
                ClearModbusTable();
                timerTimeOut.Start();
            }
        }

        private async void timerReceive_Tick(object sender, EventArgs e)
        {
            await modbusBll.ProcessReceiveDataAsync(cboProtocol.SelectedIndex);
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            lstLog.Items.Clear();
            AddLog("操作日志已清除");
        }

        private void timerTimeOut_Tick(object sender, EventArgs e)
        {
            timerTimeOut.Stop();
            if (_isUserClosing) return;
            modbusBll.IncrementRetry();
            if (modbusBll.RetryCount > modbusBll.MaxRetry)
            {
                modbusBll.SetDeviceOffline();
                AddLog($"【断线】重发{modbusBll.MaxRetry}次无响应，设备已断开！");
                ClearModbusTable();
                TryReconnect();
                return;
            }
            AddLog($"【超时】第{modbusBll.RetryCount}次自动重发指令....");
            btnSendData_Click(null, null);
        }

        private void TryReconnect()
        {
            if (_isUserClosing)
            {
                AddLog("【重连】用户已主动关闭连接，取消自动重连");
                return;
            }
            try
            {
                AddLog("【重连】检测到设备断线，正在尝试恢复通信...");
                modbusBll.CanSend = true;
                modbusBll.StopRetry();
                btnCloseSerial_Click(null, null);

                System.Threading.Thread.Sleep(100);

                btnOpenSerial_Click(null, null);
            }
            catch (Exception ex)
            {
                AddLog($"【重连失败】无法连接设备：{ex.Message}，将继续尝试重连");
            }
        }

        private bool IsDataValid() 
        {
            try
            {
                string temp = dgvModbus.Rows[0].Cells["Value"].Value.ToString();
                string humi = dgvModbus.Rows[1].Cells["Value"].Value.ToString();
                return double.TryParse(temp, out _) && double.TryParse(humi, out _);
            }
            catch (Exception ex) 
            {
                AddLog($"【异常】{ex.Message}");
                return false;
            }
        }

        private void UpdateExportButtons() 
        {
            bool hasProtocol = cboProtocol.SelectedIndex != -1;
            bool isConnected = modbusBll.IsConnected;
            bool dataValid = IsDataValid();

            btnExportTxt.Enabled = hasProtocol && dataValid && isConnected;
            btnExportCsv.Enabled = hasProtocol && dataValid && isConnected && modbusBll.HistoryCount > 0;
        }

        private async void btnExportTxt_Click(object sender, EventArgs e)
        {
            if (!IsDataValid())
            {
                AddLog("【导出失败】无有效温湿度数据，禁止导出！");
                return;
            }
            try
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.Filter = "文本文件 (*.txt)|*.txt";
                saveFile.FileName = $"温湿度历史数据_{DateTime.Now:yyyyMMddHHmmss}";
                saveFile.Title = "保存TXT文件";

                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    string path = saveFile.FileName;
                    if (await modbusBll.ExportToTxtAsync(path))
                    {
                        AddLog($"【导出成功】已导出{modbusBll.HistoryCount}条历史记录到：{path}");
                    }
                    else
                    {
                        AddLog("【导出失败】写入文件失败");
                    }
                }
                else
                {
                    AddLog("【导出取消】用户取消保存操作");
                }
            }
            catch (Exception ex)
            {
                AddLog($"【导出失败】TXT：{ex.Message}");
            }
        }

        private async void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (modbusBll.HistoryCount == 0)
            {
                AddLog("【导出失败】无历史数据可导出！");
                return;
            }
            try
            {
                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.Filter = "CSV表格文件 (*.csv)|*.csv";
                saveFile.FileName = $"温湿度历史数据_{DateTime.Now:yyyyMMddHHmmss}";
                saveFile.Title = "保存CSV文件";

                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    string path = saveFile.FileName;
                    if (await modbusBll.ExportToCsvAsync(path))
                    {
                        AddLog($"【导出成功】已导出{modbusBll.HistoryCount}条历史记录到：{path}");
                    }
                    else
                    {
                        AddLog("【导出失败】写入文件失败");
                    }
                }
                else
                {
                    AddLog("【导出取消】用户取消保存操作");
                }
            }
            catch (Exception ex)
            {
                AddLog($"【导出失败】CSV：{ex.Message}");
            }
        }

        #region 批量操作事件处理

        // 批量读取按钮点击事件
        private async void btnBatchRead_Click(object sender, EventArgs e)
        {
            try
            {
                if (!modbusBll.IsConnected)
                {
                    AddLog("【错误】请先建立连接");
                    return;
                }

                ushort startAddr = (ushort)numStartAddr.Value;
                ushort count = (ushort)numBatchCount.Value;
                _currentStartAddr = startAddr;
                _currentReadCount = count;
                modbusBll.SlaveAddress = (byte)numSlaveAddr.Value;

                AddLog($"【批量读取】从站：{numSlaveAddr.Value}，起始地址：{startAddr}，数量：{count}");

                if (await modbusBll.BatchReadAsync(startAddr, count, cboProtocol.SelectedIndex))
                {
                    timerTimeOut.Stop();
                    timerAutoSend.Stop();
                    timerTimeOut.Start();
                }
            }
            catch (Exception ex)
            {
                AddLog($"【批量读取异常】{ex.Message}");
            }
        }

        // 批量写入按钮点击事件
        private async void btnBatchWrite_Click(object sender, EventArgs e)
        {
            try
            {
                if (!modbusBll.IsConnected)
                {
                    AddLog("【错误】请先建立连接");
                    return;
                }

                ushort startAddr = (ushort)numStartAddr.Value;
                string valueText = txtBatchWriteValue.Text.Trim();

                if (string.IsNullOrEmpty(valueText))
                {
                    AddLog("【错误】请输入写入值，多个值用逗号分隔");
                    return;
                }

                string[] valueStrs = valueText.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs.Length == 0)
                {
                    AddLog("【错误】写入值格式错误");
                    return;
                }

                ushort count = (ushort)numBatchCount.Value;
                if (valueStrs.Length != count)
                {
                    AddLog($"【警告】写入值数量({valueStrs.Length})与设置数量({count})不匹配，将写入{valueStrs.Length}个值");
                }

                List<ushort> values = new List<ushort>();
                foreach (string str in valueStrs)
                {
                    if (ushort.TryParse(str.Trim(), out ushort val))
                    {
                        values.Add(val);
                    }
                    else
                    {
                        AddLog($"【错误】无效的写入值：{str}");
                        return;
                    }
                }

                DialogResult result = MessageBox.Show(
                    $"确认批量写入？\n起始地址：{startAddr}\n写入数量：{values.Count}\n写入值：{string.Join(", ", values)}",
                    "确认写入",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    AddLog("【取消】用户取消批量写入操作");
                    return;
                }

                AddLog($"【批量写入】从站：{numSlaveAddr.Value}，起始地址：{startAddr}，值：{string.Join(", ", values)}");

                if (await modbusBll.BatchWriteAsync(startAddr, values.ToArray(), cboProtocol.SelectedIndex))
                {
                    timerTimeOut.Stop();
                    timerAutoSend.Stop();
                    timerTimeOut.Start();
                }
            }
            catch (Exception ex)
            {
                AddLog($"【批量写入异常】{ex.Message}");
            }
        }

        #endregion
    }
}
