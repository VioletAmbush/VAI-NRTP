using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class MeleeManager : AbstractModManager
{
    protected override string ConfigName => "MeleeConfig";

    protected override void AfterPostDb()
    {
        Helper.IterateConfigItems(DatabaseTables, Config, SetMeleeItem);

        Constants.GetLogger().Info($"{Constants.ModTitle}: Melee changes applied!");
    }

    private void SetMeleeItem(TemplateItem item, JsonObject config, ModDatabaseTables databaseTables, JsonNode? rootConfig)
    {
        var slash = GetNumberValue(config["slashDamage"]);
        var stab = GetNumberValue(config["stabDamage"]);

        var props = item.Properties;
        if (props is null)
        {
            return;
        }

        if (slash != null)
        {
            props.KnifeHitSlashDam = (int)Math.Round(slash.Value);
        }

        if (stab != null)
        {
            props.KnifeHitStabDam = (int)Math.Round(stab.Value);
        }
    }
}
