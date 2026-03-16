using FireArrows.Entities;
using FireArrows.Items;
using Vintagestory.API.Common;

namespace FireArrows;

public class FireArrowsModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        
        api.RegisterItemClass(Mod.Info.ModID + ".arrow-firearrow", typeof(ItemFireArrow));
        api.RegisterEntity(Mod.Info.ModID + ".arrow-firearrow-entity", typeof(EntityFireArrow));
    }
}
