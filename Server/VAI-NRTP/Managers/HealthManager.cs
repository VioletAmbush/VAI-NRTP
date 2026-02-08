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
        SetBotsHealth();
        SetPlayerHealth();

        Constants.GetLogger().Info($"{Constants.ModTitle}: Health changes applied!");
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
        var botsConfig = GetConfigObject("bots");
        var bossMultiplier = GetConfigNumber("bossMultiplier", 1);
        var followerMultiplier = GetConfigNumber("followerMultiplier", 1);
        var commonMultiplier = GetConfigNumber("commonMultiplier", 1);

        var botTypes = DatabaseTables.Bots?.Types;
        if (botTypes is null)
        {
            return;
        }

        var configuredBots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (botsConfig != null)
        {
            foreach (var (botKey, botNode) in botsConfig)
            {
                if (botNode is not JsonObject botConfig)
                {
                    continue;
                }

                if (!TryGetBotType(botTypes, botKey, out var bot))
                {
                    continue;
                }

                var health = bot?.BotHealth;
                if (health is null)
                {
                    continue;
                }

                SetTypeHealthConfig(health, botConfig);
                configuredBots.Add(botKey);
            }
        }

        foreach (var (botKey, bot) in botTypes)
        {
            if (string.IsNullOrWhiteSpace(botKey) || configuredBots.Contains(botKey))
            {
                continue;
            }

            var health = bot?.BotHealth;
            if (health is null)
            {
                continue;
            }

            if (botKey.Contains("boss", StringComparison.OrdinalIgnoreCase))
            {
                SetTypeHealthMult(health, bossMultiplier);
            }
            else if (botKey.Contains("follower", StringComparison.OrdinalIgnoreCase))
            {
                SetTypeHealthMult(health, followerMultiplier);
            }
            else
            {
                SetTypeHealthMult(health, commonMultiplier);
            }
        }
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

    private static void SetTypeHealthMult(BotTypeHealth health, double multiplier)
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

            MultiplyBotPart(part, "Head", multiplier);
            MultiplyBotPart(part, "Chest", multiplier);
            MultiplyBotPart(part, "Stomach", multiplier);
            MultiplyBotPart(part, "LeftArm", multiplier);
            MultiplyBotPart(part, "RightArm", multiplier);
            MultiplyBotPart(part, "LeftLeg", multiplier);
            MultiplyBotPart(part, "RightLeg", multiplier);
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

    private static void MultiplyBotPart(BodyPart bodyPart, string partName, double multiplier)
    {
        var part = GetBotPart(bodyPart, partName);
        if (part is null)
        {
            return;
        }

        part.Max *= multiplier;
        part.Min *= multiplier;
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
