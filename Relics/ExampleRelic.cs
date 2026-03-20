using MegaCrit.Sts2.Core.Models;

namespace MyStS2Mod.Relics
{
    /// <summary>
    /// 自定义遗物示例
    ///
    /// 根据反编译结果:
    /// - MegaCrit.Sts2.Core.Models.Relics 命名空间下有 587 个遗物类型
    /// - 遗物稀有度: RelicRarity 枚举 (在 MegaCrit.Sts2.Core.Entities.Relics 中)
    /// - 遗物通过 Hook 系统触发效果 (80+ 个钩子方法)
    ///
    /// 关键 Hook 钩子 (可用于遗物效果):
    ///   - AfterModifyingPowerAmountGiven    给予能力后
    ///   - AfterModifyingPowerAmountReceived 接收能力后
    ///   - AfterPowerAmountChanged           能力数值变化后
    ///   - BeforePowerAmountChanged           能力数值变化前
    ///   - AfterModifyingRewards             修改奖励后
    ///   - AfterRewardTaken                  获取奖励后
    ///   - ModifyCardPlayCount               修改卡牌打出次数
    ///   - ModifyXValue                      修改 X 值
    ///   - ShouldDraw                        是否允许抽牌
    ///   - ShouldPlay                        是否允许出牌
    ///   - ShouldTakeExtraTurn               是否获得额外回合
    /// </summary>
    public class ExampleRelic // : 遗物基类 (需要进一步反编译确认)
    {
        // TODO: 反编译一个具体的游戏遗物查看完整实现模式
        // 遗物很可能也继承自 AbstractModel
    }
}
