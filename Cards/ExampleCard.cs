using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MyStS2Mod.Cards
{
    /// <summary>
    /// 自定义卡牌示例
    ///
    /// 根据反编译结果:
    /// - 所有卡牌继承自 CardModel (abstract)，基类为 AbstractModel
    /// - 核心属性: Rarity, EnergyCost, BaseStarCost, Keywords, Description
    /// - 核心方法: OnPlayWrapper, CanPlay, UpgradeInternal, AfterCreated
    /// - 卡牌通过 ModHelper.AddModelToPool 注册到指定的 CardPoolModel
    ///
    /// 参考游戏中的卡牌实现:
    ///   MegaCrit.Sts2.Core.Models.Cards 命名空间下有 1286 个卡牌类型
    ///   例如: BattleTrance, DrainPower, FightMe, FightThrough 等
    /// </summary>
    public class ExampleCard // : CardModel
    {
        // TODO: 反编译一张具体的游戏卡牌 (如 BattleTrance) 查看完整实现模式
        // 然后继承 CardModel 实现你的自定义卡牌

        /*
        // 卡牌创建后的初始化
        public override void AfterCreated()
        {
            base.AfterCreated();
            // 设置基础数值
        }

        // 卡牌打出时的效果 (核心逻辑)
        // 注意: 实际的打出逻辑可能在其他方法中
        // 需要参考一张真实卡牌来确定确切的重写方法

        // 升级效果
        public override void UpgradeInternal()
        {
            // 升级数值变化
        }
        */
    }
}
