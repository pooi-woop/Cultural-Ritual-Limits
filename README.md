# Cultural Ritual Limits（文化仪式招募限制）

一个 RimWorld 1.6 的小型功能性 Mod：**限制原版 Ideology DLC「文化仪式招募」招来的小人的强度**。

原版的招募仪式经常给你送来技能两位数、还带燃烧兴趣的猛人，轻松打破难度平衡。
装上本 Mod 后，仪式招募来的小人**所有技能等级最高只有 3**——名副其实的臭鱼烂虾。
上限可以在 Mod 设置里自由调整（0~20），随时反悔。

---

## 功能特性

- ✅ 仪式招募的小人，每项技能等级都会被压到上限以下（默认上限 = 3）
- ✅ 被压低的技能会**清空已积攒的经验**，防止随便干活就连升几级
- ✅ 可选：勾选后**移除招募小人的所有技能兴趣**（Passion），成长潜力归零
- ✅ **只影响文化仪式招募**，不影响其他任何来源的小人（见下文"不会影响谁"）
- ✅ 设置即时生效、全局通用（与存档无关）；把上限调回 20 就等于关闭效果

## 不会影响谁？

本 Mod 的判定非常精确（原理见下文），以下小人**完全不受影响**：

- 随机事件"流浪者加入"（Wanderer joins）
- 逃生舱坠毁的幸存者（Refugee pod crash）
- 开局选的小人、任务奖励小人、被俘虏后招募的囚犯
- 其他 Mod 生成的小人（除非它特意模拟了原版招募仪式的触发方式，极为罕见）

## 安装方法

把整个 `Cultural Ritual Limits` 文件夹复制到 RimWorld 的 `Mods` 目录：

```
<你的 RimWorld 安装目录>\Mods\Cultural Ritual Limits\
```

然后启动游戏 → Mod 列表 → 勾选 **Cultural Ritual Limits**（确保排在 **Harmony** 之后）。

> 需要 DLC：**Ideology**（招募仪式是该 DLC 的内容）。
> 需要前置 Mod：**Harmony**（几乎人人都有，Steam 创意工坊搜 Harmony 即可）。

## 设置说明

游戏主菜单 → 选项 → Mod 设置 → **Cultural Ritual Limits**：

| 设置项 | 说明 | 默认值 |
|--------|------|--------|
| 技能等级上限 | 仪式招募的小人所有技能都不会超过此等级（0~20） | **3** |
| 移除所有技能兴趣 | 勾选后招募的小人没有任何技能兴趣 | 关闭 |

> 注意：限制的是技能的**基础等级**。极少数情况下，基因（如 Biotech 的"擅长射击"）
> 或特性带来的额外等级加成（Aptitude）不受影响——那属于"天赋"，不是"底子"。

---

## 工作原理（给想了解细节的人）

### 原版招募仪式的流程

```
招募仪式判定成功
  └─ RitualAttachableOutcomeEffectWorker_RandomRecruit.Apply()
       │  50% 概率成功，然后做两件事：
       │  ① 在任务石板(Slate)里写入名为 "overridePawnGenParams" 的小人生成参数
       │  ② 触发 WandererJoins（流浪者加入）任务
       ▼
QuestNode_Root_WandererJoin_WalkIn.GeneratePawn()
       │  检查 Slate：有 overridePawnGenParams 就用它生成小人
       ▼
生成的小人 → 发"流浪者加入"信件 → 玩家接受后入队
```

关键点：**原版代码里只有招募仪式会写 `overridePawnGenParams` 这个键**
（已对 1.6 反编译源码逐一核实）。其他来源的小人要么不走这个任务节点，
要么走了但石板里没有这个键。

### 本 Mod 的补丁

用 Harmony 给 `GeneratePawn()` 加了一个**后置补丁（Postfix）**：

```csharp
// 伪代码，完整实现见 Source/ 目录
Postfix(刚生成的小人 pawn)
{
    if (当前任务石板里存在 "overridePawnGenParams")   // ⇔ 是仪式招募来的
    {
        foreach (pawn 的每项技能)
        {
            if (技能等级 > 设置的上限) { 压低到上限; 清空经验; }
            if (设置开启了移除兴趣) { 兴趣 = 无; }
        }
    }
}
```

因为压低发生在信件发出**之前**，所以"流浪者加入"信件里显示的
"最佳技能"也是压低后的数值，所见即所得。

## 项目结构

```
Cultural Ritual Limits/
├── About/
│   └── About.xml                 # Mod 元信息（名字、作者、版本、依赖）
├── Assemblies/
│   └── CulturalRitualLimits.dll  # 编译产物（游戏加载的就是它）
├── Source/
│   └── CulturalRitualLimits/
│       ├── CulturalRitualLimits.csproj               # 项目文件（含全部依赖声明）
│       ├── HarmonyInit.cs                            # Harmony 初始化入口
│       ├── CulturalRitualLimitsMod.cs                # Mod 主类 + 设置界面
│       ├── CulturalRitualLimitsSettings.cs           # 设置数据（保存/读取）
│       └── Patch_WandererJoinWalkIn_GeneratePawn.cs  # ★ 核心补丁
└── README.md
```

所有源码都带详细中文注释，新人可以从 `Patch_WandererJoinWalkIn_GeneratePawn.cs`
开始读，它注释里完整解释了原版机制。

## 从源码编译

需要：任意能跑 **.NET 8 SDK**（或更新版本）的电脑，**不需要安装 RimWorld**。

```bash
cd Source/CulturalRitualLimits
dotnet build -c Release
```

编译产物会自动输出到 `Assemblies\CulturalRitualLimits.dll`。

原理说明：

- 目标框架是 **.NET Framework 4.7.2**（RimWorld 1.6 的运行时），不是 net8.0；
- 游戏程序集引用来自 NuGet 包 **Krafs.Rimworld.Ref**（社区从游戏文件生成的
  引用程序集，仅用于编译，不会被打包）；
- **Lib.Harmony** 仅编译时引用（`ExcludeAssets="runtime"`），运行时用的是游戏
  自带的 `0Harmony.dll`，所以 Mod 里不附带 Harmony——避免与其他 Mod 冲突。

也可以用 Visual Studio 2022 / Rider 直接打开 `.csproj` 编译（F5 不行，生成即可）。

## 游戏内测试步骤

1. 启用 Mod，开一个有 Ideology 的存档（开发者模式更快捷）；
2. 给你的文化（Ideoligion）添加"招募"仪式并举行，或用开发者模式直接触发仪式结果；
3. 仪式成功后等待"流浪者加入"信件；
4. 查看小人属性：**所有技能 ≤ 3**（或你设置的上限），无红字报错即成功；
5. 对比测试：随机事件"流浪者加入"的小人**不应**被压低。

## 兼容性

- **Harmony 前置**：必须，且本 Mod 排在其后（Mod 列表里 Harmony 会自动置顶）。
- **与其他 Mod**：补丁只挂在上述这一个方法上，且只在仪式招募场景生效，
  与绝大多数 Mod 兼容。如有其他 Mod 也修改 `GeneratePawn`，一般可共存
  （Postfix 之间不冲突，先后执行）。
- **存档**：随时加入存档；移除也安全（不往存档里写任何数据）。

## 发布到创意工坊前的待办

当前是功能完整的可玩版本，正式发布前建议：

- [ ] 游戏内实测通过（见上方测试步骤）
- [ ] 添加 `About/Preview.png`（640×360 预览图）
- [ ] 确认 `About/About.xml` 里的作者名、描述为最终版本
- [ ] 用游戏内置的"上传 Mod"功能或 Steam Workshop 工具发布

## 致谢与技术参考

- 补丁目标与方法签名核实自 RimWorld 1.6 反编译源码（仅查阅 API，代码为原创）
- 编译引用：[Krafs.Rimworld.Ref](https://github.com/krafs/RimRef)
- 补丁框架：[Harmony](https://github.com/pardeike/Harmony)
