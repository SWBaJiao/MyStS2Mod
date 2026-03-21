# FriendlyFire-StS2 — 杀戮尖塔 2 友伤模组

> 按住 `Alt` 键，让你的攻击牌也能对队友「友好地」挥出一刀。

Slay the Spire 2
.NET 9.0
Harmony 2.4.2
License: MIT
AI Assisted

---

## 功能介绍


| 功能           | 说明                                                            |
| ------------ | ------------------------------------------------------------- |
| **单体攻击友伤**   | 按住 `Alt` 键时，`AnyEnemy` 类型的攻击牌可以选择队友作为目标                       |
| **AOE 扩展攻击** | 按住 `Alt` 键时，`AllEnemies` 类型的 AOE 牌会攻击**其他玩家的角色**（不伤自己和自己的召唤物） |
| **特殊效果生效**   | 卡牌附带的 debuff（易伤、虚弱等）对队友同样生效                                   |
| **JSON 白名单** | 通过配置文件精确控制哪些卡牌允许友伤                                            |
| **危险卡牌保护**   | 自动拦截访问 `Monster` 属性的卡牌，防止游戏崩溃                                 |
| **屏幕提示**     | 按住开关键时屏幕顶部显示"友军伤害开启"红色提示                                      |
| **多人同步安全**   | 通过 TargetId 信号机制确保所有客户端状态一致，不会断连                              |


### 工作流程

```
玩家打出攻击牌
  |
  +-- Alt 未按住 --> 正常游戏逻辑（不变）
  |
  +-- Alt 按住
       |
       +-- 单体攻击牌 --> 检查白名单 --> 允许选择队友
       |                               |
       |                          检查黑名单 --> 已修复? --> 执行安全替代逻辑
       |                                        未修复? --> 阻止友伤
       |
       +-- AOE 攻击牌 --> 检查白名单 --> 目标扩展为"敌人 + 其他玩家"
                                        （排除自己和自己的召唤物）
```

---

## 安装教程（玩家）

> **重要：安装任何 Mod 前，请先备份你的游戏存档！**
>
> 存档位置：
>
> - **Windows:** `%APPDATA%\..\Roaming\SlayTheSpire2\`
> - **macOS:** `~/Library/Application Support/SlayTheSpire2/`
>
> 将整个文件夹复制一份到安全的地方即可。如果 Mod 出现问题，可以随时恢复。

### 第一步：找到游戏根目录


| 平台          | 游戏根目录路径                                                                                                       |
| ----------- | ------------------------------------------------------------------------------------------------------------- |
| **Windows** | `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\`                                             |
| **macOS**   | `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/` |


> **提示：** 在 Steam 中右键游戏 → 管理 → 浏览本地文件，可以快速打开游戏根目录。

### 第二步：创建 mods 文件夹

在游戏根目录下创建名为 `mods` 的文件夹（如果已存在则跳过）：

```
游戏根目录/
  +-- sts2.dll
  +-- ...其他游戏文件...
  +-- mods/              <-- 手动创建这个文件夹
```

### 第三步：安装前置依赖 BaseLib

本 Mod 依赖 [Alchyr/BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2)，这是大部分 STS2 Mod 的基础库，**必须先安装**。

1. 前往 [BaseLib-StS2 Releases](https://github.com/Alchyr/BaseLib-StS2/releases) 下载最新版
2. 解压后将 `BaseLib` 文件夹放入 `mods/` 目录

```
mods/
  +-- BaseLib/
        +-- BaseLib.dll
        +-- BaseLib.pck
        +-- BaseLib.json
```

### 第四步：安装 FriendlyFire

1. 从本项目的 [Releases](../../releases) 页面下载最新版 `FriendlyFire.zip`
2. 解压后将 `FriendlyFire` 文件夹放入 `mods/` 目录

```
mods/
  +-- BaseLib/                      <-- 前置依赖（第三步安装的）
  |     +-- BaseLib.dll
  |     +-- BaseLib.pck
  |     +-- BaseLib.json
  |
  +-- FriendlyFire/                 <-- 本 Mod
        +-- FriendlyFire.dll        <-- 核心逻辑
        +-- FriendlyFire.pck        <-- Godot 资源包
        +-- mod_manifest.json       <-- Mod 描述文件
        +-- friendly_fire_config.cfg <-- 配置文件（可自定义）
```

### 第五步：启动游戏

1. 启动杀戮尖塔 2
2. 进入主菜单 → **Mod 管理器**
3. 确认 **BaseLib** 和 **Friendly Fire** 都已启用
4. 开始多人合作战斗

### 使用方式


| 操作                    | 效果                                |
| --------------------- | --------------------------------- |
| **不按 Alt** 打出攻击牌      | 正常行为，和原版完全一样                      |
| **按住 Alt** 打出单体攻击牌    | 可以选择队友作为目标，屏幕顶部出现红色提示             |
| **按住 Alt** 打出 AOE 攻击牌 | AOE 命中所有敌人 + 其他玩家的角色（不伤自己和自己的召唤物） |


> **多人游戏注意：** 所有玩家都需要安装**相同版本**的 Mod，且 `friendly_fire_config.cfg` 中的白名单配置**必须一致**，否则可能导致状态不同步断连。建议由房主统一分发配置文件。

### 卸载方式

1. 删除 `mods/FriendlyFire/` 文件夹
2. 重启游戏即可恢复原版，不影响存档

### 从源码编译（开发者）

**前置要求：**

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)（macOS: `brew install dotnet`）
- [Godot 4.5.1 Mono](https://godotengine.org/download)（可选，导出 .pck 需要）
- 杀戮尖塔 2 已安装（需要引用游戏 DLL）

```bash
# 克隆项目
git clone https://github.com/SWBaJiao/FriendlyFire-StS2.git
cd FriendlyFire-StS2

# 一键编译
chmod +x build.sh
./build.sh

# 编译产物在 output/FriendlyFire/ 目录下
```

**编译脚本命令：**


| 命令                   | 说明                                       |
| -------------------- | ---------------------------------------- |
| `./build.sh`         | 编译 Release 版本，输出到 `output/FriendlyFire/` |
| `./build.sh debug`   | 编译 Debug 版本                              |
| `./build.sh publish` | 编译 + 导出 .pck（需要 Godot 4.5.1 Mono）        |
| `./build.sh clean`   | 清理所有编译产物                                 |


---

## 配置说明

编辑 `friendly_fire_config.cfg` 来自定义 Mod 行为。修改后**重启游戏**生效。

> 配置文件使用 `.cfg` 扩展名（内容为 JSON 格式），以避免被游戏的 Mod 加载器误识别为 mod_manifest。

### 配置项一览

```jsonc
{
  // 按住此键启用友伤。可选: Alt, Shift, Ctrl, Tab, Space, F1~F4
  "toggle_key": "Alt",

  // 单体攻击牌白名单（卡牌类名）
  // 空数组 [] = 所有单体攻击牌都允许友伤
  // 填入类名 = 只有指定的卡牌允许友伤
  "single_target_whitelist": [],

  // 是否启用 AOE 友伤扩展
  "aoe_enabled": true,

  // AOE 攻击牌白名单，规则同上
  "aoe_whitelist": [],

  // 危险卡牌黑名单（访问 Target.Monster 会崩溃的卡牌）
  // 已有专属 Patch 修复的卡牌无需添加
  "dangerous_cards_blacklist": []
}
```

### 白名单配置示例

**示例 1** — 只允许 `猛击` 和 `上勾拳` 友伤：

```json
"single_target_whitelist": ["Bash", "Uppercut"]
```

**示例 2** — 所有单体攻击牌都允许（默认）：

```json
"single_target_whitelist": []
```

**示例 3** — 只允许 `雷鸣` 和 `旋风` AOE 友伤：

```json
"aoe_whitelist": ["Thunderclap", "Whirlwind"]
```

**示例 4** — 完全关闭 AOE 友伤：

```json
"aoe_enabled": false
```

### 判定优先级

```
黑名单（dangerous_cards_blacklist）> 已修复卡牌（FixedDangerousCards）> 白名单
```

- 黑名单中的卡牌**始终被阻止**（除非有专属 Patch 修复）
- 白名单为空 = 全部允许；非空 = 仅允许列表中的卡牌

---

## 卡牌名单速查

### 单体攻击牌（164 张）

点击展开完整名单


| 类名              | 中文名        | 类名                | 中文名       |
| --------------- | ---------- | ----------------- | --------- |
| AdaptiveStrike  | 自适应打击      | AllForOne         | 万箭齐发      |
| Anger           | 怒火         | AshenStrike       | 灰烬打击      |
| Assassinate     | 暗杀         | Backstab          | 背刺        |
| BallLightning   | 球状闪电       | Barrage           | 弹幕        |
| Bash            | 猛击         | BeamCell          | 光束单元      |
| BeatDown        | 痛殴         | BeatIntoShape     | 锤炼成型      |
| Begone          | 退散         | BlightStrike      | 枯萎打击      |
| Bludgeon        | 重击         | BodySlam          | 碾压        |
| Bolas           | 流星锤        | Bombardment       | 轰炸        |
| Break           | 破碎         | Bully             | 欺凌        |
| Bury            | 埋葬         | ByrdSwoop         | 鸟类俯冲      |
| CelestialMight  | 天界之力       | Cinder            | 余烬        |
| Clash           | 冲突         | Claw              | 利爪        |
| ColdSnap        | 寒流         | CollisionCourse   | 碰撞航线      |
| Comet           | 彗星         | CompileDriver     | 编译驱动      |
| CrescentSpear   | 新月之矛       | DaggerThrow       | 飞刀投掷      |
| Dash            | 冲刺         | DeathMarch        | 死亡进军      |
| Debilitate      | 衰弱         | Defile            | 亵渎        |
| Devastate       | 毁灭         | Dismantle         | 拆解        |
| DrainPower      | 汲取能量       | Eradicate         | 根除        |
| FallingStar     | 坠星         | Fear              | 恐惧        |
| Feed            | 吞噬         | Fetch             | 取回        |
| FiendFire       | 魔焰         | FightMe           | 来战        |
| Finisher        | 终结         | Fisticuffs        | 拳击        |
| FlashOfSteel    | 钢铁闪光       | Flatten           | 压扁        |
| Flechettes      | 飞镖         | FocusedStrike     | 专注打击      |
| Ftl             | 超光速        | GammaBlast        | 伽马射线      |
| GangUp          | 围攻         | GiantRock         | 巨岩        |
| GoForTheEyes    | 直捣黄龙 [已修复] | GoldAxe           | 黄金斧       |
| Grapple         | 擒拿         | Graveblast        | 墓穴爆破      |
| GuidingStar     | 引导之星       | GunkUp            | 粘液        |
| HandOfGreed     | 贪婪之手       | Hang              | 绞刑        |
| Headbutt        | 头槌         | HeavenlyDrill     | 天钻        |
| Hegemony        | 霸权         | HeirloomHammer    | 传家之锤      |
| HelixDrill      | 螺旋钻        | Hemokinesis       | 血液操控      |
| IceLance        | 冰矛         | IronWave          | 铁浪        |
| Jackpot         | 头奖         | KinglyKick        | 帝王踢       |
| KinglyPunch     | 帝王拳        | Knockdown         | 击倒        |
| KnockoutBlow    | 致命一击       | LeadingStrike     | 引领打击      |
| LunarBlast      | 月光冲击       | MadScience        | 疯狂科学      |
| MakeItSo        | 就这么办       | Mangle            | 撕裂        |
| Maul            | 重锤         | MementoMori       | 死亡勿忘      |
| MeteorStrike    | 流星打击       | MindBlast         | 精神冲击      |
| MinionDiveBomb  | 仆从俯冲       | MinionStrike      | 仆从打击      |
| Misery          | 苦难         | MoltenFist        | 熔岩之拳      |
| MomentumStrike  | 动量打击       | Murder            | 谋杀        |
| NeowsFury       | 尼奥之怒       | Neutralize        | 中和        |
| Null            | 归零         | Omnislice         | 全方位斩      |
| Peck            | 啄击         | PerfectedStrike   | 完美打击      |
| PhotonCut       | 光子斩        | Pillage           | 掠夺        |
| Pinpoint        | 精准         | PoisonedStab      | 毒刺        |
| Poke            | 戳击         | PommelStrike      | 刀柄打击      |
| Pounce          | 猛扑         | PreciseCut        | 精确切割      |
| Predator        | 捕食者        | Protector         | 保护者       |
| PullFromBelow   | 冥界之握       | Rampage           | 暴走        |
| Rattle          | 嘎嘎作响       | Reap              | 收割        |
| Reave           | 劫掠         | Rebound           | 反弹        |
| Refract         | 折射         | Rend              | 撕扯        |
| RightHandHand   | 右手之手       | RocketPunch       | 火箭拳       |
| Salvo           | 齐射         | Scrape            | 刮削        |
| SculptingStrike | 雕塑打击       | SeekerStrike      | 追踪打击      |
| SetupStrike     | 准备打击       | Severance         | 断裂        |
| ShiningStrike   | 闪耀打击       | Shiv              | 小刀        |
| SicEm           | 放狗咬        | Skewer            | 串刺        |
| Slice           | 切割         | Snap              | 折断        |
| SolarStrike     | 太阳打击       | SoulStorm         | 灵魂风暴      |
| SovereignBlade  | 至高之刃       | Spite             | 怨恨        |
| Squash          | 压碎         | Squeeze           | 挤压        |
| Strangle        | 扼杀         | StrikeDefect      | 打击(故障机器人) |
| StrikeIronclad  | 打击(铁甲战士)   | StrikeNecrobinder | 打击(亡灵师)   |
| StrikeRegent    | 打击(摄政)     | StrikeSilent      | 打击(沉默猎手)  |
| SuckerPunch     | 偷袭         | Sunder            | 粉碎        |
| Supermassive    | 超大质量       | Suppress          | 压制        |
| Synthesis       | 合成         | TagTeam           | 车轮战       |
| TearAsunder     | 撕裂虚空       | TeslaCoil         | 特斯拉线圈     |
| TheHunt         | 狩猎         | TheScythe         | 死神镰刀      |
| Thrash          | 猛打         | ThrummingHatchet  | 嗡鸣手斧      |
| TimesUp         | 时间到        | TwinStrike        | 双重打击      |
| UltimateStrike  | 终极打击       | Unleash           | 释放        |
| Unrelenting     | 不屈         | Uppercut          | 上勾拳       |
| Uproar          | 骚动         | Veilpiercer       | 破幕者       |
| Whistle         | 口哨         | WroughtInWar      | 战火锻造      |




### AOE 攻击牌（36 张）

点击展开完整名单


| 类名               | 中文名  | 类名             | 中文名  |
| ---------------- | ---- | -------------- | ---- |
| AstralPulse      | 星界脉冲 | BansheesCry    | 女妖之嚎 |
| BoneShards       | 骨骸碎片 | Breakthrough   | 突破   |
| Conflagration    | 大火   | CrashLanding   | 坠落着陆 |
| CrushUnder       | 碾碎   | DaggerSpray    | 飞刀雨  |
| DramaticEntrance | 华丽登场 | DyingStar      | 垂死之星 |
| EchoingSlash     | 回响斩  | Exterminate    | 灭绝   |
| FlakCannon       | 高射炮  | FlickFlack     | 后空翻  |
| FollowThrough    | 贯穿   | GrandFinale    | 大终章  |
| HighFive         | 击掌   | HowlFromBeyond | 彼岸嚎叫 |
| Hyperbeam        | 超级光束 | MeteorShower   | 流星雨  |
| PactsEnd         | 契约终结 | Radiate        | 辐射   |
| Ricochet         | 跳弹   | RipAndTear     | 撕扯   |
| SevenStars       | 七星   | Shatter        | 碎裂   |
| Sow              | 播种   | Stardust       | 星尘   |
| Stomp            | 践踏   | SweepingBeam   | 扫射光束 |
| SweepingGaze     | 扫视   | SwordBoomerang | 剑刃回旋 |
| Thunderclap      | 雷鸣   | Volley         | 齐射   |
| Whirlwind        | 旋风   |                |      |




---

## 项目结构

```
FriendlyFire-StS2/
+-- Plugin.cs                     # Mod 入口，注册 Harmony Patch + UI 指示器
+-- ModInfo.cs                    # Mod 元信息（GUID、版本号）
+-- mod_manifest.json             # 游戏 Mod 加载器识别文件
+-- friendly_fire_config.cfg      # 配置文件（白名单、开关键、黑名单）
+-- build.sh                      # 一键编译脚本
+-- MyStS2Mod.csproj              # Godot.NET.Sdk 项目文件
|
+-- Patches/
|   +-- TargetingPatches.cs       # UI 层: 目标选择 + 执行层: 目标验证 (5 个 Patch)
|   +-- AoePatches.cs             # 执行层: AOE 目标扩展（排除自己和召唤物）
|   +-- MultiplayerSyncPatches.cs # 网络层: AOE 友伤信号编码/解码 (2 个 Patch)
|   +-- CardSpecificPatches.cs    # 危险卡牌专属 Patch (GoForTheEyes 等)
|
+-- UI/
|   +-- FriendlyFireIndicator.cs  # 屏幕提示: 按住开关键时显示红色提示条
|
+-- Utils/
|   +-- FriendlyFireConfig.cs     # JSON 配置加载 & 白名单/黑名单判定
|   +-- FriendlyFireState.cs      # 运行时状态（按键检测、AOE 信号标志）
|   +-- ModHelper.cs              # 通用工具（资源加载、本地化）
```

---

## 技术实现

本 Mod 通过 [Harmony](https://github.com/pardeike/Harmony) 运行时 Patch 了游戏的 9 个方法，分为三层架构：

### 架构总览

```
+-----------------------------------------------------------+
|                    UI 层（仅本地）                           |
|  检查 Alt 键 -- 控制玩家能选中谁                             |
|                                                           |
|  [1] AllowedToTargetNode    -- 鼠标悬停: 队友可高亮          |
|  [2] AllowedToTargetCreature -- 备用（防 JIT 内联）         |
|  [3] TrackTargetingCard      -- 追踪当前选目标的卡牌         |
|  [4] TrackTargetSelection    -- 追踪备用                    |
+---------------------------+-------------------------------+
                            | 玩家确认出牌
+---------------------------v-------------------------------+
|               信号编码层（本地 --> 网络）                     |
|  将 Alt 状态编码到 PlayCardAction.TargetId                  |
|                                                           |
|  [5] PlayCardAction 构造函数                                |
|      Alt + AOE 卡 --> TargetId = 自身CombatId（信号）       |
|      --> 通过 NetPlayCardAction 自动同步到所有客户端          |
+---------------------------+-------------------------------+
                            | Action 网络同步
+---------------------------v-------------------------------+
|              执行层（所有客户端一致）                         |
|  不检查 Alt 键 -- 只用白名单 + 信号标志                      |
|                                                           |
|  [6] ExecuteAction Prefix  -- 检测信号, 设置 AOE 友伤标志   |
|  [7] IsValidTarget         -- 允许白名单卡牌攻击队友         |
|  [8] GetPossibleTargets    -- 读取标志, 扩展 AOE 目标       |
|  [9] GoForTheEyes OnPlay   -- 特殊卡牌安全处理              |
|                                                           |
|  所有端输入相同 --> 输出相同 --> 不 desync                   |
+-----------------------------------------------------------+
```

### 多人同步机制详解

STS2 多人模式使用**确定性锁步**：所有客户端独立执行相同的 Action，执行后比较 checksum。

**问题：** AOE 卡没有 targetId，`GetPossibleTargets` 在各端独立计算。如果只在按 Alt 的端扩展目标，其他端不扩展 → 状态不一致 → 断连。

**解决：** 利用 `PlayCardAction.TargetId` 作为信号载体：


| 场景     | TargetId      | 含义      |
| ------ | ------------- | ------- |
| 正常 AOE | `null`        | 只打敌人    |
| 友伤 AOE | `自身 CombatId` | 信号：扩展目标 |


这个 TargetId 通过 `NetPlayCardAction` 自动序列化/反序列化并同步到所有客户端。

### 召唤物排除机制

AOE 友伤排除自己**和自己的召唤物**（Pet），通过 `Creature.PetOwner` 属性判断所属：

```
亡灵契约师(Player A) 打出 AOE:
  亡灵契约师本体: Player=A        --> 属于A --> 排除
  骷髅兵召唤物:   PetOwner=A      --> 属于A --> 排除
  铁甲战士(Player B): Player=B    --> 不属于A --> 命中
  铁甲战士的宠物:   PetOwner=B    --> 不属于A --> 命中
```

---

## 危险卡牌系统

部分卡牌在 `OnPlay` 中访问了 `cardPlay.Target.Monster` 属性。玩家的 `Creature` 对象没有 `Monster`（为 `null`），对队友使用这些卡牌会导致 **NullReferenceException** 崩溃。

### 保护机制

```
三道防线:

1. 黑名单  --> 配置 dangerous_cards_blacklist，直接阻止友伤
2. 专属 Patch --> 编写 Harmony Prefix 替换为安全逻辑
3. 标记已修复 --> 添加到 FixedDangerousCards，绕过黑名单
```

### 当前已修复


| 卡牌           | 崩溃点                              | 修复方式             |
| ------------ | -------------------------------- | ---------------- |
| GoForTheEyes | `Target.Monster.IntendsToAttack` | 对队友跳过意图判断，直接施加虚弱 |


### 如何添加新的危险卡牌修复

如果你发现某张卡牌导致崩溃：

**临时方案** — 加入黑名单（不需要写代码）：

```json
"dangerous_cards_blacklist": ["CrashCard"]
```

**永久方案** — 编写专属 Patch：

1. 在 `Patches/CardSpecificPatches.cs` 添加新的 Patch 类：

```csharp
[HarmonyPatch(typeof(YourCard), "OnPlay")]
public static class YourCardPatch
{
    static bool Prefix(YourCard __instance, PlayerChoiceContext choiceContext,
                       CardPlay cardPlay, ref Task __result)
    {
        if (cardPlay.Target == null || !cardPlay.Target.IsPlayer)
            return true;  // 非队友目标 -> 走原逻辑

        // 替代逻辑：造成伤害 + 安全的特殊效果
        __result = SafeOnPlay(__instance, choiceContext, cardPlay);
        return false;  // 跳过原方法
    }

    private static async Task SafeOnPlay(...)
    {
        // 伤害正常执行
        await DamageCmd.Attack(damage).FromCard(instance)
            .Targeting(cardPlay.Target).Execute(choiceContext);

        // 跳过 Target.Monster 相关的逻辑，
        // 或替换为对玩家安全的等价效果
    }
}
```

1. 在 `FriendlyFireConfig.cs` 的 `FixedDangerousCards` 中注册：

```csharp
public static readonly HashSet<string> FixedDangerousCards = new()
{
    "GoForTheEyes",
    "YourCard"       // <-- 添加这一行
};
```

1. 重新编译即可

---

## FAQ

**Q: 友伤会对自己生效吗？**

> 不会。AOE 排除了攻击者本人和攻击者的所有召唤物/宠物。

**Q: 单人模式有用吗？**

> 单体友伤在单人模式下没有可选的队友目标。AOE 友伤同理（只有自己和敌人，自己被排除，效果等于原版）。该 Mod 主要为**多人合作模式**设计。

**Q: 友伤会触发卡牌的所有效果吗？**

> 是的。伤害、debuff（易伤、虚弱、中毒等）、特殊效果都会正常生效。唯一的例外是访问 `Monster` 属性的卡牌，这些会使用安全的替代逻辑。

**Q: 多人游戏会断连吗？**

> 不会。Mod 使用 TargetId 信号机制确保所有客户端执行相同的目标计算逻辑。前提是所有玩家安装相同版本且白名单配置一致。

**Q: 配置文件写错了会怎样？**

> Mod 会在控制台输出错误日志，并使用默认配置（全部允许 + Alt 键）继续运行，不会崩溃。

**Q: 怎么查看 Mod 日志？**

> 游戏控制台中搜索 `[Friendly Fire]` 前缀的日志。

---

## AI 编程说明

本项目在开发过程中大量使用了 AI 辅助编程（Claude），以下是实践中的一些提示：

### 适合 AI 辅助的部分

- **反编译分析** — 让 AI 编写 ICSharpCode.Decompiler 脚本，批量反编译游戏 DLL 中的目标类，快速理解未文档化的内部 API
- **Harmony Patch 编写** — 描述"我想让 X 方法在 Y 条件下返回 Z"，AI 能直接生成 Prefix/Postfix 代码和正确的 `[HarmonyPatch]` 属性
- **多人同步设计** — 向 AI 描述"确定性锁步"的约束条件，它能帮助设计网络安全的 Patch 架构（UI 层 vs 执行层分离）
- **边界情况发现** — AI 能通过反编译结果发现 async 方法的 Harmony Postfix 陷阱、JIT 内联导致 private 方法 Patch 失效等问题
- **跨平台 csproj** — 自动生成 Windows/macOS/Linux 的条件路径检测

### 需要人工把关的部分

- **游戏逻辑理解** — AI 无法启动游戏，像"亡灵契约师的召唤物会被 AOE 打到"这类 bug 只有实际测试才能发现
- **白名单校验** — 580 张卡牌的分类（单体/AOE/安全/危险）需要人工抽查确认
- **网络同步测试** — desync 问题只能在真实多人环境中复现和验证
- **Mod 兼容性** — 与其他 Mod 的冲突需要实际加载测试

### 使用的工具链


| 工具                                                                                  | 用途                          |
| ----------------------------------------------------------------------------------- | --------------------------- |
| [Claude Code](https://claude.com/claude-code)                                       | AI 编程助手，代码生成 / 反编译脚本 / 架构设计 |
| [ICSharpCode.Decompiler](https://github.com/icsharpcode/ILSpy)                      | 游戏 DLL 反编译（程序化调用）           |
| [Harmony 2.4.2](https://github.com/pardeike/Harmony)                                | 运行时方法 Patch                 |
| [BepInEx.AssemblyPublicizer](https://github.com/BepInEx/BepInEx.AssemblyPublicizer) | 访问游戏 private/internal 成员    |
| [Godot.NET.Sdk 4.5.1](https://godotengine.org/)                                     | 编译 + .pck 导出                |


---

## 贡献

欢迎提交 Issue 和 PR！如果你发现新的危险卡牌（友伤时崩溃），请提交 Issue 并附上崩溃日志。

---

## 开源协议

[MIT License](LICENSE) — 随意使用、修改、分发。