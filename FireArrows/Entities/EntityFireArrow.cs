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

        if (Swimming && Code.EndVariant() == "lit" && World.Side == EnumAppSide.Server)
        {
            LightHsv = [];
            ParticleProperties = [];

            AssetLocation newCode = Code.CopyWithPath("arrow-firearrow-extinct");
            EntityProperties type = World.GetEntityType(newCode);
            
            Entity newEntity = World.Api.ClassRegistry.CreateEntity(type);
            var entityarrow = newEntity as IProjectile;

            var assetLoc = ProjectileStack.Collectible.CodeWithParts("extinct");
            var item = World.GetItem(assetLoc);
            var stack = new ItemStack(item, ProjectileStack.StackSize);

            entityarrow.FiredBy = FiredBy;
            entityarrow.Damage = Damage;
            entityarrow.DamageTier = DamageTier;
            entityarrow.ProjectileStack = stack;
            entityarrow.DropOnImpactChance = DropOnImpactChance;
            entityarrow.IgnoreInvFrames = IgnoreInvFrames;
            entityarrow.WeaponStack = stack;
            entityarrow.DamageStackOnImpact = DamageStackOnImpact;
            
            newEntity.ServerPos.SetFrom(ServerPos);
            newEntity.Pos.SetFrom(Pos);
            newEntity.World = World;
            entityarrow.PreInitialize();

            newEntity.Attributes = Attributes.Clone();
            
            Api.World.PlaySoundAt(new AssetLocation("sounds/effect/extinguish"), Pos.X + 0.5, Pos.InternalY + 0.75, Pos.Z + 0.5, null, false, 16);
            World.SpawnEntity(newEntity);
            Die();
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