using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace VAI.NRTP.Managers;

[Injectable(InjectionType.Singleton)]
public sealed class RecoilManager : AbstractModManager
{
    protected override string ConfigName => "RecoilConfig";

    protected override void AfterPostDb()
    {
        foreach (var item in DatabaseTables.Templates.Items.Values)
        {
            var props = item.Properties;
            if (props is null)
            {
                continue;
            }

            var recoilBack = props.RecoilForceBack;
            var recoilUp = props.RecoilForceUp;
            var recoilCamera = props.RecoilCamera;

            if (recoilBack is null || recoilUp is null || recoilCamera is null)
            {
                continue;
            }

            var fireTypes = props.WeapFireType;
            if (fireTypes is null || fireTypes.Count == 0)
            {
                continue;
            }

            var boltAction = props.BoltAction ?? false;

            if (fireTypes.Any(t => t.Contains("fullauto", StringComparison.OrdinalIgnoreCase)))
            {
                SetGunRecoil(props, GetConfigNumber("recoilAutoMultiplier", 1));
            }
            else if (fireTypes.Any(t => t.Contains("single", StringComparison.OrdinalIgnoreCase)) && !boltAction)
            {
                SetGunRecoil(props, GetConfigNumber("recoilSingleMultiplier", 1));
            }
        }

        Constants.GetLogger().Info($"{Constants.ModTitle}: Recoil changes applied!");
    }

    private static void SetGunRecoil(TemplateItemProperties props, double recoilMultiplier)
    {
        if (props.RecoilForceBack is double recoilBack)
        {
            props.RecoilForceBack = recoilBack * recoilMultiplier;
        }

        if (props.RecoilForceUp is double recoilUp)
        {
            props.RecoilForceUp = recoilUp * recoilMultiplier;
        }

        if (props.RecoilCamera is double recoilCamera)
        {
            props.RecoilCamera = recoilCamera * recoilMultiplier;
        }
    }
}
