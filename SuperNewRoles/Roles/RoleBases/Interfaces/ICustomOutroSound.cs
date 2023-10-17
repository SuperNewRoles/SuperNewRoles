using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// ゲーム終了時のサウンドをカスタムする際に使うインターフェース
/// </summary>
public interface ICustomOutroSound
{
    /// <summary>
    /// ゲーム終了時のサウンド
    /// </summary>
    public AudioClip OutroSound { get; }
}