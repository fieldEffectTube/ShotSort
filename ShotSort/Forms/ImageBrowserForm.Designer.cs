namespace ShotSort.Forms
{
    partial class ImageBrowserForm
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.panelToolbar = new System.Windows.Forms.Panel();
            this.lblMode = new System.Windows.Forms.Label();
            this.chkDeleteRaw = new System.Windows.Forms.CheckBox();
            this.lblProgress = new System.Windows.Forms.Label();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.panelTags = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTooltip = new System.Windows.Forms.Label();
            this.tooltipTimer = new System.Windows.Forms.Timer();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblKeySelected = new System.Windows.Forms.Label();
            this.lblKeyKept = new System.Windows.Forms.Label();
            this.lblKeyDelete = new System.Windows.Forms.Label();
            this.lblKeyPrev = new System.Windows.Forms.Label();
            this.lblKeyNext = new System.Windows.Forms.Label();
            this.lblKeyRotate = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            //
            // panelTop
            //
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.panelTop.Controls.Add(this.progressBar);
            this.panelTop.Controls.Add(this.panelToolbar);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1000, 56);
            this.panelTop.TabIndex = 0;
            //
            // progressBar
            //
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 50);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1000, 6);
            this.progressBar.TabIndex = 0;
            this.progressBar.Paint += new System.Windows.Forms.PaintEventHandler(this.ProgressBar_Paint);
            //
            // panelToolbar
            //
            this.panelToolbar.Controls.Add(this.lblMode);
            this.panelToolbar.Controls.Add(this.chkDeleteRaw);
            this.panelToolbar.Controls.Add(this.lblProgress);
            this.panelToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelToolbar.Location = new System.Drawing.Point(0, 0);
            this.panelToolbar.Name = "panelToolbar";
            this.panelToolbar.Size = new System.Drawing.Size(1000, 50);
            this.panelToolbar.TabIndex = 1;
            //
            // lblMode
            //
            this.lblMode.AutoSize = true;
            this.lblMode.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblMode.Location = new System.Drawing.Point(16, 14);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(400, 20);
            this.lblMode.TabIndex = 1;
            this.lblMode.Text = "AI 废片初筛模式";
            //
            // chkDeleteRaw
            //
            this.chkDeleteRaw.AutoSize = true;
            this.chkDeleteRaw.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.chkDeleteRaw.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.chkDeleteRaw.Location = new System.Drawing.Point(420, 14);
            this.chkDeleteRaw.Name = "chkDeleteRaw";
            this.chkDeleteRaw.Size = new System.Drawing.Size(112, 24);
            this.chkDeleteRaw.TabIndex = 2;
            this.chkDeleteRaw.Text = "同时删除 RAW";
            this.chkDeleteRaw.UseVisualStyleBackColor = true;
            this.chkDeleteRaw.Visible = false;
            //
            // lblProgress
            //
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgress.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblProgress.ForeColor = System.Drawing.Color.White;
            this.lblProgress.Location = new System.Drawing.Point(850, 12);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(135, 28);
            this.lblProgress.TabIndex = 3;
            this.lblProgress.Text = "0 / 0";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pictureBox
            //
            this.pictureBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(18)))));
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 56);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(1000, 580);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox.TabIndex = 1;
            this.pictureBox.TabStop = false;
            //
            // lblInfo
            //
            this.lblInfo.AutoSize = true;
            this.lblInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(180)))));
            this.lblInfo.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.lblInfo.Location = new System.Drawing.Point(16, 72);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Padding = new System.Windows.Forms.Padding(12, 7, 12, 7);
            this.lblInfo.Size = new System.Drawing.Size(0, 29);
            this.lblInfo.TabIndex = 3;
            this.lblInfo.Visible = true;
            //
            // panelTags
            //
            this.panelTags.AutoSize = true;
            this.panelTags.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelTags.BackColor = System.Drawing.Color.Transparent;
            this.panelTags.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.panelTags.Location = new System.Drawing.Point(16, 105);
            this.panelTags.Name = "panelTags";
            this.panelTags.Size = new System.Drawing.Size(0, 0);
            this.panelTags.TabIndex = 4;
            this.panelTags.Visible = false;
            this.panelTags.WrapContents = false;
            //
            // lblTooltip
            //
            this.lblTooltip.AutoSize = true;
            this.lblTooltip.BackColor = System.Drawing.Color.Transparent;
            this.lblTooltip.Font = new System.Drawing.Font("Microsoft YaHei UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTooltip.ForeColor = System.Drawing.Color.White;
            this.lblTooltip.Location = new System.Drawing.Point(350, 100);
            this.lblTooltip.Name = "lblTooltip";
            this.lblTooltip.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);
            this.lblTooltip.Size = new System.Drawing.Size(0, 39);
            this.lblTooltip.TabIndex = 2;
            this.lblTooltip.Visible = false;
            this.lblTooltip.Paint += new System.Windows.Forms.PaintEventHandler(this.LblTooltip_Paint);
            //
            // tooltipTimer
            //
            this.tooltipTimer.Interval = 1400;
            this.tooltipTimer.Tick += new System.EventHandler(this.tooltipTimer_Tick);
            //
            // panelBottom
            //
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.panelBottom.Controls.Add(this.lblKeySelected);
            this.panelBottom.Controls.Add(this.lblKeyKept);
            this.panelBottom.Controls.Add(this.lblKeyDelete);
            this.panelBottom.Controls.Add(this.lblKeyRotate);
            this.panelBottom.Controls.Add(this.lblKeyPrev);
            this.panelBottom.Controls.Add(this.lblKeyNext);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 636);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1000, 64);
            this.panelBottom.TabIndex = 5;
            this.panelBottom.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelBottom_Paint);
            //
            // lblKeySelected
            //
            this.lblKeySelected.AutoSize = true;
            this.lblKeySelected.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblKeySelected.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblKeySelected.Location = new System.Drawing.Point(360, 22);
            this.lblKeySelected.Name = "lblKeySelected";
            this.lblKeySelected.Size = new System.Drawing.Size(78, 20);
            this.lblKeySelected.TabIndex = 4;
            this.lblKeySelected.Text = "\u2191 精选";
            //
            // lblKeyKept
            //
            this.lblKeyKept.AutoSize = true;
            this.lblKeyKept.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblKeyKept.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblKeyKept.Location = new System.Drawing.Point(470, 22);
            this.lblKeyKept.Name = "lblKeyKept";
            this.lblKeyKept.Size = new System.Drawing.Size(65, 20);
            this.lblKeyKept.TabIndex = 3;
            this.lblKeyKept.Text = "\u2193 保留";
            //
            // lblKeyDelete
            //
            this.lblKeyDelete.AutoSize = true;
            this.lblKeyDelete.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblKeyDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblKeyDelete.Location = new System.Drawing.Point(560, 22);
            this.lblKeyDelete.Name = "lblKeyDelete";
            this.lblKeyDelete.Size = new System.Drawing.Size(80, 20);
            this.lblKeyDelete.TabIndex = 2;
            this.lblKeyDelete.Text = "Space 删除";
            //
            // lblKeyPrev
            //
            this.lblKeyPrev.AutoSize = true;
            this.lblKeyPrev.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblKeyPrev.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblKeyPrev.Location = new System.Drawing.Point(190, 22);
            this.lblKeyPrev.Name = "lblKeyPrev";
            this.lblKeyPrev.Size = new System.Drawing.Size(72, 20);
            this.lblKeyPrev.TabIndex = 0;
            this.lblKeyPrev.Text = "\u2190 上一张";
            //
            // lblKeyRotate
            //
            this.lblKeyRotate.AutoSize = true;
            this.lblKeyRotate.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblKeyRotate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblKeyRotate.Location = new System.Drawing.Point(280, 22);
            this.lblKeyRotate.Name = "lblKeyRotate";
            this.lblKeyRotate.Size = new System.Drawing.Size(95, 20);
            this.lblKeyRotate.TabIndex = 5;
            this.lblKeyRotate.Text = "PgDn 旋转";
            //
            // lblKeyNext
            //
            this.lblKeyNext.AutoSize = true;
            this.lblKeyNext.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblKeyNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblKeyNext.Location = new System.Drawing.Point(280, 22);
            this.lblKeyNext.Name = "lblKeyNext";
            this.lblKeyNext.Size = new System.Drawing.Size(72, 20);
            this.lblKeyNext.TabIndex = 1;
            this.lblKeyNext.Text = "\u2192 下一张";
            //
            // ImageBrowserForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(18)))));
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.panelTags);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblTooltip);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.KeyPreview = true;
            this.Icon = global::ShotSort.Properties.Resources.AppIcon;
            this.Name = "ImageBrowserForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ShotSort - 照片浏览";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panelTop.ResumeLayout(false);
            this.panelToolbar.ResumeLayout(false);
            this.panelToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Panel panelToolbar;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.CheckBox chkDeleteRaw;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.FlowLayoutPanel panelTags;
        private System.Windows.Forms.Label lblTooltip;
        private System.Windows.Forms.Timer tooltipTimer;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblKeySelected;
        private System.Windows.Forms.Label lblKeyKept;
        private System.Windows.Forms.Label lblKeyDelete;
        private System.Windows.Forms.Label lblKeyPrev;
        private System.Windows.Forms.Label lblKeyNext;
        private System.Windows.Forms.Label lblKeyRotate;
    }
}
