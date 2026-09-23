using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class MagazinesManager : AbstractModManager
{
    protected override string ConfigName => "MagazinesConfig";

    protected override void AfterPostDb()
    {
        foreach (var item in DatabaseTables.Templates.Items.Values)
        {
            var props = item.Properties;
            if (props is null)
            {
                continue;
            }

            var cartridges = props.Cartridges;
            if (cartridges is null || !cartridges.Any())
            {
                continue;
            }

            if (string.Equals(props.ReloadMagType?.ToString(), "InternalMagazine", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            SetMagLoadModifier(props, cartridges);
        }

        Helper.IterateConfigItems(DatabaseTables, Config, SetMag);

        Constants.GetLogger().Info($"{Constants.ModTitle}: Magazines changes applied!");
    }

    private void SetMagLoadModifier(TemplateItemProperties props, IEnumerable<Slot> cartridges)
    {
        var first = cartridges.FirstOrDefault();
        var maxCount = first?.MaxCount;
        if (maxCount is null)
        {
            return;
        }

        var loadUnloadModifier = props.LoadUnloadModifier ?? 0;

        if (maxCount.Value < 50)
        {
            loadUnloadModifier += GetConfigNumber("magLoadPercent", 0);

            if (loadUnloadModifier < 0)
            {
                loadUnloadModifier *= GetConfigNumber("magLoadMultiplier", 1);
            }
        }
        else
        {
            loadUnloadModifier += GetConfigNumber("largeMagLoadPercent", 0);

            if (loadUnloadModifier < 0)
            {
                loadUnloadModifier *= GetConfigNumber("largeLoadMultiplier", 1);
            }
        }

        if (loadUnloadModifier < -50)
        {
            loadUnloadModifier = -50;
        }

        props.LoadUnloadModifier = loadUnloadModifier;
    }

    private void SetMag(TemplateItem item, JsonObject config, ModContext databaseTables, JsonNode? rootConfig)
    {
        var value = GetNumberValue(config["magLoadPercent"]);
        if (value is null)
        {
            return;
        }

        var props = item.Properties;
        if (props is null)
        {
            return;
        }

        props.LoadUnloadModifier = value.Value;
    }
}
