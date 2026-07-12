using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShotSort.Core;
using ShotSort.Models;
using ShotSort.Utils;

namespace ShotSort.Forms
{
    public partial class ImageBrowserForm : Form
    {
        private readonly List<PhotoItem> _photos;
        private readonly string _rootPath;
        private readonly bool _isAiMode;
        private int _currentIndex;
        private bool _completed;
        private CancellationTokenSource? _loadCts;

        public ImageBrowserForm(List<PhotoItem> photos, string rootPath, bool isAiMode)
        {
            _photos = photos;
            _rootPath = rootPath;
            _isAiMode = isAiMode;
            _currentIndex = 0;
            _completed = false;

            InitializeComponent();
            SetupMode();
        }

        private void SetupMode()
        {
            if (_isAiMode)
            {
                lblMode.Text = "AI 废片初筛模式";
                chkDeleteRaw.Visible = true;
                lblKeyDelete.Text = "Space 删除";
            }
            else
            {
                lblMode.Text = "浏览模式";
                chkDeleteRaw.Visible = false;
                lblKeyDelete.Text = "Space 待删除";
            }

            UpdateProgress();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LoadCurrentPhotoAsync();
        }

        #region Custom Paint Handlers

        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

            int r = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            if (r == 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            var d = r * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ProgressBar_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not ProgressBar pb) return;

            var rect = new Rectangle(0, 0, pb.Width, pb.Height);
            using var bgPath = RoundedRect(rect, 3);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var trackBrush = new SolidBrush(Color.FromArgb(55, 55, 60));
            e.Graphics.FillPath(trackBrush, bgPath);

            if (pb.Maximum > 0 && pb.Value > 0)
            {
                double ratio = (double)pb.Value / pb.Maximum;
                int fillW = (int)(pb.Width * ratio);
                if (fillW < 1) return;

                var fillRect = new Rectangle(0, 0, fillW, pb.Height);
                using var fillPath = RoundedRect(fillRect, 3);
                using var fillBrush = new LinearGradientBrush(
                    fillRect,
                    Color.FromArgb(59, 130, 246),
                    Color.FromArgb(96, 165, 250),
                    LinearGradientMode.Horizontal);
                e.Graphics.FillPath(fillBrush, fillPath);
            }
        }

        private void TagLabel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Label lbl) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, lbl.Width, lbl.Height);
            using var path = RoundedRect(rect, 4);
            Color bgColor = lbl.Tag is Color c ? c : Color.FromArgb(100, 100, 100);
            using var brush = new SolidBrush(bgColor);
            e.Graphics.FillPath(brush, path);

            using var font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.DrawString(lbl.Text, font, textBrush, rect, sf);
        }

        private void AddTag(string text, Color bgColor)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Tag = bgColor;
            lbl.AutoSize = false;

            using var g = CreateGraphics();
            using var font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
            var size = g.MeasureString(text, font);
            lbl.Width = (int)size.Width + 18;
            lbl.Height = 24;
            lbl.Margin = new Padding(0, 0, 0, 4);
            lbl.Paint += TagLabel_Paint;
            panelTags.Controls.Add(lbl);
            panelTags.Visible = true;
        }

        private void LblTooltip_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not Label lbl) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, lbl.Width, lbl.Height);

            // Shadow
            var shadowRect = new Rectangle(3, 4, rect.Width - 6, rect.Height - 6);
            using (var shadowPath = RoundedRect(shadowRect, 12))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
            }

            // Background
            using var bgPath = RoundedRect(rect, 12);
            using var bgBrush = new LinearGradientBrush(
                rect,
                Color.FromArgb(59, 130, 246),
                Color.FromArgb(37, 99, 235),
                LinearGradientMode.Vertical);
            e.Graphics.FillPath(bgBrush, bgPath);

            // Text
            using var font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.DrawString(lbl.Text, font, textBrush, rect, sf);
        }

        private void PanelBottom_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            // Top separator line
            using var pen = new Pen(Color.FromArgb(45, 45, 50), 1);
            e.Graphics.DrawLine(pen, 0, 0, panel.Width, 0);

            // Center the key hints
            var totalWidth = lblKeyPrev.Width + 20 + lblKeyNext.Width + 20 + lblKeyRotate.Width + 30 +
                             lblKeySelected.Width + 20 + lblKeyKept.Width + 20 + lblKeyDelete.Width;
            var startX = (panel.Width - totalWidth) / 2;

            var x = startX;
            lblKeyPrev.Location = new Point(x, 22);
            x += lblKeyPrev.Width + 20;
            lblKeyNext.Location = new Point(x, 22);
            x += lblKeyNext.Width + 20;
            lblKeyRotate.Location = new Point(x, 22);
            x += lblKeyRotate.Width + 30;

            // Separator dot
            using var dotBrush = new SolidBrush(Color.FromArgb(75, 75, 80));
            e.Graphics.FillEllipse(dotBrush, x - 5, 30, 4, 4);
            x += 10;

            lblKeySelected.Location = new Point(x, 22);
            x += lblKeySelected.Width + 20;
            lblKeyKept.Location = new Point(x, 22);
            x += lblKeyKept.Width + 20;
            lblKeyDelete.Location = new Point(x, 22);
        }

        #endregion

        #region Photo Loading

        private async void LoadCurrentPhotoAsync()
        {
            if (_photos.Count == 0)
            {
                ShowCompletion();
                return;
            }

            if (_currentIndex < 0) _currentIndex = 0;
            if (_currentIndex >= _photos.Count) _currentIndex = _photos.Count - 1;

            var photo = _photos[_currentIndex];

            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = new CancellationTokenSource();
            var token = _loadCts.Token;

            UpdateProgress();
            UpdateInfoLabel(photo);

            var oldImage = pictureBox.Image;
            pictureBox.Image = null;
            oldImage?.Dispose();

            // Hint GC to reclaim the previous image promptly, especially important
            // for RAW images which can be ~96MB each after half-size decode.
            if (oldImage != null)
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);

            try
            {
                var bitmap = await Task.Run(() =>
                {
                    if (token.IsCancellationRequested) return null;

                    try
                    {
                        if (photo.FileType == PhotoFileType.JPG)
                            return ImageLoader.LoadJpg(photo.FilePath, Math.Max(pictureBox.Width, 800));
                        else
                            return ImageLoader.LoadRaw(photo.FilePath, Math.Max(pictureBox.Width, 800));
                    }
                    catch
                    {
                        return null;
                    }
                }, token);

                if (token.IsCancellationRequested)
                {
                    bitmap?.Dispose();
                    return;
                }

                if (bitmap != null)
                {
                    pictureBox.Image = bitmap;
                }
                else
                {
                    ShowPlaceholder(photo.FileName);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void ShowPlaceholder(string fileName)
        {
            var bmp = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(20, 20, 24));
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Broken image icon circle
                var cx = bmp.Width / 2;
                var cy = bmp.Height / 2 - 20;
                using var circleBrush = new SolidBrush(Color.FromArgb(40, 40, 44));
                g.FillEllipse(circleBrush, cx - 30, cy - 30, 60, 60);

                // X mark
                using var xPen = new Pen(Color.FromArgb(148, 163, 184), 2);
                g.DrawLine(xPen, cx - 10, cy - 10, cx + 10, cy + 10);
                g.DrawLine(xPen, cx + 10, cy - 10, cx - 10, cy + 10);

                // Text
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using var font = new Font("Microsoft YaHei UI", 12F);
                using var brush = new SolidBrush(Color.FromArgb(100, 116, 139));
                g.DrawString($"无法加载", font, brush, new RectangleF(0, cy + 40, bmp.Width, 30), sf);
                using var smallFont = new Font("Microsoft YaHei UI", 9F);
                using var smallBrush = new SolidBrush(Color.FromArgb(71, 85, 105));
                g.DrawString(fileName, smallFont, smallBrush, new RectangleF(0, cy + 65, bmp.Width, 25), sf);
            }
            pictureBox.Image = bmp;
        }

        #endregion

        #region Info & Progress

        private void UpdateInfoLabel(PhotoItem photo)
        {
            lblInfo.Text = photo.FileName;
            panelTags.Controls.Clear();
            panelTags.Visible = false;

            if (_isAiMode && photo.AiResult != null)
            {
                var ai = photo.AiResult;
                if (!ai.HasFace)
                {
                    AddTag("无人像", Color.FromArgb(107, 114, 128));
                }
                else
                {
                    if (ai.FaceCount > 3)
                    {
                        AddTag("多人照片", Color.FromArgb(59, 130, 246));
                    }
                    else if (ai.FaceCount > 1)
                    {
                        AddTag("合影", Color.FromArgb(16, 185, 129));

                        if (ai.EyeState == EyeState.Closed)
                            AddTag("闭眼", Color.FromArgb(245, 158, 11));
                    }
                    else
                    {
                        AddTag("人像", Color.FromArgb(16, 185, 129));

                        if (ai.EyeState == EyeState.Closed)
                            AddTag("闭眼", Color.FromArgb(245, 158, 11));
                    }

                    if (ai.IsBlur)
                        AddTag("模糊", Color.FromArgb(239, 68, 68));
                }
            }

            if (photo.PairedRawPath != null)
                AddTag("含RAW", Color.FromArgb(59, 130, 246));

            panelTags.Location = new Point(16, lblInfo.Bottom + 2);
        }

        private void UpdateProgress()
        {
            if (_photos.Count == 0)
            {
                progressBar.Value = 0;
                lblProgress.Text = "0 / 0";
                return;
            }

            progressBar.Maximum = _photos.Count;
            progressBar.Value = Math.Min(_currentIndex + 1, _photos.Count);
            progressBar.Invalidate();
            lblProgress.Text = $"{_currentIndex + 1} / {_photos.Count}";
        }

        #endregion

        #region Navigation

        private void NavigateNext()
        {
            if (_completed) return;

            if (_currentIndex < _photos.Count - 1)
            {
                _currentIndex++;
                LoadCurrentPhotoAsync();
            }
            else
            {
                ShowCompletion();
            }
        }

        private void NavigatePrevious()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                LoadCurrentPhotoAsync();
            }
        }

        private void RemoveCurrentAndAdvance()
        {
            _photos.RemoveAt(_currentIndex);

            if (_currentIndex >= _photos.Count && _currentIndex > 0)
                _currentIndex = _photos.Count - 1;

            if (_photos.Count == 0)
                ShowCompletion();
            else
                LoadCurrentPhotoAsync();
        }

        private void RotateCurrentImage()
        {
            if (pictureBox.Image is not Bitmap bmp) return;

            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBox.Invalidate();
        }

        private void ShowCompletion()
        {
            _completed = true;

            MessageBox.Show("已完成所有照片的分类！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", _rootPath);
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"打开分类目录失败: {_rootPath}", ex);
            }

            Close();
        }

        #endregion

        #region Tooltip

        private void ShowTooltip(string message)
        {
            lblTooltip.Text = message;

            using var g = lblTooltip.CreateGraphics();
            using var font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
            var textSize = g.MeasureString(message, font);
            var w = (int)textSize.Width + 50;
            var h = (int)textSize.Height + 30;

            lblTooltip.Size = new Size(w, h);
            lblTooltip.Left = (ClientSize.Width - w) / 2;
            lblTooltip.Top = panelTop.Bottom + 30;
            lblTooltip.Visible = true;

            tooltipTimer.Stop();
            tooltipTimer.Start();
        }

        private void tooltipTimer_Tick(object sender, EventArgs e)
        {
            lblTooltip.Visible = false;
            tooltipTimer.Stop();
        }

        #endregion

        #region Keyboard Handling

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_completed || _photos.Count == 0)
                return base.ProcessCmdKey(ref msg, keyData);

            var action = HotkeyManager.ParseKey(keyData);
            var photo = _photos[_currentIndex];

            switch (action)
            {
                case HotkeyManager.HotkeyAction.Previous:
                    NavigatePrevious();
                    return true;

                case HotkeyManager.HotkeyAction.Next:
                    NavigateNext();
                    return true;

                case HotkeyManager.HotkeyAction.MarkSelected:
                    try
                    {
                        if (FileSyncManager.MoveToClassify(photo.FilePath, FileSyncManager.FolderSelected, _rootPath))
                        {
                            ShowTooltip($"{photo.FileName}  \u2192  精选");
                            RemoveCurrentAndAdvance();
                        }
                        else
                        {
                            ShowTooltip($"移动失败：{photo.FileName}");
                        }
                    }
                    catch (IOException ex)
                    {
                        ShowTooltip($"文件被占用：{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        ShowTooltip($"操作失败：{ex.Message}");
                        DebugLogger.LogError($"MarkSelected 失败: {photo.FilePath}", ex);
                    }
                    return true;

                case HotkeyManager.HotkeyAction.MarkKept:
                    try
                    {
                        if (FileSyncManager.MoveToClassify(photo.FilePath, FileSyncManager.FolderKept, _rootPath))
                        {
                            ShowTooltip($"{photo.FileName}  \u2192  保留");
                            RemoveCurrentAndAdvance();
                        }
                        else
                        {
                            ShowTooltip($"移动失败：{photo.FileName}");
                        }
                    }
                    catch (IOException ex)
                    {
                        ShowTooltip($"文件被占用：{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        ShowTooltip($"操作失败：{ex.Message}");
                        DebugLogger.LogError($"MarkKept 失败: {photo.FilePath}", ex);
                    }
                    return true;

                case HotkeyManager.HotkeyAction.CtrlAction:
                    try
                    {
                        if (_isAiMode)
                        {
                            if (FileSyncManager.DeletePhoto(photo.FilePath, chkDeleteRaw.Checked))
                            {
                                ShowTooltip($"{photo.FileName}  已永久删除");
                                RemoveCurrentAndAdvance();
                            }
                            else
                            {
                                ShowTooltip($"删除失败：{photo.FileName}");
                            }
                        }
                        else
                        {
                            if (FileSyncManager.MoveToClassify(photo.FilePath, FileSyncManager.FolderPendingDelete, _rootPath))
                            {
                                ShowTooltip($"{photo.FileName}  \u2192  待删除");
                                RemoveCurrentAndAdvance();
                            }
                            else
                            {
                                ShowTooltip($"移动失败：{photo.FileName}");
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        ShowTooltip($"文件被占用：{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        ShowTooltip($"操作失败：{ex.Message}");
                        DebugLogger.LogError($"CtrlAction 失败: {photo.FilePath}", ex);
                    }
                    return true;

                case HotkeyManager.HotkeyAction.RotateClockwise:
                    RotateCurrentImage();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            pictureBox.Image?.Dispose();
            pictureBox.Image = null;
            panelTags.Controls.Clear();
            base.OnFormClosed(e);
        }
    }
}
