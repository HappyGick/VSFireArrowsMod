using System;
using System.Reflection;
using System.Collections.Generic;
using CombatOverhaul.RangedSystems;
using Vintagestory.API.Util;
using Vintagestory.API.Common;

namespace FireArrows.Integrations.CombatOverhaul.Extensions;

public static class ProjectileSystemExtensions
{
    // Normally it's a horror to break encapsulation, but idc, I need to replace the projectile.
    public static void ReplaceProjectile(this ProjectileSystemServer system, Guid id, ProjectileEntity newProjectile)
    {
        Type type = system.GetType();
        if (GetPrivateFieldValue<Dictionary<Guid, ProjectileServer>>(system, "_projectiles", type) is { } dict)
        {
            ProjectileServer serverProjectile = dict.Get<Guid, ProjectileServer>(id);
            Type projectileType = serverProjectile.GetType();
            ProjectileEntity? projectile = GetPrivateFieldValue<ProjectileEntity>(serverProjectile, "_entity", projectileType);
            ProjectileStats? projectileStats = GetPrivateFieldValue<ProjectileStats>(serverProjectile, "_stats", projectileType);
            ProjectileSpawnStats spawnStats = GetPrivateFieldValueStruct<ProjectileSpawnStats>(serverProjectile, "_spawnStats", projectileType);
            ICoreAPI? api = GetPrivateFieldValue<ICoreAPI>(serverProjectile, "_api", projectileType);
            ItemStack projectileStack = new(); // not used in the constructor or the class

            if (projectile is { } && projectileStats is { } && api is { })
            {
                
                ProjectileServer newServerProjectile = new(projectile, projectileStats, spawnStats, api, projectile.ClearCallback, projectileStack);
                dict[id] = newServerProjectile;
            }
        }
    }

    static T? GetPrivateFieldValue<T>(object instance, string fieldName, Type instanceType) where T : class
    {
        return instanceType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(instance) as T;
    }

    static T GetPrivateFieldValueStruct<T>(object instance, string fieldName, Type instanceType) where T : struct
    {
        if (instanceType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(instance) is { } obj)
        {
            return (T)obj;
        }
        throw new InvalidCastException("Private field is not a struct");
    }
}