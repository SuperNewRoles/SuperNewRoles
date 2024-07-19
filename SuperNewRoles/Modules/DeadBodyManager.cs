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
    /// <summary>
    /// 死体が使用されているか確認(指定された種類のみ)
    /// </summary>
    /// <param name="deadBody">対象の死体</param>
    /// <param name="user">種類</param>
    /// <returns></returns>
    public static bool IsDeadbodyUsed(DeadBody deadBody, DeadBodyUser user)
    {
        return DeadBodyUsers.TryGetValue(getDeadBodyId(deadBody), out DeadBodyUser deadBodyUser)
            && deadBodyUser.HasFlag(user);
    }
    /// <summary>
    /// 死体が使用されているか確認(種類は問わない)
    /// </summary>
    /// <param name="deadBody">対象の死体</param>
    /// <returns></returns>
    public static bool IsDeadbodyUsed(DeadBody deadBody)
    {
        return DeadBodyUsers.TryGetValue(getDeadBodyId(deadBody), out DeadBodyUser deadBodyUser)
            && deadBodyUser != DeadBodyUser.None;
    }
    /// <summary>
    ///  死体の使用を宣言
    /// </summary>
    /// <param name="deadBody">対象の死体</param>
    /// <param name="newUser">使用種類</param>
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
    /// <summary>
    /// 死体の使用を終了したことを宣言
    /// </summary>
    /// <param name="deadBody">対象の死体</param>
    /// <param name="user">使用種類</param>
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
