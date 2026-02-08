using System.Text.Json.Nodes;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Server;

namespace VAI.NRTP;

public static class Helper
{
    public static void SetItemBuffs(DatabaseTables databaseTables, TemplateItem item, JsonNode? buffsConfig)
    {
        if (buffsConfig is null)
        {
            return;
        }

        var itemId = item.Id.ToString();
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Constants.GetLogger().Warning($"{Constants.ModTitle}: Item _id missing while setting stimulator buffs.");
            return;
        }

        var buffName = $"Buff{itemId}";
        var buffs = ConfigMapper.MapBuffs(buffsConfig);

        var buffsTable = databaseTables.Globals?.Configuration?.Health?.Effects?.Stimulator?.Buffs;
        if (buffsTable is null)
        {
            Constants.GetLogger().Warning($"{Constants.ModTitle}: Stimulator buffs table not found while setting item buffs.");
            return;
        }

        buffsTable[buffName] = buffs;

        var props = item.Properties;
        if (props is null)
        {
            Constants.GetLogger().Warning($"{Constants.ModTitle}: Item properties missing while setting stimulator buffs.");
            return;
        }

        props.StimulatorBuffs = buffName;
    }

    public static void TrySetItemRarity(TemplateItem item, JsonObject? itemConfig)
    {
        var rarityNode = itemConfig?["rarity"];
        string? rarity = null;
        if (rarityNode is JsonValue value)
        {
            if (value.TryGetValue<string>(out var stringValue))
            {
                rarity = stringValue;
            }
            else if (value.TryGetValue<int>(out var intValue))
            {
                rarity = intValue.ToString();
            }
            else if (value.TryGetValue<double>(out var doubleValue))
            {
                rarity = ((int)doubleValue).ToString();
            }
        }

        if (string.IsNullOrWhiteSpace(rarity))
        {
            return;
        }

        if (item.Properties is not null)
        {
            item.Properties.BackgroundColor = ConfigMapper.MapRarity(rarity);
        }
    }

    public static void IterateConfigItems(
        DatabaseTables databaseTables,
        JsonNode? config,
        Action<TemplateItem, JsonObject, DatabaseTables, JsonNode?>? action = null,
        bool trySetRarity = true)
    {
        if (config is not JsonObject obj)
        {
            return;
        }

        if (!obj.TryGetPropertyValue("items", out var itemsNode) || itemsNode is null)
        {
            return;
        }

        var items = databaseTables.Templates.Items;

        IEnumerable<JsonObject> itemConfigs = Enumerable.Empty<JsonObject>();
        if (itemsNode is JsonArray itemsArray)
        {
            itemConfigs = itemsArray.OfType<JsonObject>();
        }
        else if (itemsNode is JsonObject itemsObject)
        {
            itemConfigs = itemsObject.Select(kvp => kvp.Value).OfType<JsonObject>();
        }

        foreach (var itemConfig in itemConfigs)
        {
            var id = itemConfig["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            if (!MongoId.IsValidMongoId(id))
            {
                continue;
            }

            if (!items.TryGetValue(new MongoId(id), out var item))
            {
                Constants.GetLogger().Error($"{Constants.ModTitle}: Item with id {id} not found!");
                continue;
            }

            if (trySetRarity)
            {
                TrySetItemRarity(item, itemConfig);
            }

            action?.Invoke(item, itemConfig, databaseTables, config);
        }
    }
}
