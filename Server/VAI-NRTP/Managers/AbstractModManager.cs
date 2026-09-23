using System.Text.Json.Nodes;
using SPTarkov.Server.Core.Utils;

namespace VAI.NRTP.Managers;

public abstract class AbstractModManager
{
    protected abstract string ConfigName { get; }
    protected JsonNode? Config;

    protected bool PreSptInitialized;
    protected bool PostDbInitialized;
    protected bool PostSptInitialized;

    protected ModContext DatabaseTables = null!;
    protected JsonUtil JsonUtil = null!;

    public virtual int Priority => 1;

    public void PreSptLoad()
    {
        EnsurePreSptInitialized();

        if (!IsEnabled())
        {
            return;
        }

        AfterPreSpt();
    }

    public void PostDbLoad()
    {
        EnsurePreSptInitialized();

        if (!IsEnabled())
        {
            return;
        }

        EnsurePostDbInitialized();
        AfterPostDb();
    }

    public void PostSptLoad()
    {
        EnsurePreSptInitialized();

        if (!IsEnabled())
        {
            return;
        }

        EnsurePostDbInitialized();
        EnsurePostSptInitialized();
        AfterPostSpt();
    }

    public void FinalLoad()
    {
        EnsurePreSptInitialized();

        if (!IsEnabled())
        {
            return;
        }

        EnsurePostDbInitialized();
        EnsurePostSptInitialized();
        AfterFinal();
    }

    protected virtual void PreSptInitialize()
    {
        Config = LoadConfig(ConfigName);
        PreSptInitialized = true;
    }

    protected virtual void PostDbInitialize()
    {
        JsonUtil = ModContext.Current.JsonUtil;
        DatabaseTables = ModContext.Current;
        PostDbInitialized = true;
    }

    protected virtual void PostSptInitialize()
    {
        PostSptInitialized = true;
    }

    protected virtual void AfterPreSpt()
    {
    }

    protected virtual void AfterPostDb()
    {
    }

    protected virtual void AfterPostSpt()
    {
    }

    protected virtual void AfterFinal()
    {
    }

    protected bool GetConfigBool(string key, bool defaultValue = false)
    {
        if (Config is not JsonObject obj)
        {
            return defaultValue;
        }

        if (!obj.TryGetPropertyValue(key, out var node) || node is null)
        {
            return defaultValue;
        }

        return node is JsonValue value && value.TryGetValue<bool>(out var result) ? result : defaultValue;
    }

    protected double GetConfigNumber(string key, double defaultValue = 0)
    {
        if (Config is not JsonObject obj)
        {
            return defaultValue;
        }

        if (!obj.TryGetPropertyValue(key, out var node) || node is null)
        {
            return defaultValue;
        }

        var value = GetNumberValue(node);
        return value ?? defaultValue;
    }

    protected JsonArray? GetConfigArray(string key)
    {
        return Config is JsonObject obj && obj.TryGetPropertyValue(key, out var node)
            ? node as JsonArray
            : null;
    }

    protected JsonObject? GetConfigObject(string key)
    {
        return Config is JsonObject obj && obj.TryGetPropertyValue(key, out var node)
            ? node as JsonObject
            : null;
    }

    protected bool IsEnabled()
    {
        if (Config is null)
        {
            return false;
        }

        var enabledNode = Config["enabled"];
        return enabledNode is JsonValue value && value.TryGetValue<bool>(out var enabled) && enabled;
    }

    protected JsonNode? LoadConfig(string configName)
    {
        var path = Path.Combine(ModContext.Current.ConfigPath, $"{configName}.json");

        if (!File.Exists(path))
        {
            Constants.GetLogger().Warning($"{Constants.ModTitle}: Missing config {configName}.json");
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonNode.Parse(json);
    }

    protected static double? GetNumberValue(JsonNode? node)
    {
        if (node is JsonValue value)
        {
            if (value.TryGetValue<double>(out var doubleVal))
            {
                return doubleVal;
            }

            if (value.TryGetValue<int>(out var intVal))
            {
                return intVal;
            }

            if (value.TryGetValue<long>(out var longVal))
            {
                return longVal;
            }
        }

        return null;
    }

    private void EnsurePreSptInitialized()
    {
        if (!PreSptInitialized)
        {
            PreSptInitialize();
        }
    }

    private void EnsurePostDbInitialized()
    {
        if (!PostDbInitialized)
        {
            PostDbInitialize();
        }
    }

    private void EnsurePostSptInitialized()
    {
        if (!PostSptInitialized)
        {
            PostSptInitialize();
        }
    }
}
