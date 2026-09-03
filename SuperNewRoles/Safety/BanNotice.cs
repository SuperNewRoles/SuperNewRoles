using System;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Safety;

public sealed class BanInfo
{
    public string Reason;
    public string ExpiresAtIso;
    public string InquiryId;

    public static BanInfo From(System.Collections.Generic.Dictionary<string, object> row)
    {
        return new BanInfo
        {
            Reason = Read(row, "reason"),
            ExpiresAtIso = Read(row, "expires_at", "expiresAt"),
            InquiryId = Read(row, "inquiry_id", "inquiryId"),
        };
    }

    private static string Read(System.Collections.Generic.Dictionary<string, object> row, params string[] keys)
    {
        foreach (string key in keys)
        {
            if (row.TryGetValue(key, out var value) && value != null)
                return value.ToString();
        }
        return string.Empty;
    }
}

public static class BanNotice
{
    public const string Prefix = "SNR_BAN";

    public static bool TryParse(string raw, out BanInfo info)
    {
        info = null;
        if (string.IsNullOrEmpty(raw) || !raw.StartsWith(Prefix, StringComparison.Ordinal))
            return false;

        string json = raw.Substring(Prefix.Length).TrimStart('\r', '\n', ' ');
        if (JsonParser.Parse(json) is not System.Collections.Generic.Dictionary<string, object> row)
            return false;

        info = BanInfo.From(row);
        return true;
    }

    public static string FormatExpiry(string expiresAtIso)
    {
        if (string.IsNullOrEmpty(expiresAtIso))
            return ModTranslation.GetString("SafetyBanExpiresNever");
        if (!DateTimeOffset.TryParse(expiresAtIso, out DateTimeOffset parsed))
            return expiresAtIso;
        return parsed.ToLocalTime().ToString("yyyy/MM/dd HH:mm");
    }

    public static string BuildBody(BanInfo info)
    {
        BanInfo value = info ?? new BanInfo();
        return string.Format(
            ModTranslation.GetString("SafetyBanBody"),
            string.IsNullOrEmpty(value.Reason) ? "-" : value.Reason,
            FormatExpiry(value.ExpiresAtIso),
            string.IsNullOrEmpty(value.InquiryId) ? "-" : value.InquiryId);
    }
}
