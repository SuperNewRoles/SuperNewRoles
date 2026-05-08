using System;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Modules;

public static class BugReportSettingsCollector
{
    public static string CollectAndCompress()
    {
        try
        {
            string json = CollectSettingsJson();
            if (string.IsNullOrEmpty(json))
                return null;
            return LogCompression.CompressAndEncryptLog(json);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to collect settings for bug report: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    private static string CollectSettingsJson()
    {
        var sb = new StringBuilder();
        sb.Append('{');

        bool hasPrev = false;

        // Vanilla settings
        string vanillaJson = CollectVanillaSettingsJson();
        if (!string.IsNullOrEmpty(vanillaJson))
        {
            sb.Append("\"vanilla_settings\":{");
            sb.Append(vanillaJson);
            sb.Append('}');
            hasPrev = true;
        }

        // SNR Custom Options
        string customOptionsJson = CollectSNRCustomOptionsJson();
        if (!string.IsNullOrEmpty(customOptionsJson))
        {
            if (hasPrev) sb.Append(',');
            sb.Append("\"custom_options\":[");
            sb.Append(customOptionsJson);
            sb.Append(']');
            hasPrev = true;
        }

        // SNR Role Options
        string roleOptionsJson = CollectSNRRoleOptionsJson();
        if (!string.IsNullOrEmpty(roleOptionsJson))
        {
            if (hasPrev) sb.Append(',');
            sb.Append("\"role_options\":[");
            sb.Append(roleOptionsJson);
            sb.Append(']');
            hasPrev = true;
        }

        // SNR Modifier Role Options
        string modifierOptionsJson = CollectSNRModifierRoleOptionsJson();
        if (!string.IsNullOrEmpty(modifierOptionsJson))
        {
            if (hasPrev) sb.Append(',');
            sb.Append("\"modifier_role_options\":[");
            sb.Append(modifierOptionsJson);
            sb.Append(']');
            hasPrev = true;
        }

        // SNR Ghost Role Options
        string ghostOptionsJson = CollectSNRGhostRoleOptionsJson();
        if (!string.IsNullOrEmpty(ghostOptionsJson))
        {
            if (hasPrev) sb.Append(',');
            sb.Append("\"ghost_role_options\":[");
            sb.Append(ghostOptionsJson);
            sb.Append(']');
            hasPrev = true;
        }

        // SNR Exclusivity Settings
        string exclusivityJson = CollectSNRExclusivitySettingsJson();
        if (!string.IsNullOrEmpty(exclusivityJson))
        {
            if (hasPrev) sb.Append(',');
            sb.Append("\"exclusivity_settings\":[");
            sb.Append(exclusivityJson);
            sb.Append(']');
        }

        sb.Append('}');
        return sb.ToString();
    }

    #region Vanilla Settings

    private static string CollectVanillaSettingsJson()
    {
        var sb = new StringBuilder();
        bool hasPrev = false;

        try
        {
            var gameOptions = GameOptionsManager.Instance?.CurrentGameOptions;
            if (gameOptions == null) return string.Empty;

            AppendEnumOptions(sb, gameOptions, ref hasPrev);

            try
            {
                string gameMode = GameOptionsManager.Instance.currentGameMode.ToString();
                AppendJsonField(sb, "GameMode", gameMode, ref hasPrev);
            }
            catch { }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Vanilla settings collection failed: {ex.Message}");
        }

        return sb.ToString();
    }

    private static void AppendEnumOptions(StringBuilder sb, IGameOptions opts, ref bool hasPrev)
    {
        try
        {
            foreach (var val in Enum.GetValues(typeof(ByteOptionNames)))
            {
                try
                {
                    byte value = opts.GetByte((ByteOptionNames)val);
                    AppendJsonField(sb, $"Byte_{val}", value, ref hasPrev);
                }
                catch { }
            }
        }
        catch { }

        try
        {
            foreach (var val in Enum.GetValues(typeof(FloatOptionNames)))
            {
                try
                {
                    float value = opts.GetFloat((FloatOptionNames)val);
                    AppendJsonField(sb, $"Float_{val}", value, ref hasPrev);
                }
                catch { }
            }
        }
        catch { }

        try
        {
            foreach (var val in Enum.GetValues(typeof(Int32OptionNames)))
            {
                try
                {
                    int value = opts.GetInt((Int32OptionNames)val);
                    AppendJsonField(sb, $"Int_{val}", value, ref hasPrev);
                }
                catch { }
            }
        }
        catch { }

        try
        {
            foreach (var val in Enum.GetValues(typeof(BoolOptionNames)))
            {
                try
                {
                    bool value = opts.GetBool((BoolOptionNames)val);
                    AppendJsonField(sb, $"Bool_{val}", value, ref hasPrev);
                }
                catch { }
            }
        }
        catch { }
    }

    #endregion

    #region SNR Custom Options

    private static string CollectSNRCustomOptionsJson()
    {
        var sb = new StringBuilder();
        try
        {
            bool hasPrev = false;
            foreach (var opt in CustomOptionManager.CustomOptions)
            {
                if (hasPrev) sb.Append(',');
                sb.Append('{');
                sb.Append($"\"id\":{EscapeJson(opt.Id)},");
                sb.Append($"\"name\":{EscapeJson(opt.Name)},");
                sb.Append($"\"selection\":{opt.Selection},");
                sb.Append($"\"value\":{EscapeJson(ToInvariantString(opt.Value))},");
                sb.Append($"\"default_selection\":{opt.DefaultSelection},");
                sb.Append($"\"is_default\":{opt.IsDefaultValue.ToString().ToLower()}");
                sb.Append('}');
                hasPrev = true;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"SNR custom options collection failed: {ex.Message}");
        }
        return sb.ToString();
    }

    #endregion

    #region SNR Role Options

    private static string CollectSNRRoleOptionsJson()
    {
        var sb = new StringBuilder();
        try
        {
            if (RoleOptionManager.RoleOptions == null) return string.Empty;
            bool hasPrev = false;
            foreach (var opt in RoleOptionManager.RoleOptions)
            {
                if (hasPrev) sb.Append(',');
                sb.Append('{');
                sb.Append($"\"role_id\":{EscapeJson(opt.RoleId.ToString())},");
                sb.Append($"\"number_of_crews\":{opt.NumberOfCrews},");
                sb.Append($"\"percentage\":{opt.Percentage},");
                sb.Append($"\"option_ids\":[{CollectCustomOptionIdsArrayJson(opt.Options)}]");
                sb.Append('}');
                hasPrev = true;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"SNR role options collection failed: {ex.Message}");
        }
        return sb.ToString();
    }

    #endregion

    #region SNR Modifier Role Options

    private static string CollectSNRModifierRoleOptionsJson()
    {
        var sb = new StringBuilder();
        try
        {
            if (RoleOptionManager.ModifierRoleOptions == null) return string.Empty;
            bool hasPrev = false;
            foreach (var opt in RoleOptionManager.ModifierRoleOptions)
            {
                if (hasPrev) sb.Append(',');
                sb.Append('{');
                sb.Append($"\"modifier_role_id\":{EscapeJson(opt.ModifierRoleId.ToString())},");
                sb.Append($"\"number_of_crews\":{opt.NumberOfCrews},");
                sb.Append($"\"percentage\":{opt.Percentage},");
                sb.Append($"\"max_impostors\":{opt.MaxImpostors},");
                sb.Append($"\"impostor_chance\":{opt.ImpostorChance},");
                sb.Append($"\"max_neutrals\":{opt.MaxNeutrals},");
                sb.Append($"\"neutral_chance\":{opt.NeutralChance},");
                sb.Append($"\"max_crewmates\":{opt.MaxCrewmates},");
                sb.Append($"\"crewmate_chance\":{opt.CrewmateChance},");
                sb.Append($"\"option_ids\":[{CollectCustomOptionIdsArrayJson(opt.Options)}]");
                sb.Append('}');
                hasPrev = true;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"SNR modifier role options collection failed: {ex.Message}");
        }
        return sb.ToString();
    }

    #endregion

    #region SNR Ghost Role Options

    private static string CollectSNRGhostRoleOptionsJson()
    {
        var sb = new StringBuilder();
        try
        {
            if (RoleOptionManager.GhostRoleOptions == null) return string.Empty;
            bool hasPrev = false;
            foreach (var opt in RoleOptionManager.GhostRoleOptions)
            {
                if (hasPrev) sb.Append(',');
                sb.Append('{');
                sb.Append($"\"ghost_role_id\":{EscapeJson(opt.RoleId.ToString())},");
                sb.Append($"\"number_of_crews\":{opt.NumberOfCrews},");
                sb.Append($"\"percentage\":{opt.Percentage},");
                sb.Append($"\"option_ids\":[{CollectCustomOptionIdsArrayJson(opt.Options)}]");
                sb.Append('}');
                hasPrev = true;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"SNR ghost role options collection failed: {ex.Message}");
        }
        return sb.ToString();
    }

    #endregion

    #region SNR Exclusivity Settings

    private static string CollectSNRExclusivitySettingsJson()
    {
        var sb = new StringBuilder();
        try
        {
            bool hasPrev = false;
            foreach (var setting in RoleOptionManager.ExclusivitySettings)
            {
                if (hasPrev) sb.Append(',');
                sb.Append('{');
                sb.Append($"\"max_assign\":{setting.MaxAssign},");
                sb.Append("\"roles\":[");
                if (setting.Roles != null)
                {
                    bool roleHasPrev = false;
                    foreach (var role in setting.Roles)
                    {
                        if (roleHasPrev) sb.Append(',');
                        sb.Append(EscapeJson(role.ToString()));
                        roleHasPrev = true;
                    }
                }
                sb.Append(']');
                sb.Append('}');
                hasPrev = true;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"SNR exclusivity settings collection failed: {ex.Message}");
        }
        return sb.ToString();
    }

    #endregion

    #region Helpers

    private static string CollectCustomOptionIdsArrayJson(CustomOption[] options)
    {
        if (options == null || options.Length == 0) return string.Empty;
        var sb = new StringBuilder();
        bool hasPrev = false;
        foreach (var opt in options)
        {
            if (hasPrev) sb.Append(',');
            sb.Append(EscapeJson(opt.Id));
            hasPrev = true;
        }
        return sb.ToString();
    }

    private static string EscapeJson(string value)
    {
        if (value == null) return "null";
        return Newtonsoft.Json.JsonConvert.ToString(value);
    }

    private static string ToInvariantString(object value)
    {
        if (value == null) return null;
        return value switch
        {
            float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            decimal m => m.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    private static void AppendJsonField(StringBuilder sb, string key, object value, ref bool hasPrev)
    {
        if (hasPrev) sb.Append(',');
        sb.Append(EscapeJson(key));
        sb.Append(':');
        if (value is string s)
            sb.Append(EscapeJson(s));
        else if (value is Enum e)
            sb.Append(EscapeJson(e.ToString()));
        else if (value is bool b)
            sb.Append(b.ToString().ToLower());
        else if (value is float f)
            sb.Append(f.ToString(System.Globalization.CultureInfo.InvariantCulture));
        else if (value is double d)
            sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
        else if (value is decimal m)
            sb.Append(m.ToString(System.Globalization.CultureInfo.InvariantCulture));
        else
            sb.Append(value);
        hasPrev = true;
    }

    #endregion
}
