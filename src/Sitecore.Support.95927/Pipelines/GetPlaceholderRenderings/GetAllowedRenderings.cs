using Sitecore.Caching.Placeholders;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetPlaceholderRenderings;
using Sitecore.SecurityModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        item = this.GetPlaceholderItem(args.PlaceholderKey, args.ContentDatabase, args.LayoutDefinition);
      }
      else
      {
        using (new DeviceSwitcher(args.DeviceId, args.ContentDatabase))
        {
          item = this.GetPlaceholderItem(args.PlaceholderKey, args.ContentDatabase, args.LayoutDefinition);
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

    private Item GetPlaceholderItem(string placeholderKey, Database database, string layoutDefinition)
    {
      Assert.ArgumentNotNull(placeholderKey, "placeholderKey");
      Assert.ArgumentNotNull(database, "database");
      Assert.ArgumentNotNull(layoutDefinition, "layoutDefinition");
      var placeholderDefinition = Client.Page.GetPlaceholderDefinition(layoutDefinition, placeholderKey);
      if (placeholderDefinition != null)
      {
        string metaDataItemId = placeholderDefinition.MetaDataItemId;
        if (string.IsNullOrEmpty(metaDataItemId))
        {
          return null;
        }
        using (new SecurityDisabler())
        {
          return database.GetItem(metaDataItemId);
        }
      }
      var placeholderCache = PlaceholderCacheManager.GetPlaceholderCache(database.Name);
      Item item = placeholderCache[placeholderKey];
      if (item != null)
      {
        return item;
      }
      var builder = new StringBuilder("/sitecore/layout/placeholder settings//*[");
      builder.AppendFormat("comparecaseinsensitive(@{0},'{1}')", "Placeholder Key", placeholderKey);
      int num = placeholderKey.LastIndexOf('/');
      if (num >= 0)
      {
        string str2 = StringUtil.Mid(placeholderKey, num + 1);
        builder.AppendFormat(" or comparecaseinsensitive(@{0},'{1}')", "Placeholder Key", str2);
        item = placeholderCache[str2];
      }
      if (item != null)
      {
        return item;
      }
      builder.Append("]");
      var source = database.SelectItems(builder.ToString());
      if (source == null)
      {
        return item;
      }
      return (source.FirstOrDefault(i => ((i.Fields["Placeholder Key"] != null) && i.Fields["Placeholder Key"].Value.Contains("/"))) ?? source.FirstOrDefault());
    }
  }
}