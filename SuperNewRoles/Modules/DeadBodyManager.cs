using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Modules;

[Flags]
public enum DeadBodyUser
{
    None         = 1 << 0,
    Frankenstein = 1 << 1,
    Matryoshka   = 1 << 2,
}

public static class DeadBodyManager
{
    private static Dictionary<int, DeadBodyUser> DeadBodyUsers;
    public static void ClearAndReloads()
    {
        DeadBodyUsers = new();
    }
    public static bool IsDeadbodyUsed(DeadBody deadBody, DeadBodyUser user)
    {
        return DeadBodyUsers.TryGetValue(getDeadBodyId(deadBody), out DeadBodyUser deadBodyUser) && deadBodyUser.HasFlag(user);
    }
    public static bool IsDeadbodyUsed(DeadBody deadBody)
    {
        return DeadBodyUsers.TryGetValue(getDeadBodyId(deadBody), out DeadBodyUser deadBodyUser) && deadBodyUser != DeadBodyUser.None;
    }
    public static void UseDeadbody(DeadBody deadBody, DeadBodyUser newUser)
    {
        int bodyId = getDeadBodyId(deadBody);
        if (DeadBodyUsers.TryGetValue(bodyId, out DeadBodyUser currentUser))
        {
            if (!currentUser.HasFlag(newUser))
                DeadBodyUsers[bodyId] |= newUser;
        } else
            DeadBodyUsers[bodyId] = newUser;
    }
    public static void EndedUseDeadbody(DeadBody deadBody, DeadBodyUser user)
    {
        int bodyId = getDeadBodyId(deadBody);
        if (DeadBodyUsers.TryGetValue(deadBody.GetInstanceID(), out DeadBodyUser deadBodyUser) && deadBodyUser.HasFlag(user))
        {
            DeadBodyUsers[bodyId] &= ~user;
        }
    }

    private static int getDeadBodyId(DeadBody deadBody)
        => deadBody.GetInstanceID();

    public static void ClearStatus(DeadBody deadBody)
        => DeadBodyUsers.Remove(deadBody.GetInstanceID());

}
