using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetPlaceholderRenderings;
using System.Collections.Generic;

namespace Sitecore.Support.Pipelines.GetPlaceholderRenderings
{
  public class GetAllowedRenderings: Sitecore.Pipelines.GetPlaceholderRenderings.GetAllowedRenderings
  {
    public new void Process([NotNull]GetPlaceholderRenderingsArgs args)
    {
      Assert.IsNotNull(args, "args");
      Item item = null;
      if (ID.IsNullOrEmpty(args.DeviceId))
      {
        item = Client.Page.GetPlaceholderItem(args.PlaceholderKey, args.ContentDatabase, args.LayoutDefinition);
      }
      else
      {
        using (new DeviceSwitcher(args.DeviceId, args.ContentDatabase))
        {
          item = Client.Page.GetPlaceholderItem(args.PlaceholderKey, args.ContentDatabase, args.LayoutDefinition);
        }
      }

      List<Item> allowedRenderings = null;
      if (item != null)
      {
        args.HasPlaceholderSettings = true;
        bool allowedControlsSpecified;
        allowedRenderings = base.GetRenderings(item, out allowedControlsSpecified);
        if (allowedControlsSpecified)
        {
          args.Options.ShowTree = false;
        }
      }

      if (allowedRenderings == null)
      {
        return;
      }

      if (args.PlaceholderRenderings == null)
      {
        args.PlaceholderRenderings = new List<Item>();
      }

      args.PlaceholderRenderings.AddRange(allowedRenderings);
    }
  }
}