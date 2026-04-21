# Knot - Unity 剧情演出系统

## 项目简介

Knot 是一个基于 Unity 的剧情演出系统，采用数据驱动架构，支持 JSON 格式的剧情脚本序列化与反序列化。系统提供帧级别的指令执行机制，支持对象池化的执行器管理，可扩展的指令系统设计。

## 特性

- **数据驱动**: 基于 JSON 的剧情脚本格式，支持序列化与反序列化
- **帧级执行**: 按帧执行剧情指令，精确控制演出时序
- **对象池化**: 执行器对象池管理，优化运行时性能
- **可扩展**: 灵活的指令系统，支持自定义指令参数与执行器
- **编辑器支持**: 提供编辑器工具辅助开发

## 项目结构

```
Knot/
├── Runtime/                          # 运行时脚本
│   ├── Allocation/                   # 对象分配与池化
│   │   ├── ExecutePool.cs           # 执行器对象池
│   │   └── FrameExecuteList.cs      # 帧执行列表管理
│   ├── Attributes/                   # 自定义属性
│   │   ├── DisplayInEditorAttribute.cs
│   │   └── VersionAttribute.cs      # 版本标记属性
│   ├── Core/                         # 核心接口与基类
│   │   ├── InstrExecute.cs          # 指令执行器基类
│   │   ├── InstrParam.cs            # 指令参数基类
│   │   └── InstructionTemplate.cs   # 指令模板
│   ├── Data/                         # 数据模型
│   │   ├── Frame.cs                 # 帧数据模型
│   │   └── FrameList.cs             # 帧列表模型
│   ├── Execution/                    # 执行控制
│   │   ├── FrameExecute.cs          # 帧执行器
│   │   └── PlotPerformSys.cs        # 剧情演出系统主控制器
│   └── Utilities/                    # 工具类
│       ├── JsonPrettyPrinter.cs     # JSON 格式化工具
│       ├── ReflectionHelper.cs      # 反射辅助工具
│       ├── ReflectionPrinter.cs     # 反射打印工具
│       ├── ReflectionSerializer.cs  # 反射序列化工具
│       └── StringExtensions.cs      # 字符串扩展方法
├── Editor/                           # 编辑器扩展脚本
│   ├── Construct/                    # 构建工具
│   │   ├── ScriptTemplateGenerator.cs
│   │   └── ScriptTemplateGenerator.json
│   └── Utility/                      # 编辑器工具
│       └── ClassNameValidator.cs    # 类名验证器
├── Instruction/                      # 指令实现
│   ├── AUDI.cs                       # 音频指令
│   ├── IMAG.cs                       # 图像指令
│   ├── JUMP.cs                       # 跳转指令
│   ├── SELC.cs                       # 选择指令
│   ├── TypeDialogue.cs               # 对话指令
│   └── VIDE.cs                       # 视频指令
├── Samples~/                         # 示例代码（打包时排除）
│   ├── Data/
│   │   └── SamplePlot.json          # 示例剧情脚本
│   └── Scripts/
│       ├── SampleExecute.cs         # 示例执行器
│       ├── SampleParam.cs           # 示例参数
│       └── SampleSceneController.cs # 示例场景控制器
├── Tests/                            # 测试代码
│   ├── Editor/                       # 编辑器测试
│   └── Runtime/                      # 运行时测试
└── Documentation~/                   # 文档（打包时排除）
    ├── 基于行为序列化方式的剧情演出系统内核.md
    └── Knot Editor Manual/
        ├── Knot Editor 开发手册.md
        ├── 用户手册.md
        ├── 首页.md
        └── Knot Editor 开发手册/
            ├── Knot 产品目标.md
            ├── Knot 架构.md
            ├── Knot 产品目标/
            │   ├── 玩家端-游戏界面.md
            │   ├── 程序端-开发协议.md
            │   ├── 策划端-编辑窗口.md
            │   └── 策划端-编辑窗口/
            │       └── 演出策划招募.md
            └── Knot 架构/
                ├── 分配层.md
                ├── 执行层.md
                ├── 扩展层.md
                ├── 数据层.md
                └── 表现层.md
```

## 命名空间规范

- `Knot.Runtime.*` - 运行时脚本命名空间
- `Knot.Editor.*` - 编辑器脚本命名空间
- `Knot.Instruction` - 指令实现命名空间
- `Knot.Samples` - 示例代码命名空间

## 快速开始

### 1. 创建剧情脚本

创建一个 JSON 格式的剧情脚本文件：

```json
{
  "frames": [
    {
      "frameIndex": 0,
      "instructions": [
        {
          "name": "TypeDialogueParam",
          "dialogue": {
            "name": "角色名",
            "sentence": "对话内容"
          }
        }
      ]
    }
  ]
}
```

### 2. 配置剧情系统

在场景中添加 `PlotPerformSys` 组件，并指定剧情脚本文件。

### 3. 自定义指令

继承 `InstrParam` 创建指令参数类，继承 `InstrExecute` 创建执行器类。

## 系统架构

Knot 采用分层架构设计：

- **数据层**: 负责 JSON 数据的序列化与反序列化
- **分配层**: 管理执行器对象池与帧执行列表
- **执行层**: 控制帧级指令执行流程
- **扩展层**: 提供自定义指令扩展接口

## 版本要求

- Unity 2021.3 或更高版本
- .NET Standard 2.1

## 许可证

MIT License

## 贡献指南

欢迎提交 Issue 和 Pull Request。请确保代码符合项目的命名规范和架构设计。
