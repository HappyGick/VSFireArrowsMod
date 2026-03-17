using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FireArrows.Items;

public class ItemFireArrowLit : ItemArrow
{
    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        dsc.AppendLine();
        dsc.AppendLine(Lang.Get("firearrows:item-firearrow-flavortext"));
    }
}