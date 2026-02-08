using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class AmmoManager : AbstractModManager
{
    protected override string ConfigName => "AmmoConfig";

    protected override void AfterPostDb()
    {
        var multiplier = GetConfigNumber("ammoStackMultiplier", 1);

        foreach (var item in DatabaseTables.Templates.Items.Values)
        {
            var props = item.Properties;
            if (props is null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(props.AmmoType))
            {
                continue;
            }

            var stackSize = props.StackMaxSize;
            if (stackSize is null)
            {
                continue;
            }

            var newValue = (int)Math.Round(stackSize.Value * multiplier);
            if (newValue < 1)
            {
                newValue = 1;
            }
            props.StackMaxSize = newValue;
        }

        Helper.IterateConfigItems(DatabaseTables, Config);

        Constants.GetLogger().Info($"{Constants.ModTitle}: Ammo changes applied!");
    }
}
