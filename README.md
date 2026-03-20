# ⚔️ Friendly Fire Mod — 杀戮尖塔 2 友伤模组

> 按住 `Alt` 键，让你的攻击牌也能对队友「友好地」挥出一刀。

![Slay the Spire 2](https://img.shields.io/badge/Slay%20the%20Spire%202-Mod-red?style=flat-square)
![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue?style=flat-square)
![Harmony 2.4.2](https://img.shields.io/badge/Harmony-2.4.2-green?style=flat-square)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)

---

## 📖 功能介绍

| 功能 | 说明 |
|------|------|
| **单体攻击友伤** | 按住 `Alt` 键时，`AnyEnemy` 类型的攻击牌可以选择队友作为目标 |
| **AOE 扩展攻击** | 按住 `Alt` 键时，`AllEnemies` 类型的 AOE 牌会攻击**除自己以外的所有人**（敌人 + 队友） |
| **特殊效果生效** | 卡牌附带的 debuff（易伤、虚弱等）对队友同样生效 |
| **JSON 白名单** | 通过配置文件精确控制哪些卡牌允许友伤 |
| **危险卡牌保护** | 自动拦截访问 `Monster` 属性的卡牌，防止游戏崩溃 |

### 工作流程

```
玩家打出攻击牌
  │
  ├─ Alt 未按住 → 正常游戏逻辑（不变）
  │
  └─ Alt 按住
       │
       ├─ 单体攻击牌 → 检查白名单 → 允许选择队友
       │                              ↓
       │                         检查黑名单 → 已修复? → 执行安全替代逻辑
       │                                       未修复? → 阻止友伤
       │
       └─ AOE 攻击牌 → 检查白名单 → 目标扩展为"除自己外所有人"
```

---

## 🚀 快速开始

### 安装（玩家）

1. 从 [Releases](../../releases) 下载最新版 `MyStS2Mod.zip`
2. 解压得到 `MyStS2Mod/` 文件夹
3. 将整个文件夹复制到游戏的 `mods/` 目录：

```
# macOS
~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/
  └── SlayTheSpire2.app/Contents/Resources/mods/
        └── MyStS2Mod/            ← 放在这里
              ├── MyStS2Mod.dll
              ├── mod_manifest.json
              └── friendly_fire_config.json
```

4. 启动游戏，在 Mod 管理器中启用 **Friendly Fire**
5. 战斗中按住 `Alt` 键拖动攻击牌即可选择队友

### 从源码编译（开发者）

**前置要求：**
- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)（macOS: `brew install dotnet`）
- 杀戮尖塔 2 已安装（需要引用游戏 DLL）

```bash
# 克隆项目
git clone https://github.com/your-username/StS2-FriendlyFire.git
cd StS2-FriendlyFire

# 一键编译
chmod +x build.sh
./build.sh

# 编译产物在 output/MyStS2Mod/ 目录下
```

**编译脚本命令：**

| 命令 | 说明 |
|------|------|
| `./build.sh` | 编译 Release 版本，输出到 `output/MyStS2Mod/` |
| `./build.sh debug` | 编译 Debug 版本 |
| `./build.sh clean` | 清理所有编译产物 |

---

## ⚙️ 配置说明

编辑 `friendly_fire_config.json` 来自定义 Mod 行为。修改后**重启游戏**生效。

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

## 🃏 卡牌名单速查

### 单体攻击牌（164 张）

<details>
<summary>点击展开完整名单</summary>

| 类名 | 中文名 | 类名 | 中文名 |
|------|--------|------|--------|
| AdaptiveStrike | 自适应打击 | AllForOne | 万箭齐发 |
| Anger | 怒火 | AshenStrike | 灰烬打击 |
| Assassinate | 暗杀 | Backstab | 背刺 |
| BallLightning | 球状闪电 | Barrage | 弹幕 |
| Bash | 猛击 | BeamCell | 光束单元 |
| BeatDown | 痛殴 | BeatIntoShape | 锤炼成型 |
| Begone | 退散 | BlightStrike | 枯萎打击 |
| Bludgeon | 重击 | BodySlam | 碾压 |
| Bolas | 流星锤 | Bombardment | 轰炸 |
| Break | 破碎 | Bully | 欺凌 |
| Bury | 埋葬 | ByrdSwoop | 鸟类俯冲 |
| CelestialMight | 天界之力 | Cinder | 余烬 |
| Clash | 冲突 | Claw | 利爪 |
| ColdSnap | 寒流 | CollisionCourse | 碰撞航线 |
| Comet | 彗星 | CompileDriver | 编译驱动 |
| CrescentSpear | 新月之矛 | DaggerThrow | 飞刀投掷 |
| Dash | 冲刺 | DeathMarch | 死亡进军 |
| Debilitate | 衰弱 | Defile | 亵渎 |
| Devastate | 毁灭 | Dismantle | 拆解 |
| DrainPower | 汲取能量 | Eradicate | 根除 |
| FallingStar | 坠星 | Fear | 恐惧 |
| Feed | 吞噬 | Fetch | 取回 |
| FiendFire | 魔焰 | FightMe | 来战 |
| Finisher | 终结 | Fisticuffs | 拳击 |
| FlashOfSteel | 钢铁闪光 | Flatten | 压扁 |
| Flechettes | 飞镖 | FocusedStrike | 专注打击 |
| Ftl | 超光速 | GammaBlast | 伽马射线 |
| GangUp | 围攻 | GiantRock | 巨岩 |
| GoForTheEyes ⚡ | 直捣黄龙 | GoldAxe | 黄金斧 |
| Grapple | 擒拿 | Graveblast | 墓穴爆破 |
| GuidingStar | 引导之星 | GunkUp | 粘液 |
| HandOfGreed | 贪婪之手 | Hang | 绞刑 |
| Headbutt | 头槌 | HeavenlyDrill | 天钻 |
| Hegemony | 霸权 | HeirloomHammer | 传家之锤 |
| HelixDrill | 螺旋钻 | Hemokinesis | 血液操控 |
| IceLance | 冰矛 | IronWave | 铁浪 |
| Jackpot | 头奖 | KinglyKick | 帝王踢 |
| KinglyPunch | 帝王拳 | Knockdown | 击倒 |
| KnockoutBlow | 致命一击 | LeadingStrike | 引领打击 |
| LunarBlast | 月光冲击 | MadScience | 疯狂科学 |
| MakeItSo | 就这么办 | Mangle | 撕裂 |
| Maul | 重锤 | MementoMori | 死亡勿忘 |
| MeteorStrike | 流星打击 | MindBlast | 精神冲击 |
| MinionDiveBomb | 仆从俯冲 | MinionStrike | 仆从打击 |
| Misery | 苦难 | MoltenFist | 熔岩之拳 |
| MomentumStrike | 动量打击 | Murder | 谋杀 |
| NeowsFury | 尼奥之怒 | Neutralize | 中和 |
| Null | 归零 | Omnislice | 全方位斩 |
| Peck | 啄击 | PerfectedStrike | 完美打击 |
| PhotonCut | 光子斩 | Pillage | 掠夺 |
| Pinpoint | 精准 | PoisonedStab | 毒刺 |
| Poke | 戳击 | PommelStrike | 刀柄打击 |
| Pounce | 猛扑 | PreciseCut | 精确切割 |
| Predator | 捕食者 | Protector | 保护者 |
| PullFromBelow | 冥界之握 | Rampage | 暴走 |
| Rattle | 嘎嘎作响 | Reap | 收割 |
| Reave | 劫掠 | Rebound | 反弹 |
| Refract | 折射 | Rend | 撕扯 |
| RightHandHand | 右手之手 | RocketPunch | 火箭拳 |
| Salvo | 齐射 | Scrape | 刮削 |
| SculptingStrike | 雕塑打击 | SeekerStrike | 追踪打击 |
| SetupStrike | 准备打击 | Severance | 断裂 |
| ShiningStrike | 闪耀打击 | Shiv | 小刀 |
| SicEm | 放狗咬 | Skewer | 串刺 |
| Slice | 切割 | Snap | 折断 |
| SolarStrike | 太阳打击 | SoulStorm | 灵魂风暴 |
| SovereignBlade | 至高之刃 | Spite | 怨恨 |
| Squash | 压碎 | Squeeze | 挤压 |
| Strangle | 扼杀 | StrikeDefect | 打击(故障机器人) |
| StrikeIronclad | 打击(铁甲战士) | StrikeNecrobinder | 打击(亡灵师) |
| StrikeRegent | 打击(摄政) | StrikeSilent | 打击(沉默猎手) |
| SuckerPunch | 偷袭 | Sunder | 粉碎 |
| Supermassive | 超大质量 | Suppress | 压制 |
| Synthesis | 合成 | TagTeam | 车轮战 |
| TearAsunder | 撕裂虚空 | TeslaCoil | 特斯拉线圈 |
| TheHunt | 狩猎 | TheScythe | 死神镰刀 |
| Thrash | 猛打 | ThrummingHatchet | 嗡鸣手斧 |
| TimesUp | 时间到 | TwinStrike | 双重打击 |
| UltimateStrike | 终极打击 | Unleash | 释放 |
| Unrelenting | 不屈 | Uppercut | 上勾拳 |
| Uproar | 骚动 | Veilpiercer | 破幕者 |
| Whistle | 口哨 | WroughtInWar | 战火锻造 |

> ⚡ 标记 = 有专属 Patch 修复的危险卡牌，可安全用于友伤

</details>

### AOE 攻击牌（36 张）

<details>
<summary>点击展开完整名单</summary>

| 类名 | 中文名 | 类名 | 中文名 |
|------|--------|------|--------|
| AstralPulse | 星界脉冲 | BansheesCry | 女妖之嚎 |
| BoneShards | 骨骸碎片 | Breakthrough | 突破 |
| Conflagration | 大火 | CrashLanding | 坠落着陆 |
| CrushUnder | 碾碎 | DaggerSpray | 飞刀雨 |
| DramaticEntrance | 华丽登场 | DyingStar | 垂死之星 |
| EchoingSlash | 回响斩 | Exterminate | 灭绝 |
| FlakCannon | 高射炮 | FlickFlack | 后空翻 |
| FollowThrough | 贯穿 | GrandFinale | 大终章 |
| HighFive | 击掌 | HowlFromBeyond | 彼岸嚎叫 |
| Hyperbeam | 超级光束 | MeteorShower | 流星雨 |
| PactsEnd | 契约终结 | Radiate | 辐射 |
| Ricochet | 跳弹 | RipAndTear | 撕扯 |
| SevenStars | 七星 | Shatter | 碎裂 |
| Sow | 播种 | Stardust | 星尘 |
| Stomp | 践踏 | SweepingBeam | 扫射光束 |
| SweepingGaze | 扫视 | SwordBoomerang | 剑刃回旋 |
| Thunderclap | 雷鸣 | Volley | 齐射 |
| Whirlwind | 旋风 | | |

</details>

---

## 🏗️ 项目结构

```
MyStS2Mod/
├── Plugin.cs                  # Mod 入口，注册 Harmony Patch
├── ModInfo.cs                 # Mod 元信息（GUID、版本号）
├── mod_manifest.json          # 游戏 Mod 加载器识别文件
├── friendly_fire_config.json  # 配置文件（白名单、开关键、黑名单）
├── build.sh                   # 一键编译脚本
├── MyStS2Mod.csproj           # .NET 项目文件
├── NuGet.config               # NuGet 包源配置
│
├── Patches/
│   ├── TargetingPatches.cs    # 核心 Patch: 目标选择系统 (3 个 Patch)
│   ├── AoePatches.cs          # AOE Patch: 多目标扩展
│   └── CardSpecificPatches.cs # 危险卡牌专属 Patch (GoForTheEyes 等)
│
├── Utils/
│   ├── FriendlyFireConfig.cs  # JSON 配置加载 & 白名单/黑名单判定
│   ├── FriendlyFireState.cs   # 运行时状态（按键检测、当前卡牌追踪）
│   └── ModHelper.cs           # 通用工具（资源加载、本地化）
│
└── Assets/
    └── localization/
        ├── en.json            # 英文本地化
        └── zh-CN.json         # 中文本地化
```

---

## 🔧 技术实现

本 Mod 通过 [Harmony](https://github.com/pardeike/Harmony) 运行时 Patch 了游戏的 5 个方法：

### Patch 列表

| # | 目标方法 | Patch 类型 | 作用 |
|---|---------|-----------|------|
| 1 | `NTargetManager.AllowedToTargetCreature` | Postfix | UI 层面允许选中队友 |
| 2 | `CardModel.IsValidTarget` | Postfix | 逻辑层面允许队友作为合法目标 |
| 3 | `NMouseCardPlay.SingleCreatureTargeting` | Prefix + Postfix | 追踪当前打出的卡牌类名（供白名单查询） |
| 4 | `AttackCommand.GetPossibleTargets` | Postfix | AOE 目标扩展为除自己外所有人 |
| 5 | `GoForTheEyes.OnPlay` | Prefix | 安全替代逻辑，跳过 `Monster.IntendsToAttack` |

### 目标选择链路

```
卡牌打出 → NMouseCardPlay.TargetSelection
              │
              ├─ AnyEnemy/AnyAlly → SingleCreatureTargeting
              │     │
              │     ├─ NTargetManager.StartTargeting(targetType)
              │     │     └─ AllowedToTargetCreature()  ← Patch 1
              │     │
              │     └─ CardModel.IsValidTarget()        ← Patch 2
              │
              └─ AllEnemies → MultiCreatureTargeting
                    └─ OnPlay → AttackCommand
                          └─ GetPossibleTargets()       ← Patch 4
```

---

## 🛡️ 危险卡牌系统

部分卡牌在 `OnPlay` 中访问了 `cardPlay.Target.Monster` 属性。玩家的 `Creature` 对象没有 `Monster`（为 `null`），对队友使用这些卡牌会导致 **NullReferenceException** 崩溃。

### 保护机制

```
三道防线:

1. 黑名单  → 配置 dangerous_cards_blacklist，直接阻止友伤
2. 专属 Patch → 编写 Harmony Prefix 替换为安全逻辑
3. 标记已修复 → 添加到 FixedDangerousCards，绕过黑名单
```

### 当前已修复

| 卡牌 | 崩溃点 | 修复方式 |
|------|--------|---------|
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
            return true;  // 非队友目标 → 走原逻辑

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

2. 在 `FriendlyFireConfig.cs` 的 `FixedDangerousCards` 中注册：

```csharp
public static readonly HashSet<string> FixedDangerousCards = new()
{
    "GoForTheEyes",
    "YourCard"       // ← 添加这一行
};
```

3. 重新编译即可

---

## ❓ FAQ

**Q: 友伤会对自己生效吗？**
> 不会。Mod 明确排除了自己，AOE 的目标列表是「除攻击者以外的所有 Creature」。

**Q: 单人模式有用吗？**
> 单体友伤在单人模式下没有可选的队友目标。AOE 友伤同理（只有自己和敌人，自己被排除，效果等于原版）。该 Mod 主要为**多人合作模式**设计。

**Q: 友伤会触发卡牌的所有效果吗？**
> 是的。伤害、debuff（易伤、虚弱、中毒等）、特殊效果都会正常生效。唯一的例外是访问 `Monster` 属性的卡牌，这些会使用安全的替代逻辑。

**Q: 配置文件写错了会怎样？**
> Mod 会在控制台输出错误日志，并使用默认配置（全部允许 + Alt 键）继续运行，不会崩溃。

**Q: 怎么查看 Mod 日志？**
> 游戏控制台中搜索 `[Friendly Fire]` 前缀的日志。

---

## 🤝 贡献

欢迎提交 Issue 和 PR！如果你发现新的危险卡牌（友伤时崩溃），请提交 Issue 并附上崩溃日志。

---

## 📜 开源协议

[MIT License](LICENSE) — 随意使用、修改、分发。
