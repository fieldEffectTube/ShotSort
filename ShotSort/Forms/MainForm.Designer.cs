namespace ShotSort.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelInput = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.panelPathCard = new System.Windows.Forms.Panel();
            this.txtFolderPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.panelOptionsCard = new System.Windows.Forms.Panel();
            this.chkLoadJpg = new System.Windows.Forms.CheckBox();
            this.lblJpgDesc = new System.Windows.Forms.Label();
            this.chkLoadRaw = new System.Windows.Forms.CheckBox();
            this.lblRawDesc = new System.Windows.Forms.Label();
            this.btnStartRead = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.panelResult = new System.Windows.Forms.Panel();
            this.lstPhotos = new System.Windows.Forms.ListView();
            this.thumbnailImageList = new System.Windows.Forms.ImageList(new System.ComponentModel.Container());
            this.panelResultRight = new System.Windows.Forms.Panel();
            this.lblScanTitle = new System.Windows.Forms.Label();
            this.panelStatsCard = new System.Windows.Forms.Panel();
            this.lblJpgCount = new System.Windows.Forms.Label();
            this.lblRawCount = new System.Windows.Forms.Label();
            this.lblTotalCount = new System.Windows.Forms.Label();
            this.panelDivider1 = new System.Windows.Forms.Panel();
            this.rbAiMode = new System.Windows.Forms.RadioButton();
            this.lblAiDesc = new System.Windows.Forms.Label();
            this.rbManualMode = new System.Windows.Forms.RadioButton();
            this.lblManualDesc = new System.Windows.Forms.Label();
            this.btnStartProcess = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.panelAiProgress = new System.Windows.Forms.Panel();
            this.lblAiTitle = new System.Windows.Forms.Label();
            this.lblAiStatus = new System.Windows.Forms.Label();
            this.progressBarAI = new System.Windows.Forms.ProgressBar();
            this.lblAiHint = new System.Windows.Forms.Label();
            this.panelInput.SuspendLayout();
            this.panelPathCard.SuspendLayout();
            this.panelOptionsCard.SuspendLayout();
            this.panelResult.SuspendLayout();
            this.panelResultRight.SuspendLayout();
            this.panelStatsCard.SuspendLayout();
            this.panelAiProgress.SuspendLayout();
            this.SuspendLayout();
            //
            // panelInput
            //
            this.panelInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.panelInput.Controls.Add(this.lblTitle);
            this.panelInput.Controls.Add(this.lblSubtitle);
            this.panelInput.Controls.Add(this.panelPathCard);
            this.panelInput.Controls.Add(this.panelOptionsCard);
            this.panelInput.Controls.Add(this.btnStartRead);
            this.panelInput.Controls.Add(this.lblVersion);
            this.panelInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelInput.Location = new System.Drawing.Point(0, 0);
            this.panelInput.Name = "panelInput";
            this.panelInput.Size = new System.Drawing.Size(800, 500);
            this.panelInput.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblTitle.Location = new System.Drawing.Point(50, 35);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(180, 52);
            this.lblTitle.TabIndex = 5;
            this.lblTitle.Text = "ShotSort";
            //
            // lblSubtitle
            //
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSubtitle.Location = new System.Drawing.Point(52, 88);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(196, 23);
            this.lblSubtitle.TabIndex = 6;
            this.lblSubtitle.Text = "智能照片批量分类工具";
            //
            // panelPathCard
            //
            this.panelPathCard.BackColor = System.Drawing.Color.White;
            this.panelPathCard.Controls.Add(this.txtFolderPath);
            this.panelPathCard.Controls.Add(this.btnBrowse);
            this.panelPathCard.Location = new System.Drawing.Point(50, 125);
            this.panelPathCard.Name = "panelPathCard";
            this.panelPathCard.Padding = new System.Windows.Forms.Padding(2);
            this.panelPathCard.Size = new System.Drawing.Size(700, 56);
            this.panelPathCard.TabIndex = 7;
            this.panelPathCard.Paint += new System.Windows.Forms.PaintEventHandler(this.CardPanel_Paint);
            //
            // txtFolderPath
            //
            this.txtFolderPath.BackColor = System.Drawing.Color.White;
            this.txtFolderPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtFolderPath.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.txtFolderPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtFolderPath.Location = new System.Drawing.Point(16, 16);
            this.txtFolderPath.Name = "txtFolderPath";
            this.txtFolderPath.ReadOnly = true;
            this.txtFolderPath.Size = new System.Drawing.Size(558, 25);
            this.txtFolderPath.TabIndex = 0;
            this.txtFolderPath.Text = "点击右侧按钮选择照片文件夹...";
            //
            // btnBrowse
            //
            this.btnBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowse.FlatAppearance.BorderSize = 0;
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Location = new System.Drawing.Point(584, 12);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(100, 32);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "选择文件夹";
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            this.btnBrowse.MouseEnter += new System.EventHandler(this.ButtonBlue_MouseEnter);
            this.btnBrowse.MouseLeave += new System.EventHandler(this.ButtonBlue_MouseLeave);
            //
            // panelOptionsCard
            //
            this.panelOptionsCard.BackColor = System.Drawing.Color.White;
            this.panelOptionsCard.Controls.Add(this.chkLoadJpg);
            this.panelOptionsCard.Controls.Add(this.lblJpgDesc);
            this.panelOptionsCard.Controls.Add(this.chkLoadRaw);
            this.panelOptionsCard.Controls.Add(this.lblRawDesc);
            this.panelOptionsCard.Location = new System.Drawing.Point(50, 200);
            this.panelOptionsCard.Name = "panelOptionsCard";
            this.panelOptionsCard.Size = new System.Drawing.Size(700, 125);
            this.panelOptionsCard.TabIndex = 8;
            this.panelOptionsCard.Paint += new System.Windows.Forms.PaintEventHandler(this.CardPanel_Paint);
            //
            // chkLoadJpg
            //
            this.chkLoadJpg.AutoSize = true;
            this.chkLoadJpg.Checked = true;
            this.chkLoadJpg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLoadJpg.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.chkLoadJpg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.chkLoadJpg.Location = new System.Drawing.Point(16, 15);
            this.chkLoadJpg.Name = "chkLoadJpg";
            this.chkLoadJpg.Size = new System.Drawing.Size(196, 27);
            this.chkLoadJpg.TabIndex = 0;
            this.chkLoadJpg.Text = "加载 JPG/JPEG 图片";
            this.chkLoadJpg.UseVisualStyleBackColor = true;
            //
            // lblJpgDesc
            //
            this.lblJpgDesc.AutoSize = true;
            this.lblJpgDesc.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblJpgDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblJpgDesc.Location = new System.Drawing.Point(38, 42);
            this.lblJpgDesc.Name = "lblJpgDesc";
            this.lblJpgDesc.Size = new System.Drawing.Size(230, 20);
            this.lblJpgDesc.TabIndex = 1;
            this.lblJpgDesc.Text = "通用照片格式，包括 .jpg / .jpeg";
            //
            // chkLoadRaw
            //
            this.chkLoadRaw.AutoSize = true;
            this.chkLoadRaw.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.chkLoadRaw.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.chkLoadRaw.Location = new System.Drawing.Point(16, 70);
            this.chkLoadRaw.Name = "chkLoadRaw";
            this.chkLoadRaw.Size = new System.Drawing.Size(296, 27);
            this.chkLoadRaw.TabIndex = 2;
            this.chkLoadRaw.Text = "加载 RAW 原图（CR2/CR3/DNG/NEF/RAF）";
            this.chkLoadRaw.UseVisualStyleBackColor = true;
            //
            // lblRawDesc
            //
            this.lblRawDesc.AutoSize = true;
            this.lblRawDesc.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblRawDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblRawDesc.Location = new System.Drawing.Point(38, 97);
            this.lblRawDesc.Name = "lblRawDesc";
            this.lblRawDesc.Size = new System.Drawing.Size(280, 20);
            this.lblRawDesc.TabIndex = 3;
            this.lblRawDesc.Text = "相机原始格式 CR2 / CR3 / DNG / NEF / RAF";
            //
            // btnStartRead
            //
            this.btnStartRead.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnStartRead.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartRead.FlatAppearance.BorderSize = 0;
            this.btnStartRead.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartRead.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartRead.ForeColor = System.Drawing.Color.White;
            this.btnStartRead.Location = new System.Drawing.Point(50, 350);
            this.btnStartRead.Name = "btnStartRead";
            this.btnStartRead.Size = new System.Drawing.Size(240, 50);
            this.btnStartRead.TabIndex = 4;
            this.btnStartRead.Text = "开始读取";
            this.btnStartRead.UseVisualStyleBackColor = false;
            this.btnStartRead.Click += new System.EventHandler(this.btnStartRead_Click);
            this.btnStartRead.MouseEnter += new System.EventHandler(this.ButtonPrimary_MouseEnter);
            this.btnStartRead.MouseLeave += new System.EventHandler(this.ButtonPrimary_MouseLeave);
            //
            // lblVersion
            //
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.lblVersion.Location = new System.Drawing.Point(50, 470);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(72, 18);
            this.lblVersion.TabIndex = 9;
            this.lblVersion.Text = "ShotSort v1.0";
            //
            // panelResult
            //
            this.panelResult.Controls.Add(this.panelAiProgress);
            this.panelResult.Controls.Add(this.panelResultRight);
            this.panelResult.Controls.Add(this.lstPhotos);
            this.panelResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelResult.Location = new System.Drawing.Point(0, 0);
            this.panelResult.Name = "panelResult";
            this.panelResult.Size = new System.Drawing.Size(800, 500);
            this.panelResult.TabIndex = 1;
            this.panelResult.Visible = false;
            //
            // thumbnailImageList
            //
            this.thumbnailImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.thumbnailImageList.ImageSize = new System.Drawing.Size(96, 96);
            //
            // lstPhotos
            //
            this.lstPhotos.BackColor = System.Drawing.Color.FromArgb(244, 245, 249);
            this.lstPhotos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstPhotos.Dock = System.Windows.Forms.DockStyle.Left;
            this.lstPhotos.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lstPhotos.LargeImageList = this.thumbnailImageList;
            this.lstPhotos.Location = new System.Drawing.Point(0, 0);
            this.lstPhotos.MultiSelect = false;
            this.lstPhotos.Name = "lstPhotos";
            this.lstPhotos.Size = new System.Drawing.Size(380, 500);
            this.lstPhotos.TabIndex = 0;
            this.lstPhotos.View = System.Windows.Forms.View.LargeIcon;
            //
            // panelResultRight
            //
            this.panelResultRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.panelResultRight.Controls.Add(this.lblScanTitle);
            this.panelResultRight.Controls.Add(this.panelStatsCard);
            this.panelResultRight.Controls.Add(this.panelDivider1);
            this.panelResultRight.Controls.Add(this.rbAiMode);
            this.panelResultRight.Controls.Add(this.lblAiDesc);
            this.panelResultRight.Controls.Add(this.rbManualMode);
            this.panelResultRight.Controls.Add(this.lblManualDesc);
            this.panelResultRight.Controls.Add(this.btnStartProcess);
            this.panelResultRight.Controls.Add(this.btnBack);
            this.panelResultRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelResultRight.Location = new System.Drawing.Point(380, 0);
            this.panelResultRight.Name = "panelResultRight";
            this.panelResultRight.Size = new System.Drawing.Size(420, 500);
            this.panelResultRight.TabIndex = 1;
            //
            // lblScanTitle
            //
            this.lblScanTitle.AutoSize = true;
            this.lblScanTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblScanTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblScanTitle.Location = new System.Drawing.Point(30, 25);
            this.lblScanTitle.Name = "lblScanTitle";
            this.lblScanTitle.Size = new System.Drawing.Size(140, 33);
            this.lblScanTitle.TabIndex = 7;
            this.lblScanTitle.Text = "扫描结果";
            //
            // panelStatsCard
            //
            this.panelStatsCard.BackColor = System.Drawing.Color.White;
            this.panelStatsCard.Controls.Add(this.lblJpgCount);
            this.panelStatsCard.Controls.Add(this.lblRawCount);
            this.panelStatsCard.Controls.Add(this.lblTotalCount);
            this.panelStatsCard.Location = new System.Drawing.Point(30, 68);
            this.panelStatsCard.Name = "panelStatsCard";
            this.panelStatsCard.Size = new System.Drawing.Size(360, 95);
            this.panelStatsCard.TabIndex = 10;
            this.panelStatsCard.Paint += new System.Windows.Forms.PaintEventHandler(this.CardPanel_Paint);
            //
            // lblJpgCount
            //
            this.lblJpgCount.AutoSize = true;
            this.lblJpgCount.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.lblJpgCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblJpgCount.Location = new System.Drawing.Point(16, 12);
            this.lblJpgCount.Name = "lblJpgCount";
            this.lblJpgCount.Size = new System.Drawing.Size(126, 23);
            this.lblJpgCount.TabIndex = 0;
            this.lblJpgCount.Text = "JPG 文件数：0";
            //
            // lblRawCount
            //
            this.lblRawCount.AutoSize = true;
            this.lblRawCount.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.lblRawCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblRawCount.Location = new System.Drawing.Point(16, 38);
            this.lblRawCount.Name = "lblRawCount";
            this.lblRawCount.Size = new System.Drawing.Size(136, 23);
            this.lblRawCount.TabIndex = 1;
            this.lblRawCount.Text = "RAW 文件数：0";
            //
            // lblTotalCount
            //
            this.lblTotalCount.AutoSize = true;
            this.lblTotalCount.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTotalCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.lblTotalCount.Location = new System.Drawing.Point(16, 62);
            this.lblTotalCount.Name = "lblTotalCount";
            this.lblTotalCount.Size = new System.Drawing.Size(95, 35);
            this.lblTotalCount.TabIndex = 2;
            this.lblTotalCount.Text = "合计：0";
            //
            // panelDivider1
            //
            this.panelDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.panelDivider1.Location = new System.Drawing.Point(30, 180);
            this.panelDivider1.Name = "panelDivider1";
            this.panelDivider1.Size = new System.Drawing.Size(360, 1);
            this.panelDivider1.TabIndex = 8;
            //
            // rbAiMode
            //
            this.rbAiMode.AutoSize = true;
            this.rbAiMode.Checked = true;
            this.rbAiMode.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.rbAiMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.rbAiMode.Location = new System.Drawing.Point(30, 200);
            this.rbAiMode.Name = "rbAiMode";
            this.rbAiMode.Size = new System.Drawing.Size(206, 27);
            this.rbAiMode.TabIndex = 3;
            this.rbAiMode.TabStop = true;
            this.rbAiMode.Text = "计算机辅助人像初筛";
            this.rbAiMode.UseVisualStyleBackColor = true;
            //
            // lblAiDesc
            //
            this.lblAiDesc.AutoSize = true;
            this.lblAiDesc.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblAiDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblAiDesc.Location = new System.Drawing.Point(48, 227);
            this.lblAiDesc.Name = "lblAiDesc";
            this.lblAiDesc.Size = new System.Drawing.Size(242, 20);
            this.lblAiDesc.TabIndex = 11;
            this.lblAiDesc.Text = "AI 自动检测人像、闭眼、模糊状态";
            //
            // rbManualMode
            //
            this.rbManualMode.AutoSize = true;
            this.rbManualMode.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.rbManualMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.rbManualMode.Location = new System.Drawing.Point(30, 260);
            this.rbManualMode.Name = "rbManualMode";
            this.rbManualMode.Size = new System.Drawing.Size(165, 27);
            this.rbManualMode.TabIndex = 4;
            this.rbManualMode.TabStop = true;
            this.rbManualMode.Text = "手动快速分类";
            this.rbManualMode.UseVisualStyleBackColor = true;
            //
            // lblManualDesc
            //
            this.lblManualDesc.AutoSize = true;
            this.lblManualDesc.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblManualDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblManualDesc.Location = new System.Drawing.Point(48, 287);
            this.lblManualDesc.Name = "lblManualDesc";
            this.lblManualDesc.Size = new System.Drawing.Size(182, 20);
            this.lblManualDesc.TabIndex = 12;
            this.lblManualDesc.Text = "全手动键盘操作快速分类";
            //
            // btnStartProcess
            //
            this.btnStartProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnStartProcess.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartProcess.FlatAppearance.BorderSize = 0;
            this.btnStartProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartProcess.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartProcess.ForeColor = System.Drawing.Color.White;
            this.btnStartProcess.Location = new System.Drawing.Point(30, 325);
            this.btnStartProcess.Name = "btnStartProcess";
            this.btnStartProcess.Size = new System.Drawing.Size(240, 50);
            this.btnStartProcess.TabIndex = 5;
            this.btnStartProcess.Text = "开始处理";
            this.btnStartProcess.UseVisualStyleBackColor = false;
            this.btnStartProcess.Click += new System.EventHandler(this.btnStartProcess_Click);
            this.btnStartProcess.MouseEnter += new System.EventHandler(this.ButtonPrimary_MouseEnter);
            this.btnStartProcess.MouseLeave += new System.EventHandler(this.ButtonPrimary_MouseLeave);
            //
            // btnBack
            //
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.btnBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.btnBack.Location = new System.Drawing.Point(30, 435);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(130, 36);
            this.btnBack.TabIndex = 6;
            this.btnBack.Text = "\u2190 返回重选";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            //
            // panelAiProgress
            //
            this.panelAiProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.panelAiProgress.Controls.Add(this.lblAiTitle);
            this.panelAiProgress.Controls.Add(this.lblAiStatus);
            this.panelAiProgress.Controls.Add(this.progressBarAI);
            this.panelAiProgress.Controls.Add(this.lblAiHint);
            this.panelAiProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAiProgress.Location = new System.Drawing.Point(380, 0);
            this.panelAiProgress.Name = "panelAiProgress";
            this.panelAiProgress.Size = new System.Drawing.Size(420, 500);
            this.panelAiProgress.TabIndex = 2;
            this.panelAiProgress.Visible = false;
            //
            // lblAiTitle
            //
            this.lblAiTitle.AutoSize = true;
            this.lblAiTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblAiTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblAiTitle.Location = new System.Drawing.Point(30, 35);
            this.lblAiTitle.Name = "lblAiTitle";
            this.lblAiTitle.Size = new System.Drawing.Size(140, 33);
            this.lblAiTitle.TabIndex = 2;
            this.lblAiTitle.Text = "AI 识别中";
            //
            // lblAiStatus
            //
            this.lblAiStatus.AutoSize = true;
            this.lblAiStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblAiStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblAiStatus.Location = new System.Drawing.Point(30, 85);
            this.lblAiStatus.Name = "lblAiStatus";
            this.lblAiStatus.Size = new System.Drawing.Size(210, 23);
            this.lblAiStatus.TabIndex = 1;
            this.lblAiStatus.Text = "正在初始化 AI 模型...";
            //
            // progressBarAI
            //
            this.progressBarAI.Location = new System.Drawing.Point(30, 125);
            this.progressBarAI.Name = "progressBarAI";
            this.progressBarAI.Size = new System.Drawing.Size(360, 28);
            this.progressBarAI.TabIndex = 0;
            this.progressBarAI.Paint += new System.Windows.Forms.PaintEventHandler(this.ProgressBarAI_Paint);
            //
            // lblAiHint
            //
            this.lblAiHint.AutoSize = true;
            this.lblAiHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblAiHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblAiHint.Location = new System.Drawing.Point(30, 165);
            this.lblAiHint.Name = "lblAiHint";
            this.lblAiHint.Size = new System.Drawing.Size(310, 20);
            this.lblAiHint.TabIndex = 3;
            this.lblAiHint.Text = "首次加载 AI 模型需要较长时间，请耐心等待";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.panelResult);
            this.Controls.Add(this.panelInput);
            this.MinimumSize = new System.Drawing.Size(640, 440);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ShotSort - 照片批量分类工具";
            this.panelInput.ResumeLayout(false);
            this.panelInput.PerformLayout();
            this.panelPathCard.ResumeLayout(false);
            this.panelPathCard.PerformLayout();
            this.panelOptionsCard.ResumeLayout(false);
            this.panelOptionsCard.PerformLayout();
            this.panelResult.ResumeLayout(false);
            this.panelResultRight.ResumeLayout(false);
            this.panelResultRight.PerformLayout();
            this.panelStatsCard.ResumeLayout(false);
            this.panelStatsCard.PerformLayout();
            this.panelAiProgress.ResumeLayout(false);
            this.panelAiProgress.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panelInput;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel panelPathCard;
        private System.Windows.Forms.TextBox txtFolderPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Panel panelOptionsCard;
        private System.Windows.Forms.CheckBox chkLoadJpg;
        private System.Windows.Forms.Label lblJpgDesc;
        private System.Windows.Forms.CheckBox chkLoadRaw;
        private System.Windows.Forms.Label lblRawDesc;
        private System.Windows.Forms.Button btnStartRead;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Panel panelResult;
        private System.Windows.Forms.ListView lstPhotos;
        private System.Windows.Forms.ImageList thumbnailImageList;
        private System.Windows.Forms.Panel panelResultRight;
        private System.Windows.Forms.Label lblScanTitle;
        private System.Windows.Forms.Panel panelStatsCard;
        private System.Windows.Forms.Label lblJpgCount;
        private System.Windows.Forms.Label lblRawCount;
        private System.Windows.Forms.Label lblTotalCount;
        private System.Windows.Forms.Panel panelDivider1;
        private System.Windows.Forms.RadioButton rbAiMode;
        private System.Windows.Forms.Label lblAiDesc;
        private System.Windows.Forms.RadioButton rbManualMode;
        private System.Windows.Forms.Label lblManualDesc;
        private System.Windows.Forms.Button btnStartProcess;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Panel panelAiProgress;
        private System.Windows.Forms.Label lblAiTitle;
        private System.Windows.Forms.Label lblAiStatus;
        private System.Windows.Forms.ProgressBar progressBarAI;
        private System.Windows.Forms.Label lblAiHint;
    }
}
