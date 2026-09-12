using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;

namespace VAI.NRTP;

[Injectable(TypePriority = OnLoadOrder.Preload)]
public class PreSptEntry(ModManager manager) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        manager.PreSptLoad();
        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class PostDbEntry(ModManager manager) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        manager.PostDbLoad();
        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 2)]
public class PostSptEntry(ModManager manager) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        manager.PostSptLoad();
        return Task.CompletedTask;
    }
}

// IOnLoad hooks run in TypePriority order across every mod, not per mod, and mods are free to
// pick any priority they like. A priority this far out (well past OnLoadOrder.PostLoad) is last
// by construction rather than by out-guessing other mods, while still running before the web
// server accepts requests.
[Injectable(TypePriority = int.MaxValue / 2)]
public class FinalEntry(ModManager manager) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        manager.FinalLoad();
        return Task.CompletedTask;
    }
}
