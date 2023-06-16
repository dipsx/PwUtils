using PWFrameWork;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PW_PacketListener {
    public class frmMain : Form {
        private bool bInjectMode;

        private cPacket pSendPacket = new cPacket();

        private cPacketInspector PacketInspector = new cPacketInspector();

        private cHookModule Hook = new cHookModule();

        private myClientFinder ClientFinder;

        private IContainer components;

        private System.Windows.Forms.Timer Timer;

        private Button cmdStop;

        private ComboBox cmbPW;

        private Button cmdRefreshPW;

        private TabControl tabControl1;

        private TabPage tabPage1;

        private TabPage tab_PacketProcessor;

        private ListBox lstPackets;

        private TabPage tabPage4;

        private Label label3;

        private Label label2;

        private Label label1;

        private Button cmdSendPacket;

        private Label label4;

        private TextBox txtSendPacket;

        private Label label5;

        private Label lblSendPacket;

        private Label label6;

        private GroupBox groupBox1;

        private Button cmdStart;

        private GroupBox groupBox2;

        private Button cmdInjectOn;

        private Button cmdInjectOff;

        private TabPage tabPage5;

        private Label label7;

        private ComboBox cmbKnownPackets;

        private WebBrowser webPacket;

        private Button SaveToBin;

        private Button SaveToTXT;

        private System.Windows.Forms.ContextMenuStrip contextMenulstPackets;

        private ToolStripMenuItem menuPacketCopy;

        private ToolStripMenuItem menuPacketSend;

        private Button cmdClearList;

        private System.Windows.Forms.Timer timPacketSend;

        private GroupBox groupBox3;

        private Button cmdTimerSendPacketOff;

        private Button cmdTimerSendPacketOn;

        private Label label8;

        private TextBox txtSendPacketInterval;

        public frmMain() {
            this.InitializeComponent();
        }

        private void cmbKnownPackets_SelectedIndexChanged(object sender, EventArgs e) {
            this.webPacket.DocumentText = ((cKnownPacket)this.cmbKnownPackets.SelectedItem).GetDescription();
        }

        private void cmdClearList_Click(object sender, EventArgs e) {
            this.lstPackets.Items.Clear();
        }

        private void cmdInjectOff_Click(object sender, EventArgs e) {
            this.cmdStop_Click(null, null);
            MemoryManager.CloseProcess();
            this.bInjectMode = false;
            this.RefreshInterface();
        }

        private void cmdInjectOn_Click(object sender, EventArgs e) {
            ClientWindow item = this.cmbPW.Items[this.cmbPW.SelectedIndex] as ClientWindow;
            MemoryManager.OpenProcess(item.ProcessId);
            this.bInjectMode = true;
            this.RefreshInterface();
        }

        private void cmdRefreshPW_Click(object sender, EventArgs e) {
            this.RefreshPW();
        }


        cPacketInjection _cPacketInjection = new();
        private async void cmdSendPacket_Click(object sender, EventArgs e) {
            if (this.pSendPacket.isEmpty())
                return;

            await _cPacketInjection.SendPacket(this.pSendPacket.GetPacket());
        }

        private void cmdStart_Click(object sender, EventArgs e) {
            this.Hook.StartHook();
            this.Timer.Enabled = true;
            this.RefreshInterface();
        }

        private void cmdStop_Click(object sender, EventArgs e) {
            if (this.Timer.Enabled) {
                this.Timer.Enabled = false;
                this.Hook.StopHook();
                this.RefreshInterface();
            }
        }

        private void cmdTimerSendPacketOff_Click(object sender, EventArgs e) {
        }

        private void cmdTimerSendPacketOn_Click(object sender, EventArgs e) {
        }

        protected override void Dispose(bool disposing) {
            if (disposing && this.components != null) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            this.cmdStop_Click(null, null);
            this.cmdInjectOff_Click(null, null);
        }

        private void frmMain_HelpButtonClicked(object sender, CancelEventArgs e) {
            MessageBox.Show("PW PacketListener. N00bSa1b0t. 2011-2013");
            e.Cancel = true;
        }

        private void frmMain_Load(object sender, EventArgs e) {
            this.tabControl1.TabPages.Remove(this.tab_PacketProcessor);
            cOptions.ReadConfigFile();
            this.ClientFinder = new myClientFinder();
            this.RefreshInterface();
            this.RefreshPW();
            this.cmbKnownPackets.Items.AddRange(this.PacketInspector.GetPackets());
        }

        private void InitializeComponent() {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(frmMain));
            Timer = new Timer(components);
            cmdStop = new Button();
            cmbPW = new ComboBox();
            cmdRefreshPW = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            cmdClearList = new Button();
            SaveToBin = new Button();
            SaveToTXT = new Button();
            lstPackets = new ListBox();
            tab_PacketProcessor = new TabPage();
            webPacket = new WebBrowser();
            cmbKnownPackets = new ComboBox();
            tabPage4 = new TabPage();
            groupBox3 = new GroupBox();
            cmdTimerSendPacketOff = new Button();
            cmdTimerSendPacketOn = new Button();
            label8 = new Label();
            txtSendPacketInterval = new TextBox();
            lblSendPacket = new Label();
            label6 = new Label();
            txtSendPacket = new TextBox();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            cmdSendPacket = new Button();
            tabPage5 = new TabPage();
            label7 = new Label();
            contextMenulstPackets = new ContextMenuStrip(components);
            menuPacketCopy = new ToolStripMenuItem();
            menuPacketSend = new ToolStripMenuItem();
            groupBox1 = new GroupBox();
            cmdStart = new Button();
            groupBox2 = new GroupBox();
            cmdInjectOn = new Button();
            cmdInjectOff = new Button();
            timPacketSend = new Timer(components);
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tab_PacketProcessor.SuspendLayout();
            tabPage4.SuspendLayout();
            groupBox3.SuspendLayout();
            tabPage5.SuspendLayout();
            contextMenulstPackets.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // Timer
            // 
            Timer.Interval = 1;
            Timer.Tick += Timer_Tick;
            // 
            // cmdStop
            // 
            cmdStop.Location = new Point(5, 80);
            cmdStop.Margin = new Padding(4, 3, 4, 3);
            cmdStop.Name = "cmdStop";
            cmdStop.Size = new Size(220, 53);
            cmdStop.TabIndex = 1;
            cmdStop.Text = "Стоп";
            cmdStop.UseVisualStyleBackColor = true;
            cmdStop.Click += cmdStop_Click;
            // 
            // cmbPW
            // 
            cmbPW.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPW.FormattingEnabled = true;
            cmbPW.Location = new Point(14, 14);
            cmbPW.Margin = new Padding(4, 3, 4, 3);
            cmbPW.Name = "cmbPW";
            cmbPW.Size = new Size(231, 23);
            cmbPW.TabIndex = 2;
            // 
            // cmdRefreshPW
            // 
            cmdRefreshPW.Location = new Point(14, 482);
            cmdRefreshPW.Margin = new Padding(4, 3, 4, 3);
            cmdRefreshPW.Name = "cmdRefreshPW";
            cmdRefreshPW.Size = new Size(232, 27);
            cmdRefreshPW.TabIndex = 3;
            cmdRefreshPW.Text = "Поискать окна PW еще разок";
            cmdRefreshPW.UseVisualStyleBackColor = true;
            cmdRefreshPW.Click += cmdRefreshPW_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tab_PacketProcessor);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Location = new Point(253, 14);
            tabControl1.Margin = new Padding(4, 3, 4, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(761, 495);
            tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(cmdClearList);
            tabPage1.Controls.Add(SaveToBin);
            tabPage1.Controls.Add(SaveToTXT);
            tabPage1.Controls.Add(lstPackets);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Margin = new Padding(4, 3, 4, 3);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4, 3, 4, 3);
            tabPage1.Size = new Size(753, 467);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Пакеты";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // cmdClearList
            // 
            cmdClearList.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdClearList.Location = new Point(586, 432);
            cmdClearList.Margin = new Padding(4, 3, 4, 3);
            cmdClearList.Name = "cmdClearList";
            cmdClearList.Size = new Size(159, 27);
            cmdClearList.TabIndex = 4;
            cmdClearList.Text = "Очистить список";
            cmdClearList.UseVisualStyleBackColor = true;
            cmdClearList.Click += cmdClearList_Click;
            // 
            // SaveToBin
            // 
            SaveToBin.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            SaveToBin.Enabled = false;
            SaveToBin.Location = new Point(420, 432);
            SaveToBin.Margin = new Padding(4, 3, 4, 3);
            SaveToBin.Name = "SaveToBin";
            SaveToBin.Size = new Size(159, 27);
            SaveToBin.TabIndex = 3;
            SaveToBin.Text = "Сохранить пакеты в bin";
            SaveToBin.UseVisualStyleBackColor = true;
            // 
            // SaveToTXT
            // 
            SaveToTXT.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            SaveToTXT.Location = new Point(254, 432);
            SaveToTXT.Margin = new Padding(4, 3, 4, 3);
            SaveToTXT.Name = "SaveToTXT";
            SaveToTXT.Size = new Size(159, 27);
            SaveToTXT.TabIndex = 2;
            SaveToTXT.Text = "Сохранить пакеты в txt";
            SaveToTXT.UseVisualStyleBackColor = true;
            SaveToTXT.Click += SaveToTXT_Click;
            // 
            // lstPackets
            // 
            lstPackets.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstPackets.Font = new Font("Lucida Console", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            lstPackets.FormattingEnabled = true;
            lstPackets.Location = new Point(4, 3);
            lstPackets.Margin = new Padding(4, 3, 4, 3);
            lstPackets.Name = "lstPackets";
            lstPackets.Size = new Size(744, 420);
            lstPackets.TabIndex = 0;
            lstPackets.SelectedIndexChanged += lstPackets_SelectedIndexChanged;
            lstPackets.MouseDown += lstPackets_MouseDown;
            lstPackets.MouseUp += lstPackets_MouseUp;
            // 
            // tab_PacketProcessor
            // 
            tab_PacketProcessor.Controls.Add(webPacket);
            tab_PacketProcessor.Controls.Add(cmbKnownPackets);
            tab_PacketProcessor.Location = new Point(4, 24);
            tab_PacketProcessor.Margin = new Padding(4, 3, 4, 3);
            tab_PacketProcessor.Name = "tab_PacketProcessor";
            tab_PacketProcessor.Size = new Size(753, 467);
            tab_PacketProcessor.TabIndex = 2;
            tab_PacketProcessor.Text = "Распознавание пакетов";
            tab_PacketProcessor.UseVisualStyleBackColor = true;
            // 
            // webPacket
            // 
            webPacket.Location = new Point(4, 38);
            webPacket.Margin = new Padding(4, 3, 4, 3);
            webPacket.MinimumSize = new Size(23, 23);
            webPacket.Name = "webPacket";
            webPacket.ScriptErrorsSuppressed = true;
            webPacket.Size = new Size(744, 423);
            webPacket.TabIndex = 1;
            // 
            // cmbKnownPackets
            // 
            cmbKnownPackets.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKnownPackets.FormattingEnabled = true;
            cmbKnownPackets.Location = new Point(4, 7);
            cmbKnownPackets.Margin = new Padding(4, 3, 4, 3);
            cmbKnownPackets.Name = "cmbKnownPackets";
            cmbKnownPackets.Size = new Size(318, 23);
            cmbKnownPackets.TabIndex = 0;
            cmbKnownPackets.SelectedIndexChanged += cmbKnownPackets_SelectedIndexChanged;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(groupBox3);
            tabPage4.Controls.Add(lblSendPacket);
            tabPage4.Controls.Add(label6);
            tabPage4.Controls.Add(txtSendPacket);
            tabPage4.Controls.Add(label5);
            tabPage4.Controls.Add(label4);
            tabPage4.Controls.Add(label3);
            tabPage4.Controls.Add(label2);
            tabPage4.Controls.Add(label1);
            tabPage4.Controls.Add(cmdSendPacket);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Margin = new Padding(4, 3, 4, 3);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(753, 467);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Отправка пакетов";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(cmdTimerSendPacketOff);
            groupBox3.Controls.Add(cmdTimerSendPacketOn);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(txtSendPacketInterval);
            groupBox3.Location = new Point(407, 107);
            groupBox3.Margin = new Padding(4, 3, 4, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 3, 4, 3);
            groupBox3.Size = new Size(331, 91);
            groupBox3.TabIndex = 9;
            groupBox3.TabStop = false;
            groupBox3.Text = "Повторяющаяся отправка";
            groupBox3.Visible = false;
            // 
            // cmdTimerSendPacketOff
            // 
            cmdTimerSendPacketOff.Enabled = false;
            cmdTimerSendPacketOff.Location = new Point(112, 58);
            cmdTimerSendPacketOff.Margin = new Padding(4, 3, 4, 3);
            cmdTimerSendPacketOff.Name = "cmdTimerSendPacketOff";
            cmdTimerSendPacketOff.Size = new Size(88, 27);
            cmdTimerSendPacketOff.TabIndex = 3;
            cmdTimerSendPacketOff.Text = "Выкл";
            cmdTimerSendPacketOff.UseVisualStyleBackColor = true;
            cmdTimerSendPacketOff.Visible = false;
            cmdTimerSendPacketOff.Click += cmdTimerSendPacketOff_Click;
            // 
            // cmdTimerSendPacketOn
            // 
            cmdTimerSendPacketOn.Location = new Point(7, 58);
            cmdTimerSendPacketOn.Margin = new Padding(4, 3, 4, 3);
            cmdTimerSendPacketOn.Name = "cmdTimerSendPacketOn";
            cmdTimerSendPacketOn.Size = new Size(88, 27);
            cmdTimerSendPacketOn.TabIndex = 2;
            cmdTimerSendPacketOn.Text = "Вкл";
            cmdTimerSendPacketOn.UseVisualStyleBackColor = true;
            cmdTimerSendPacketOn.Visible = false;
            cmdTimerSendPacketOn.Click += cmdTimerSendPacketOn_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(90, 23);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(74, 15);
            label8.TabIndex = 1;
            label8.Text = "милисекунд";
            label8.Visible = false;
            // 
            // txtSendPacketInterval
            // 
            txtSendPacketInterval.Location = new Point(7, 20);
            txtSendPacketInterval.Margin = new Padding(4, 3, 4, 3);
            txtSendPacketInterval.Name = "txtSendPacketInterval";
            txtSendPacketInterval.Size = new Size(75, 23);
            txtSendPacketInterval.TabIndex = 0;
            txtSendPacketInterval.Text = "200";
            txtSendPacketInterval.Visible = false;
            // 
            // lblSendPacket
            // 
            lblSendPacket.AutoSize = true;
            lblSendPacket.Location = new Point(7, 348);
            lblSendPacket.Margin = new Padding(4, 0, 4, 0);
            lblSendPacket.Name = "lblSendPacket";
            lblSendPacket.Size = new Size(0, 15);
            lblSendPacket.TabIndex = 8;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(7, 323);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(258, 15);
            label6.TabIndex = 7;
            label6.Text = "Таким Ваш пакет увидела данная программа:";
            // 
            // txtSendPacket
            // 
            txtSendPacket.Location = new Point(10, 250);
            txtSendPacket.Margin = new Padding(4, 3, 4, 3);
            txtSendPacket.Name = "txtSendPacket";
            txtSendPacket.Size = new Size(737, 23);
            txtSendPacket.TabIndex = 6;
            txtSendPacket.TextChanged += txtSendPacket_TextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(7, 202);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(476, 45);
            label5.TabIndex = 5;
            label5.Text = "Отправленный пакет также будет пойман программой, если включен режим ловли.\r\n\r\nВведите свой пакет в поле:\r\n";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(7, 127);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(34, 60);
            label4.TabIndex = 4;
            label4.Text = "55 00\r\n0800\r\n2e 00\r\n2f00";
            // 
            // label3
            // 
            label3.Location = new Point(7, 83);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(744, 35);
            label3.TabIndex = 3;
            label3.Text = "Пакеты вводить в шестандцатеричном виде. Пробел между байтами можно оставлять, можно и убирать. Ниже пример правильных пакетов:";
            // 
            // label2
            // 
            label2.Location = new Point(4, 46);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(744, 37);
            label2.TabIndex = 2;
            label2.Text = resources.GetString("label2.Text");
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(4, 7);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(77, 13);
            label1.TabIndex = 1;
            label1.Text = "ВНИМАНИЕ";
            // 
            // cmdSendPacket
            // 
            cmdSendPacket.Location = new Point(10, 280);
            cmdSendPacket.Margin = new Padding(4, 3, 4, 3);
            cmdSendPacket.Name = "cmdSendPacket";
            cmdSendPacket.Size = new Size(142, 27);
            cmdSendPacket.TabIndex = 0;
            cmdSendPacket.Text = "Отправить";
            cmdSendPacket.UseVisualStyleBackColor = true;
            cmdSendPacket.Click += cmdSendPacket_Click;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(label7);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Margin = new Padding(4, 3, 4, 3);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new Size(753, 467);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Помощь";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.Dock = DockStyle.Fill;
            label7.Location = new Point(0, 0);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(753, 467);
            label7.TabIndex = 0;
            label7.Text = "1. Сначала выберите окно игры в списке в левом верхнем углу.\r\n2. Далее надо внедриться в процесс\r\n3. После этого можно либо отправлять пакеты в игру, либо запустить прослушку отправленных пакетов.";
            // 
            // contextMenulstPackets
            // 
            contextMenulstPackets.Items.AddRange(new ToolStripItem[] { menuPacketCopy, menuPacketSend });
            contextMenulstPackets.Name = "contextMenulstPackets";
            contextMenulstPackets.Size = new Size(140, 48);
            // 
            // menuPacketCopy
            // 
            menuPacketCopy.Name = "menuPacketCopy";
            menuPacketCopy.Size = new Size(139, 22);
            menuPacketCopy.Text = "Копировать";
            menuPacketCopy.Click += menuPacketCopy_Click;
            // 
            // menuPacketSend
            // 
            menuPacketSend.Name = "menuPacketSend";
            menuPacketSend.Size = new Size(139, 22);
            menuPacketSend.Text = "Отправить";
            menuPacketSend.Click += menuPacketSend_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(cmdStart);
            groupBox1.Controls.Add(cmdStop);
            groupBox1.Location = new Point(14, 203);
            groupBox1.Margin = new Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 3, 4, 3);
            groupBox1.Size = new Size(232, 143);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Захват пакетов";
            // 
            // cmdStart
            // 
            cmdStart.Location = new Point(7, 20);
            cmdStart.Margin = new Padding(4, 3, 4, 3);
            cmdStart.Name = "cmdStart";
            cmdStart.Size = new Size(218, 53);
            cmdStart.TabIndex = 1;
            cmdStart.Text = "Старт";
            cmdStart.UseVisualStyleBackColor = true;
            cmdStart.Click += cmdStart_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(cmdInjectOn);
            groupBox2.Controls.Add(cmdInjectOff);
            groupBox2.Location = new Point(14, 46);
            groupBox2.Margin = new Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 3, 4, 3);
            groupBox2.Size = new Size(232, 143);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            groupBox2.Text = "Внедрение в процесс";
            // 
            // cmdInjectOn
            // 
            cmdInjectOn.Location = new Point(7, 20);
            cmdInjectOn.Margin = new Padding(4, 3, 4, 3);
            cmdInjectOn.Name = "cmdInjectOn";
            cmdInjectOn.Size = new Size(218, 53);
            cmdInjectOn.TabIndex = 1;
            cmdInjectOn.Text = "Вкл";
            cmdInjectOn.UseVisualStyleBackColor = true;
            cmdInjectOn.Click += cmdInjectOn_Click;
            // 
            // cmdInjectOff
            // 
            cmdInjectOff.Location = new Point(5, 80);
            cmdInjectOff.Margin = new Padding(4, 3, 4, 3);
            cmdInjectOff.Name = "cmdInjectOff";
            cmdInjectOff.Size = new Size(220, 53);
            cmdInjectOff.TabIndex = 1;
            cmdInjectOff.Text = "Выкл";
            cmdInjectOff.UseVisualStyleBackColor = true;
            cmdInjectOff.Click += cmdInjectOff_Click;
            // 
            // timPacketSend
            // 
            timPacketSend.Tick += timPacketSend_Tick;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1028, 523);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(cmdRefreshPW);
            Controls.Add(tabControl1);
            Controls.Add(cmbPW);
            HelpButton = true;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(1044, 561);
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PWPacketListener";
            HelpButtonClicked += frmMain_HelpButtonClicked;
            FormClosing += frmMain_FormClosing;
            Load += frmMain_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tab_PacketProcessor.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            tabPage5.ResumeLayout(false);
            contextMenulstPackets.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void lstPackets_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Right) {
                return;
            }
            this.lstPackets.SelectedIndex = this.lstPackets.IndexFromPoint(new Point(e.X, e.Y));
        }

        private void lstPackets_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Right) {
                return;
            }
            if (this.lstPackets.SelectedIndex == -1) {
                return;
            }
            this.contextMenulstPackets.Show(this.lstPackets, e.X, e.Y);
        }

        private void lstPackets_SelectedIndexChanged(object sender, EventArgs e) {
        }

        private void menuPacketCopy_Click(object sender, EventArgs e) {
            if (this.lstPackets.SelectedIndex > -1) {
                Clipboard.SetText(this.lstPackets.SelectedItem.ToString());
            }
        }

        private async void menuPacketSend_Click(object sender, EventArgs e) {
            if (!this.bInjectMode || this.lstPackets.SelectedIndex < 0) {
                return;
            }
            cPacketInjection _cPacketInjection = new cPacketInjection();
            await _cPacketInjection.SendPacket(((cPacket)this.lstPackets.SelectedItem).GetPacket());
        }

        private void RefreshInterface() {
            this.cmdStart.Enabled = (!this.bInjectMode ? false : !this.Timer.Enabled);
            this.cmdStop.Enabled = (!this.bInjectMode ? false : this.Timer.Enabled);
            this.cmdRefreshPW.Enabled = (this.bInjectMode ? false : !this.Timer.Enabled);
            this.cmbPW.Enabled = (this.bInjectMode ? false : !this.Timer.Enabled);
            this.cmdInjectOn.Enabled = !this.bInjectMode;
            this.cmdInjectOff.Enabled = this.bInjectMode;
            this.cmdSendPacket.Enabled = this.bInjectMode;
        }

        private void RefreshPW() {
            this.cmbPW.Items.Clear();
            this.cmbPW.Items.AddRange(this.ClientFinder.GetWindows());
            if (this.cmbPW.Items.Count <= 0) {
                MessageBox.Show("Клиент PW не был найден. Запустите игру и нажмите на кнопку ниже.");
                this.cmdInjectOn.Enabled = false;
                return;
            }
            this.cmbPW.SelectedIndex = 0;
            this.cmdInjectOn.Enabled = true;
        }

        private void SaveToTXT_Click(object sender, EventArgs e) {
            if (this.lstPackets.Items.Count == 0) {
                MessageBox.Show("Нет пакетов, нечего сохранять.");
                return;
            }
            string str = "";
            for (int i = 0; i < this.lstPackets.Items.Count; i++) {
                str = string.Concat(str, this.lstPackets.Items[i], "\r\n");
            }
            DateTime now = DateTime.Now;
            string str1 = string.Concat("packets_", now.ToString(), ".txt");
            str1 = str1.Replace(":", ".");
            StreamWriter streamWriter = File.CreateText(str1);
            streamWriter.Write(str);
            streamWriter.Close();
            MessageBox.Show(string.Concat("Пакеты сохранены в файл ", str1));
        }

        private async void Timer_Tick(object sender, EventArgs e) {
            byte[] numArray = this.Hook.TimerTick();
            if (numArray != null && !cIgnore.IsPacketIgnored(numArray)) {
                this.lstPackets.Items.Add(this.PacketInspector.ParseByteArray(numArray));
            }
            await Task.FromResult(0);
        }

        private void timPacketSend_Tick(object sender, EventArgs e) {
        }

        private void txtSendPacket_TextChanged(object sender, EventArgs e) {
            this.pSendPacket.ConvertFromString(this.txtSendPacket.Text);
            this.lblSendPacket.Text = this.pSendPacket.ToString();
        }
    }
}