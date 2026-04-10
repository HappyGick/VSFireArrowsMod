using FireArrows.Entities;
using FireArrows.Items;
using FireArrows.Patches;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace FireArrows;

public class FireArrowsModSystem : ModSystem
{
    public Harmony HarmonyInstance
    {
        get;
        private set;
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        api.RegisterItemClass(Mod.Info.ModID + ".arrow-firearrow", typeof(ItemFireArrow));
        api.RegisterEntity(Mod.Info.ModID + ".arrow-firearrow-entity", typeof(EntityFireArrow));

        api.Logger.Debug("[FireArrows] Loaded base entities");
        api.Logger.Debug("[FireArrows] Finished Start routine");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        
        HarmonyInstance = new Harmony(Mod.Info.ModID);
        HarmonyInstance.CreateClassProcessor(typeof(RenderItemParticlesPatch)).Patch();

        api.Logger.Notification("[FireArrows] All Harmony patches loaded.");
    }
}
