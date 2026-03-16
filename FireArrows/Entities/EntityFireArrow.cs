using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FireArrows.Entities;

public class EntityFireArrow : EntityProjectile
{
    private float accDt = 0;

    AdvancedParticleProperties[] ParticleProperties = [];

    public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
    {
        base.Initialize(properties, api, InChunkIndex3d);

        LightHsv = properties.Attributes["lightHsv"].AsArray<byte>();
        ParticleProperties = properties.Attributes["particleProperties"].AsArray<AdvancedParticleProperties>();
    }

    public override void OnGameTick(float dt)
    {
        base.OnGameTick(dt);

        if (RenderParticles(Pos.XYZ, World, accDt))
        {
            accDt = 0;
        }
        else
        {
            accDt += dt;
        }
    }

    private bool RenderParticles(Vec3d atPosition, IWorldAccessor world, float dt)
    {
        if (ParticleProperties != null && ParticleProperties.Length > 0)
        {
            if (dt > 0.025)
            {
                for (int i = 0; i < ParticleProperties.Length; i++)
                {
                    AdvancedParticleProperties bps = ParticleProperties[i];
                    bps.basePos.X = atPosition.X;
                    bps.basePos.Y = atPosition.Y + 0.12f;
                    bps.basePos.Z = atPosition.Z;

                    world.SpawnParticles(bps);
                }
                return true;
            }
        }
        return false;
    }
}