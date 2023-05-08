using System.Reflection;
using static TarkovRPG.ConfigRepository;

namespace TarkovRPG
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

        public static void CallPrivateMethod<TInstance>(this TInstance instance, string method, params object[] args) where TInstance : class
        {
            instance.GetType().GetMethod(
                method,
                BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(instance, args);
        }

        public static TResult CallPrivateMethod<TInstance, TResult>(this TInstance instance, string method, params object[] args) where TInstance : class
        {
            return (TResult)instance.GetType().GetMethod(
                method,
                BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(instance, args);
        }

        public static MethodInfo GetPrivateMethod(this object instance, string methodName) =>
            instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

        public static T GetPrivatePropertyValue<T>(this object instance, string propertyName) =>
            (T)instance.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);

        public static T GetPrivateFieldValue<T>(this object instance, string fieldName) =>
            (T)instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
    }
}
