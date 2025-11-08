using ExtremeFirestarters.Code.Behaviors;
using ExtremeFirestarters.Code.Config;
using InsanityLib.Attributes.Auto;
using Vintagestory.API.Common;

[assembly: AutoRegistry("extremefirestartersreforged")]
namespace ExtremeFirestarters;
public class ExtremeFirestarterReforgedModSystem : ModSystem
{

    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);

        foreach(var collectible in api.World.Collectibles)
        {
            if(collectible.Code?.Domain != "extremefirestartersreforged") continue;
            var fireStarterBehavior = collectible.GetBehavior<FireStarter>();
            if(fireStarterBehavior is null) continue;

            var mult = ExtremeFirestarterReforged.Instance.DurabilityMultiplier;
            collectible.Durability = mult > 0 ? (int)(collectible.Durability * mult) : 0;
        }
    }
}