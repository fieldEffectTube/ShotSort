using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShotSort.Core;
using ShotSort.Models;
using ShotSort.Utils;

namespace ShotSort.Forms
{
    public partial class MainForm : Form
    {
        private ScanResult? _scanResult;
        private string? _rootPath;
        private CancellationTokenSource? _aiCts;
        private static readonly Color PrimaryColor = Color.FromArgb(59, 130, 246);
        private static readonly Color PrimaryHoverColor = Color.FromArgb(37, 99, 235);
        private static readonly Color BlueHoverColor = Color.FromArgb(37, 99, 235);
        private const int CardRadius = 10;

        // Track which thumbnail slots in the ImageList hold real (non-placeholder) images,
        // so they can be replaced back to placeholders when evicted from LRU.
        private const int MaxCachedThumbnails = 200;
        private readonly LinkedList<int> _thumbnailLru = new();
        private readonly HashSet<int> _thumbnailLoading = new();

        public MainForm()
        {
            InitializeComponent();
            AppSettings.Load();
            var savedPath = AppSettings.GetValidPathOrDrive();
            if (!string.IsNullOrEmpty(savedPath))
                txtFolderPath.Text = savedPath;
            else
                txtFolderPath.Text = "点击右侧按钮选择照片文件夹...";

            ApplyRoundRegion(btnBrowse, 6);
            ApplyRoundRegion(btnStartRead, 8);
            ApplyRoundRegion(btnStartProcess, 8);

            lstPhotos.MouseDoubleClick += lstPhotos_MouseDoubleClick;

            // Hook ListView scroll events for on-demand thumbnail loading
            _listViewScrollHook = new ListViewScrollHook(lstPhotos);
            _listViewScrollHook.Scrolled += (s, e) => _ = LoadVisibleThumbnailsAsync();

            DebugLogger.Log("ShotSort 启动");
        }

        private readonly ListViewScrollHook _listViewScrollHook;

        #region Card & Rounded Corner Helpers

        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int r = Math.Min(radius, rect.Width / 2);
            r = Math.Min(r, rect.Height / 2);
            var d = r * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void ApplyRoundRegion(Control control, int radius)
        {
            var rect = new Rectangle(0, 0, control.Width, control.Height);
            using var path = RoundedRect(rect, radius);
            control.Region = new Region(path);
        }

        private void CardPanel_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Shadow
            var shadowRect = new Rectangle(2, 3, panel.Width - 4, panel.Height - 4);
            using (var shadowPath = RoundedRect(shadowRect, CardRadius))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
            }

            // Card body
            var cardRect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using var cardPath = RoundedRect(cardRect, CardRadius);
            using var cardBrush = new SolidBrush(Color.White);
            e.Graphics.FillPath(cardBrush, cardPath);

            // Border
            using var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1);
            e.Graphics.DrawPath(borderPen, cardPath);
        }

        #endregion

        #region Button Hover Effects

        private void ButtonPrimary_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Enabled)
            {
                btn.BackColor = PrimaryHoverColor;
                ApplyRoundRegion(btn, 8);
            }
        }

        private void ButtonPrimary_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackColor = PrimaryColor;
                ApplyRoundRegion(btn, 8);
            }
        }

        private void ButtonBlue_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Enabled)
            {
                btn.BackColor = BlueHoverColor;
                ApplyRoundRegion(btn, 6);
            }
        }

        private void ButtonBlue_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackColor = PrimaryColor;
                ApplyRoundRegion(btn, 6);
            }
        }

        #endregion

        #region Custom Progress Bar

        private void ProgressBarAI_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not ProgressBar pb) return;

            var rect = new Rectangle(0, 0, pb.Width, pb.Height);
            using var bgPath = RoundedRect(rect, 6);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Background track
            using var trackBrush = new SolidBrush(Color.FromArgb(226, 232, 240));
            e.Graphics.FillPath(trackBrush, bgPath);

            // Fill
            if (pb.Maximum > 0 && pb.Value > 0)
            {
                double ratio = (double)pb.Value / pb.Maximum;
                int fillW = (int)(pb.Width * ratio);
                if (fillW < 1) return;

                var fillRect = new Rectangle(0, 0, fillW, pb.Height);
                using var fillPath = RoundedRect(fillRect, 6);
                using var fillBrush = new LinearGradientBrush(
                    fillRect,
                    Color.FromArgb(59, 130, 246),
                    Color.FromArgb(96, 165, 250),
                    LinearGradientMode.Horizontal);
                e.Graphics.FillPath(fillBrush, fillPath);
            }
        }

        #endregion

        #region Browse & Read

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "选择照片所在文件夹";
            dialog.ShowNewFolderButton = false;
            if (!string.IsNullOrEmpty(txtFolderPath.Text) && txtFolderPath.Text != "点击右侧按钮选择照片文件夹...")
                dialog.SelectedPath = txtFolderPath.Text;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = dialog.SelectedPath;
                AppSettings.LastFolderPath = dialog.SelectedPath;
                AppSettings.Save();
            }
        }

        private void btnStartRead_Click(object sender, EventArgs e)
        {
            var path = txtFolderPath.Text.Trim();
            if (string.IsNullOrEmpty(path) || path == "点击右侧按钮选择照片文件夹...")
            {
                MessageBox.Show("请先选择文件夹路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(path))
            {
                MessageBox.Show("所选文件夹不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var loadJpg = chkLoadJpg.Checked;
            var loadRaw = chkLoadRaw.Checked;

            if (!loadJpg && !loadRaw)
            {
                MessageBox.Show("请至少选择一种图片格式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnStartRead.Enabled = false;
            btnStartRead.Text = "读取中...";
            btnStartRead.BackColor = Color.FromArgb(147, 197, 253);

            try
            {
                var result = FileScanner.ScanWithCategories(path, loadJpg, loadRaw);
                if (result.Photos.Count == 0)
                {
                    MessageBox.Show("未找到匹配的照片文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _scanResult = result;
                _rootPath = path;
                DebugLogger.Log($"扫描完成: {result.JpgCount} JPG, {result.RawCount} RAW, 共 {result.Photos.Count} 张");
                ShowScanResult();
            }
            finally
            {
                btnStartRead.Enabled = true;
                btnStartRead.Text = "开始读取";
                btnStartRead.BackColor = PrimaryColor;
            }
        }

        #endregion

        #region Scan Result Display

        private void ShowScanResult()
        {
            if (_scanResult == null) return;

            panelInput.Visible = false;
            panelResult.Visible = true;

            lblJpgCount.Text = $"JPG 文件数：{_scanResult.JpgCount}";
            lblRawCount.Text = $"RAW 文件数：{_scanResult.RawCount}";
            lblTotalCount.Text = $"合计：{_scanResult.Photos.Count}";

            _thumbnailLru.Clear();
            lock (_thumbnailLoading) { _thumbnailLoading.Clear(); }

            lstPhotos.BeginUpdate();
            lstPhotos.Items.Clear();
            thumbnailImageList.Images.Clear();

            for (int i = 0; i < _scanResult.Photos.Count; i++)
            {
                var photo = _scanResult.Photos[i];
                var tag = photo.FileType == PhotoFileType.RAW ? "[RAW] " : "";
                var catTag = photo.Category != PhotoCategory.None ? $"[{CategoryToLabel(photo.Category)}] " : "";
                var item = new ListViewItem(catTag + tag + photo.FileName) { ImageIndex = i };
                lstPhotos.Items.Add(item);

                thumbnailImageList.Images.Add(CreatePlaceholderThumbnail(photo));
            }

            lstPhotos.EndUpdate();

            if (lstPhotos.Items.Count > 0)
                lstPhotos.Items[0].Selected = true;

            _ = LoadVisibleThumbnailsAsync();
        }

        private static string CategoryToLabel(PhotoCategory category) => category switch
        {
            PhotoCategory.HasPerson => "有人",
            PhotoCategory.Selected => "精选",
            PhotoCategory.Kept => "保留",
            PhotoCategory.PendingDelete => "待删除",
            _ => ""
        };

        private static Color CategoryToColor(PhotoCategory category) => category switch
        {
            PhotoCategory.HasPerson => Color.FromArgb(239, 68, 68),
            PhotoCategory.Selected => Color.FromArgb(16, 185, 129),
            PhotoCategory.Kept => Color.FromArgb(59, 130, 246),
            PhotoCategory.PendingDelete => Color.FromArgb(148, 163, 184),
            _ => Color.Transparent
        };

        #endregion

        #region Thumbnail Loading

        private CancellationTokenSource? _thumbnailCts;

        private async Task LoadVisibleThumbnailsAsync()
        {
            if (_scanResult == null || IsDisposed) return;

            _thumbnailCts?.Cancel();
            _thumbnailCts?.Dispose();
            _thumbnailCts = new CancellationTokenSource();
            var token = _thumbnailCts.Token;

            var visibleIndices = GetVisibleItemIndices();
            if (visibleIndices.Count == 0) return;

            EvictDistantThumbnails(visibleIndices);

            const int jpgConcurrency = 8;
            const int rawConcurrency = 2;
            var jpgSemaphore = new SemaphoreSlim(jpgConcurrency);
            var rawSemaphore = new SemaphoreSlim(rawConcurrency);
            var tasks = new List<Task>();

            foreach (var idx in visibleIndices)
            {
                if (token.IsCancellationRequested) break;
                // Already loaded (in LRU means it has a real thumbnail in ImageList)
                if (_thumbnailLru.Contains(idx)) continue;

                lock (_thumbnailLoading)
                {
                    if (_thumbnailLoading.Contains(idx)) continue;
                    _thumbnailLoading.Add(idx);
                }

                var photo = _scanResult.Photos[idx];
                var isRaw = photo.FileType == PhotoFileType.RAW;
                var sem = isRaw ? rawSemaphore : jpgSemaphore;

                await sem.WaitAsync(token);
                if (token.IsCancellationRequested) { sem.Release(); break; }

                var capturedIdx = idx;
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var thumb = LoadThumbnail(photo.FilePath, photo.Category);
                        if (thumb != null && !token.IsCancellationRequested && !IsDisposed)
                        {
                            BeginInvoke(new Action(() =>
                            {
                                if (!token.IsCancellationRequested && capturedIdx < thumbnailImageList.Images.Count)
                                {
                                    thumbnailImageList.Images[capturedIdx] = thumb;
                                    TouchThumbnailLru(capturedIdx);
                                    lstPhotos.Invalidate(lstPhotos.Items[capturedIdx].GetBounds(ItemBoundsPortion.Entire));
                                }
                                else
                                {
                                    thumb.Dispose();
                                }
                            }));
                        }
                        else
                        {
                            thumb?.Dispose();
                        }
                    }
                    finally
                    {
                        lock (_thumbnailLoading) { _thumbnailLoading.Remove(capturedIdx); }
                        sem.Release();
                    }
                }, token));
            }

            try { await Task.WhenAll(tasks); }
            catch (OperationCanceledException) { }
        }

        private List<int> GetVisibleItemIndices()
        {
            var result = new List<int>();
            if (_scanResult == null || lstPhotos.Items.Count == 0) return result;

            var clientRect = lstPhotos.ClientRectangle;
            var firstItem = PointToClient(lstPhotos.PointToScreen(new Point(clientRect.Left + 5, clientRect.Top + 5)));
            var lastItem = PointToClient(lstPhotos.PointToScreen(new Point(clientRect.Left + 5, clientRect.Bottom - 5)));

            var startInfo = lstPhotos.GetItemAt(firstItem.X, firstItem.Y);
            var endInfo = lstPhotos.GetItemAt(lastItem.X, lastItem.Y);

            int start = startInfo?.Index ?? 0;
            int end = endInfo?.Index ?? (lstPhotos.Items.Count - 1);

            const int buffer = 10;
            start = Math.Max(0, start - buffer);
            end = Math.Min(lstPhotos.Items.Count - 1, end + buffer);

            for (int i = start; i <= end; i++)
                result.Add(i);

            return result;
        }

        private void TouchThumbnailLru(int index)
        {
            _thumbnailLru.Remove(index);
            _thumbnailLru.AddLast(index);

            // Evict LRU entries: replace their ImageList slot with placeholder
            while (_thumbnailLru.Count > MaxCachedThumbnails)
            {
                var oldest = _thumbnailLru.First!.Value;
                _thumbnailLru.RemoveFirst();
                ReplaceThumbnailWithPlaceholder(oldest);
            }
        }

        private void EvictDistantThumbnails(List<int> visibleIndices)
        {
            if (visibleIndices.Count == 0 || _thumbnailLru.Count == 0) return;
            var minVisible = visibleIndices[0];
            var maxVisible = visibleIndices[visibleIndices.Count - 1];
            const int evictionMargin = 50;

            var toEvict = new List<int>();
            foreach (var idx in _thumbnailLru)
            {
                if (idx < minVisible - evictionMargin || idx > maxVisible + evictionMargin)
                    toEvict.Add(idx);
            }

            foreach (var idx in toEvict)
            {
                _thumbnailLru.Remove(idx);
                ReplaceThumbnailWithPlaceholder(idx);
            }
        }

        private void ReplaceThumbnailWithPlaceholder(int index)
        {
            if (_scanResult == null || index >= _scanResult.Photos.Count || index >= thumbnailImageList.Images.Count)
                return;

            var photo = _scanResult.Photos[index];
            using var placeholder = CreatePlaceholderThumbnail(photo);
            thumbnailImageList.Images[index] = placeholder;
            lstPhotos.Invalidate(lstPhotos.Items[index].GetBounds(ItemBoundsPortion.Entire));
        }

        private Bitmap? LoadThumbnail(string filePath, PhotoCategory category)
        {
            try
            {
                var ext = Path.GetExtension(filePath).ToUpperInvariant();
                var rawExts = new HashSet<string> { ".CR2", ".CR3", ".DNG", ".NEF", ".RAF" };
                var size = thumbnailImageList.ImageSize;

                if (rawExts.Contains(ext))
                {
                    using var rawBmp = ImageLoader.LoadRaw(filePath, 256);
                    if (rawBmp == null) return null;
                    return FitToTile(rawBmp, size, category);
                }

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var original = Image.FromStream(fs, false, false))
                {
                    ImageLoader.ApplyExifOrientation(original);

                    if (original.PropertyItems?.Length > 0)
                    {
                        var exifThumb = GetExifThumbnail(original);
                        if (exifThumb != null)
                        {
                            ImageLoader.ApplyExifOrientation(exifThumb);
                            return FitToTile(exifThumb, size, category);
                        }
                    }

                    return FitToTile(original, size, category);
                }
            }
            catch
            {
                return null;
            }
        }

        private static Image? GetExifThumbnail(Image img)
        {
            const int ExifThumbnailData = 0x501B;
            try
            {
                foreach (var prop in img.PropertyItems)
                {
                    if (prop.Id == ExifThumbnailData && prop.Value?.Length > 0)
                    {
                        using var ms = new MemoryStream(prop.Value);
                        return Image.FromStream(ms);
                    }
                }
            }
            catch { }
            return null;
        }

        private static Bitmap FitToTile(Image original, Size tileSize, PhotoCategory category = PhotoCategory.None)
        {
            var ratio = Math.Min((double)tileSize.Width / original.Width, (double)tileSize.Height / original.Height);
            var w = (int)(original.Width * ratio);
            var h = (int)(original.Height * ratio);
            var bmp = new Bitmap(tileSize.Width, tileSize.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(241, 245, 249));
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var x = (tileSize.Width - w) / 2;
                var y = (tileSize.Height - h) / 2;
                g.DrawImage(original, x, y, w, h);

                if (category != PhotoCategory.None)
                    DrawCategoryBanner(g, tileSize, category);
            }
            return bmp;
        }

        private static void DrawCategoryBanner(Graphics g, Size tileSize, PhotoCategory category)
        {
            var label = CategoryToLabel(category);
            var color = CategoryToColor(category);
            using var font = new Font("Microsoft YaHei UI", 7F, FontStyle.Bold);
            var textSize = g.MeasureString(label, font);
            var padding = 5;
            var bannerW = (int)textSize.Width + padding * 2;
            var bannerH = (int)textSize.Height + padding;
            var bannerX = 3;
            var bannerY = 3;

            var bannerRect = new Rectangle(bannerX, bannerY, bannerW, bannerH);
            using var bannerPath = RoundedRect(bannerRect, 3);
            using var brush = new SolidBrush(color);
            g.FillPath(brush, bannerPath);
            using var textBrush = new SolidBrush(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.DrawString(label, font, textBrush, bannerX + padding, bannerY + padding / 2);
        }

        private Bitmap CreatePlaceholderThumbnail(PhotoItem photo)
        {
            var size = thumbnailImageList.ImageSize;
            var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(226, 232, 240));
                g.InterpolationMode = InterpolationMode.Default;

                // File type icon circle
                var cx = size.Width / 2;
                var cy = size.Height / 2 - 4;
                var circleR = 18;
                var circleColor = photo.FileType == PhotoFileType.RAW
                    ? Color.FromArgb(251, 191, 36)
                    : Color.FromArgb(96, 165, 250);
                using (var circleBrush = new SolidBrush(Color.FromArgb(30, circleColor)))
                    g.FillEllipse(circleBrush, cx - circleR, cy - circleR, circleR * 2, circleR * 2);

                // Extension text
                var ext = photo.Extension.ToUpperInvariant().Replace(".", "");
                using var extFont = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold);
                using var extBrush = new SolidBrush(circleColor);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(ext, extFont, extBrush, new RectangleF(cx - circleR, cy - circleR, circleR * 2, circleR * 2), sf);

                // File name below
                var nameText = photo.FileName;
                if (nameText.Length > 18) nameText = nameText[..15] + "...";
                using var nameFont = new Font("Microsoft YaHei UI", 7F);
                using var nameBrush = new SolidBrush(Color.FromArgb(100, 116, 139));
                var nameSf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
                g.DrawString(nameText, nameFont, nameBrush, new RectangleF(0, 0, size.Width, size.Height - 3), nameSf);

                if (photo.Category != PhotoCategory.None)
                    DrawCategoryBanner(g, size, photo.Category);
            }
            return bmp;
        }

        #endregion

        #region Process & AI

        private async void btnStartProcess_Click(object? sender, EventArgs e)
        {
            if (_scanResult == null || _rootPath == null) return;

            var isAiMode = rbAiMode.Checked;
            DebugLogger.Log($"开始处理: 模式={(isAiMode ? "AI初筛" : "手动分类")}");

            if (isAiMode)
            {
                await RunAiProcess();
            }
            else
            {
                var loadJpg = chkLoadJpg.Checked;
                var loadRaw = chkLoadRaw.Checked;
                var allPhotos = FileScanner.ScanWithCategories(_rootPath, loadJpg, loadRaw);
                OpenBrowser(allPhotos.Photos);
            }
        }

        private async Task RunAiProcess()
        {
            if (_scanResult == null || _rootPath == null) return;

            var jpgPhotos = _scanResult.Photos.FindAll(p => p.FileType == PhotoFileType.JPG && p.Category == PhotoCategory.None);
            var jpgPaths = jpgPhotos.ConvertAll(p => p.FilePath);

            if (jpgPaths.Count == 0)
            {
                MessageBox.Show("没有 JPG 文件可供 AI 识别", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            panelResultRight.Visible = false;
            panelAiProgress.Visible = true;

            progressBarAI.Minimum = 0;
            progressBarAI.Maximum = jpgPaths.Count;
            progressBarAI.Value = 0;
            progressBarAI.Invalidate();
            lblAiStatus.Text = "正在初始化 AI 模型...";

            btnStartProcess.Enabled = false;
            btnStartProcess.Text = "取消";
            btnStartProcess.Click -= btnStartProcess_Click;
            btnStartProcess.Click += CancelAiProcess;

            _aiCts?.Dispose();
            _aiCts = new CancellationTokenSource();

            List<ClassifyResult> results = new();
            bool cancelled = false;
            bool failed = false;
            string errorMsg = "";

            try
            {
                var token = _aiCts.Token;
                var progress = new Progress<(int completed, string fileName)>(p =>
                {
                    progressBarAI.Value = Math.Min(p.completed, progressBarAI.Maximum);
                    progressBarAI.Invalidate();
                    lblAiStatus.Text = $"AI 识别中：{p.completed} / {jpgPaths.Count}  —  {p.fileName}";
                });

                DebugLogger.Log($"RunAiProcess: 开始 AI 识别，共 {jpgPaths.Count} 张 JPG");

                results = await Task.Run(() =>
                {
                    var detector = new ViewFaceDetector();
                    DebugLogger.Log($"RunAiProcess: ViewFaceDetector 创建完成, IsInitialized={detector.IsInitialized}");

                    if (!detector.IsInitialized)
                    {
                        DebugLogger.LogError("RunAiProcess: AI 模型初始化失败", new Exception(detector.InitError ?? "未知错误"));
                        throw new InvalidOperationException($"AI 模型初始化失败：{detector.InitError}");
                    }

                    var batchResults = detector.BatchDetect(jpgPaths, progress as IProgress<(int completed, string fileName)>, token, fullAnalysis: true);
                    detector.Dispose();
                    return batchResults;
                }, _aiCts.Token);

                DebugLogger.Log($"RunAiProcess: AI 识别完成，结果数 {results.Count}");
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
                DebugLogger.Log("RunAiProcess: 用户取消 AI 识别");
            }
            catch (Exception ex)
            {
                failed = true;
                errorMsg = ex.Message;
                DebugLogger.LogError("RunAiProcess: AI 识别异常", ex);
            }

            ResetProcessButton();
            panelAiProgress.Visible = false;
            panelResultRight.Visible = true;

            if (cancelled)
            {
                lblTotalCount.Text = $"合计：{_scanResult.Photos.Count}";
                return;
            }

            if (failed)
            {
                MessageBox.Show($"AI 识别失败：{errorMsg}\n\n详细日志见桌面 ShotSort_debug.txt",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblTotalCount.Text = $"合计：{_scanResult.Photos.Count}";
                return;
            }

            for (int i = 0; i < jpgPhotos.Count && i < results.Count; i++)
            {
                jpgPhotos[i].AiResult = results[i];
            }

            FileSyncManager.EnsureClassifyFolders(_rootPath);
            var personPhotos = new List<PhotoItem>();

            foreach (var photo in jpgPhotos)
            {
                if (photo.AiResult?.HasFace == true)
                {
                    if (FileSyncManager.MoveToClassify(photo.FilePath, FileSyncManager.FolderHasPerson, _rootPath))
                    {
                        photo.FilePath = Path.Combine(_rootPath, FileSyncManager.FolderHasPerson, photo.FileName);
                        photo.Category = PhotoCategory.HasPerson;
                        personPhotos.Add(photo);
                    }
                    else
                    {
                        DebugLogger.Log($"RunAiProcess: 移动失败，跳过 {photo.FileName}");
                    }
                }
            }

            DebugLogger.Log($"RunAiProcess: 检测到 {personPhotos.Count} 张有人像照片");

            if (personPhotos.Count == 0)
            {
                MessageBox.Show($"AI 识别完成，未检测到有人像的照片\n（共扫描 {jpgPaths.Count} 张 JPG）", "结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show(
                $"AI 识别完成！\n检测到 {personPhotos.Count} 张有人像照片\n即将进入人工复核",
                "AI 初筛完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

            OpenBrowser(personPhotos);
        }

        private void CancelAiProcess(object? sender, EventArgs e)
        {
            _aiCts?.Cancel();
        }

        private void ResetProcessButton()
        {
            btnStartProcess.Click -= CancelAiProcess;
            btnStartProcess.Click += btnStartProcess_Click;
            btnStartProcess.Text = "开始处理";
            btnStartProcess.Enabled = true;
            btnStartProcess.BackColor = PrimaryColor;
            ApplyRoundRegion(btnStartProcess, 8);
        }

        #endregion

        private void lstPhotos_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (_scanResult == null || _rootPath == null) return;

            var hitInfo = lstPhotos.HitTest(e.Location);
            if (hitInfo.Item == null) return;

            int index = hitInfo.Item.Index;
            if (index < 0 || index >= _scanResult.Photos.Count) return;

            var photo = _scanResult.Photos[index];
            OpenBrowser(new List<PhotoItem> { photo });
        }

        #region Browser & Navigation

        private void OpenBrowser(List<PhotoItem> photos)
        {
            if (_rootPath == null) return;

            var isAiMode = rbAiMode.Checked;
            var browserForm = new ImageBrowserForm(photos, _rootPath, isAiMode);
            Hide();
            browserForm.ShowDialog(this);
            Show();

            if (_scanResult != null)
                RefreshPhotoList();
            else
                ResetToInput();
        }

        private void RefreshPhotoList()
        {
            if (_rootPath == null) return;

            var loadJpg = chkLoadJpg.Checked;
            var loadRaw = chkLoadRaw.Checked;
            var result = FileScanner.ScanWithCategories(_rootPath, loadJpg, loadRaw);

            _scanResult = result;
            ShowScanResult();
        }

        private void ResetToInput()
        {
            _thumbnailCts?.Cancel();
            panelResult.Visible = false;
            panelInput.Visible = true;
            _scanResult = null;
            _rootPath = null;
            lstPhotos.Items.Clear();
            thumbnailImageList.Images.Clear();
            _thumbnailLru.Clear();
            lock (_thumbnailLoading) { _thumbnailLoading.Clear(); }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            ResetToInput();
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _thumbnailCts?.Cancel();
            _thumbnailCts?.Dispose();
            _aiCts?.Cancel();
            _aiCts?.Dispose();
            _thumbnailLru.Clear();
            var currentPath = txtFolderPath.Text.Trim();
            if (!string.IsNullOrEmpty(currentPath) && currentPath != "点击右侧按钮选择照片文件夹...")
            {
                AppSettings.LastFolderPath = currentPath;
                AppSettings.Save();
            }
            DebugLogger.Log("ShotSort 关闭");
            base.OnFormClosed(e);
        }
    }
}
