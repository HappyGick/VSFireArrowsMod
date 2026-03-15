using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FireArrows.Items;

public class ItemFireArrow : ItemArrow
{
    long lastMsSinceParticleRender = 0;

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        dsc.AppendLine();
        dsc.AppendLine(Lang.Get("firearrows:item-firearrow-flavortext"));
    }

    public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
    {
        base.OnHeldIdle(slot, byEntity);
        
        if (byEntity.World.Side == EnumAppSide.Client)
        {
            AttachmentPointAndPose apap = byEntity.AnimManager?.Animator?.GetAttachmentPointPose("RightHand");
            if (apap == null) return;

            float[] modelMat = apap.AnimModelMatrix;
            
            Vec3d handWorldPos = byEntity.Pos.XYZ.Add(
                modelMat[12],
                modelMat[13],
                modelMat[14]
            );

            long dt = byEntity.World.ElapsedMilliseconds - lastMsSinceParticleRender;
            if (RenderParticles(handWorldPos, byEntity.World, dt))
            {
                lastMsSinceParticleRender = byEntity.World.ElapsedMilliseconds;
            }
        }
    }

    public override void OnGroundIdle(EntityItem entityItem)
    {
        base.OnGroundIdle(entityItem);

        long dt = entityItem.World.ElapsedMilliseconds - lastMsSinceParticleRender;
        
        Vec3d renderWorldPos = GetWorldPos(entityItem, new Vec3f(0.15f, 0.1f, 0f));
        if (RenderParticles(renderWorldPos, entityItem.World, dt))
        {
            lastMsSinceParticleRender = entityItem.World.ElapsedMilliseconds;
        }
    }

    private bool RenderParticles(Vec3d atPosition, IWorldAccessor world, long dtMs)
    {
        if (ParticleProperties != null && ParticleProperties.Length > 0)
        {
            if (dtMs > 25)
            {
                for (int i = 0; i < ParticleProperties.Length; i++)
                {
                    AdvancedParticleProperties bps = ParticleProperties[i];
                    bps.basePos.X = atPosition.X;
                    bps.basePos.Y = atPosition.Y;
                    bps.basePos.Z = atPosition.Z;

                    world.SpawnParticles(bps);
                }
                return true;
            }
        }
        return false;
    }

    private Vec3d GetWorldPos(Entity entity, Vec3f localOffset)
    {
        Matrixf mat = new Matrixf()
            .Translate(entity.Pos.X, entity.Pos.Y, entity.Pos.Z)
            .RotateY(entity.Pos.Yaw)
            .RotateX(entity.Pos.Pitch)
            .RotateZ(entity.Pos.Roll);

        Vec4f pos = new(localOffset.X, localOffset.Y, localOffset.Z, 1f);
        Vec4f worldPoint = mat.TransformVector(pos);

        return new Vec3d(worldPoint.X, worldPoint.Y, worldPoint.Z);
    }
}