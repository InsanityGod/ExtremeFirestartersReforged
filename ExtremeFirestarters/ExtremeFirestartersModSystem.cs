using ExtremeFirestarters.Code.Behaviors;
using ExtremeFirestarters.Code.Config;
using InsanityLib.Attributes.Auto;
using Vintagestory.API.Common;

[assembly: AutoRegistry("extremefirestarters")]
namespace ExtremeFirestarters;
public class ExtremeFirestartersModSystem : ModSystem
{

    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);

        foreach(var collectible in api.World.Collectibles)
        {
            if(collectible.Code?.Domain != "extremefirestarters") continue;
            var fireStarterBehavior = collectible.GetBehavior<FireStarter>();
            if(fireStarterBehavior is null) continue;

            var mult = ExtremeFirestartersConfig.Instance.DurabilityMultiplier;
            collectible.Durability = mult > 0 ? (int)(collectible.Durability * mult) : 0;
        }
    }
}