using static TapkovRPG.ConfigRepository;

namespace TapkovRPG
{
    internal static class Extensions
    {
        public static string ToSectionString(this Section section)
        {
            switch (section)
            {
                case Section.DamageSettings: return "Damage settings";
                case Section.ArmorSettings: return "Armor settings";
                case Section.ColorSettings: return "Color settings";
                default: return "Unknown section";
            }
        }
    }
}
