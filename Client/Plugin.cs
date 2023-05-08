using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TarkovRPG
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public ConfigRepository? ConfigRepository { get; set; }

        public static Plugin? Instance { get; set; }

        public new ManualLogSource Logger => base.Logger;

        private void Awake()
        {
            Instance = this;
            ConfigRepository = new ConfigRepository(Config);

            Harmony.CreateAndPatchAll(typeof(Patches));
        }

        private void OnEnable()
        {
            Config.SettingChanged += ConfigRepository!.UpdateValue;
        }

        private void OnDisable()
        {
            Config.SettingChanged -= ConfigRepository!.UpdateValue;
        }

        private void OnDestroy()
        {
            Instance = null;
            Config.SettingChanged -= ConfigRepository!.UpdateValue;
            ConfigRepository = null;
        }
    }
}
