using SPTarkov.DI.Annotations;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class ArmorManager : AbstractModManager
{
    protected override string ConfigName => "ArmorConfig";

    protected override void AfterPostDb()
    {
        Helper.IterateConfigItems(DatabaseTables, Config);

        Constants.GetLogger().Info($"{Constants.ModTitle}: Armor changes applied!");
    }
}
