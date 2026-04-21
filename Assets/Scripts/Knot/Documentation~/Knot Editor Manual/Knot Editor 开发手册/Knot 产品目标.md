# Knot 产品目标

# 项目背景

随着游戏产业的快速发展，剧情演出已成为各类游戏的核心组成部分，尤其是在国内游戏行业，一个游戏项目在以年为单位的运营过程中，可能出现剧情演出方式极大的变化，对应着技术开发需求的变化。

当下游戏的剧情演出，已经出现了影视化方向发展，分镜、运镜、转场、音频、场景设计、对话、文本等都可能出现动态加载的情况，并且在有些游戏当中，甚至探索出了专属游戏的演出方式。

这些现象都揭示了当前剧情演出的复杂化，都使得传统的剧情演出编辑系统无法满足当前游戏开发的需求。

当前多数游戏开发团队，尤其是学生团队和小型工作室，在剧情演出开发中面临策划与程序耦合过紧、开发效率低下、资源管理混乱等问题。

在 Unity 生态中，已存在成熟的剧情系统解决方案。其中，**Fungus** 是一款免费开源的互动叙事可视化脚本插件，自推出以来被广泛应用于各类叙事驱动型游戏的开发。它通过流程图式的可视化脚本系统，让非程序员也能创建复杂的对话和剧情逻辑。

**Fungus 的核心特点**：

- **无代码编程**：通过拖拽节点和配置命令块实现剧情逻辑
- **流程图式编辑**：Block 作为基本单元，通过连线表示流程走向
- **丰富的内置命令**：Say（对话）、Menu（选项）、Set Variable（设置变量）、If（条件判断）等
- **角色与肖像系统**：支持多角色、多表情管理
- **相机与音效控制**：内置 Fade Camera、Play Sound 等命令
- **本地化支持**：多语言对话管理
- **开源免费**：MIT 许可证，社区活跃

**代表案例**：《炉石传说》《INSIDE》《空洞骑士》《看火人》等商业游戏都使用了 Fungus。

尽管 Fungus 功能强大且成熟，但经过深入调研和分析，我们发现其设计定位与本项目的核心目标存在显著差异：

<table>
<tr>
<td>**维度**<br/></td><td>**Fungus**<br/></td><td>**本项目**<br/></td></tr>
<tr>
<td>**核心理念**<br/></td><td>可视化脚本工具<br/></td><td>数据驱动框架<br/></td></tr>
<tr>
<td>**数据存储**<br/></td><td>场景内对象、序列化文件<br/></td><td>JSON纯数据文件<br/></td></tr>
<tr>
<td>**运行时架构**<br/></td><td>对象+组件<br/></td><td>数据+执行器池<br/></td></tr>
<tr>
<td>**扩展方式**<br/></td><td>继承Command类<br/></td><td>继承InstrExecute基类<br/></td></tr>
<tr>
<td>**资源复用**<br/></td><td>动态实例化<br/></td><td>静态预分配+复用<br/></td></tr>
<tr>
<td>**适用场景**<br/></td><td>中小型叙事游戏<br/></td><td>大型项目、热更新需求<br/></td></tr>
<tr>
<td>**开发流程**<br/></td><td>策划在编辑器内操作<br/></td><td>策划使用编辑器编写<br/></td></tr>
<tr>
<td>**使用平台**<br/></td><td>Unity2019<br/></td><td>Unity2022<br/></td></tr>
</table>

**核心差异点**：

1. **数据驱动程度**：Fungus 的剧情逻辑依附于场景中的 Flowchart 对象，难以实现纯数据驱动的热更新和动态加载。有开发者尝试改造 Fungus 以实现数据驱动，但需要自定义 Command 并修改源码。
2. **资源复用机制**：Fungus 采用动态实例化方式创建对话对象，而本项目设计的静态预分配 + 执行器复用机制，在大量指令场景下内存占用更优。
3. **序列化方案**：Fungus 使用 Unity 的序列化系统，而本项目采用 JSON 纯文本序列化，更易于版本控制、多语言管理和热更新。
4. **学习目标**：本项目作为软件工程课程设计，目标是深入理解数据驱动架构、反射序列化、设计模式等核心概念，而不仅仅是使用现成工具。

因此，**本项目并非重复造轮子，而是在借鉴 Fungus 优秀设计的基础上，探索一条差异化技术路线**——更强调数据驱动、资源优化和可扩展性，为游戏开发提供另一种技术选型。

---

# 功能架构

## 玩家端（游戏运行情境）

玩家得到的是已经开发并打包好的游戏成品，不会接触任何与 Unity Editor 或 Knot Editor 相关的界面与功能。

玩家得到的有以下基本功能：

### 基础功能

- **帧运行**：在一帧内，玩家可以等到完整的指令运行；不同帧的指令相互不干扰。
- **帧执行增强**：

  - **限时决策 (Timed Choices)：** 在分支选择处加入倒计时，超时自动触发默认选项或隐藏分支（如《行尸走肉》系列）。
  - **QTE / 指令交互：** 演出过程中插入简单的动作交互（按键、滑动、连打），成功与否直接改变下一帧的演出效果。
- **帧推进**：本帧所有指令运行完成后，停止，玩家通过输入操作进入下一帧；或接管推进权限，禁止玩家手动中断和推进。
- **暂停**：点击暂停，停止当前帧所有正在运行的指令；解除暂停后，当前帧所有指令恢复运行，中间不中断或重置。
- **快进**：对当前帧所有运行指令进行倍速运行。
- **跳过**：

  - **帧中断**：玩家通过输入操作<u>跳过本帧</u>。
  - **全跳过**：玩家将跳过本段剧情至演出策划指定的位置。
  - **即时回退 (Rollback)：** 不同于存档读取，玩家可以通过鼠标滚轮向上或点击按钮，**逐帧**反向运行指令（撤销立绘位移、立绘淡出、音效播放等），将游戏状态恢复到前几秒的样子。
- **自动播放**：自动推进帧运行。
- **对话记录**：点击后暂停，进入记录界面，显示之前演出过的帧的<u>对话、音频或图片</u>；退出后解除暂停。

### 分支与保存

- **状态快照**** (State Snapshot) **：保存当前状态的全局参数，全局参数代表了玩家的体验经历。
- **快速存读档**：在存读档处记录并跳转。
- **自动存档点****：** 在重大转折或选项前系统自动记录，防止玩家忘记存档。
- **选项回溯：** 选错后能立即返回上一个决策点，无需手动读取存档。
- **分支选择**：根据前面演出时玩家做出的动作，记录玩家动作，并在选择分支时通过记录走向满足预设条件的分支。
- **分支流程图系统**：可视化剧情走向，显示已锁定的分支和达成条件，并支持**直接跳转**到特定节点。

### 视觉与沉浸式体验

- **UI 隐藏：** 一键清空所有按键和对话框，方便玩家截图或欣赏 CG。
- **文本透明度调节：** 防止对话框遮挡立绘关键部位。
- **多语言切换：** 实时切换中英文文本（常用于外语学习或对比翻译）。
- **语音平衡调节：** 可以单独开关或调节某一个角色的音量（比如玩家讨厌某个角色的声音，或者某个声优声音太小）。
- **人物/词条百科 (Glossary/Tips)：** 遇到专有名词时，点击文本即可弹出解释弹窗。
- **屏幕比例调整**：在沉浸式过场时调整 16:9 或 16:10 比例为 21:9 等电影比例。

### 辅助功能与无障碍 (Accessibility)

- **语音转文字/辅助描述 (Audio Description)：** 为视障或听障玩家提供背景音效的文字描述（如：[远处传来沉闷的雷声]）。
- **色盲模式：** 针对 UI 中的选项、流程图连线提供色觉辅助。

## 策划开发端（Unity Editor 情境）

演出策划在 Unity Editor 中的特殊编辑窗口（Knot Editor）中编辑剧情演出，其核心特点是：**可视化、低代码、实时预览**。

策划在 Knot Editor 窗口中可以得到以下功能：

### 剧情列表编辑器

- **节点类型支持**：
  - **一般节点**：一般帧执行。
  - **分支节点**：引用分支条件，判断走向。
  - **跳转节点**： 跨章节或长距离流程跳转。
    - 比较复杂
      ```markdown
      ```

“跨章节”或“长距离流程跳转”在剧情编辑器（Graph Editor）中通常被称为 **“锚点跳转” (Anchor / Global Jump)** 或 **“剧情传送门”**。
在处理数万字、上千个节点的复杂剧本时，策划不可能只靠一条长线连到底。这个功能的核心逻辑是**打破物理连线的限制，实现逻辑上的瞬间转移**。
以下是该功能在 Unity 编辑器中具体的实现形态和必要性：

1. 为什么需要这个功能？（痛点解决）
   **防止“面条代码” (Spaghetti Connections)：** 如果一个序章的选项会导致第五章的结局，你不可能在编辑器里拉一根横跨几万像素的线，那会让画布变得无法维护。
   **公共模块调用：** 比如“进入战斗”或“商店系统”是独立的剧情块，任何章节都可能跳转过去，结束后再跳回。
   **开发调试：** 策划需要直接跳到“第三章·教堂冲突”开始测试，而不需要从第一秒开始跑游戏。
2. 在编辑器中的表现形式
   通常有两种主流的设计方案：
   A. 标签与监听模式 (Label & Goto)
   类似于编程里的 goto 语句。
   **入口节点 (Jump Gate)：** 策划在节点里填入一个 ID（如 Ch3_Battle_Start）。
   **出口节点 (Anchor/Label)：** 在另一个远端画布或章节文件中，放置一个名为 Ch3_Battle_Start 的起始点。
   **效果：** 当运行到入口时，演出引擎自动卸载当前资源，加载目标位置。
   B. 传送门节点 (Teleport Node)
   在可视化画布上，成对出现的特殊节点。
   左键点击“传送门入口”，编辑器视角会自动平移（Pan）或切换文件，定位到对应的“出口”节点。这样策划在编辑时不会迷路。
3. 最核心的技术难点：状态重构 (State Restoration)
   这是“长距离跳转”最硬核的部分。如果你从第一章直接跳到第五章，**游戏环境（Context）是不对的**。
   为了让跳转可用，你的编辑器功能需要配套：
   **预设状态注入 (Initial State Injection)：** 策划在跳转节点上可以配置：“假设此时好感度=50，主角拥有道具 B，背景是黄昏”。
   **快进模拟执行 (Context Simulation)：** 系统后台快速静默运行一遍所有前置逻辑变量，确保跳转后的数值逻辑是正确的。
4. 编辑器中的辅助功能
   为了让策划用得爽，这个功能还需要：
   **引用检查 (Reference Finder)：** 右键点击一个锚点，能看到“全剧本有哪些地方跳向了这里”。
   **跳转回溯：** 跳转过去后，点击一个按钮能“跳回”上一个编辑的位置（类似浏览器的后退键）。
5. 实际应用场景示例
   **养成系统：** 每天“行程结束”后，所有分支最终都指向一个统一的“黑屏转场”节点，然后跳转回“大地图”节点。
   **坏结局 (Bad End)：** 所有的死亡选项都统一跳向一个“死亡演出”通用脚本，播放完后再跳回标题画面。
   **多线并行：** 玩家在 A 线做了某个动作，逻辑节点判断后，直接跳转到 B 线对应的反应帧。
   **总结来说：** “长距离跳转”就是剧情脚本的**非线性索引**。没有它，大型游戏的剧情编辑会变成一场灾难。
   **如果你要实现这个，你是打算做成“同一个文件内的远距离跳转”，还是“跨文件（如从 Chapter1.json 跳到 Chapter5.json）”的跳转？** 这涉及到底层资源加载策略的不同。

```

- **帧添加与删除**：加入或插入新换帧。

- **指令查找与添加与删除**：通过字符相关搜索查找对应指令，允许在一个帧内添加多个并发指令。

- **即时参数调节**：对指令参数进行直接调整。立即对执行器应用修改的参数，将参数映射到Inspector和Executor上，使场景应用当前帧的修改结果。在场景或其它地方对Executor的修改也要即时反映到编辑器的参数上。

- **指令队列管理**：在一个帧内添加多个指令，调整指令的并发顺序，串联事件关联。

- **分组注释**：允许对列表中的行进行选中着色与编写注释。

- **保存结果**：对已编辑的部分进行保存，如果可能，保存时对比 difference。

- **全局变量引用**：添加并引用全局变量数据，演出时进行同步更改。

### 实时预览与调试工具(Debugger & Preview)

- **“所见即所得”预览**： 在不运行游戏（Non-Play Mode）的情况下，点击节点能直接在 Game 窗口更新当前的背景和立绘布局。

- **跳转执行 (Fast-Forward to Node)：** 右键点击任意节点“从此处运行”，编辑器自动回溯前置状态并渲染。

- **变量监视器：** 实时显示当前的全局变量（Flag）和好感度数值，支持手动修改以测试不同分支。

- **错误检查：** 自动检测非法跳转、资源路径丢失或非法参数，并在节点上打红叉提示。

### 资源与词条管理 (Asset Manager)

- **立绘/表情选择器：** 点击选择框弹出缩略图预览，而不是让策划手打文件名。

- **语音自动匹配：** 如果命名规范统一，支持一键根据文本 ID 匹配对应的 .wav 或 .mp3。

- **百科词条索引：** 在编辑文本时，选中文段即可快捷关联到你提到的“人物/词条百科”。

## 程序开发端（C#开发情境）

程序可以在预设的指令不能满足演出需求时，按照结绳编辑器的内部协议要求，重头开发新的指令。新开发的指令可以直接应用，无需另外接入系统。

程序按照此模板开发新的指令：

```csharp
#region Param
[Serializable]
public class _InstrParamTemplate_: InstrParam
{
    #region "These Properties MUSTn't Be Changed!"

    [DisplayInEditor("指令名称", Order = -100)]
    public override string Name { get; protected set; } = nameof(_InstrParamTemplate_);

    protected override string _ExecutorType { get; set; } = nameof(_InstrExecuteTemplate_);

    #endregion

    #region Const Implemented Property

    [DisplayInEditor("描述", Order = -80)]
    public override string Description { get; protected set; } = "_InstrParamTemplate_ Description";

    [DisplayInEditor("允许共存", Order = 10, Tooltip = "同一帧中是否允许存在多个相同类型的指令")]
    public override bool IsCanCoexist { get;  set; } = false;

    #endregion

    #region Basic Property

    [DisplayInEditor("是否在此处执行后释放", Order = 20)]
    public override bool IsRelease { get; set; } = false;

    [DisplayInEditor("可跳过", Order = 30, Tooltip = "该指令是否可以被跳过")]
    public override bool IsCanBeSkipped { get; set; } = true;

    [DisplayInEditor("需要等待", Order = 40, Tooltip = "该指令是否需要等待执行完成")]
    public override bool IsBeWaited { get; set; } = false;

    #endregion
}
#endregion

#region Executor
public class **_InstrExecuteTemplate_**: InstrExecute
{
    #region Execute Param


    #endregion

    #region Executor

    void Start()
    {

    }

    void Update()
    {

    }
    _// Init will be auto called,so it's not necessary to call Init in this part._
_    _protected override void Init()
    {

    }

    _// Execute will be called before CoExecute if there exists contents here._
_    _protected override void Execute()
    {

    }

    _// CoExecute can be ignored if not needed, or auto called._
_    _protected override IEnumerator CoExecute()
    {

        yield return null;
    }

    _// When executing this Instr, Interrupt it._
_    _protected override void Interrupt()
    {

    }

    _// When this Instr is completed, End this._
_    _protected override void End()
    {

    }
    #endregion
}
#endregion
```

在结绳编辑器中，指令资源分为**指令参数**和**指令执行**。

**指令参数**是负责具体行为序列化的部分，在演出中，指令具体该怎样做，这些参数都会被保存在指令参数中。

**指令执行**是具体的执行资源和执行基础，具体表现为 Unity 中的 MonoBehaviour，会根据需求在代码中编写具体行为；一个指令执行可以反复引用不同的指令参数，以达到资源复用。

---

# 功能扩展

## 文本表现指令 (Text & Typography)

这不仅仅是显示文字，而是控制“阅读体验”的动态过程。

<table>
<tr>
<td>**功能细分**<br/></td><td>**指令详述 (以 Naninovel/Ren'Py 为例)**<br/></td><td>**参考功能**<br/></td></tr>
<tr>
<td>**打字机控制**<br/></td><td>控制文字显示速度、中途停顿（Wait for input）、即时显示。<br/></td><td>speed, wait, nowait<br/></td></tr>
<tr>
<td>**内联指令**<br/></td><td>在一句话中间触发事件（如：说到某词时角色变脸、震动）。<br/></td><td>[vshake], [char smile]<br/></td></tr>
<tr>
<td>**注音/旁注 (Ruby)**<br/></td><td>在文字上方显示小字（常用于日语假名或特殊术语解释）。<br/></td><td><ruby="text"><br/></td></tr>
<tr>
<td>**富文本标记**<br/></td><td>改变局部文字颜色、大小、间距、轮廓线、甚至动态抖动。<br/></td><td><b>, <i>, <color>, <size><br/></td></tr>
<tr>
<td>**文本外观切换**<br/></td><td>切换对话框模板（普通、咆哮、内心独白、气泡）。<br/></td><td>Appearance, Printer<br/></td></tr>
<tr>
<td>**变量嵌入**<br/></td><td>在文本中实时渲染玩家名字或数值（如：“你好，[PlayerName]”）。<br/></td><td>{var_name}, {score}<br/></td></tr>
</table>

- **参考：** [Naninovel Text Printers Guide](https://naninovel.com/guide/text-printers.html) | [Ren'Py Text Tags](https://www.renpy.org/doc/html/text.html)

---

## 角色与演员控制 (Actors & Stage)

3D 时代的角色控制是“位置 + 状态 + 物理”的集合。

### 2D 部分 (Sprite/Live2D/Spine)

- **表情差分 (Appearance):** 瞬间切换或平滑过渡到另一个表情 ID。
- **转场效果 (Transitions):** 入场时的溶解 (Dissolve)、平移 (Slide)、卷帘 (Wipe)。
- **动画叠加:** 循环动画（呼吸、眨眼）与单次动画（挥手）的叠加层级。
- **唇音同步 (Lip-sync):** 根据语音强度动态驱动口型混合形状。

### 3D 部分 (3D Model/Humanoid)

- **骨骼动画 (Play Animation):** 指定 Clip 播放，支持 **Crossfade (淡入淡出)** 融合两个动作。
- **LookAt (看向):** 强制头部骨骼看向摄像机或另一个角色，支持权重（Weight）调节（只动眼球、动头或转动上半身）。
- **NavMesh 移动:** 调用 `agent.SetDestination`，让角色沿路网移动，自动触发 Walk/Run 动画切换。
- **附件挂载 (Attach/Parent):** 将武器、道具瞬间挂载到角色的特定骨骼节点（如 Hand_R）。
- **参考：** [Naninovel Characters Guide](https://naninovel.com/guide/characters.html) | [Fungus Character Command](https://www.google.com/search?q=https://fungusgames.com/documentation/command_reference%23character)

---

## 镜头语言与摄影机 (Cinematography)

这是 3D 演出的灵魂，将 AVG 提升为电影的关键。

- **切换 (Cut):** 瞬间改变当前活跃的摄像机。
- **平滑位移 (Pan/Dolly):** 在指定时间内从 A 点移动到 B 点，支持 **Easing (缓动曲线)**。
- **焦距控制 (Zoom/FOV):** 模拟长焦或广角效果，增强情绪张力。
- **景深 (Depth of Field):** 实时调整聚焦距离和光圈大小，虚化背景以突出角色。
- **屏幕震动 (Shake):** 区分“受击震动”（短促）和“地震震动”（持续、有频率）。
- **正交/透视切换:** 用于 2D/3D 混合演出的特殊镜头转换。
- **参考：** [Unity Cinemachine Documentation](https://docs.unity3d.com/Packages/com.unity.cinemachine@2.9/manual/index.html) (Naninovel 等常配合此系统)

---

## 逻辑分支与数据状态 (Logic & State)

决定了剧情的非线性复杂度和回溯能力。

- **变量操作:** `Set`, `Add`, `Subtract`, `Toggle` (布尔值切换)。
- **条件判定 (If/Else):** 支持复杂逻辑表达式 `(A > 10 && B == true)`。
- **选项管理 (Choice/Menu):**

  - **超时选项:** 倒计时结束自动选择默认分支。
  - **锁定选项:** 满足特定 Flag 才可见或可点击。
- **脚本跳转 (Jump/Call):** * `Jump`: 永久跳往新章节。

  - `Call`: 跳往子程序，运行完 `Return` 回原地。
- **存档标记 (Checkpoint):** 手动或指令触发的隐性存档点，用于“选项回溯”。
- **参考：** [Ren'Py Conditional Statements](https://www.renpy.org/doc/html/conditional.html) | [Yarn Spinner Logic Guide](https://www.google.com/search?q=https://docs.yarnspinner.dev/getting-started/syntax-guide/logic-and-variables)

---

## 资源与环境控制 (Media & Environment)

- **音频分轨 (Audio Mixer):** 分别控制 BGM、SE、Ambience (环境音)、Voice。支持 **Audio Ducking**（有语音时自动降低音乐音量）。
- **全局视觉特效 (Spawn/VFX):** * **粒子系统:** 下雨、花瓣、落叶、爆炸。

  - **后期处理:** 颜色分级 (Color Grading)、噪点 (Grain)、旧电影滤镜。
- **背景管理 (Scene):** 3D 场景切换时的异步加载指令，支持进度条回调。
- **遮罩 (Backdrop/Mask):** 局部遮挡画面，或创建特定的剪影效果。
- **参考：** [Naninovel Special Effects](https://naninovel.com/guide/special-effects.html)

---

## 元功能与系统交互 (Meta & System)

- **存档属性注入:** 存档时不只存位置，还要存当时所有的 `Flags`、`Camera Position`、`BGM Pitch`。
- **解锁器 (Unlockables):** 解锁画廊 (Gallery)、场景回想 (Scenario Replay)、音乐鉴赏。
- **本地化指令:** 实时切换 `Language Package`，调整文本行高以适配不同语言。
- **输入捕获 (Input Interaction):** * **QTE:** 指定时间内按下特定键。

  - **调查模式:** 点击 3D 场景中的 Collider 触发特定对话帧。
