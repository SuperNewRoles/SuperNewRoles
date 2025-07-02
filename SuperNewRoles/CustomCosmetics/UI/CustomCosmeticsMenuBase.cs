using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomCosmetics.UI;

public abstract class CustomCosmeticsMenuBase<T> : BaseSingleton<T>, ICustomCosmeticsMenu where T : CustomCosmeticsMenuBase<T>, new()
{
    public abstract CustomCosmeticsMenuType MenuType { get; }
    public abstract void Initialize();
    public abstract void Update();
    public abstract void Hide();
}
public interface ICustomCosmeticsMenu
{
    public CustomCosmeticsMenuType MenuType { get; }
    public void Initialize();
    public void Update();
    public void Hide();
}
public enum CustomCosmeticsMenuType
{
    color,
    costume,
    pet,
    plate,
    cube,
}


