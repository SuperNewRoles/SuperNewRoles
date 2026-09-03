namespace SuperNewRoles.Safety;

public static class SafetyDisconnectCopy
{
    public const string HostBlockedJa = "ホストにブロックされています";
    public const string YouBlockedHostJa = "ブロックしたホストの部屋には参加できません";
    public const string NeedConductJa = "行動規範への同意が必要です";

    public static bool IsNeedConduct(string stringReason)
    {
        return !string.IsNullOrEmpty(stringReason) && stringReason.Contains(NeedConductJa);
    }

    public static string TranslationKey(string stringReason)
    {
        if (string.IsNullOrEmpty(stringReason))
            return null;
        if (stringReason.Contains(YouBlockedHostJa))
            return "SafetyYouBlockedHostDisconnect";
        if (stringReason.Contains(HostBlockedJa))
            return "SafetyHostBlockedDisconnect";
        return null;
    }
}
