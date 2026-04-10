using System;
using System.Reflection;
using FireArrows.CombatOverhaul.Entities;
using FireArrows.CombatOverhaul.Patches;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace FireArrows.CombatOverhaul;

public class FireArrowsCOModSystem : ModSystem
{
    private Harmony? _harmony;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        api.RegisterEntity("firearrowscocompat.arrow-firearrow-entity-combatoverhaul", typeof(EntityFireArrowCO));

        api.Logger.Debug("[FireArrows CO Compat] Registered Combat Overhaul entities");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);

        _harmony = new Harmony(Mod.Info.ModID);

        api.Logger.Notification("[FireArrows CO Compat] Attempting to apply Harmony patches...");

        Type? attachmentType = AccessTools.TypeByName("CombatOverhaul.Animations.Attachment");
        if (attachmentType == null)
        {
            api.Logger.Warning("[FireArrows CO Compat] Could not find CombatOverhaul.Animations.Attachment type, skipping patch.");
            return;
        }

        ConstructorInfo? targetMethod = AccessTools.Constructor(attachmentType, [
            typeof(ICoreClientAPI), typeof(string), typeof(ItemStack), typeof(ModelTransform), typeof(bool)
        ]);

        MethodInfo? transpiler = AccessTools.Method(typeof(AnimationsLibAttachmentPatch), nameof(AnimationsLibAttachmentPatch.Transpiler));

        _harmony.Patch(targetMethod, transpiler: new HarmonyMethod(transpiler));

        api.Logger.Notification("[FireArrows CO Compat] Successfully applied animations patch to OverhaulLib.");
    }

    public override void Dispose()
    {
        base.Dispose();
        _harmony?.UnpatchAll(_harmony.Id);
    }
}
