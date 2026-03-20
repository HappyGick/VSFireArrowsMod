namespace FireArrows.Patches;

using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

// This adds the necessary functionality to render particles for non-Block items
// when they're on the ground. Very ooga booga caveman to do a Postfix, but
// I'm too lazy and too busy to go ahead and understand IL. I barely know what I'm
// doing already.

[HarmonyPatch(typeof(EntityItemRenderer), "DoRender3DOpaque")]
public class RenderItemParticlesPatch
{
    private static readonly AccessTools.FieldRef<EntityItemRenderer, float> accumField = 
        AccessTools.FieldRefAccess<EntityItemRenderer, float>("accum");
    private static readonly AccessTools.FieldRef<EntityItemRenderer, Vec4f> particleOutTransformField = 
        AccessTools.FieldRefAccess<EntityItemRenderer, Vec4f>("particleOutTransform");

    [HarmonyPostfix]
    public static void Postfix(EntityItemRenderer __instance, float dt, bool isShadowPass)
    {
        if (isShadowPass) return;

        if (__instance.GetType()
            .GetField("entityitem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(__instance) is not EntityItem entityitem) return;

        if (entityitem.Api is ICoreClientAPI capi)
        {
            float accum = accumField(__instance);
            Vec4f particleOutTransform = particleOutTransformField(__instance);

            float[] currentMatrix = __instance.ModelMat;

            ItemStack stack = entityitem.Itemstack;
            // my workaround for this bug: https://github.com/anegostudios/VintageStory-Issues/issues/8736
            AdvancedParticleProperties[] ParticleProperties = stack.Collectible.Attributes["particleProperties"].AsArray<AdvancedParticleProperties>();

            if (ParticleProperties == null || ParticleProperties.Length == 0) return; // skip the heavy calculations if we don't have particles

            if (stack.Block == null && stack.Collectible != null && !capi.IsGamePaused)
            {
                // This code is copied and adapted from vsessentialsmod/EntityRenderer/EntityItemRenderer.cs::DoRender3DOpaque:200-216
                // My changes were mainly to make it work as a Harmony patch.
                FastVec3f offset = stack.Collectible.Attributes["particleOffset"].AsObject<FastVec3f>();
                Mat4f.MulWithVec4(
                    currentMatrix,
                    new Vec4f(
                        // Turns out, the "magic numbers" that appear in EntityItemRenderer are just the particle offset.
                        stack.Collectible.TopMiddlePos.X + offset.X,
                        stack.Collectible.TopMiddlePos.Y + offset.Y,
                        stack.Collectible.TopMiddlePos.Z + offset.Z, 
                        0
                    ), particleOutTransformField(__instance));

                accumField(__instance) += dt;
                if (ParticleProperties != null && ParticleProperties.Length > 0 && accum > 0.025f)
                {
                    accumField(__instance) %= 0.025f;

                    for (int i = 0; i < ParticleProperties.Length; i++)
                    {
                        AdvancedParticleProperties bps = ParticleProperties[i];
                        bps.basePos.X = particleOutTransform.X + __instance.entity.Pos.X;
                        bps.basePos.Y = particleOutTransform.Y + __instance.entity.Pos.InternalY;
                        bps.basePos.Z = particleOutTransform.Z + __instance.entity.Pos.Z;

                        entityitem.World.SpawnParticles(bps);
                    }
                }
            }
        }
    }
}