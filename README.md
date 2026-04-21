# ModbusRTU_TCP

一个基于 C# Windows Forms 开发的 Modbus RTU/TCP 通信工具，支持温湿度数据采集、监控、SQLite 持久化和导出功能。

## 功能特性

### 核心功能
- **双协议支持**：同时支持 Modbus RTU（串口通信）和 Modbus TCP（网络通信）
- **批量读写操作**：支持批量读取和写入保持寄存器（功能码 0x03 和 0x10）
- **自动数据采集**：每 3 秒自动采集数据，实时更新显示
- **温湿度监控**：专门针对温湿度传感器设计，支持温度/湿度阈值预警
- **数据导出**：支持导出历史数据为 TXT 和 CSV 格式
- **SQLite 持久化**：数据自动写入 SQLite 数据库，支持完整 CRUD 操作

### 高级特性
- **指数退避重连**：设备断线后采用指数退避策略自动重连（1s→2s→4s→8s...最大30s），避免疯狂轮询消耗系统资源
- **异常分类处理**：区分业务异常（Modbus 异常码 0x01-0x08）与通信异常（超时/断连），业务异常跳过重试，通信异常触发重连
- **超时重发机制**：自动检测超时并重发指令（最多重试 3 次）
- **CRC16 校验**：RTU 模式下自动计算和验证 CRC16 校验码
- **状态可视化**：实时显示设备连接状态和数据状态
- **内存泄漏防护**：事件订阅/解绑配对、数据库连接池管理、资源正确 Dispose

## 技术栈

- **开发语言**：C# 
- **框架版本**：.NET Framework 4.7.2
- **应用类型**：Windows Forms 应用程序
- **架构模式**：三层架构（BLL/DAL/Model）
- **数据库**：SQLite（System.Data.SQLite 1.0.119.0）

## 项目结构

```
ModbusRTU_TCP/
├── BLL/                      # 业务逻辑层
│   └── ModbusBLL.cs         # Modbus 通信核心逻辑（含指数退避、异常分类）
├── DAL/                      # 数据访问层
│   ├── DataExportDAL.cs     # SQLite 数据库操作 + 数据导出
│   └── Model/
│       └── DataRecord.cs    # 数据记录模型
├── Properties/               # 项目属性
├── Form1.cs                  # 主窗体
├── Program.cs                # 程序入口
└── App.config                # 应用配置
```

## SQLite 数据库说明

### 数据库位置
- **路径**：`D:\SQLiteData\ModbusRTU_TCP.db`
- 程序启动时自动创建目录和数据库文件

### 数据表结构
```sql
CREATE TABLE ModbusDataRecords (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CollectTime TEXT NOT NULL,
    Temperature REAL NOT NULL,
    Humidity REAL NOT NULL,
    Status TEXT NOT NULL
);
```

### 数据库 CRUD 接口

| 方法 | 说明 |
|------|------|
| `SaveRecordToSqlite(DataRecord)` | 新增记录 |
| `GetAllRecordsFromSqlite()` | 查询全部记录（按时间倒序） |
| `GetRecordById(long)` | 按 ID 查询 |
| `QueryRecordsByTimeRange(DateTime, DateTime)` | 按时间范围查询 |
| `UpdateRecord(DataRecord)` | 更新记录 |
| `DeleteRecordById(long)` | 按 ID 删除 |
| `DeleteRecordsByTimeRange(DateTime, DateTime)` | 按时间范围删除 |
| `ClearAllRecords()` | 清空全部记录 |

> 采集到新数据时，会在 `AddHistoryRecord(...)` 中自动写入 SQLite。

## 指数退避重连策略

当设备通信异常（超时、Socket 断开）时，系统采用指数退避策略进行重连：

| 重连次数 | 等待时间 | 说明 |
|----------|----------|------|
| 第 1 次 | 1 秒 | 初始等待 |
| 第 2 次 | 2 秒 | 指数增长 |
| 第 3 次 | 4 秒 | 指数增长 |
| 第 4 次 | 8 秒 | 指数增长 |
| 第 5 次 | 16 秒 | 指数增长 |
| 第 N 次 | 最长 30 秒 | 封顶限制 |

### 相关配置
```csharp
private const int EXPONENTIAL_BACKOFF_BASE_MS = 1000;   // 基础等待时间 1s
private const int EXPONENTIAL_BACKOFF_MAX_MS = 30000;    // 最大等待时间 30s
```

## 异常分类处理

系统将 Modbus 通信异常分为两类，采用不同处理策略：

### 业务异常（跳过重试）
设备返回的 Modbus 协议异常，属于设备正常响应，无需重试：

| 异常码 | 名称 | 说明 |
|--------|------|------|
| 0x01 | 非法功能码 | 设备不支持该功能码 |
| 0x02 | 非法数据地址 | 寄存器地址不存在 |
| 0x03 | 非法数据值 | 数据值超出范围 |
| 0x04 | 从站设备故障 | 设备内部错误 |
| 0x05 | 确认 | 设备已收到请求 |
| 0x06 | 从站设备忙 | 设备忙碌 |
| 0x08 | 存储奇偶性错误 | 存储器错误 |

**处理方式**：记录日志 → 停止超时计时器 → 恢复自动采集

### 通信异常（触发重连）
网络或串口通信故障，需要重连恢复：

| 异常类型 | 说明 |
|----------|------|
| 连接超时 | 设备未在规定时间内响应 |
| Socket 断开 | TCP 连接已断开 |
| 串口异常 | 串口通信故障 |

**处理方式**：记录日志 → 触发指数退避重连

## 内存泄漏防护

### 事件订阅管理
- `InitModbusBllEvents()`：窗体初始化时订阅所有事件
- `UnsubscribeModbusBllEvents()`：窗体关闭时解绑所有事件
- `UnsubscribeAllEvents()`：BLL 层 Dispose 时清空所有事件

### 数据库连接管理
- 所有 SQLite 操作使用 `using` 语句确保连接正确释放
- 启用连接池（`Pooling=true;Max Pool Size=10`）
- `DataExportDAL.Dispose()` 调用 `SQLiteConnection.ClearAllPools()` 释放连接池

### 资源释放链
```
Form1.Dispose()
  → UnsubscribeModbusBllEvents()    // 解绑事件
  → modbusBll.Dispose()             // 释放 BLL
      → StopExponentialReconnect()  // 停止重连定时器
      → serialPort.Dispose()        // 释放串口
      → tcpSocket.Close()           // 关闭 TCP
      → dataExportDAL.Dispose()     // 释放 DAL
          → SQLiteConnection.ClearAllPools()  // 清理连接池
      → UnsubscribeAllEvents()      // 清空所有事件引用
      → historyRecords.Clear()      // 清空内存列表
```

## 使用方法

### 环境要求
- Windows 操作系统
- .NET Framework 4.7.2 或更高版本
- Visual Studio 2017 或更高版本（用于开发）

### 快速开始

1. **克隆仓库**
   ```bash
   git clone https://github.com/RobhcC/ModbusRTU-TCP.git
   ```

2. **打开项目**
   - 使用 Visual Studio 打开 `ModbusRTU_TCP.slnx`
   - 等待 NuGet 包还原完成

3. **配置连接**
   
   **RTU 模式（串口）：**
   - 选择串口号（如 COM3）
   - 设置波特率（默认 9600）
   - 点击"打开连接"

   **TCP 模式（网络）：**
   - 输入设备 IP 地址
   - 输入端口号（默认 502）
   - 点击"打开连接"

4. **数据采集**
   - 连接成功后自动开始采集（每 3 秒）
   - 或手动点击"批量读取"按钮

5. **数据操作**
   - 点击"新增/修改/删除/查询"按钮操作数据库
   - 点击"导出 TXT"或"导出 CSV"导出文件

## Modbus 协议说明

### 支持的功能码
| 功能码 | 名称 | 说明 |
|--------|------|------|
| 0x03 | 读保持寄存器 | 批量读取寄存器值（最多 125 个） |
| 0x10 | 写多个寄存器 | 批量写入寄存器值（最多 123 个） |

### 寄存器地址映射（示例）
| 地址 | 名称 | 数据类型 | 说明 |
|------|------|----------|------|
| 0x0000 | 温度值 | UInt16 | 实际温度 = 原始值 / 10 |
| 0x0001 | 湿度值 | UInt16 | 实际湿度 = 原始值 / 10 |
| 0x0002 | 运行状态 | UInt16 | 1=运行，0=停止 |
| 0x0003 | 设定温度 | UInt16 | 目标温度 |
| 0x0004 | 设定湿度 | UInt16 | 目标湿度 |

## 配置参数

### 串口参数（RTU 模式）
- 数据位：8
- 校验位：None
- 停止位：One
- 波特率：9600/19200/38400（可选）

### TCP 参数
- 默认端口：502
- 连接超时：5000ms
- 缓冲区大小：1024 字节

### 通信参数
- 从站地址：1-247（默认 1）
- 最大重试次数：3 次
- 自动采集间隔：3000ms
- 指数退避基础时间：1000ms
- 指数退避最大时间：30000ms

## 依赖说明

项目通过 NuGet 引用 SQLite 依赖：
- `System.Data.SQLite.Core (1.0.119.0)`
- `Stub.System.Data.SQLite.Core.NetFramework (1.0.119.0)`

首次打开项目请先进行 NuGet 还原。

## 更新日志

### v1.1.0
- SQLite 数据库路径迁移至 `D:\SQLiteData\ModbusRTU_TCP.db`
- 数据表重命名为 `ModbusDataRecords`
- 实现指数退避重连策略（1s→2s→4s→8s...最大30s）
- 区分业务异常与通信异常，业务异常跳过重试，通信异常触发重连
- 修复内存泄漏：事件订阅/解绑配对、数据库连接池管理
- 完善数据库操作日志记录（所有 CRUD 操作均有日志输出）
- DataExportDAL 实现 IDisposable，正确释放 SQLite 连接池

### v1.0.0
- 初始版本发布
- 支持 RTU/TCP 双协议
- 实现批量读写功能
- 添加数据导出功能
- 接入 SQLite 本地数据库

## 许可证

本项目采用 MIT 许可证，详见 [LICENSE](LICENSE) 文件。

## 贡献指南

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 提交 Pull Request

## 联系方式

如有问题或建议，请提交 Issue 或联系项目维护者。

---

**注意**：本项目仅供学习和研究使用，生产环境使用请进行充分测试。
