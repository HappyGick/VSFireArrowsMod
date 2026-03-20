using FireArrows.Entities;
using FireArrows.Items;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace FireArrows;

public class FireArrowsModSystem : ModSystem
{
    private Harmony harmony;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        
        api.RegisterItemClass(Mod.Info.ModID + ".arrow-firearrow", typeof(ItemFireArrow));
        api.RegisterEntity(Mod.Info.ModID + ".arrow-firearrow-entity", typeof(EntityFireArrow));
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        
        harmony = new Harmony(Mod.Info.ModID);
        harmony.PatchAll();
    }
}
