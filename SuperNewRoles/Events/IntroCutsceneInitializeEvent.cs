using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class IntroCutsceneInitializeEvent : EventTargetBase<IntroCutsceneInitializeEvent>
{
    public static void Invoke()
    {
        Instance.Awake();
    }
}
