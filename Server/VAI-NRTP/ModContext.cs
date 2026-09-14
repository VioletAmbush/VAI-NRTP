using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils;

namespace VAI.NRTP;

[Injectable(InjectionType.Singleton)]
public sealed class ModContext
{
    public static ModContext Current { get; private set; } = null!;

    public ModHelper ModHelper { get; }
    public ProfileHelper ProfileHelper { get; }
    public BotTable Bots { get; }
    public GlobalTable Globals { get; }
    public HideoutTable Hideout { get; }
    public LocationTable Locations { get; }
    public TemplateTable Templates { get; }
    public JsonUtil JsonUtil { get; }
    public HashUtil HashUtil { get; }
    public RandomUtil RandomUtil { get; }
    public ISptLogger<ModContext> Logger { get; }

    public string ModPath { get; }
    public string ConfigPath { get; }

    public ModContext(
        ModHelper modHelper,
        BotTable bots,
        GlobalTable globals,
        HideoutTable hideout,
        LocationTable locations,
        TemplateTable templates,
        JsonUtil jsonUtil,
        HashUtil hashUtil,
        RandomUtil randomUtil,
        ISptLogger<ModContext> logger,
        ProfileHelper profileHelper)
    {
        ModHelper = modHelper;
        ProfileHelper = profileHelper;
        Bots = bots;
        Globals = globals;
        Hideout = hideout;
        Locations = locations;
        Templates = templates;
        JsonUtil = jsonUtil;
        HashUtil = hashUtil;
        RandomUtil = randomUtil;
        Logger = logger;

        ModPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        ConfigPath = Path.Combine(ModPath, "config");

        Current = this;
    }
}
