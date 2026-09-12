using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class HealingManager : AbstractModManager
{
    protected override string ConfigName => "HealingConfig";

    protected override void AfterPostDb()
    {
        Helper.IterateConfigItems(DatabaseTables, Config, SetHealingItem);

        Constants.GetLogger().Info($"{Constants.ModTitle}: Healing items changes applied!");
    }

    private void SetHealingItem(TemplateItem item, JsonObject itemConfig, ModDatabaseTables databaseTables, JsonNode? rootConfig)
    {
        var props = item.Properties;
        if (props is null)
        {
            return;
        }

        var maxHpResource = GetNumberValue(itemConfig["MaxHpResource"]);
        if (maxHpResource != null)
        {
            props.MaxHpResource = (int)Math.Round(maxHpResource.Value);
        }

        var hpResourceRate = GetNumberValue(itemConfig["HPResourceRate"]);
        if (hpResourceRate != null)
        {
            props.HpResourceRate = hpResourceRate.Value;
        }

        var effectsDamage = props.EffectsDamage;
        if (effectsDamage != null)
        {
            var lightCost = GetNumberValue(itemConfig["LightBleedingCost"]);
            var heavyCost = GetNumberValue(itemConfig["HeavyBleedingCost"]);
            var fractureCost = GetNumberValue(itemConfig["FractureCost"]);

            SetEffectCost(effectsDamage, DamageEffectType.LightBleeding, lightCost);
            SetEffectCost(effectsDamage, DamageEffectType.HeavyBleeding, heavyCost);
            SetEffectCost(effectsDamage, DamageEffectType.Fracture, fractureCost);
        }

        if (itemConfig["Regen"] is JsonObject regenConfig)
        {
            var rate = GetNumberValue(regenConfig["Rate"]);
            var time = GetNumberValue(regenConfig["Time"]);

            if (rate != null && time != null)
            {
                var buffName = $"Buff{item.Id}";
                SetRegenBuff(databaseTables, buffName, rate.Value, time.Value);

                props.StimulatorBuffs = buffName;
                props.EffectsDamage = new Dictionary<DamageEffectType, EffectsDamageProperties>();

                item.Parent = new MongoId("5448f3a14bdc2d27728b4569");
            }
        }

        var useTime = GetNumberValue(itemConfig["useTime"]);
        if (useTime != null)
        {
            props.MedUseTime = useTime.Value;
        }
    }

    private static void SetEffectCost(Dictionary<DamageEffectType, EffectsDamageProperties> effectsDamage, DamageEffectType effectType, double? cost)
    {
        if (cost is null)
        {
            return;
        }

        if (!effectsDamage.TryGetValue(effectType, out var effect) || effect is null)
        {
            return;
        }

        effect.Cost = cost.Value;
    }

    private static void SetRegenBuff(ModDatabaseTables databaseTables, string buffName, double rate, double time)
    {
        var buffsTable = databaseTables.Globals?.Configuration?.Health?.Effects?.Stimulator?.Buffs;
        if (buffsTable is null)
        {
            return;
        }

        var buff = new Buff
        {
            BuffType = "HealthRate",
            Chance = 1,
            Delay = 1,
            Duration = time,
            Value = rate,
            AbsoluteValue = true,
            SkillName = string.Empty
        };

        buffsTable[buffName] = new List<Buff> { buff };
    }
}
