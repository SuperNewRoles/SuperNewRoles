using System.Globalization;

namespace SuperNewRoles.Roles.Ability.CustomButton;

// 残り回数が変わらない HUD 更新では、書式展開と int のボックス化を繰り返さない。
internal sealed class ButtonCountTextCache
{
    private string _format;
    private int _count;
    private CultureInfo _culture;
    private string _text;

    public string GetText(string format, int count)
    {
        var culture = CultureInfo.CurrentCulture;
        if (_text == null || _format != format || _count != count || _culture != culture)
        {
            _text = string.Format(culture, format, count);
            _format = format;
            _count = count;
            _culture = culture;
        }
        return _text;
    }
}
