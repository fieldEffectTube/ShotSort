# ShotSort

基于预训练模型的 AI 人脸检测照片快速筛选分类工具，通过AI预筛和简单的键盘快捷键手动操作，帮助摄影师从大量照片中高效分拣照片。

![Version](https://img.shields.io/badge/version-1.0.0-blue) ![.NET](https://img.shields.io/badge/.NET-8.0-purple) ![Platform](https://img.shields.io/badge/platform-Windows%20x64-lightgrey)

## 下载

| 平台 | 下载 |
|------|------|
| Windows x64 (免安装) | [百度网盘](链接: https://pan.baidu.com/s/1f3TNKgHiHgdPPHVlany2vA?pwd=garx 提取码: garx) |

下载后解压，双击 `ShotSort.exe` 即可运行，无需安装 .NET 运行时。

## 快速上手

1. 选择照片所在的文件夹
2. 勾选要加载的格式（JPG / RAW），点击「开始读取」
3. 选择工作模式，点击「开始处理」：
   - **AI 初筛模式** — AI 自动检测人像，将有人像的照片移入「有人」文件夹，之后进入浏览界面人工复核
   - **手动分类模式** — 直接进入浏览界面，逐张手动分类
4. 在浏览界面用快捷键快速标记每张照片
5. 完成后自动打开分类目录

## 使用说明

### 第一步：选择文件夹并扫描

启动 ShotSort 后进入主界面：

1. 点击「浏览」按钮选择存放照片的文件夹
2. 勾选需要扫描的图片格式：
   - **JPG** — 扫描 `.jpg` / `.jpeg` 文件，AI 识别仅支持 JPG
   - **RAW** — 扫描 `.cr2` / `.cr3` / `.dng` / `.nef` / `.raf` 文件
3. 点击「开始读取」，扫描完成后显示缩略图列表和文件统计

### 第二步：选择模式并处理

扫描完成后，右侧面板选择工作模式：

#### AI 初筛模式

点击「开始处理」后，AI 会自动对每张 JPG 进行人脸检测，分析：
- 是否包含人脸及人脸数量
- 眼睛状态（睁眼 / 闭眼）
- 清晰度（是否模糊）
- 置信度（是否为低置信度人像）

检测完成后：
- 有人像的照片自动移入「有人」子文件夹
- 在缩略图上标注分类标签（人像 / 合影 / 多人 / 闭眼 / 模糊 / 低置信度 / 低质量）
- 弹窗提示检测结果，可选择立即进入人工复核

> 处理过程中按钮变为「取消」，可随时中断。

#### 手动分类模式

点击「开始处理」后直接进入照片浏览界面，逐张查看并分类，不经过 AI 检测。

### 第三步：浏览与分类

浏览界面以大图逐张展示照片，通过快捷键快速操作：

| 快捷键 | AI 初筛模式 | 手动分类模式 |
|--------|------------|-------------|
| ← | 上一张 | 上一张 |
| → | 下一张 | 下一张 |
| ↑ | 标记为「精选」 | 标记为「精选」 |
| ↓ | 标记为「保留」 | 标记为「保留」 |
| Space / Delete | **永久删除** | 移入「待删除」 |
| PageDown | 顺时针旋转 90° | 顺时针旋转 90° |

- 标记后照片自动移入对应分类文件夹并跳转到下一张
- AI 模式下 Space/Delete 为永久删除，勾选「同时删除 RAW」可连带删除配对的 RAW 文件
- 手动模式下 Space/Delete 为移入「待删除」文件夹，不会真正删除
- 照片左上角显示 AI 检测标签（人像 / 合影 / 闭眼 / 模糊 等）
- 如果照片有配对的 RAW 文件，会显示「含RAW」标签，分类时 RAW 文件会同步移动

### 分类结果

照片分类后，原文件夹下自动创建以下子目录：

```
你的照片文件夹/
├── 有人/          ← AI 检测到人像的照片
├── 精选/          ← 人工标记的精选照片
├── 保留/          ← 人工标记的保留照片
├── 待删除/        ← 标记为待删除的照片（仅手动模式）
└── (未分类照片)    ← 未被处理的原始照片
```

> AI 初筛模式下，有人像的照片先移入「有人」文件夹，再在浏览界面中进一步细分为精选/保留/删除。

## 功能特性

- **AI 人脸检测** — 基于 ViewFaceCore (SeetaFace6) 自动识别照片中的人脸、眼睛状态和清晰度
- **智能分类** — 四级分类（有人 → 精选/保留/删除），AI 初筛 + 人工复核
- **RAW 文件联动** — 自动关联同名的 RAW 文件（CR2/CR3/DNG/NEF/RAF），分类时同步移动
- **缩略图浏览** — 卡片式缩略图布局，按需加载，支持大量图片流畅滚动
- **快捷键操作** — 键盘单手操作，快速完成分类
- **EXIF 旋转** — 自动读取 EXIF 方向信息，正确显示照片朝向
- **断点安全** — 分类结果实时写入磁盘，随时退出不丢失

## 技术栈

- .NET 8 / C# WinForms
- [ViewFaceCore](https://github.com/ViewFaceCore/ViewFaceCore) — 基于 SeetaFace6 的人脸识别框架
- [Magick.NET](https://github.com/dlemstra/Magick.NET) — RAW 格式图像解码

## 构建与运行

### 前置要求

- .NET 8 SDK
- Windows x64

### 从源码构建

```bash
dotnet restore
dotnet build
dotnet run --project ShotSort
```

### 发布独立部署

```bash
dotnet publish ShotSort/ShotSort.csproj -c Release -r win-x64 --self-contained true
```

发布产物无需安装 .NET 运行时即可运行。

## 项目结构

```
ShotSort/
├── ShotSort.sln
├── ShotSort/
│   ├── Program.cs                      # 应用入口
│   ├── ShotSort.csproj
│   ├── app.ico                         # 应用图标
│   ├── Properties/
│   │   └── Resources.cs                # 嵌入资源访问
│   ├── Core/
│   │   ├── ViewFaceDetector.cs         # AI 人脸检测核心
│   │   ├── ViewFaceCoreInitializer.cs  # ViewFaceCore 运行时初始化
│   │   ├── FileScanner.cs              # 图片文件扫描器
│   │   ├── FileSyncManager.cs          # 分类目录管理与文件同步
│   │   └── HotkeyManager.cs            # 快捷键管理
│   ├── Forms/
│   │   ├── MainForm.cs                 # 主窗口（扫描 & 缩略图）
│   │   └── ImageBrowserForm.cs         # 浏览器（查看 & 分类）
│   ├── Models/
│   │   ├── ClassifyResult.cs           # AI 分类结果模型
│   │   └── PhotoItem.cs                # 照片项模型
│   └── Utils/
│       ├── AppSettings.cs              # 应用设置持久化
│       ├── DebugLogger.cs              # 调试日志工具
│       ├── ImageLoader.cs              # 图片加载器
│       ├── ListViewScrollHook.cs       # 滚动优化钩子
│       └── RawFileHelper.cs            # RAW 文件配对工具
├── IsolationTest/                      # ViewFaceCore 隔离测试项目
└── docs/                               # 参考文档
```

## 已知问题

- **AMD Ryzen 兼容性** — ViewFaceCore 的 tennis.dll 使用 AVX2/FMA 指令集，在部分 AMD 处理器上可能触发 IllegalInstruction 异常。已通过 `GlobalConfig.SetX86Instruction(X86Instruction.SSE2)` 降级处理
- **ViewFaceCore API 差异** — NuGet runtime 包 6.0.6 与源码 0.4.0-alpha4 的 EntryPoint 名称不一致，本项目使用源码引用方式

## 许可证

本项目仅供个人学习使用。
