using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace TarkovRPG
{
    public class ConfigRepository
    {
        internal enum Section
        {
            DamageSettings,
            ArmorSettings,
            ColorSettings,
        }

        internal enum DamageStatKey
        {
            Default,
            BoltAction,
            Marksman,
            Pistol,
            Revolver,
            Shotgun,
            Mass,
        }

        internal enum ArmorStatKey
        {
            Unarmored,
            Class1,
            Class2,
            Class3,
            Class4,
            Class5,
            Class6,
            Default
        }

        private const string KeyTemplate = "{0}-{1}";

        private ConfigFile Config { get; set; }

        internal Dictionary<Section, bool> SectionValues { get; private set; } = new Dictionary<Section, bool>();
        internal Dictionary<string, float> Values { get; private set; } = new Dictionary<string, float>();

        internal bool this[Section key]
        {
            get => SectionValues[key];
        }

        internal float this[DamageStatKey key]
        {
            get => Values[GetKey(Section.DamageSettings, key)];
        }

        internal float this[ArmorStatKey key] 
        {
            get => Values[GetKey(Section.ArmorSettings, key)];
        }

        public ConfigRepository(ConfigFile configFile)
        {
            Config = configFile;

            BindConfig(Section.DamageSettings, true, "Damage changes enabled");

            BindConfig(Section.DamageSettings, DamageStatKey.Default, 1f, "Default shot damage multiplier");
            BindConfig(Section.DamageSettings, DamageStatKey.BoltAction, 4f, "Bolt-action sniper rifle damage multiplier");
            BindConfig(Section.DamageSettings, DamageStatKey.Marksman, 2f, "Marksman rifle damage multiplier");
            BindConfig(Section.DamageSettings, DamageStatKey.Pistol, 1.8f, "Pistol damage multiplier");
            BindConfig(Section.DamageSettings, DamageStatKey.Revolver, 3f, "Revolver damage multiplier");
            BindConfig(Section.DamageSettings, DamageStatKey.Shotgun, 1f, "Shotgun damage multiplier");
            BindConfig(Section.DamageSettings, DamageStatKey.Mass, 0.2f, "Body-Spread damage multiplier");

            BindConfig(Section.ArmorSettings, true, "Armor changes enabled");

            BindConfig(Section.ArmorSettings, ArmorStatKey.Unarmored, 1f, "Unarmored damage multipler");
            BindConfig(Section.ArmorSettings, ArmorStatKey.Class1, 0.75f, "Class 1 armor damage multipler");
            BindConfig(Section.ArmorSettings, ArmorStatKey.Class2, 0.66f, "Class 2 armor damage multipler");
            BindConfig(Section.ArmorSettings, ArmorStatKey.Class3, 0.5f, "Class 3 armor damage multipler");
            BindConfig(Section.ArmorSettings, ArmorStatKey.Class4, 0.3f, "Class 4 armor damage multipler");
            BindConfig(Section.ArmorSettings, ArmorStatKey.Class5, 0.14f, "Class 5 armor damage multipler");
            BindConfig(Section.ArmorSettings, ArmorStatKey.Class6, -0.2f, "Class 6 armor damage multipler");
            BindConfig(Section.ArmorSettings, ArmorStatKey.Default, 0.05f, "Any other armor damage multipler");

            BindConfig(Section.ColorSettings, true, "Item background color changes enabled (Restart may be required)");
        }

        public void UpdateValue(object sender, SettingChangedEventArgs args)
        {
            if (args == null)
                return;

            if (args.ChangedSetting.BoxedValue is float floatVal)
            {
                Values[GetKey(args.ChangedSetting.Definition.Section, args.ChangedSetting.Definition.Key)] = floatVal;
            }

            if (args.ChangedSetting.BoxedValue is bool boolVal)
            {
                var section = (Section)Enum.Parse(typeof(Section), args.ChangedSetting.Definition.Key);

                SectionValues[section] = boolVal;
            }                
        }

        private void BindConfig<TStat>(Section section, TStat key, float def, string description) where TStat : Enum
        {
            var entry = Config.Bind(
                new ConfigDefinition(section.ToSectionString(), key.ToString()),
                def,
                new ConfigDescription(description));

            Values[GetKey(section, key)] = entry.Value;
        }

        private void BindConfig(Section section, bool def, string description)
        {
            var entry = Config.Bind(
                new ConfigDefinition(section.ToSectionString(), section.ToString()),
                def,
                new ConfigDescription(description));

            SectionValues[section] = entry.Value;
        }

        private string GetKey<TStat>(Section section, TStat stat) where TStat : Enum
        {
            return string.Format(KeyTemplate, section.ToSectionString(), stat.ToString());
        }

        private string GetKey(string section, string key)
        {
            return string.Format(KeyTemplate, section, key);
        }
    } 
}
