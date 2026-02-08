using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace VAI.NRTP;

[Injectable(InjectionType.Singleton)]
public sealed class ModContext
{
    public static ModContext Current { get; private set; } = null!;

    public ModHelper ModHelper { get; }
    public ProfileHelper ProfileHelper { get; }
    public DatabaseServer DatabaseServer { get; }
    public JsonUtil JsonUtil { get; }
    public HashUtil HashUtil { get; }
    public RandomUtil RandomUtil { get; }
    public ISptLogger<ModContext> Logger { get; }

    public string ModPath { get; }
    public string ConfigPath { get; }

    public ModContext(
        ModHelper modHelper,
        DatabaseServer databaseServer,
        JsonUtil jsonUtil,
        HashUtil hashUtil,
        RandomUtil randomUtil,
        ISptLogger<ModContext> logger,
        ProfileHelper profileHelper)
    {
        ModHelper = modHelper;
        ProfileHelper = profileHelper;
        DatabaseServer = databaseServer;
        JsonUtil = jsonUtil;
        HashUtil = hashUtil;
        RandomUtil = randomUtil;
        Logger = logger;

        ModPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        ConfigPath = Path.Combine(ModPath, "config");

        Current = this;
    }
}
