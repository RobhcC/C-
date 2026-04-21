using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading.Tasks;
using ModbusRTU_TCP.Model;
using ModbusRTU_TCP.DAL;

namespace ModbusRTU_TCP.BLL
{
    // 设备连接状态枚举
    public enum DeviceStatus
    {
        Offline,     // 离线
        Online       // 在线
    }

    // Modbus异常类型枚举
    public enum ModbusExceptionType
    {
        None,                   // 无异常
        BusinessException,      // 业务异常（Modbus协议异常码）
        CommunicationException  // 通信异常（超时/断连）
    }

    // Modbus异常信息类 - 封装异常类型、异常码和描述
    public class ModbusExceptionInfo
    {
        public ModbusExceptionType ExceptionType { get; set; }
        public byte ExceptionCode { get; set; }
        public string Message { get; set; }

        public ModbusExceptionInfo(ModbusExceptionType type, byte code, string message)
        {
            ExceptionType = type;
            ExceptionCode = code;
            Message = message;
        }
    }

    // Modbus业务逻辑层 - 负责Modbus通信的核心业务逻辑处理
    // 包括串口通信、TCP通信、数据解析、超时重发、批量读写、指数退避重连、异常分类等功能
    public class ModbusBLL : IDisposable
    {
        #region 常量配置

        // 重试配置
        private const int MAX_RETRY_COUNT = 3;

        // Modbus协议限制
        private const int MAX_READ_REGISTER_COUNT = 125;   // 读取寄存器最大数量
        private const int MAX_WRITE_REGISTER_COUNT = 123;  // 写入寄存器最大数量

        // TCP配置
        private const int TCP_TIMEOUT = 5000;              // TCP超时时间(毫秒)
        private const int TCP_BUFFER_SIZE = 1024;          // TCP缓冲区大小

        // 指数退避重连配置
        private const int EXPONENTIAL_BACKOFF_BASE_MS = 1000;   // 基础等待时间(毫秒)
        private const int EXPONENTIAL_BACKOFF_MAX_MS = 30000;   // 最大等待时间(毫秒)

        #endregion

        #region 私有字段

        // 串口通信对象
        private SerialPort serialPort = new SerialPort();
        // TCP套接字对象
        private Socket tcpSocket = null;
        // TCP接收缓冲区
        private byte[] tcpBuffer = new byte[TCP_BUFFER_SIZE];
        // 当前重试次数
        private int _retryCount = 0;
        // 设备状态
        private DeviceStatus _deviceStatus = DeviceStatus.Offline;
        // 是否允许发送数据标志
        private bool _canSend = true;
        // 历史数据记录列表
        private List<DataRecord> historyRecords = new List<DataRecord>();
        // 数据导出DAL对象
        private DataExportDAL dataExportDAL;
        // 当前从站地址（默认1）
        private byte _slaveAddress = 0x01;
        // 当前读取的起始地址（用于解析响应时确定数据对应的寄存器）
        private ushort _currentStartAddr = 0x0000;
        // 资源释放标记
        private bool _disposed = false;
        // 最近一次Modbus异常信息
        private ModbusExceptionInfo _lastException = null;
        // 指数退避重连定时器
        private System.Threading.Timer _reconnectTimer = null;
        // 当前重连尝试次数
        private int _reconnectAttempt = 0;
        // 是否正在重连中
        private bool _isReconnecting = false;

        #endregion

        #region 事件定义
        
        // 日志添加事件 - 当有新日志需要显示时触发
        public event Action<string> OnLogAdded;
        // 数据更新事件 - 当成功解析数据后触发
        public event Action<double, double, string, System.Drawing.Color> OnDataUpdated;
        // 数据清除事件 - 当需要清除显示数据时触发
        public event Action OnDataCleared;
        // 数据接收完成事件 - 当成功接收到有效数据时触发
        public event Action OnDataReceived;
        // 批量数据更新事件 - 当批量读取数据成功时触发
        public event Action<ushort, ushort[]> OnBatchDataReceived;
        // 写入完成事件 - 当批量写入成功时触发
        public event Action<ushort, ushort> OnWriteCompleted;
        // Modbus异常事件 - 当发生业务异常或通信异常时触发
        public event Action<ModbusExceptionInfo> OnModbusException;
        // 重连尝试事件 - 当指数退避重连尝试时触发
        public event Action<int> OnReconnectAttempt;
        
        #endregion

        #region 构造函数

        public ModbusBLL()
        {
            dataExportDAL = new DataExportDAL();
            dataExportDAL.OnDbLog += (msg) => AddLog(msg);
        }

        #endregion

        #region 公共属性
        
        // 获取当前连接状态
        public bool IsConnected
        {
            get
            {
                if (serialPort.IsOpen) return true;
                if (tcpSocket != null && tcpSocket.Connected) return true;
                return false;
            }
        }

        // 获取或设置是否允许发送数据
        public bool CanSend
        {
            get { return _canSend; }
            set { _canSend = value; }
        }

        // 获取设备状态
        public DeviceStatus Status
        {
            get { return _deviceStatus; }
        }

        // 获取设备是否在线
        public bool IsDeviceOnline
        {
            get { return _deviceStatus == DeviceStatus.Online; }
        }

        // 获取当前重试次数
        public int RetryCount
        {
            get { return _retryCount; }
        }

        // 获取最大重试次数
        public int MaxRetry
        {
            get { return MAX_RETRY_COUNT; }
        }

        // 获取历史记录数量
        public int HistoryCount
        {
            get { return historyRecords.Count; }
        }

        // 获取最大记录保存条数
        public int MaxRecordCount
        {
            get { return dataExportDAL.GetMaxRecordCount(); }
        }
        
        // 获取或设置从站地址
        public byte SlaveAddress
        {
            get { return _slaveAddress; }
            set 
            { 
                if (value >= 1 && value <= 247)
                {
                    _slaveAddress = value;
                }
                else
                {
                    AddLog("【警告】从站地址必须在1-247之间");
                }
            }
        }
        
        // 获取最大读取寄存器数量
        public int MaxReadRegisterCount
        {
            get { return MAX_READ_REGISTER_COUNT; }
        }
        
        // 获取最大写入寄存器数量
        public int MaxWriteRegisterCount
        {
            get { return MAX_WRITE_REGISTER_COUNT; }
        }

        // 获取最近一次Modbus异常信息
        public ModbusExceptionInfo LastException
        {
            get { return _lastException; }
        }

        // 获取是否正在重连中
        public bool IsReconnecting
        {
            get { return _isReconnecting; }
        }
        
        #endregion

        #region 串口相关方法
        
        // 初始化串口参数
        public void InitSerialPort(string portName, int baudRate)
        {
            serialPort.BaudRate = baudRate;
            serialPort.PortName = portName;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
        }

        // 打开串口
        public bool OpenSerialPort()
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    AddLog($"【成功】{serialPort.PortName}串口已打开");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                AddLog($"【错误】串口打开失败{ex.Message}");
                return false;
            }
        }

        // 关闭串口
        public void CloseSerialPort()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                AddLog($"【成功】{serialPort.PortName}串口已关闭");
            }
        }

        // 检查串口是否已打开
        public bool IsSerialPortOpen()
        {
            return serialPort.IsOpen;
        }
        
        #endregion

        #region TCP相关方法
        
        // 建立TCP连接（异步）
        public async Task<bool> ConnectTcpAsync(string ipAddress, int port)
        {
            Socket tempSocket = null;
            try
            {
                if (tcpSocket != null)
                {
                    tcpSocket.Close();
                    tcpSocket = null;
                }

                tempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tempSocket.ReceiveTimeout = TCP_TIMEOUT;
                tempSocket.SendTimeout = TCP_TIMEOUT;

                var connectArgs = new SocketAsyncEventArgs();
                connectArgs.RemoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                
                var tcs = new TaskCompletionSource<bool>();
                
                EventHandler<SocketAsyncEventArgs> handler = null;
                handler = (sender, e) =>
                {
                    try
                    {
                        e.Completed -= handler;
                        if (e.SocketError == SocketError.Success)
                        {
                            tcs.TrySetResult(true);
                        }
                        else
                        {
                            tcs.TrySetResult(false);
                        }
                    }
                    catch
                    {
                        tcs.TrySetResult(false);
                    }
                };
                connectArgs.Completed += handler;
                
                bool willRaiseEvent = tempSocket.ConnectAsync(connectArgs);
                if (!willRaiseEvent)
                {
                    if (connectArgs.SocketError == SocketError.Success)
                    {
                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        tcs.TrySetResult(false);
                    }
                }
                
                bool connected = await tcs.Task;
                
                if (connected)
                {
                    tcpSocket = tempSocket;
                    AddLog("【TCP连接成功】");
                    return true;
                }
                else
                {
                    string errorMsg = GetSocketErrorMessage(connectArgs.SocketError, "连接失败");
                    AddLog($"【TCP】连接失败：{errorMsg}");
                    if (tempSocket != null)
                    {
                        try { tempSocket.Close(); } catch { }
                    }
                    return false;
                }
            }
            catch (SocketException ex)
            {
                string errorMsg = GetSocketErrorMessage(ex.SocketErrorCode, ex.Message);
                AddLog($"【TCP】连接失败：{errorMsg}");
                if (tempSocket != null)
                {
                    try { tempSocket.Close(); } catch { }
                }
                return false;
            }
            catch (Exception ex)
            {
                AddLog($"【TCP】连接失败：{ex.Message}");
                if (tempSocket != null)
                {
                    try { tempSocket.Close(); } catch { }
                }
                return false;
            }
        }

        private string GetSocketErrorMessage(SocketError socketError, string originalMessage)
        {
            switch (socketError)
            {
                case SocketError.ConnectionRefused:
                    return "目标设备拒绝连接（请确认设备已启动且端口正确）";
                case SocketError.TimedOut:
                    return "连接超时（请检查网络连接）";
                case SocketError.HostUnreachable:
                    return "无法访问目标主机";
                case SocketError.NetworkUnreachable:
                    return "网络不可达";
                case SocketError.AddressNotAvailable:
                    return "地址不可用";
                default:
                    return originalMessage;
                }
        }

        // 关闭TCP连接
        public void CloseTcp()
        {
            if (tcpSocket != null && tcpSocket.Connected)
            {
                tcpSocket.Close();
                AddLog("【TCP】已断开连接");
            }
            tcpSocket = null;
        }
        
        #endregion

        #region Modbus报文处理
        
        // 构建批量读取命令（功能码0x03）
        // startAddr: 起始寄存器地址
        // count: 读取寄存器数量（1-125）
        public byte[] BuildBatchReadCommand(ushort startAddr, ushort count)
        {
            if (count < 1 || count > MAX_READ_REGISTER_COUNT)
            {
                AddLog($"【错误】读取数量必须在1-{MAX_READ_REGISTER_COUNT}之间");
                return null;
            }
            
            // 保存当前起始地址，用于解析响应
            _currentStartAddr = startAddr;
            
            byte[] command = new byte[6];
            command[0] = _slaveAddress;              // 从站地址
            command[1] = 0x03;                       // 功能码：读保持寄存器
            command[2] = (byte)(startAddr >> 8);     // 起始地址高8位
            command[3] = (byte)(startAddr & 0xFF);   // 起始地址低8位
            command[4] = (byte)(count >> 8);         // 寄存器数量高8位
            command[5] = (byte)(count & 0xFF);       // 寄存器数量低8位

            // 计算并添加CRC校验
            byte[] crc = CalculateCRC16(command);
            byte[] fullCommand = new byte[8];
            Array.Copy(command, fullCommand, 6);
            fullCommand[6] = crc[0];
            fullCommand[7] = crc[1];

            return fullCommand;
        }

        // 构建批量写入命令（功能码0x10 - 写多个寄存器）
        // startAddr: 起始寄存器地址
        // values: 要写入的寄存器值数组
        public byte[] BuildBatchWriteCommand(ushort startAddr, ushort[] values)
        {
            // 参数验证
            if (values == null || values.Length == 0)
            {
                AddLog("【错误】写入数据不能为空");
                return null;
            }
            
            if (values.Length > MAX_WRITE_REGISTER_COUNT)
            {
                AddLog($"【错误】写入数量不能超过{MAX_WRITE_REGISTER_COUNT}个寄存器");
                return null;
            }
            
            int registerCount = values.Length;
            int byteCount = registerCount * 2;  // 每个寄存器2字节
            
            // 构建命令：从站地址(1) + 功能码(1) + 起始地址(2) + 寄存器数量(2) + 字节计数(1) + 数据(n*2) + CRC(2)
            byte[] command = new byte[7 + byteCount];
            
            command[0] = _slaveAddress;              // 从站地址
            command[1] = 0x10;                       // 功能码：写多个寄存器
            command[2] = (byte)(startAddr >> 8);     // 起始地址高8位
            command[3] = (byte)(startAddr & 0xFF);   // 起始地址低8位
            command[4] = (byte)(registerCount >> 8); // 寄存器数量高8位
            command[5] = (byte)(registerCount & 0xFF); // 寄存器数量低8位
            command[6] = (byte)byteCount;            // 字节计数
            
            // 填充数据（每个寄存器2字节，高字节在前）
            for (int i = 0; i < registerCount; i++)
            {
                command[7 + i * 2] = (byte)(values[i] >> 8);     // 高字节
                command[7 + i * 2 + 1] = (byte)(values[i] & 0xFF); // 低字节
            }
            
            // 计算并添加CRC校验
            byte[] crc = CalculateCRC16(command);
            byte[] fullCommand = new byte[command.Length + 2];
            Array.Copy(command, fullCommand, command.Length);
            fullCommand[command.Length] = crc[0];
            fullCommand[command.Length + 1] = crc[1];

            return fullCommand;
        }

        // 计算Modbus CRC16校验码
        public byte[] CalculateCRC16(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return new byte[] { (byte)crc, (byte)(crc >> 8) };
        }

        private bool ValidateCRC(byte[] data)
        {
            if (data.Length < 4) return false;
            
            byte[] dataWithoutCrc = new byte[data.Length - 2];
            Array.Copy(data, 0, dataWithoutCrc, 0, dataWithoutCrc.Length);
            
            byte[] calculatedCrc = CalculateCRC16(dataWithoutCrc);
            
            return (data[data.Length - 2] == calculatedCrc[0] && 
                    data[data.Length - 1] == calculatedCrc[1]);
        }

        // 根据协议类型发送数据（异步）
        public async Task SendByProtocolAsync(byte[] command, int protocolIndex)
        {
            if (command == null)
            {
                AddLog("【错误】命令为空，无法发送");
                return;
            }
            
            if (protocolIndex == 0)
            {
                AddLog("发送报文：" + BitConverter.ToString(command).Replace("-", " "));
                await serialPort.BaseStream.WriteAsync(command, 0, command.Length);
            }
            else
            {
                int pduLength = command.Length - 3;
                byte[] mbap = new byte[7];
                mbap[0] = 0x00;
                mbap[1] = 0x01;
                mbap[2] = 0x00;
                mbap[3] = 0x00;
                mbap[4] = (byte)((pduLength + 1) >> 8);
                mbap[5] = (byte)((pduLength + 1) & 0xFF);
                mbap[6] = command[0];

                byte[] pdu = new byte[pduLength];
                Array.Copy(command, 1, pdu, 0, pduLength);

                byte[] tcpSend = new byte[mbap.Length + pdu.Length];
                Array.Copy(mbap, tcpSend, mbap.Length);
                Array.Copy(pdu, 0, tcpSend, mbap.Length, pdu.Length);

                await Task.Run(() => tcpSocket.Send(tcpSend));
                AddLog("TCP发送报文：" + BitConverter.ToString(tcpSend).Replace("-", ""));
            }
        }

        // 处理接收到的数据（异步）
        public async Task ProcessReceiveDataAsync(int protocolIndex)
        {
            try
            {
                if (protocolIndex == 0)
                {
                    await ProcessSerialDataAsync();
                }
                else
                {
                    await ProcessTcpDataAsync();
                }
            }
            catch (Exception ex)
            {
                AddLog("【接收错误】" + ex.Message);
                ClearData();
                if (protocolIndex == 0 && serialPort.IsOpen)
                {
                    serialPort.DiscardInBuffer();
                }
            }
        }

        // 处理串口接收数据（异步）
        private async Task ProcessSerialDataAsync()
        {
            if (!serialPort.IsOpen)
            {
                AddLog("【错误】串口未打开");
                return;
            }
            if (serialPort.BytesToRead <= 0) return;

            byte[] receiveData = new byte[serialPort.BytesToRead];
            await serialPort.BaseStream.ReadAsync(receiveData, 0, receiveData.Length);
            serialPort.DiscardInBuffer();

            AddLog("RTU接收报文：" + BitConverter.ToString(receiveData).Replace("-", " "));

            ParseModbusResponse(receiveData, 0);
        }

        // 处理TCP接收数据（异步）
        private async Task ProcessTcpDataAsync()
        {
            if (tcpSocket == null || !tcpSocket.Connected || tcpSocket.Available <= 0)
                return;

            int len = await Task.Run(() => tcpSocket.Receive(tcpBuffer));
            if (len < 7)
            {
                AddLog("【警告】TCP数据太短，无法解析");
                return;
            }

            byte[] fullFrame = new byte[len];
            Array.Copy(tcpBuffer, 0, fullFrame, 0, len);
            AddLog("TCP接收完整帧：" + BitConverter.ToString(fullFrame).Replace("-", ""));

            // 提取PDU（去掉7字节MBAP头）
            byte[] pdu = new byte[len - 7];
            Array.Copy(tcpBuffer, 7, pdu, 0, pdu.Length);

            // 解析PDU
            ParseModbusResponse(pdu, 1);
        }

        // 解析Modbus响应数据
        // data: 接收到的数据
        // protocolIndex: 0=RTU, 1=TCP
        private void ParseModbusResponse(byte[] data, int protocolIndex)
        {
            if (data == null || data.Length < 2)
            {
                AddLog("【警告】数据长度不足");
                return;
            }

            if (protocolIndex == 0)
            {
                if (data.Length < 4)
                {
                    AddLog("【警告】RTU数据长度不足（最少4字节）");
                    return;
                }
                if (!ValidateCRC(data))
                {
                    AddLog("【错误】RTU CRC校验失败，数据可能损坏");
                    return;
                }
            }

            byte functionCode = (protocolIndex == 0) ? data[1] : data[0];
            
            if ((functionCode & 0x80) != 0)
            {
                int minExceptionLength = (protocolIndex == 0) ? 3 : 2;
                if (data.Length < minExceptionLength)
                {
                    AddLog("【警告】异常响应数据长度不足");
                    ClearData();
                    return;
                }
                byte exceptionCode = (protocolIndex == 0) ? data[2] : data[1];
                string errorMsg = GetExceptionMessage(exceptionCode);
                AddLog($"【业务异常】功能码：0x{functionCode:X2}，异常码：0x{exceptionCode:X2}，{errorMsg}");

                _lastException = new ModbusExceptionInfo(ModbusExceptionType.BusinessException, exceptionCode, errorMsg);
                if (OnModbusException != null)
                {
                    OnModbusException(_lastException);
                }

                ClearData();
                return;
            }

            switch (functionCode)
            {
                case 0x03:  // 读保持寄存器响应
                    ParseReadResponse(data, protocolIndex);
                    break;
                    
                case 0x10:  // 写多个寄存器响应
                    ParseBatchWriteResponse(data, protocolIndex);
                    break;
                    
                default:
                    AddLog($"【未知功能码】0x{functionCode:X2}");
                    break;
            }
        }

        // 解析读取响应（功能码0x03）
        private void ParseReadResponse(byte[] data, int protocolIndex)
        {
            int dataStartIndex = (protocolIndex == 0) ? 2 : 1;  // RTU需要跳过从站地址
            
            if (data.Length < dataStartIndex + 1)
            {
                AddLog("【警告】读取响应数据长度不足");
                return;
            }

            byte byteCount = data[dataStartIndex];
            int registerCount = byteCount / 2;
            
            if (data.Length < dataStartIndex + 1 + byteCount)
            {
                AddLog($"【警告】数据长度不匹配，期望{byteCount}字节");
                return;
            }

            // 解析寄存器值
            ushort[] registerValues = new ushort[registerCount];
            for (int i = 0; i < registerCount; i++)
            {
                int index = dataStartIndex + 1 + i * 2;
                registerValues[i] = (ushort)((data[index] << 8) | data[index + 1]);
            }

            // 只有当起始地址为0且读取数量>=2时，才触发温湿度更新事件
            if (_currentStartAddr == 0 && registerCount >= 2)
            {
                short tempRaw = (short)registerValues[0];
                double temperature = tempRaw / 10.0;
                ushort humiRaw = registerValues[1];
                double humidity = humiRaw / 10.0;

                AddLog($"【解析】温度原始值:0x{registerValues[0]:X4}({tempRaw})→{temperature}°C, 湿度原始值:0x{registerValues[1]:X4}({humiRaw})→{humidity}%");

                string status;
                System.Drawing.Color color;
                if (temperature > 35)
                {
                    status = "高温预警";
                    color = System.Drawing.Color.Red;
                }
                else if (temperature < 10)
                {
                    status = "低温预警";
                    color = System.Drawing.Color.Blue;
                }
                else
                {
                    status = "正常";
                    color = System.Drawing.Color.Green;
                }

                UpdateData(temperature, humidity, status, color);
            }
            
            // 触发批量数据接收事件
            if (OnBatchDataReceived != null)
            {
                OnBatchDataReceived(_currentStartAddr, registerValues);
            }
            
            AddLog($"【读取成功】共{registerCount}个寄存器");
        }

        // 解析批量写入响应（功能码0x10）
        private void ParseBatchWriteResponse(byte[] data, int protocolIndex)
        {
            int dataStartIndex = (protocolIndex == 0) ? 2 : 1;
            
            if (data.Length < dataStartIndex + 4)
            {
                AddLog("【警告】批量写入响应数据长度不足");
                return;
            }

            ushort startAddr = (ushort)((data[dataStartIndex] << 8) | data[dataStartIndex + 1]);
            ushort count = (ushort)((data[dataStartIndex + 2] << 8) | data[dataStartIndex + 3]);
            
            AddLog($"【批量写入成功】起始地址：{startAddr}，写入数量：{count}");
            
            if (OnWriteCompleted != null)
            {
                OnWriteCompleted(startAddr, count);
            }
        }

        // 获取异常码描述
        private string GetExceptionMessage(byte exceptionCode)
        {
            switch (exceptionCode)
            {
                case 0x01: return "非法功能码";
                case 0x02: return "非法数据地址";
                case 0x03: return "非法数据值";
                case 0x04: return "从站设备故障";
                case 0x05: return "确认";
                case 0x06: return "从站设备忙";
                case 0x08: return "存储奇偶性错误";
                default: return "未知异常";
            }
        }
        
        #endregion

        #region 数据更新与历史记录
        
        // 更新数据并触发相关事件
        private void UpdateData(double temp, double humi, string status, System.Drawing.Color color)
        {
            AddHistoryRecord(temp, humi, status);
            if (OnDataUpdated != null)
            {
                OnDataUpdated(temp, humi, status, color);
            }
            if (OnDataReceived != null)
            {
                OnDataReceived();
            }
        }

        // 清除数据显示
        private void ClearData()
        {
            if (OnDataCleared != null)
            {
                OnDataCleared();
            }
        }

        // 添加历史记录
        private void AddHistoryRecord(double temp, double humi, string status)
        {
            DataRecord record = new DataRecord(temp, humi, status);
            historyRecords.Add(record);
            dataExportDAL.SaveRecordToSqlite(record);

            // 超过最大记录数时删除最早的记录
            if (historyRecords.Count > MaxRecordCount)
            {
                historyRecords.RemoveAt(0);
            }
        }

        // 获取历史记录列表
        public List<DataRecord> GetHistoryRecords()
        {
            return new List<DataRecord>(historyRecords);
        }

        // 导出历史记录到TXT文件（异步）
        public async Task<bool> ExportToTxtAsync(string filePath)
        {
            return await dataExportDAL.ExportToTxtAsync(historyRecords, filePath);
        }

        // 导出历史记录到CSV文件（异步）
        public async Task<bool> ExportToCsvAsync(string filePath)
        {
            return await dataExportDAL.ExportToCsvAsync(historyRecords, filePath);
        }

        // 手动新增一条数据库记录
        public bool AddRecordToDb(DataRecord record)
        {
            return dataExportDAL.SaveRecordToSqlite(record);
        }

        // 从SQLite读取全部历史记录
        public List<DataRecord> GetAllRecordsFromDb()
        {
            return dataExportDAL.GetAllRecordsFromSqlite();
        }

        // 按ID查询记录
        public DataRecord GetRecordById(long id)
        {
            return dataExportDAL.GetRecordById(id);
        }

        // 按时间范围查询记录
        public List<DataRecord> QueryRecordsByTimeRange(DateTime startTime, DateTime endTime)
        {
            return dataExportDAL.QueryRecordsByTimeRange(startTime, endTime);
        }

        // 更新指定记录
        public bool UpdateRecord(DataRecord record)
        {
            return dataExportDAL.UpdateRecord(record);
        }

        // 按ID删除记录
        public bool DeleteRecordById(long id)
        {
            return dataExportDAL.DeleteRecordById(id);
        }

        // 按时间范围删除记录
        public int DeleteRecordsByTimeRange(DateTime startTime, DateTime endTime)
        {
            return dataExportDAL.DeleteRecordsByTimeRange(startTime, endTime);
        }

        // 清空数据库记录
        public int ClearAllDbRecords()
        {
            return dataExportDAL.ClearAllRecords();
        }
        
        #endregion

        #region 辅助方法
        
        // 添加日志
        public void AddLog(string msg)
        {
            if (OnLogAdded != null)
            {
                OnLogAdded(msg);
            }
        }

        // 增加重试计数
        public void IncrementRetry()
        {
            _retryCount++;
        }

        // 重置重试计数和状态
        public void ResetRetry()
        {
            _retryCount = 0;
            _canSend = true;
            _deviceStatus = DeviceStatus.Online;
            _lastException = null;
        }

        // 设置设备离线状态
        public void SetDeviceOffline()
        {
            _deviceStatus = DeviceStatus.Offline;
            _canSend = false;
        }

        // 停止重试（仅重置计数）
        public void StopRetry()
        {
            _retryCount = 0;
        }

        // 触发通信异常（供外部调用）
        public void TriggerCommunicationException(string message)
        {
            _lastException = new ModbusExceptionInfo(ModbusExceptionType.CommunicationException, 0, message);
            AddLog($"【通信异常】{message}");
            if (OnModbusException != null)
            {
                OnModbusException(_lastException);
            }
        }

        // 计算指数退避延迟时间（毫秒）
        public int GetExponentialBackoffDelay(int attempt)
        {
            int delay = EXPONENTIAL_BACKOFF_BASE_MS * (int)Math.Pow(2, attempt);
            return Math.Min(delay, EXPONENTIAL_BACKOFF_MAX_MS);
        }

        // 启动指数退避重连
        public void StartExponentialReconnect(Action reconnectAction)
        {
            if (_isReconnecting) return;
            _isReconnecting = true;
            _reconnectAttempt = 0;

            StopExponentialReconnect();

            _reconnectTimer = new System.Threading.Timer(_ =>
            {
                try
                {
                    _reconnectAttempt++;
                    int delay = GetExponentialBackoffDelay(_reconnectAttempt - 1);
                    AddLog($"【指数退避重连】第{_reconnectAttempt}次尝试，等待{delay}ms后重连...");
                    if (OnReconnectAttempt != null)
                    {
                        OnReconnectAttempt(_reconnectAttempt);
                    }

                    System.Threading.Thread.Sleep(delay);

                    reconnectAction();
                }
                catch (Exception ex)
                {
                    AddLog($"【指数退避重连异常】{ex.Message}");
                }
            }, null, 0, System.Threading.Timeout.Infinite);
        }

        // 停止指数退避重连
        public void StopExponentialReconnect()
        {
            if (_reconnectTimer != null)
            {
                _reconnectTimer.Dispose();
                _reconnectTimer = null;
            }
            _isReconnecting = false;
            _reconnectAttempt = 0;
        }

        // 解绑所有事件订阅（防止内存泄漏）
        public void UnsubscribeAllEvents()
        {
            OnLogAdded = null;
            OnDataUpdated = null;
            OnDataCleared = null;
            OnDataReceived = null;
            OnBatchDataReceived = null;
            OnWriteCompleted = null;
            OnModbusException = null;
            OnReconnectAttempt = null;
        }
        
        #endregion

        #region 批量操作公共方法

        // 批量读取寄存器（异步）
        public async Task<bool> BatchReadAsync(ushort startAddr, ushort count, int protocolIndex)
        {
            if (count < 1 || count > MAX_READ_REGISTER_COUNT)
            {
                AddLog($"【错误】读取数量必须在1-{MAX_READ_REGISTER_COUNT}之间");
                return false;
            }
            
            if (!IsConnected)
            {
                AddLog("【错误】未连接设备");
                return false;
            }
            
            byte[] command = BuildBatchReadCommand(startAddr, count);
            if (command == null)
            {
                return false;
            }
            
            try
            {
                await SendByProtocolAsync(command, protocolIndex);
                return true;
            }
            catch (Exception ex)
            {
                AddLog($"【发送失败】{ex.Message}");
                return false;
            }
        }

        // 批量写入寄存器（异步）
        public async Task<bool> BatchWriteAsync(ushort startAddr, ushort[] values, int protocolIndex)
        {
            if (values == null || values.Length == 0)
            {
                AddLog("【错误】写入数据不能为空");
                return false;
            }
            
            if (values.Length > MAX_WRITE_REGISTER_COUNT)
            {
                AddLog($"【错误】写入数量不能超过{MAX_WRITE_REGISTER_COUNT}个寄存器");
                return false;
            }
            
            if (!IsConnected)
            {
                AddLog("【错误】未连接设备");
                return false;
            }
            
            byte[] command = BuildBatchWriteCommand(startAddr, values);
            if (command == null)
            {
                return false;
            }
            
            try
            {
                await SendByProtocolAsync(command, protocolIndex);
                AddLog($"【批量写入】起始地址：{startAddr}，数量：{values.Length}");
                return true;
            }
            catch (Exception ex)
            {
                AddLog($"【发送失败】{ex.Message}");
                return false;
            }
        }
        
        #endregion

        #region IDisposable 资源释放

        // 释放资源
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // 释放资源的核心方法
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                StopExponentialReconnect();

                if (serialPort != null)
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                    serialPort.Dispose();
                    serialPort = null;
                }
                if (tcpSocket != null)
                {
                    if (tcpSocket.Connected)
                    {
                        tcpSocket.Close();
                    }
                    tcpSocket = null;
                }
                if (dataExportDAL != null)
                {
                    dataExportDAL.Dispose();
                    dataExportDAL = null;
                }

                UnsubscribeAllEvents();
                historyRecords?.Clear();
            }

            _disposed = true;
        }

        // 析构函数 - 兜底保护，防止用户忘记调用 Dispose
        ~ModbusBLL()
        {
            Dispose(false);
        }

        #endregion
    }
}
