using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Servers;
using SptLocations = SPTarkov.Server.Core.Models.Spt.Server.Locations;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class GlobalsManager : AbstractModManager
{
    private static readonly HashSet<string> IgnoredLocations = new(StringComparer.OrdinalIgnoreCase)
    {
        "hideout",
        "develop",
        "privatearea",
        "suburbs",
        "town",
        "terminal"
    };

    private readonly LocationConfig _locationConfig;

    protected override string ConfigName => "GlobalsConfig";

    public GlobalsManager(ConfigServer configServer)
    {
        _locationConfig = configServer.GetConfig<LocationConfig>();
    }

    protected override void AfterPostDb()
    {
        var config = DatabaseTables.Globals?.Configuration;
        if (config != null)
        {
            if (GetConfigBool("removeSkillFatigue"))
            {
                config.SkillFatigueReset = 0;
            }

            if (GetConfigBool("removeInRaidItemRestrictions"))
            {
                config.RestrictionsInRaid = Array.Empty<RestrictionsInRaid>();
            }
        }

        SetEscapeTimeLimits();
        TrySetConstructionTime();
        TrySetExitsAlwaysAvailable();
        TrySetAllExitsAvailable();
        TrySetAllBossesAlwaysAvailable();
        TryRemoveRunThroughs();
    }

    protected override void AfterPostSpt()
    {
        TryDisableContainerRandomisation();
        SetLootMultipliers();

        Constants.GetLogger().Info($"{Constants.ModTitle}: Globals config changes applied!");
    }

    private void SetLootMultipliers()
    {
        var staticLoot = _locationConfig.StaticLootMultiplier;
        var looseLoot = _locationConfig.LooseLootMultiplier;
        if (staticLoot != null && looseLoot != null)
        {
            var staticMult = GetConfigNumber("staticLootMultiplier", 1);
            var looseMult = GetConfigNumber("looseLootMultiplier", 1);

            var keys = staticLoot.Keys.ToList();
            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(key) || IgnoredLocations.Contains(key))
                {
                    continue;
                }

                if (!staticLoot.TryGetValue(key, out var staticValue))
                {
                    continue;
                }

                if (!looseLoot.TryGetValue(key, out var looseValue))
                {
                    continue;
                }

                staticLoot[key] = staticValue * staticMult;
                looseLoot[key] = looseValue * looseMult;
            }
        }

        var botTypes = DatabaseTables.Bots?.Types;
        var botLootMultiplier = GetConfigNumber("botLootMultiplier", 1);
        if (botTypes is null)
        {
            return;
        }

        foreach (var bot in botTypes.Values)
        {
            var inventory = bot?.BotInventory;
            var itemPools = inventory?.Items;
            if (itemPools is null)
            {
                continue;
            }

            MultiplyItemPools(itemPools, botLootMultiplier);
        }
    }

    private void TryDisableContainerRandomisation()
    {
        if (!GetConfigBool("disableContainerRandomisation"))
        {
            return;
        }

        var settings = _locationConfig.ContainerRandomisationSettings;
        if (settings is null)
        {
            return;
        }

        settings.Enabled = false;

        if (settings.Maps is null)
        {
            return;
        }

        var keys = settings.Maps.Keys.ToList();
        foreach (var key in keys)
        {
            settings.Maps[key] = false;
        }
    }

    private void SetEscapeTimeLimits()
    {
        var raidTimeMultiplier = GetConfigNumber("raidTimeMultiplier", 1);

        foreach (var (locKey, location) in EnumerateLocations())
        {
            if (string.IsNullOrWhiteSpace(locKey) || IgnoredLocations.Contains(locKey))
            {
                continue;
            }

            var baseInfo = location.Base;
            if (baseInfo is null)
            {
                continue;
            }

            if (baseInfo.EscapeTimeLimit != null)
            {
                baseInfo.EscapeTimeLimit *= raidTimeMultiplier;
            }

            if (baseInfo.EscapeTimeLimitCoop != null)
            {
                baseInfo.EscapeTimeLimitCoop = (int)Math.Round(baseInfo.EscapeTimeLimitCoop.Value * raidTimeMultiplier);
            }
        }
    }

    private void TrySetConstructionTime()
    {
        if (!GetConfigBool("removeConstructionTime"))
        {
            return;
        }

        var hideout = DatabaseTables.Hideout;
        var areas = hideout?.Areas;
        if (areas is null)
        {
            return;
        }

        foreach (var area in areas)
        {
            if (area?.Stages is null)
            {
                continue;
            }

            foreach (var stage in area.Stages.Values)
            {
                if (stage?.ConstructionTime != null)
                {
                    stage.ConstructionTime = 0;
                }
            }
        }
    }

    private void TrySetExitsAlwaysAvailable()
    {
        if (!GetConfigBool("exitsAlwaysAvailable"))
        {
            return;
        }

        foreach (var (locKey, location) in EnumerateLocations())
        {
            if (string.IsNullOrWhiteSpace(locKey) || IgnoredLocations.Contains(locKey))
            {
                continue;
            }

            var extracts = location.AllExtracts;
            if (extracts is null)
            {
                continue;
            }

            foreach (var extract in extracts)
            {
                extract.Chance = 100;
                extract.ChancePVE = 100;
            }
        }
    }

    private void TrySetAllExitsAvailable()
    {
        if (!GetConfigBool("allExitsAvailable"))
        {
            return;
        }

        SetAllExitsAvailable(GetLocation("bigmap"), "Customs,Boiler Tanks");
        SetAllExitsAvailable(GetLocation("interchange"), "MallSE,MallNW");
        SetAllExitsAvailable(GetLocation("lighthouse"), "Tunnel,North");
        SetAllExitsAvailable(GetLocation("shoreline"), "Village,Riverside");
        SetAllExitsAvailable(GetLocation("tarkovstreets"), "E1_2,E2_3,E3_4,E4_5,E5_6,E6_1");
        SetAllExitsAvailable(GetLocation("woods"), "House,Old Station");
        SetAllExitsAvailable(GetLocation("sandbox"), "west,east");
        SetAllExitsAvailable(GetLocation("sandbox_high"), "west,east");
    }

    private void TrySetAllBossesAlwaysAvailable()
    {
        if (!GetConfigBool("allBossesAlwaysAvailable"))
        {
            return;
        }

        foreach (var (_, location) in EnumerateLocations())
        {
            var bosses = location.Base?.BossLocationSpawn;
            if (bosses is null)
            {
                continue;
            }

            foreach (var boss in bosses)
            {
                if (boss is null)
                {
                    continue;
                }

                boss.BossChance = 100;
                boss.ForceSpawn = true;
            }
        }
    }

    private static void SetAllExitsAvailable(Location? location, string entryPoints)
    {
        var exits = location?.Base?.Exits;
        if (exits is null)
        {
            return;
        }

        foreach (var exit in exits)
        {
            if (exit is null)
            {
                continue;
            }

            exit.EntryPoints = entryPoints;
        }
    }

    private void TryRemoveRunThroughs()
    {
        if (!GetConfigBool("removeRunThroughs"))
        {
            return;
        }

        var matchEnd = DatabaseTables.Globals?.Configuration?.Exp?.MatchEnd;
        if (matchEnd is null)
        {
            return;
        }

        matchEnd.SurvivedExperienceRequirement = 0;
        matchEnd.SurvivedSecondsRequirement = 0;
    }

    private IEnumerable<(string Key, Location Location)> EnumerateLocations()
    {
        var locations = DatabaseTables.Locations;
        if (locations is null)
        {
            yield break;
        }

        foreach (var entry in EnumerateLocations(locations))
        {
            yield return entry;
        }
    }

    private static IEnumerable<(string Key, Location Location)> EnumerateLocations(SptLocations locations)
    {
        var mapped = locations.GetDictionary();
        if (mapped is null)
        {
            yield break;
        }

        foreach (var (key, location) in mapped)
        {
            if (string.IsNullOrWhiteSpace(key) || location is null)
            {
                continue;
            }

            yield return (key, location);
        }
    }

    private static Location? GetLocation(string key)
    {
        var locations = Constants.GetDatabaseTables()?.Locations;
        if (locations is null)
        {
            return null;
        }

        var normalizedKey = NormalizeLocationKey(key);
        foreach (var (locationKey, location) in EnumerateLocations(locations))
        {
            if (NormalizeLocationKey(locationKey) == normalizedKey)
            {
                return location;
            }
        }

        return null;
    }

    private static string NormalizeLocationKey(string key)
    {
        return key.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    }

    private static void MultiplyItemPools(ItemPools itemPools, double multiplier)
    {
        MultiplyItemPool(itemPools.Backpack, multiplier);
        MultiplyItemPool(itemPools.Pockets, multiplier);
        MultiplyItemPool(itemPools.SpecialLoot, multiplier);
        MultiplyItemPool(itemPools.TacticalVest, multiplier);
    }

    private static void MultiplyItemPool(Dictionary<MongoId, double>? pool, double multiplier)
    {
        if (pool is null)
        {
            return;
        }

        var keys = pool.Keys.ToList();
        foreach (var key in keys)
        {
            pool[key] = pool[key] * multiplier;
        }
    }
}
