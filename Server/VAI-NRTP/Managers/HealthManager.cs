using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class HealthManager : AbstractModManager
{
    protected override string ConfigName => "HealthConfig";

    protected override void AfterPostDb()
    {
        SetPlayerHealth();

        Constants.GetLogger().Info($"{Constants.ModTitle}: Player health changes applied!");
    }

    // Runs last so bot types other mods register after PostDb are in the database before any
    // are seeded - MoreBotsAPI adds mod bot types at PostDBModLoader + 80085, well after our
    // PostDb hook, so anything polling the bot list earlier never sees them.
    protected override void AfterFinal()
    {
        SetBotsHealth();

        Constants.GetLogger().Info($"{Constants.ModTitle}: Bot health changes applied!");
    }

    private void SetPlayerHealth()
    {
        var playerConfig = GetConfigObject("player");
        if (playerConfig is null)
        {
            return;
        }

        var profiles = DatabaseTables.Templates?.Profiles;
        if (profiles is null)
        {
            return;
        }

        foreach (var profile in profiles.Values)
        {
            if (profile is null)
            {
                continue;
            }

            SetSideHealth(profile.Bear?.Character?.Health, playerConfig);
            SetSideHealth(profile.Usec?.Character?.Health, playerConfig);
        }
    }

    private static void SetSideHealth(BotBaseHealth? health, JsonObject config)
    {
        var bodyParts = health?.BodyParts;
        if (bodyParts is null)
        {
            return;
        }

        SetPlayerPart(bodyParts, "Head", config["head"]);
        SetPlayerPart(bodyParts, "Chest", config["chest"]);
        SetPlayerPart(bodyParts, "Stomach", config["stomach"]);
        SetPlayerPart(bodyParts, "LeftArm", config["arm"]);
        SetPlayerPart(bodyParts, "RightArm", config["arm"]);
        SetPlayerPart(bodyParts, "LeftLeg", config["leg"]);
        SetPlayerPart(bodyParts, "RightLeg", config["leg"]);
    }

    private void SetBotsHealth()
    {
        var botTypes = DatabaseTables.Bots?.Types;
        if (botTypes is null)
        {
            return;
        }

        var botsConfig = GetOrCreateBotsConfig();
        if (botsConfig is null)
        {
            return;
        }

        // Give every bot the config does not know about an entry of its own, so the multipliers
        // are only ever read once per bot type - the entry is what drives the values from here on.
        var seededBots = SeedMissingBots(botsConfig, botTypes);

        foreach (var (botKey, botNode) in botsConfig)
        {
            if (botNode is not JsonObject botConfig)
            {
                continue;
            }

            if (!TryGetBotType(botTypes, botKey, out var bot))
            {
                Constants.GetLogger().Warning($"{Constants.ModTitle}: No bot type named {botKey} in the database, skipping its health config.");
                continue;
            }

            var health = bot?.BotHealth;
            if (health is null)
            {
                continue;
            }

            SetTypeHealthConfig(health, botConfig);
        }

        if (seededBots.Count == 0)
        {
            return;
        }

        Constants.GetLogger().Info(
            $"{Constants.ModTitle}: Added {seededBots.Count} new bot types to HealthConfig: {string.Join(", ", seededBots)}");

        if (GetConfigBool("autoAddNewBots", true))
        {
            SaveConfig();
        }
    }

    private JsonObject? GetOrCreateBotsConfig()
    {
        var botsConfig = GetConfigObject("bots");
        if (botsConfig is not null)
        {
            return botsConfig;
        }

        if (Config is not JsonObject config)
        {
            return null;
        }

        botsConfig = new JsonObject();
        config["bots"] = botsConfig;

        return botsConfig;
    }

    // Scales a bot's current health by the multiplier for its category and stores the result as a
    // normal config entry. The multiplier is only used to work out these numbers - it is never
    // applied to the database itself.
    private List<string> SeedMissingBots(JsonObject botsConfig, Dictionary<string, BotType?> botTypes)
    {
        var bossMultiplier = GetConfigNumber("bossMultiplier", 1);
        var followerMultiplier = GetConfigNumber("followerMultiplier", 1);
        var commonMultiplier = GetConfigNumber("commonMultiplier", 1);

        var configuredBots = new HashSet<string>(
            botsConfig.Select(entry => entry.Key),
            StringComparer.OrdinalIgnoreCase);

        var seededBots = new List<string>();

        foreach (var (botKey, bot) in botTypes)
        {
            if (string.IsNullOrWhiteSpace(botKey) || configuredBots.Contains(botKey))
            {
                continue;
            }

            var bodyPart = bot?.BotHealth?.BodyParts?.FirstOrDefault();
            if (bodyPart is null)
            {
                continue;
            }

            var multiplier = botKey.Contains("boss", StringComparison.OrdinalIgnoreCase)
                ? bossMultiplier
                : botKey.Contains("follower", StringComparison.OrdinalIgnoreCase)
                    ? followerMultiplier
                    : commonMultiplier;

            var entry = BuildBotHealthEntry(bodyPart, multiplier);
            if (entry is null)
            {
                continue;
            }

            botsConfig[botKey] = entry;
            seededBots.Add(botKey);
        }

        return seededBots;
    }

    private static JsonObject? BuildBotHealthEntry(BodyPart bodyPart, double multiplier)
    {
        var head = ScaleBotPart(bodyPart, "Head", multiplier);
        var chest = ScaleBotPart(bodyPart, "Chest", multiplier);
        var stomach = ScaleBotPart(bodyPart, "Stomach", multiplier);
        var arm = ScaleBotPart(bodyPart, "LeftArm", multiplier);
        var leg = ScaleBotPart(bodyPart, "LeftLeg", multiplier);

        if (head is null || chest is null || stomach is null || arm is null || leg is null)
        {
            return null;
        }

        return new JsonObject
        {
            ["head"] = head.Value,
            ["chest"] = chest.Value,
            ["stomach"] = stomach.Value,
            ["arm"] = arm.Value,
            ["leg"] = leg.Value
        };
    }

    private static double? ScaleBotPart(BodyPart bodyPart, string partName, double multiplier)
    {
        var part = GetBotPart(bodyPart, partName);

        return part is null ? null : Math.Round(part.Max * multiplier);
    }

    private static void SetTypeHealthConfig(BotTypeHealth health, JsonObject config)
    {
        if (health.BodyParts is null)
        {
            return;
        }

        foreach (var part in health.BodyParts)
        {
            if (part is null)
            {
                continue;
            }

            SetBotPart(part, "Head", config["head"]);
            SetBotPart(part, "Chest", config["chest"]);
            SetBotPart(part, "Stomach", config["stomach"]);
            SetBotPart(part, "LeftArm", config["arm"]);
            SetBotPart(part, "RightArm", config["arm"]);
            SetBotPart(part, "LeftLeg", config["leg"]);
            SetBotPart(part, "RightLeg", config["leg"]);
        }
    }

    private static void SetBotPart(BodyPart bodyPart, string partName, JsonNode? valueNode)
    {
        var value = GetNumberValue(valueNode);
        if (value is null)
        {
            return;
        }

        var part = GetBotPart(bodyPart, partName);
        if (part is null)
        {
            return;
        }

        part.Max = value.Value;
        part.Min = value.Value;
    }

    private static MinMax<double>? GetBotPart(BodyPart bodyPart, string partName)
    {
        return partName switch
        {
            "Head" => bodyPart.Head,
            "Chest" => bodyPart.Chest,
            "Stomach" => bodyPart.Stomach,
            "LeftArm" => bodyPart.LeftArm,
            "RightArm" => bodyPart.RightArm,
            "LeftLeg" => bodyPart.LeftLeg,
            "RightLeg" => bodyPart.RightLeg,
            _ => null
        };
    }

    private static void SetPlayerPart(Dictionary<string, BodyPartHealth> bodyParts, string partName, JsonNode? valueNode)
    {
        var value = GetNumberValue(valueNode);
        if (value is null)
        {
            return;
        }

        if (!TryGetBodyPart(bodyParts, partName, out var part))
        {
            return;
        }

        var health = part.Health;
        if (health is null)
        {
            return;
        }

        health.Maximum = value.Value;
        health.Current = value.Value;
    }

    private static bool TryGetBodyPart(Dictionary<string, BodyPartHealth> bodyParts, string partName, out BodyPartHealth part)
    {
        if (bodyParts.TryGetValue(partName, out part!))
        {
            return true;
        }

        foreach (var (key, value) in bodyParts)
        {
            if (string.Equals(key, partName, StringComparison.OrdinalIgnoreCase))
            {
                part = value;
                return true;
            }
        }

        part = null!;
        return false;
    }

    private static bool TryGetBotType(Dictionary<string, BotType?> botTypes, string key, out BotType? bot)
    {
        if (botTypes.TryGetValue(key, out var direct))
        {
            bot = direct;
            return true;
        }

        foreach (var (botKey, botValue) in botTypes)
        {
            if (string.Equals(botKey, key, StringComparison.OrdinalIgnoreCase))
            {
                bot = botValue;
                return true;
            }
        }

        bot = null;
        return false;
    }
}
