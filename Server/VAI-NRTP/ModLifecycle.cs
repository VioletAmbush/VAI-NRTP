using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;

namespace VAI.NRTP;

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader + 1)]
public class PreSptEntry(ModManager manager) : IOnLoad
{
    public Task OnLoad()
    {
        manager.PreSptLoad();
        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class PostDbEntry(ModManager manager) : IOnLoad
{
    public Task OnLoad()
    {
        manager.PostDbLoad();
        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1)]
public class PostSptEntry(ModManager manager) : IOnLoad
{
    public Task OnLoad()
    {
        manager.PostSptLoad();
        return Task.CompletedTask;
    }
}

// IOnLoad hooks run in TypePriority order across every mod, not per mod, and mods are free to
// pick any priority they like - MoreBotsAPI registers its custom bot types at
// PostDBModLoader + 80085. A priority this far out is last by construction rather than by
// out-guessing other mods, while still running before the web server accepts requests.
[Injectable(TypePriority = int.MaxValue / 2)]
public class FinalEntry(ModManager manager) : IOnLoad
{
    public Task OnLoad()
    {
        manager.FinalLoad();
        return Task.CompletedTask;
    }
}
