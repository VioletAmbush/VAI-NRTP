using System.Diagnostics;
using SPTarkov.DI.Annotations;
using VAI.NRTP.Managers;

namespace VAI.NRTP;

[Injectable(InjectionType.Singleton)]
public sealed class ModManager
{
    private readonly List<AbstractModManager> _managers = [];

    public ModManager(
        AmmoManager ammoManager,
        GlobalsManager globalsManager,
        HealingManager healingManager,
        HealthManager healthManager,
        MagazinesManager magazinesManager,
        MeleeManager meleeManager,
        RecoilManager recoilManager,
        FoodManager foodManager,
        StimulatorsManager stimulatorsManager,
        ArmorManager armorManager,
        WeaponsManager weaponsManager,
        EffectsManager effectsManager,
        ModContext _)
    {
        _managers.Add(ammoManager);
        _managers.Add(globalsManager);
        _managers.Add(healingManager);
        _managers.Add(healthManager);
        _managers.Add(magazinesManager);
        _managers.Add(meleeManager);
        _managers.Add(recoilManager);
        _managers.Add(foodManager);
        _managers.Add(stimulatorsManager);
        _managers.Add(armorManager);
        _managers.Add(weaponsManager);
        _managers.Add(effectsManager);

        _managers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public void PreSptLoad()
    {
        foreach (var manager in _managers)
        {
            manager.PreSptLoad();
        }
    }

    public void PostDbLoad()
    {
        foreach (var manager in _managers)
        {
#if DEBUG
            var managerName = manager.GetType().Name;
            Constants.GetLogger().Info($"{Constants.ModTitle}: PostDB start {managerName}");
            var sw = Stopwatch.StartNew();
#endif
            manager.PostDbLoad();
#if DEBUG
            sw.Stop();
            Constants.GetLogger().Info($"{Constants.ModTitle}: PostDB end {managerName} ({sw.ElapsedMilliseconds}ms)");
#endif
        }
    }

    public void PostSptLoad()
    {
        foreach (var manager in _managers)
        {
            manager.PostSptLoad();
        }
    }

    public void FinalLoad()
    {
        foreach (var manager in _managers)
        {
            manager.FinalLoad();
        }
    }
}
