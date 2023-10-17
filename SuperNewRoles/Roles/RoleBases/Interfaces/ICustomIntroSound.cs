using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// Introサウンドが個別である場合に使うインターフェース
/// </summary>
public interface ICustomIntroSound
{
    /// <summary>
    /// Introのサウンド
    /// </summary>
    public AudioClip IntroSound { get; }
}