# ShotSort

基于 AI 人脸检测的照片快速筛选分类工具，帮助摄影师从大量照片中高效挑选出最佳人像。

## 功能特性

- **AI 人脸检测** — 基于 ViewFaceCore (SeetaFace) 自动识别照片中的人脸、眼睛状态和清晰度
- **智能分类** — 将照片自动分类为「有人」「精选」「保留」「待删除」四个类别
- **RAW 文件联动** — 自动关联同名的 RAW 文件（CR2/CR3/DNG/NEF/RAF），分类时同步处理
- **缩略图浏览** — 卡片式缩略图布局，虚拟化滚动加载，流畅浏览大量图片
- **快捷键操作** — 支持键盘快捷键快速标记和切换照片
- **批量处理** — 一键扫描目录下所有 JPG 图片，批量 AI 分析后人工复核

## 技术栈

- .NET 8 / C# WinForms
- [ViewFaceCore](https://github.com/ViewFaceCore/ViewFaceCore) — 基于 SeetaFace6 的人脸识别框架

## 项目结构

```
ShotSort/
├── ShotSort.sln
├── ShotSort/
│   ├── Program.cs              # 应用入口
│   ├── ShotSort.csproj
│   ├── Core/
│   │   ├── ViewFaceDetector.cs       # AI 人脸检测核心
│   │   ├── ViewFaceCoreInitializer.cs # ViewFaceCore 运行时初始化
│   │   ├── FileScanner.cs            # 图片文件扫描器
│   │   ├── FileSyncManager.cs        # 分类目录管理与文件同步
│   │   └── HotkeyManager.cs          # 快捷键管理
│   ├── Forms/
│   │   ├── MainForm.cs               # 主窗口（扫描 & 缩略图）
│   │   └── ImageBrowserForm.cs       # AI 浏览器（筛选 & 标记）
│   ├── Models/
│   │   ├── ClassifyResult.cs         # AI 分类结果模型
│   │   └── PhotoItem.cs              # 照片项模型
│   └── Utils/
│       ├── AppSettings.cs            # 应用设置持久化
│       ├── DebugLogger.cs            # 调试日志工具
│       ├── ImageLoader.cs            # 图片加载器
│       ├── ListViewScrollHook.cs     # 滚动优化钩子
│       └── RawFileHelper.cs          # RAW 文件配对工具
├── IsolationTest/                    # ViewFaceCore 隔离测试项目
└── docs/                             # 参考文档
    ├── ViewFaceCore API 文档.md
    └── ViewFaceCore issues.txt
```

## 分类逻辑

| 类别 | 说明 |
|------|------|
| 有人 | AI 检测到人脸的照片 |
| 精选 | 人工标记为精选的照片 |
| 保留 | 人工标记为保留的照片 |
| 待删除 | 人工标记为待删除的照片 |

## 构建与运行

```bash
# 还原依赖
dotnet restore

# 构建
dotnet build

# 运行
dotnet run --project ShotSort
```

> **注意：** ViewFaceCore 运行时依赖本地 native 库，需确保对应平台的 runtime 包已正确安装。在 AMD Ryzen 处理器上需设置 `GlobalConfig.SetX86Instruction(X86Instruction.SSE2)` 以避免 AVX2 兼容性问题。

## 许可证

本项目仅供个人学习使用。
