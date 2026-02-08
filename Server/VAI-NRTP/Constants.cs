using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace VAI.NRTP;

public static class Constants
{
    public static string ModTitle = "VAI-NRTP";

    public static DatabaseTables GetDatabaseTables() => ModContext.Current.DatabaseServer.GetTables();

    public static JsonUtil GetJsonUtil() => ModContext.Current.JsonUtil;

    public static HashUtil GetHashUtil() => ModContext.Current.HashUtil;

    public static RandomUtil GetRandomUtil() => ModContext.Current.RandomUtil;

    public static ISptLogger<ModContext> GetLogger() => ModContext.Current.Logger;
}
