using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FireArrows.Items;

public class ItemFireArrow : ItemArrow, IIgnitable
{
    private bool isExtinct;
    private bool isLit;
    private bool changedState = false;
    public bool IsExtinct => isExtinct;
    public CollectibleObject? ExtinctVariant { get; private set; }
    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        if (Variant.ContainsKey("state"))
        {
            AssetLocation loc = CodeWithParts("extinct");
            ExtinctVariant = api.World.GetItem(loc);
            isExtinct = Variant["state"] == "extinct";
            isLit = Variant["state"] == "lit";
        }
    }

    public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
    {
        if (api.World.Side == EnumAppSide.Server && byEntity.Swimming && !IsExtinct && ExtinctVariant != null)
        {
            api.World.PlaySoundAt(new AssetLocation("sounds/effect/extinguish"), byEntity.Pos.X + 0.5, byEntity.Pos.InternalY + 0.75, byEntity.Pos.Z + 0.5, null, false, 16);

            int q = slot.Itemstack.StackSize;
            slot.Itemstack = new(ExtinctVariant)
            {
                StackSize = q
            };
            slot.MarkDirty();
            changedState = true;
        }

        if (slot.Itemstack.Collectible != null)
        {
            // Dirty hack to force particle rendering on the client.
            // Yes, this means that the particle properties are defined twice in the .json file.
            // Particles also seem hardcoded for torches, so they don't work in any other way other than
            // to set up the item (shape, config, everything) as if it was a torch.
            if (slot.Itemstack.Collectible.ParticleProperties == null || changedState)
            {
                // my workaround for this bug: https://github.com/anegostudios/VintageStory-Issues/issues/8736
                AdvancedParticleProperties[] pps = slot.Itemstack.Collectible.Attributes["particleProperties"].AsArray<AdvancedParticleProperties>();
                slot.Itemstack.Collectible.ParticleProperties = pps;
                changedState = false;
            }
        }
    }

    public override void OnGroundIdle(EntityItem entityItem)
    {
        if (isLit && entityItem.Swimming && ExtinctVariant != null)
        {
            api.World.PlaySoundAt(new AssetLocation("sounds/effect/extinguish"), entityItem.Pos.X + 0.5, entityItem.Pos.InternalY + 0.75, entityItem.Pos.Z + 0.5, null, false, 16);

            int q = entityItem.Itemstack.StackSize;
            entityItem.Itemstack = new ItemStack(ExtinctVariant)
            {
                StackSize = q
            };
        }
    }
    
    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        dsc.AppendLine();
        dsc.AppendLine(Lang.Get("firearrows:item-firearrow-flavortext"));
    }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        var world = byEntity.World;
        if (byEntity.LeftHandItemSlot.Itemstack != null && isExtinct)
        {
            if (byEntity.LeftHandItemSlot.Itemstack.Collectible is BlockTorch)
            {
                byEntity.StartAnimation("interactstatictwohanded");
                handling = EnumHandHandling.PreventDefault;
                return;
            }
        }
        else
        {
            if (blockSel?.Position is { } pos && world.BlockAccessor.GetBlock(pos).GetInterface<IIgnitable>(world, pos) is { } ign)
            {
                if (byEntity is EntityPlayer player && !world.Claims.TryAccess(player.Player, pos, EnumBlockAccessFlags.Use))
                {
                    return;
                }

                if (isExtinct)
                {
                    if (ign.OnTryIgniteStack(byEntity, pos, slot, 0) == EnumIgniteState.Ignitable)
                    {
                        byEntity.World.PlaySoundAt(new AssetLocation("sounds/torch-ignite"), byEntity, (byEntity as EntityPlayer)?.Player, false, 16);
                        handling = EnumHandHandling.PreventDefault;
                        return;
                    }
                }
            }
        }

        handling = EnumHandHandling.NotHandled;
        base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
    }

    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
    {
        byEntity.StopAnimation("interactstatictwohanded");
    }

    public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
    {
        if (!isExtinct) return base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);

        var world = byEntity.World;

        if (byEntity.LeftHandItemSlot.Itemstack?.Collectible is IIgnitable coll)
        {
            return TryIgnite(coll, world, blockSel, entitySel, byEntity, null, slot, secondsUsed);
        }

        if (blockSel?.Position is not { } pos || world.BlockAccessor.GetBlock(pos).GetInterface<IIgnitable>(world, pos) is not { } ign)
        {
            return base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);
        }

        if (byEntity is EntityPlayer player && !world.Claims.TryAccess(player.Player, pos, EnumBlockAccessFlags.Use))
        {
            return false;
        }

        return TryIgnite(ign, world, blockSel, entitySel, byEntity, pos, slot, secondsUsed);
    }

    private bool TryIgnite(IIgnitable ign, IWorldAccessor world, BlockSelection blockSel, EntitySelection entitySel, EntityAgent byEntity, BlockPos? pos, ItemSlot slot, float secondsUsed)
    {
        switch (ign.OnTryIgniteStack(byEntity, pos, slot, secondsUsed))
        {
            case EnumIgniteState.Ignitable:
            {
                if (pos != null)
                {
                    if (world is not IClientWorldAccessor) return true;
                    if (!(secondsUsed > 0.25f) || (int)(30 * secondsUsed) % 2 != 1) return true;

                    Random rand = world.Rand;
                    Vec3d offset = new(rand.NextDouble() * 0.25 - 0.125, rand.NextDouble() * 0.25 - 0.125, rand.NextDouble() * 0.25 - 0.125);

                    Block blockFire = world.GetBlock(new AssetLocation("fire"));
                    AdvancedParticleProperties props = blockFire.ParticleProperties[^1].Clone();
                    props.basePos = pos.ToVec3d().Add(blockSel.HitPosition).Add(offset);
                    props.Quantity.avg = 0.5f;
                    world.SpawnParticles(props);

                    props.Quantity.avg = 0;
                }

                return true;
            }
            case EnumIgniteState.IgniteNow:
            {
                if (world.Side == EnumAppSide.Client) return false;

                var assetLoc = CodeWithParts("lit");
                var item = byEntity.World.GetItem(assetLoc);
                var stack = new ItemStack(item);

                if (slot.StackSize == 1)
                {
                    slot.Itemstack = stack;
                }
                else
                {
                    slot.TakeOut(1);
                    if (!byEntity.TryGiveItemStack(stack))
                    {
                        world.SpawnItemEntity(stack, byEntity.Pos.XYZ);
                    }
                }

                slot.MarkDirty();
                return false;
            }
            default:
                return base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);
        }
    }

    public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
    {
        byEntity.StopAnimation("interactstatictwohanded");
        return base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason);
    }

    public EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
    {
        return EnumIgniteState.NotIgnitable;
    }

    public void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
    {
    }

    public EnumIgniteState OnTryIgniteStack(EntityAgent byEntity, BlockPos pos, ItemSlot slot, float secondsIgniting)
    {
        if (!IsExtinct) return secondsIgniting > 2 ? EnumIgniteState.IgniteNow : EnumIgniteState.Ignitable;
        return EnumIgniteState.NotIgnitable;
    }
}