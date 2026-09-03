using InnerNet;

namespace SuperNewRoles.Safety;

public static class SafetyReportCategories
{
    public static readonly string[] Ids =
    {
        "inappropriate_name",
        "inappropriate_chat",
        "cheating",
        "harassment",
        "other"
    };

    public static string TranslationKey(string id) => "SafetyReportReason." + id;

    public static string FromVanilla(ReportReasons reason)
    {
        return reason switch
        {
            ReportReasons.InappropriateName => "inappropriate_name",
            ReportReasons.InappropriateChat => "inappropriate_chat",
            ReportReasons.Cheating_Hacking => "cheating",
            ReportReasons.Harassment_Misconduct => "harassment",
            _ => "other"
        };
    }

    public static string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "other";
        string value = raw.Trim().ToLowerInvariant().Replace('-', '_');
        foreach (string id in Ids)
        {
            if (id == value) return id;
        }
        return value switch
        {
            "inappropriatename" or "inappropriate name" => "inappropriate_name",
            "inappropriatechat" or "inappropriate chat" => "inappropriate_chat",
            "cheating_hacking" or "hacking" => "cheating",
            "harassment_misconduct" or "misconduct" => "harassment",
            _ => "other"
        };
    }
}
