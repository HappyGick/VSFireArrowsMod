using System;
using System.Reflection;
using FireArrows.Entities;
using FireArrows.Integrations.CombatOverhaul.Entities;
using FireArrows.Items;
using FireArrows.Patches;
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

        if (api.ModLoader.IsModEnabled("combatoverhaul"))
        {
            api.RegisterEntity(Mod.Info.ModID + ".arrow-firearrow-entity-combatoverhaul", typeof(EntityFireArrowCO));
        }
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        
        harmony = new Harmony(Mod.Info.ModID);
        harmony.PatchAll();

        if (api.ModLoader.IsModEnabled("combatoverhaul"))
        {
            ApplyOverhaulLibPatches(api);
        }

        api.Logger.Notification("[FireArrows] All Harmony patches loaded.");
    }

    private void ApplyOverhaulLibPatches(ICoreClientAPI api)
    {
        api.Logger.Notification("[FireArrows] CombatOverhaul is enabled, attempting to apply patches...");
        Type attachmentType = AccessTools.TypeByName("CombatOverhaul.Animations.Attachment");
        if (attachmentType == null) return;

        ConstructorInfo targetMethod = AccessTools.Constructor(attachmentType, [
            typeof(ICoreClientAPI), typeof(string), typeof(ItemStack), typeof(ModelTransform), typeof(bool) 
        ]);

        MethodInfo transpiler = AccessTools.Method(typeof(AnimationsLibAttachmentPatch), nameof(AnimationsLibAttachmentPatch.Transpiler));

        harmony.Patch(targetMethod, transpiler: new HarmonyMethod(transpiler));
        
        api.Logger.Notification("[FireArrows] Successfully applied animations patch to OverhaulLib.");
    }
}
