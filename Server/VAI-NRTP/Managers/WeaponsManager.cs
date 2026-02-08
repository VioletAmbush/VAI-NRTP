using SPTarkov.DI.Annotations;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class WeaponsManager : AbstractModManager
{
    protected override string ConfigName => "WeaponsConfig";

    protected override void AfterPostDb()
    {
        Helper.IterateConfigItems(DatabaseTables, Config);

        Constants.GetLogger().Info($"{Constants.ModTitle}: Weapons changes applied!");
    }
}
