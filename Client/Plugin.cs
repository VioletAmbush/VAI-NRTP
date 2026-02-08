using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace TarkovRPG
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public ConfigRepository? ConfigRepository { get; set; }

        public static Plugin? Instance { get; set; }
        private EventHandler<SettingChangedEventArgs>? _configSettingChangedHandler;

        public new ManualLogSource Logger => base.Logger;

        private void Awake()
        {
            Instance = this;
            ConfigRepository = new ConfigRepository(Config);

            Harmony.CreateAndPatchAll(typeof(Patches));

            Logger.LogInfo($"[{PluginInfo.PLUGIN_NAME}] loaded!");
        }

        private void OnEnable()
        {
            if (ConfigRepository is null)
            {
                return;
            }

            _configSettingChangedHandler ??= (_, args) => ConfigRepository.UpdateValue(args);
            Config.SettingChanged += _configSettingChangedHandler;
        }

        private void OnDisable()
        {
            if (_configSettingChangedHandler is null)
            {
                return;
            }

            Config.SettingChanged -= _configSettingChangedHandler;
        }

        private void OnDestroy()
        {
            Instance = null;

            if (_configSettingChangedHandler is not null)
            {
                Config.SettingChanged -= _configSettingChangedHandler;
            }

            _configSettingChangedHandler = null;
            ConfigRepository = null;
        }
    }
}
