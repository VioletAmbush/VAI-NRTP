using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class FoodManager : AbstractModManager
{
    protected override string ConfigName => "FoodConfig";

    protected override void AfterPostDb()
    {
        Helper.IterateConfigItems(DatabaseTables, Config, SetFood);

        Constants.GetLogger().Info($"{Constants.ModTitle}: Food changes applied!");
    }

    private void SetFood(TemplateItem item, JsonObject config, ModContext databaseTables, JsonNode? rootConfig)
    {
        if (config["buffs"] is JsonArray buffs && buffs.Count > 0)
        {
            Helper.SetItemBuffs(databaseTables, item, buffs);
        }

        var hydrationCost = GetNumberValue(config["hydrationCost"]);
        var energyCost = GetNumberValue(config["energyCost"]);
        if (hydrationCost is null || energyCost is null || hydrationCost.Value == 0 || energyCost.Value == 0)
        {
            return;
        }

        var props = item.Properties;
        if (props is null)
        {
            return;
        }

        var effects = new Dictionary<HealthFactor, EffectsHealthProperties>();
        effects[HealthFactor.Hydration] = new EffectsHealthProperties
        {
            Value = hydrationCost.Value
        };
        effects[HealthFactor.Energy] = new EffectsHealthProperties
        {
            Value = energyCost.Value
        };

        props.EffectsHealth = effects;
    }
}
