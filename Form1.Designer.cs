namespace ModbusRTU_TCP
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                timerAutoSend?.Stop();
                timerAutoSend?.Dispose();
                modbusBll?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnCloseSerial = new System.Windows.Forms.Button();
            this.btnSendData = new System.Windows.Forms.Button();
            this.btnOpenSerial = new System.Windows.Forms.Button();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.groupBoxSerial = new System.Windows.Forms.GroupBox();
            this.cboBaudRate = new System.Windows.Forms.ComboBox();
            this.cboPortName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvModbus = new System.Windows.Forms.DataGridView();
            this.colAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblProtcol = new System.Windows.Forms.Label();
            this.cboProtocol = new System.Windows.Forms.ComboBox();
            this.groupBoxTcp = new System.Windows.Forms.GroupBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblIP = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnDbClear = new System.Windows.Forms.Button();
            this.btnDbDelete = new System.Windows.Forms.Button();
            this.btnDbUpdate = new System.Windows.Forms.Button();
            this.btnDbQuery = new System.Windows.Forms.Button();
            this.btnDbCreate = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.btnExportTxt = new System.Windows.Forms.Button();
            this.groupBoxBatch = new System.Windows.Forms.GroupBox();
            this.lblBatchWriteValue = new System.Windows.Forms.Label();
            this.txtBatchWriteValue = new System.Windows.Forms.TextBox();
            this.btnBatchWrite = new System.Windows.Forms.Button();
            this.lblBatchCount = new System.Windows.Forms.Label();
            this.numBatchCount = new System.Windows.Forms.NumericUpDown();
            this.lblStartAddr = new System.Windows.Forms.Label();
            this.numStartAddr = new System.Windows.Forms.NumericUpDown();
            this.btnBatchRead = new System.Windows.Forms.Button();
            this.lblSlaveAddr = new System.Windows.Forms.Label();
            this.numSlaveAddr = new System.Windows.Forms.NumericUpDown();
            this.timerReceive = new System.Windows.Forms.Timer(this.components);
            this.timerTimeOut = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBoxSerial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvModbus)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBoxTcp.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBoxBatch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBatchCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartAddr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSlaveAddr)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.20359F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.79641F));
            this.tableLayoutPanel1.Controls.Add(this.lstLog, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxSerial, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dgvModbus, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxTcp, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxBatch, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.727273F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.18182F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 17.09091F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.90909F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 550);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lstLog
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.lstLog, 2);
            this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 12;
            this.lstLog.Location = new System.Drawing.Point(3, 234);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(794, 120);
            this.lstLog.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnDbClear);
            this.groupBox2.Controls.Add(this.btnExportTxt);
            this.groupBox2.Controls.Add(this.btnDbDelete);
            this.groupBox2.Controls.Add(this.btnExportCsv);
            this.groupBox2.Controls.Add(this.btnDbUpdate);
            this.groupBox2.Controls.Add(this.btnDbCreate);
            this.groupBox2.Controls.Add(this.btnDbQuery);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 140);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(379, 88);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "数据操作";
            // 
            // btnCloseSerial
            // 
            this.btnCloseSerial.Location = new System.Drawing.Point(207, 12);
            this.btnCloseSerial.Name = "btnCloseSerial";
            this.btnCloseSerial.Size = new System.Drawing.Size(75, 26);
            this.btnCloseSerial.TabIndex = 5;
            this.btnCloseSerial.Text = "关闭串口";
            this.btnCloseSerial.UseVisualStyleBackColor = true;
            this.btnCloseSerial.Click += new System.EventHandler(this.btnCloseSerial_Click);
            // 
            // btnSendData
            // 
            this.btnSendData.Location = new System.Drawing.Point(6, 12);
            this.btnSendData.Name = "btnSendData";
            this.btnSendData.Size = new System.Drawing.Size(75, 26);
            this.btnSendData.TabIndex = 3;
            this.btnSendData.Text = "发送报文";
            this.btnSendData.UseVisualStyleBackColor = true;
            this.btnSendData.Click += new System.EventHandler(this.btnSendData_Click);
            // 
            // btnOpenSerial
            // 
            this.btnOpenSerial.Location = new System.Drawing.Point(105, 12);
            this.btnOpenSerial.Name = "btnOpenSerial";
            this.btnOpenSerial.Size = new System.Drawing.Size(75, 26);
            this.btnOpenSerial.TabIndex = 4;
            this.btnOpenSerial.Text = "打开串口";
            this.btnOpenSerial.UseVisualStyleBackColor = true;
            this.btnOpenSerial.Click += new System.EventHandler(this.btnOpenSerial_Click);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(300, 12);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(85, 26);
            this.btnClearLog.TabIndex = 4;
            this.btnClearLog.Text = " 清除日志";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // groupBoxSerial
            // 
            this.groupBoxSerial.Controls.Add(this.cboBaudRate);
            this.groupBoxSerial.Controls.Add(this.cboPortName);
            this.groupBoxSerial.Controls.Add(this.label2);
            this.groupBoxSerial.Controls.Add(this.label1);
            this.groupBoxSerial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSerial.Location = new System.Drawing.Point(3, 51);
            this.groupBoxSerial.Name = "groupBoxSerial";
            this.groupBoxSerial.Size = new System.Drawing.Size(379, 83);
            this.groupBoxSerial.TabIndex = 3;
            this.groupBoxSerial.TabStop = false;
            this.groupBoxSerial.Text = "串口配置";
            // 
            // cboBaudRate
            // 
            this.cboBaudRate.FormattingEnabled = true;
            this.cboBaudRate.Location = new System.Drawing.Point(80, 50);
            this.cboBaudRate.Name = "cboBaudRate";
            this.cboBaudRate.Size = new System.Drawing.Size(121, 20);
            this.cboBaudRate.TabIndex = 3;
            this.cboBaudRate.SelectedIndexChanged += new System.EventHandler(this.cboBaudRate_SelectedIndexChanged);
            // 
            // cboPortName
            // 
            this.cboPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPortName.FormattingEnabled = true;
            this.cboPortName.Location = new System.Drawing.Point(80, 20);
            this.cboPortName.Name = "cboPortName";
            this.cboPortName.Size = new System.Drawing.Size(121, 20);
            this.cboPortName.TabIndex = 2;
            this.cboPortName.SelectedIndexChanged += new System.EventHandler(this.cboPortName_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "波特率";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "串口号";
            // 
            // dgvModbus
            // 
            this.dgvModbus.AllowUserToAddRows = false;
            this.dgvModbus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvModbus.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAddress,
            this.colName,
            this.colValue,
            this.colStatus});
            this.tableLayoutPanel1.SetColumnSpan(this.dgvModbus, 2);
            this.dgvModbus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvModbus.Location = new System.Drawing.Point(3, 360);
            this.dgvModbus.Name = "dgvModbus";
            this.dgvModbus.ReadOnly = true;
            this.dgvModbus.RowTemplate.Height = 23;
            this.dgvModbus.Size = new System.Drawing.Size(794, 187);
            this.dgvModbus.TabIndex = 4;
            // 
            // colAddress
            // 
            this.colAddress.HeaderText = "寄存器地址";
            this.colAddress.Name = "colAddress";
            this.colAddress.ReadOnly = true;
            // 
            // colName
            // 
            this.colName.HeaderText = "数据名称";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Width = 120;
            // 
            // colValue
            // 
            this.colValue.HeaderText = "数值";
            this.colValue.Name = "colValue";
            this.colValue.ReadOnly = true;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "状态说明";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            this.colStatus.Width = 150;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblProtcol);
            this.groupBox1.Controls.Add(this.cboProtocol);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(379, 42);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "协议选择";
            // 
            // lblProtcol
            // 
            this.lblProtcol.AutoSize = true;
            this.lblProtcol.Location = new System.Drawing.Point(10, 16);
            this.lblProtcol.Name = "lblProtcol";
            this.lblProtcol.Size = new System.Drawing.Size(29, 12);
            this.lblProtcol.TabIndex = 1;
            this.lblProtcol.Text = "协议";
            // 
            // cboProtocol
            // 
            this.cboProtocol.FormattingEnabled = true;
            this.cboProtocol.Items.AddRange(new object[] {
            "Modbus RTU",
            "Modbus TCP"});
            this.cboProtocol.Location = new System.Drawing.Point(60, 12);
            this.cboProtocol.Name = "cboProtocol";
            this.cboProtocol.Size = new System.Drawing.Size(121, 20);
            this.cboProtocol.TabIndex = 0;
            this.cboProtocol.SelectedIndexChanged += new System.EventHandler(this.cboProtocol_SelectedIndexChanged);
            // 
            // groupBoxTcp
            // 
            this.groupBoxTcp.Controls.Add(this.txtIP);
            this.groupBoxTcp.Controls.Add(this.txtPort);
            this.groupBoxTcp.Controls.Add(this.lblPort);
            this.groupBoxTcp.Controls.Add(this.lblIP);
            this.groupBoxTcp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxTcp.Location = new System.Drawing.Point(388, 51);
            this.groupBoxTcp.Name = "groupBoxTcp";
            this.groupBoxTcp.Size = new System.Drawing.Size(409, 83);
            this.groupBoxTcp.TabIndex = 6;
            this.groupBoxTcp.TabStop = false;
            this.groupBoxTcp.Text = "TCP配置";
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(60, 22);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(120, 21);
            this.txtIP.TabIndex = 3;
            this.txtIP.Text = "127.0.0.1";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(60, 52);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(120, 21);
            this.txtPort.TabIndex = 2;
            this.txtPort.Text = "502";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(10, 56);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 12);
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "端口";
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(10, 26);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(41, 12);
            this.lblIP.TabIndex = 0;
            this.lblIP.Text = "IP地址";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnClearLog);
            this.groupBox3.Controls.Add(this.btnSendData);
            this.groupBox3.Controls.Add(this.btnCloseSerial);
            this.groupBox3.Controls.Add(this.btnOpenSerial);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(388, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(409, 42);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "功能按钮";
            // 
            // btnDbClear
            // 
            this.btnDbClear.Location = new System.Drawing.Point(184, 20);
            this.btnDbClear.Name = "btnDbClear";
            this.btnDbClear.Size = new System.Drawing.Size(55, 23);
            this.btnDbClear.TabIndex = 6;
            this.btnDbClear.Text = "清空";
            this.btnDbClear.UseVisualStyleBackColor = true;
            this.btnDbClear.Click += new System.EventHandler(this.btnDbClear_Click);
            // 
            // btnDbDelete
            // 
            this.btnDbDelete.Location = new System.Drawing.Point(90, 54);
            this.btnDbDelete.Name = "btnDbDelete";
            this.btnDbDelete.Size = new System.Drawing.Size(60, 23);
            this.btnDbDelete.TabIndex = 5;
            this.btnDbDelete.Text = "删除";
            this.btnDbDelete.UseVisualStyleBackColor = true;
            this.btnDbDelete.Click += new System.EventHandler(this.btnDbDelete_Click);
            // 
            // btnDbUpdate
            // 
            this.btnDbUpdate.Location = new System.Drawing.Point(184, 54);
            this.btnDbUpdate.Name = "btnDbUpdate";
            this.btnDbUpdate.Size = new System.Drawing.Size(55, 23);
            this.btnDbUpdate.TabIndex = 4;
            this.btnDbUpdate.Text = "修改";
            this.btnDbUpdate.UseVisualStyleBackColor = true;
            this.btnDbUpdate.Click += new System.EventHandler(this.btnDbUpdate_Click);
            // 
            // btnDbQuery
            // 
            this.btnDbQuery.Location = new System.Drawing.Point(268, 54);
            this.btnDbQuery.Name = "btnDbQuery";
            this.btnDbQuery.Size = new System.Drawing.Size(55, 23);
            this.btnDbQuery.TabIndex = 3;
            this.btnDbQuery.Text = "查询";
            this.btnDbQuery.UseVisualStyleBackColor = true;
            this.btnDbQuery.Click += new System.EventHandler(this.btnDbQuery_Click);
            // 
            // btnDbCreate
            // 
            this.btnDbCreate.Location = new System.Drawing.Point(9, 54);
            this.btnDbCreate.Name = "btnDbCreate";
            this.btnDbCreate.Size = new System.Drawing.Size(60, 23);
            this.btnDbCreate.TabIndex = 2;
            this.btnDbCreate.Text = "新增";
            this.btnDbCreate.UseVisualStyleBackColor = true;
            this.btnDbCreate.Click += new System.EventHandler(this.btnDbCreate_Click);
            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Location = new System.Drawing.Point(90, 17);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(60, 23);
            this.btnExportCsv.TabIndex = 1;
            this.btnExportCsv.Text = "导出CSV";
            this.btnExportCsv.UseVisualStyleBackColor = true;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);
            // 
            // btnExportTxt
            // 
            this.btnExportTxt.Location = new System.Drawing.Point(9, 18);
            this.btnExportTxt.Name = "btnExportTxt";
            this.btnExportTxt.Size = new System.Drawing.Size(60, 23);
            this.btnExportTxt.TabIndex = 0;
            this.btnExportTxt.Text = "导出TXT";
            this.btnExportTxt.UseVisualStyleBackColor = true;
            this.btnExportTxt.Click += new System.EventHandler(this.btnExportTxt_Click);
            // 
            // groupBoxBatch
            // 
            this.groupBoxBatch.Controls.Add(this.lblBatchWriteValue);
            this.groupBoxBatch.Controls.Add(this.txtBatchWriteValue);
            this.groupBoxBatch.Controls.Add(this.btnBatchWrite);
            this.groupBoxBatch.Controls.Add(this.lblBatchCount);
            this.groupBoxBatch.Controls.Add(this.numBatchCount);
            this.groupBoxBatch.Controls.Add(this.lblStartAddr);
            this.groupBoxBatch.Controls.Add(this.numStartAddr);
            this.groupBoxBatch.Controls.Add(this.btnBatchRead);
            this.groupBoxBatch.Controls.Add(this.lblSlaveAddr);
            this.groupBoxBatch.Controls.Add(this.numSlaveAddr);
            this.groupBoxBatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxBatch.Location = new System.Drawing.Point(388, 140);
            this.groupBoxBatch.Name = "groupBoxBatch";
            this.groupBoxBatch.Size = new System.Drawing.Size(409, 88);
            this.groupBoxBatch.TabIndex = 8;
            this.groupBoxBatch.TabStop = false;
            this.groupBoxBatch.Text = "批量操作";
            // 
            // lblBatchWriteValue
            // 
            this.lblBatchWriteValue.AutoSize = true;
            this.lblBatchWriteValue.Location = new System.Drawing.Point(205, 54);
            this.lblBatchWriteValue.Name = "lblBatchWriteValue";
            this.lblBatchWriteValue.Size = new System.Drawing.Size(59, 12);
            this.lblBatchWriteValue.TabIndex = 8;
            this.lblBatchWriteValue.Text = "写入值(,)";
            // 
            // txtBatchWriteValue
            // 
            this.txtBatchWriteValue.Location = new System.Drawing.Point(265, 50);
            this.txtBatchWriteValue.Name = "txtBatchWriteValue";
            this.txtBatchWriteValue.Size = new System.Drawing.Size(130, 21);
            this.txtBatchWriteValue.TabIndex = 9;
            this.txtBatchWriteValue.Text = "100,200";
            // 
            // btnBatchWrite
            // 
            this.btnBatchWrite.Location = new System.Drawing.Point(110, 50);
            this.btnBatchWrite.Name = "btnBatchWrite";
            this.btnBatchWrite.Size = new System.Drawing.Size(85, 23);
            this.btnBatchWrite.TabIndex = 7;
            this.btnBatchWrite.Text = "批量写入";
            this.btnBatchWrite.UseVisualStyleBackColor = true;
            this.btnBatchWrite.Click += new System.EventHandler(this.btnBatchWrite_Click);
            // 
            // lblBatchCount
            // 
            this.lblBatchCount.AutoSize = true;
            this.lblBatchCount.Location = new System.Drawing.Point(260, 22);
            this.lblBatchCount.Name = "lblBatchCount";
            this.lblBatchCount.Size = new System.Drawing.Size(29, 12);
            this.lblBatchCount.TabIndex = 4;
            this.lblBatchCount.Text = "数量";
            // 
            // numBatchCount
            // 
            this.numBatchCount.Location = new System.Drawing.Point(300, 18);
            this.numBatchCount.Maximum = new decimal(new int[] {
            125,
            0,
            0,
            0});
            this.numBatchCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBatchCount.Name = "numBatchCount";
            this.numBatchCount.Size = new System.Drawing.Size(50, 21);
            this.numBatchCount.TabIndex = 5;
            this.numBatchCount.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // lblStartAddr
            // 
            this.lblStartAddr.AutoSize = true;
            this.lblStartAddr.Location = new System.Drawing.Point(130, 22);
            this.lblStartAddr.Name = "lblStartAddr";
            this.lblStartAddr.Size = new System.Drawing.Size(53, 12);
            this.lblStartAddr.TabIndex = 2;
            this.lblStartAddr.Text = "起始地址";
            // 
            // numStartAddr
            // 
            this.numStartAddr.Location = new System.Drawing.Point(190, 18);
            this.numStartAddr.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numStartAddr.Name = "numStartAddr";
            this.numStartAddr.Size = new System.Drawing.Size(60, 21);
            this.numStartAddr.TabIndex = 3;
            // 
            // btnBatchRead
            // 
            this.btnBatchRead.Location = new System.Drawing.Point(10, 50);
            this.btnBatchRead.Name = "btnBatchRead";
            this.btnBatchRead.Size = new System.Drawing.Size(85, 23);
            this.btnBatchRead.TabIndex = 6;
            this.btnBatchRead.Text = "批量读取";
            this.btnBatchRead.UseVisualStyleBackColor = true;
            this.btnBatchRead.Click += new System.EventHandler(this.btnBatchRead_Click);
            // 
            // lblSlaveAddr
            // 
            this.lblSlaveAddr.AutoSize = true;
            this.lblSlaveAddr.Location = new System.Drawing.Point(10, 22);
            this.lblSlaveAddr.Name = "lblSlaveAddr";
            this.lblSlaveAddr.Size = new System.Drawing.Size(53, 12);
            this.lblSlaveAddr.TabIndex = 0;
            this.lblSlaveAddr.Text = "从站地址";
            // 
            // numSlaveAddr
            // 
            this.numSlaveAddr.Location = new System.Drawing.Point(70, 18);
            this.numSlaveAddr.Maximum = new decimal(new int[] {
            247,
            0,
            0,
            0});
            this.numSlaveAddr.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSlaveAddr.Name = "numSlaveAddr";
            this.numSlaveAddr.Size = new System.Drawing.Size(50, 21);
            this.numSlaveAddr.TabIndex = 1;
            this.numSlaveAddr.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // timerReceive
            // 
            this.timerReceive.Interval = 1000;
            this.timerReceive.Tick += new System.EventHandler(this.timerReceive_Tick);
            // 
            // timerTimeOut
            // 
            this.timerTimeOut.Interval = 5000;
            this.timerTimeOut.Tick += new System.EventHandler(this.timerTimeOut_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 550);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Modbus温湿度采集系统 - 批量读写版";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBoxSerial.ResumeLayout(false);
            this.groupBoxSerial.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvModbus)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxTcp.ResumeLayout(false);
            this.groupBoxTcp.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBoxBatch.ResumeLayout(false);
            this.groupBoxBatch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBatchCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartAddr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSlaveAddr)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBoxSerial;
        private System.Windows.Forms.Button btnCloseSerial;
        private System.Windows.Forms.Button btnOpenSerial;
        private System.Windows.Forms.ComboBox cboBaudRate;
        private System.Windows.Forms.ComboBox cboPortName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSendData;
        private System.Windows.Forms.Timer timerReceive;
        private System.Windows.Forms.DataGridView dgvModbus;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Timer timerTimeOut;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblProtcol;
        private System.Windows.Forms.ComboBox cboProtocol;
        private System.Windows.Forms.GroupBox groupBoxTcp;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnExportTxt;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Button btnDbClear;
        private System.Windows.Forms.Button btnDbDelete;
        private System.Windows.Forms.Button btnDbUpdate;
        private System.Windows.Forms.Button btnDbQuery;
        private System.Windows.Forms.Button btnDbCreate;
        
        // 批量操作控件
        private System.Windows.Forms.GroupBox groupBoxBatch;
        private System.Windows.Forms.Label lblSlaveAddr;
        private System.Windows.Forms.NumericUpDown numSlaveAddr;
        private System.Windows.Forms.Label lblStartAddr;
        private System.Windows.Forms.NumericUpDown numStartAddr;
        private System.Windows.Forms.Label lblBatchCount;
        private System.Windows.Forms.NumericUpDown numBatchCount;
        private System.Windows.Forms.Button btnBatchRead;
        private System.Windows.Forms.Button btnBatchWrite;
        private System.Windows.Forms.Label lblBatchWriteValue;
        private System.Windows.Forms.TextBox txtBatchWriteValue;
        
        // DataGridView列
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
    }
}
