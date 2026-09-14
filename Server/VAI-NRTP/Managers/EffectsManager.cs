using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class EffectsManager : AbstractModManager
{
    protected override string ConfigName => "EffectsConfig";

    protected override void AfterPostDb()
    {
        var effects = DatabaseTables.Globals?.Configuration?.Health?.Effects;
        if (effects is null)
        {
            return;
        }

        var probChangesEnabled = GetConfigBool("probChangesEnabled");
        var lightBleed = GetConfigObject("lightBleed");
        var heavyBleed = GetConfigObject("heavyBleed");
        var fracture = GetConfigObject("fracture");
        var toxin = GetConfigObject("toxin");

        var lightBleeding = effects.LightBleeding;
        var heavyBleeding = effects.HeavyBleeding;
        var fractureEffect = effects.Fracture;
        var intoxication = effects.Intoxication;

        if (probChangesEnabled)
        {
            var lightProb = lightBleed?["prob"] as JsonObject;
            var heavyProb = heavyBleed?["prob"] as JsonObject;
            var fractureFallProb = fracture?["fallProb"] as JsonObject;
            var fractureBulletProb = fracture?["bulletProb"] as JsonObject;

            SetProbability(lightBleeding?.Probability, lightProb);
            SetProbability(heavyBleeding?.Probability, heavyProb);
            SetProbability(fractureEffect?.FallingProbability, fractureFallProb);
            SetProbability(fractureEffect?.BulletHitProbability, fractureBulletProb);
        }

        MultiplyMember(lightBleeding, GetNumber(lightBleed?["damageMult"]), (target, value) => target.DamageHealth *= value);
        MultiplyMember(lightBleeding, GetNumber(lightBleed?["priceMult"]), (target, value) => target.RemovePrice *= value);

        MultiplyMember(heavyBleeding, GetNumber(heavyBleed?["damageMult"]), (target, value) => target.DamageHealth *= value);
        MultiplyMember(heavyBleeding, GetNumber(heavyBleed?["priceMult"]), (target, value) => target.RemovePrice *= value);

        MultiplyMember(fractureEffect, GetNumber(fracture?["priceMult"]), (target, value) => target.RemovePrice *= value);

        MultiplyMember(intoxication, GetNumber(toxin?["damageMult"]), (target, value) => target.DamageHealth *= value);
        MultiplyMember(intoxication, GetNumber(toxin?["priceMult"]), (target, value) => target.RemovePrice *= value);

        Constants.GetLogger().Info($"{Constants.ModTitle}: Effects changes applied!");
    }

    private static void SetProbability(Probability? probability, JsonObject? config)
    {
        if (probability is null || config is null)
        {
            return;
        }

        var threshold = GetNumber(config["T"]);
        var k = GetNumber(config["K"]);
        var b = GetNumber(config["B"]);

        if (threshold != null)
        {
            probability.Threshold = threshold.Value;
        }

        if (k != null)
        {
            probability.K = k.Value;
        }

        if (b != null)
        {
            probability.B = b.Value;
        }
    }

    private static void MultiplyMember<T>(T? target, double? multiplier, Action<T, double> apply)
        where T : class
    {
        if (target is null || multiplier is null)
        {
            return;
        }

        apply(target, multiplier.Value);
    }

    private static double? GetNumber(JsonNode? node)
    {
        return GetNumberValue(node);
    }
}
