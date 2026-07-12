# ShotSort 开发步骤

> 基于 C# WinForm .NET Framework 4.8 的本地照片批量分类桌面工具

---

## 阶段一：项目搭建与基础架构 ✅

### 1.1 创建 WinForm 项目
- 新建 C# WinForm .NET Framework 4.8 项目，命名为 ShotSort
- 配置项目属性、输出类型、默认命名空间

### 1.2 引入依赖包
- NuGet 安装 ViewFaceCore（人脸检测核心库）
- NuGet 安装 ViewFaceCore 对应的运行时模型包（viewfacecore.runtime.win.x86 / x64）
- NuGet 安装其他必要包（如图片解码库 Magick.NET 用于 RAW 支持）

### 1.3 建立项目分层目录结构
```
ShotSort/
├── Forms/                  # 窗体
│   ├── MainForm.cs         # 首页窗体
│   └── ImageBrowserForm.cs # 图片浏览快速处理窗体
├── Core/                   # 核心业务逻辑
│   ├── FileScanner.cs      # 文件夹扫描工具类
│   ├── FileSyncManager.cs  # 文件移动同步工具类
│   ├── ViewFaceDetector.cs # ViewFaceCore AI 识别封装类
│   └── HotkeyManager.cs    # 全局快捷键监听模块
├── Models/                 # 数据模型
│   ├── PhotoItem.cs        # 照片文件数据模型
│   └── ClassifyResult.cs   # 分类结果模型
├── Utils/                  # 通用工具
│   ├── ImageLoader.cs      # 图片轻量化加载工具
│   └── RawFileHelper.cs    # RAW 文件配对与格式识别工具
└── Program.cs              # 入口
```

---

## 阶段二：首页窗体（MainForm） ✅

### 2.1 界面布局实现
- 顶部：文件夹路径文本框 + 文件夹选择按钮（FolderBrowserDialog）
- 中部：两个 CheckBox —— 「加载 JPG/JPEG 图片」「加载 RAW 原图（CR2/CR3/DNG/NEF/RAF）」
- 底部：【开始读取】按钮

### 2.2 文件夹扫描逻辑（FileScanner）
- 遍历指定目录下所有文件，根据勾选项过滤格式
- JPG/JPEG 扩展名匹配：.jpg、.jpeg
- RAW 扩展名匹配：.cr2、.cr3、.dng、.nef、.raf
- 分别统计 JPG 文件数、RAW 文件数
- 返回扫描结果列表（List<PhotoItem>）

### 2.3 扫描完成后界面切换
- 隐藏初始输入区域，显示左右分栏布局：
  - 左侧：ListBox/ListView 展示全部照片文件名
  - 右侧上方：汇总信息区（JPG 总数、RAW 总数、合计总数）
  - 右侧下方：两个 RadioButton（「计算机废片 AI 初筛」「手动快速分类」）+ 【开始处理】按钮
- 点击【开始处理】根据选中模式跳转至 ImageBrowserForm

---

## 阶段三：图片浏览快速处理窗体（ImageBrowserForm） ✅

### 3.1 窗体基础布局
- 顶部：ProgressBar + 模式说明文字 + 模式专属 CheckBox（AI 模式可见：「同时删除 RAW」）
- 中间：PictureBox 大图预览区，显示当前照片
- 顶部悬浮提示标签：操作反馈文字，2 秒自动消失

### 3.2 图片轻量化加载（ImageLoader）
- JPG 文件使用 System.Drawing.Bitmap 加载，设置 DecodePixelWidth 限制解码尺寸，降低内存占用
- RAW 文件使用 Magick.NET 解码为 Bitmap 后显示
- 切换图片时释放上一张资源，避免内存泄漏

### 3.3 全局快捷键监听（HotkeyManager）
- 重写窗体 ProcessCmdKey 方法，拦截方向键与 Ctrl 键
- 快捷键映射：
  - ← 上一张照片
  - → 下一张照片
  - ↑ 标记精选，移至【精选】文件夹
  - ↓ 标记留档，移至【保留】文件夹
  - Ctrl 模式差异化行为（见 3.4 / 3.5）
- 操作完成后显示悬浮提示：「{文件名} 已被正确处理」，2 秒后自动隐藏

### 3.4 模式 1：AI 废片初筛 —— 浏览阶段行为
- Ctrl 键：直接永久删除当前 JPG（无确认弹窗）；勾选「同时删除 RAW」时同步删除配对 RAW
- ↑/↓ 键：均执行保留逻辑，分别移入【精选】/【保留】文件夹
- 进度条显示：当前第 N 张 / 总人像照片数
- 遍历至最后一张弹窗提示分类完成

### 3.5 模式 2：手动快速分类 —— 浏览阶段行为
- Ctrl 键：将当前照片移入【待删除】文件夹（非物理删除）
- ↑/↓ 键：分别移入【精选】/【保留】文件夹
- 进度条显示：手动分类完成进度
- 遍历至最后一张弹窗提示分类完成

### 3.6 照片列表遍历与导航
- 维护当前照片索引，支持前后切换
- 切换时加载对应照片至预览区
- 到达末尾时弹出完成提示

---

## 阶段四：ViewFaceCore AI 识别封装（ViewFaceDetector） ✅

### 4.1 人脸检测封装
- 初始化 ViewFaceCore FaceDetector
- 输入 JPG 图片路径，返回人脸数量及位置信息
- 无人脸 → 标记为「无人像」
- 有人脸 → 进入二次识别

### 4.2 眼部状态识别
- 调用 ViewFaceCore EyeStateDetector
- 区分：双眼正常睁开 / 单眼闭眼 / 双眼闭眼
- 返回眼部状态枚举结果

### 4.3 人脸清晰度 / 对焦评分
- 调用 ViewFaceCore 人脸质量评估 API（FaceQuality）
- 评估指标：模糊度、对焦评分
- 区分：正常清晰 / 失焦模糊废片

### 4.4 AI 批量处理流程
- 仅对 JPG 文件执行 AI 识别（跳过 RAW 降低算力）
- 逐张调用：人脸检测 → 有人脸则继续眼部 + 清晰度
- 标记分类结果：无人像 / 有人像-正常 / 有人像-闭眼 / 有人像-失焦
- 全部识别完成后：所有「有人像」照片移动至【有人】文件夹
- 自动跳转至浏览界面，仅加载【有人】文件夹照片

---

## 阶段五：文件移动同步工具（FileSyncManager） ✅

### 5.1 RAW 配对逻辑（RawFileHelper）
- 根据 JPG 文件名（不含扩展名），在同目录查找同名 RAW 文件
- 遍历 RAW 扩展名列表匹配：.cr2 / .cr3 / .dng / .nef / .raf
- 找到配对 RAW → 返回完整路径；未找到 → 返回 null

### 5.2 分类文件夹自动创建
- 在读取根目录下自动创建子文件夹：
  - 【有人】—— AI 模式有人像照片
  - 【精选】—— 人工标记精选
  - 【保留】—— 人工标记留档
  - 【待删除】—— 手动模式待删照片
- 文件夹已存在则跳过创建

### 5.3 同步移动方法
- MoveToClassify(sourceJpg, targetFolder)
- 自动查找配对 RAW，JPG + RAW 同步移动至目标文件夹
- 单张移动失败不影响另一张

### 5.4 同步删除方法（AI 模式专用）
- DeletePhoto(sourceJpg, deleteRaw)
- 永久删除 JPG 文件
- deleteRaw=true 时同步永久删除配对 RAW 文件
- deleteRaw=false 时仅删除 JPG

### 5.5 移入待删除方法（手动模式专用）
- MoveToPendingDelete(sourceJpg)
- JPG + 配对 RAW 同步移入【待删除】文件夹

---

## 阶段六：数据模型定义 ✅

### 6.1 PhotoItem 模型
- FilePath：文件完整路径
- FileName：文件名（含扩展名）
- Extension：文件扩展名
- FileType：枚举（JPG / RAW）
- PairedRawPath：配对 RAW 文件路径（可为空）
- AiResult：AI 识别结果（可为空，手动模式不使用）

### 6.2 ClassifyResult 模型
- HasFace：是否有人脸
- EyeState：眼部状态枚举（Open / OneEyeClosed / BothClosed）
- ClarityScore：清晰度评分
- IsBlur：是否模糊废片

---

## 阶段七：集成与联调 ✅

### 7.1 首页 → 浏览窗体数据传递
- 通过构造函数 / 属性传递：照片列表、当前模式、根目录路径

### 7.2 AI 模式完整流程联调
- 首页扫描 → 调用 ViewFaceDetector 批量识别 → 移动有人像至【有人】→ 打开浏览窗体加载【有人】照片 → 人工复核

### 7.3 手动模式完整流程联调
- 首页扫描 → 直接打开浏览窗体加载全部照片 → 手动分类

### 7.4 快捷键 + 文件同步端到端验证
- 验证每个快捷键操作：JPG 与配对 RAW 同步移动/删除
- 验证悬浮提示正常显示与消失
- 验证遍历完成弹窗

---

## 阶段八：性能优化与异常处理 ✅

### 8.1 批量读取性能优化
- 文件扫描使用并行遍历（Parallel / PLINQ），加快大目录扫描速度
- 图片列表虚拟化加载（ListView VirtualMode），避免大量 Item 卡顿

### 8.2 图片预览性能优化
- 大图按控件尺寸缩小解码，不加载全分辨率
- 切换图片时异步加载，避免界面卡顿
- 及时 Dispose 上一张 Bitmap

### 8.3 AI 处理性能优化
- 仅对 JPG 执行 AI 运算，RAW 完全跳过
- 考虑后台线程执行 AI 批量识别，前台显示进度

### 8.4 异常处理
- 文件被占用/锁定时的异常捕获与跳过
- RAW 配对不存在时的降级处理（仅操作 JPG）
- AI 识别异常时的容错（标记为未知，不中断流程）

---

## 阶段九：测试与交付 ✅

### 9.1 功能测试
- JPG / RAW 混合目录扫描正确性
- AI 无人像 / 有人像分类准确性
- 快捷键四方向 + Ctrl 全场景验证
- 文件同步移动/删除一致性验证
- 遍历结束提示验证

### 9.2 边界测试
- 空目录 / 无匹配文件
- 仅 JPG 无 RAW / 仅 RAW 无 JPG
- 超大目录（1000+ 照片）性能
- 同名不同扩展名文件配对

### 9.3 打包发布
- 确认 ViewFaceCore 运行时模型文件随输出目录一起打包
- 确认 Magick.NET 原生库正确复制到输出目录
- 生成可独立运行的发布包
