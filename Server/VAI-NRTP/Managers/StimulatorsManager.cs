using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class StimulatorsManager : AbstractModManager
{
    protected override string ConfigName => "StimulatorsConfig";

    protected override void AfterPostDb()
    {
        Helper.IterateConfigItems(DatabaseTables, Config, SetStimulator);

        Constants.GetLogger().Info($"{Constants.ModTitle}: Stimulators changes applied!");
    }

    private void SetStimulator(TemplateItem item, JsonObject config, ModDatabaseTables databaseTables, JsonNode? rootConfig)
    {
        if (config["buffs"] is JsonArray buffs)
        {
            Helper.SetItemBuffs(databaseTables, item, buffs);
        }

        var props = item.Properties;
        if (props is null)
        {
            return;
        }

        var effectsDamage = props.EffectsDamage;
        if (effectsDamage is null)
        {
            return;
        }

        var painDuration = GetNumberValue(config["painkillerDuration"]);
        var contusionDuration = GetNumberValue(config["contusionDuration"]);
        var intoxicationDuration = GetNumberValue(config["intoxicationDuration"]);
        var radExposureDuration = GetNumberValue(config["radExposureDuration"]);

        SetEffectDuration(effectsDamage, DamageEffectType.Pain, painDuration);
        SetEffectDuration(effectsDamage, DamageEffectType.Contusion, contusionDuration);
        SetEffectDuration(effectsDamage, DamageEffectType.Intoxication, intoxicationDuration);
        SetEffectDuration(effectsDamage, DamageEffectType.RadExposure, radExposureDuration);

        if (config["removeGoldenStarContusion"]?.GetValue<bool>() == true)
        {
            RemoveEffect(effectsDamage, DamageEffectType.Contusion);
        }
    }

    private static void SetEffectDuration(Dictionary<DamageEffectType, EffectsDamageProperties> effectsDamage, DamageEffectType effectType, double? duration)
    {
        if (duration is null)
        {
            return;
        }

        if (!effectsDamage.TryGetValue(effectType, out var effect) || effect is null)
        {
            return;
        }

        effect.Duration = duration.Value;
    }

    private static void RemoveEffect(Dictionary<DamageEffectType, EffectsDamageProperties> effectsDamage, DamageEffectType effectType)
    {
        effectsDamage.Remove(effectType);
    }
}
