using System;
using System.Collections.Generic;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Mode;

/// <summary>
/// ゲームモードの基底クラス
/// </summary>
public abstract class ModeBase<T> : BaseSingleton<T>, IModeBase where T : ModeBase<T>, new()
{
    public abstract ModeId Mode { get; }
    public abstract string ModeName { get; }
    public abstract Color32 ModeColor { get; }

    /// <summary>
    /// ゲーム開始時に呼ばれる
    /// </summary>
    public abstract void OnGameStart();

    /// <summary>
    /// ゲーム終了時に呼ばれる
    /// </summary>
    public abstract void OnGameEnd();

    /// <summary>
    /// プレイヤーが死亡したときに呼ばれる
    /// </summary>
    public virtual void OnPlayerDeath(PlayerControl player, PlayerControl killer) { }

    /// <summary>
    /// 勝利条件をチェックする
    /// </summary>
    public abstract bool CheckWinCondition();

    /// <summary>
    /// イントロ情報を取得する
    /// </summary>
    /// <param name="player">プレイヤー</param>
    /// <returns>イントロ情報</returns>
    public virtual ModeIntroInfo GetIntroInfo(PlayerControl player)
    {
        return new ModeIntroInfo
        {
            RoleTitle = ModeName,
            RoleSubTitle = "",
            IntroMessage = "",
            RoleColor = ModeColor,
            TeamMembers = new List<PlayerControl> { player }
        };
    }

    /// <summary>
    /// チームメンバーのリストを取得する
    /// </summary>
    /// <param name="player">プレイヤー</param>
    /// <returns>チームメンバーのリスト</returns>
    public virtual List<PlayerControl> GetTeamMembers(PlayerControl player)
    {
        return new List<PlayerControl> { player };
    }

    /// <summary>
    /// モードがイントロをカスタマイズするかどうか
    /// </summary>
    public virtual bool HasCustomIntro => false;
}

public interface IModeBase
{
    ModeId Mode { get; }
    string ModeName { get; }
    Color32 ModeColor { get; }
    void OnGameStart();
    void OnGameEnd();
    void OnPlayerDeath(PlayerControl player, PlayerControl killer);
    bool CheckWinCondition();
    ModeIntroInfo GetIntroInfo(PlayerControl player);
    List<PlayerControl> GetTeamMembers(PlayerControl player);
    bool HasCustomIntro { get; }
}

/// <summary>
/// モードのイントロ情報
/// </summary>
public class ModeIntroInfo
{
    /// <summary>
    /// 役職名
    /// </summary>
    public string RoleTitle { get; set; }

    /// <summary>
    /// 役職サブタイトル
    /// </summary>
    public string RoleSubTitle { get; set; }

    /// <summary>
    /// イントロメッセージ
    /// </summary>
    public string IntroMessage { get; set; }

    /// <summary>
    /// 役職の色
    /// </summary>
    public Color RoleColor { get; set; }

    /// <summary>
    /// チームメンバーのリスト
    /// </summary>
    public List<PlayerControl> TeamMembers { get; set; } = new List<PlayerControl>();

    /// <summary>
    /// イントロサウンドのタイプ（nullの場合はデフォルト）
    /// </summary>
    public AmongUs.GameOptions.RoleTypes? IntroSoundType { get; set; } = null;

    /// <summary>
    /// カスタムイントロサウンド（nullの場合はIntroSoundTypeを使用）
    /// </summary>
    public UnityEngine.AudioClip CustomIntroSound { get; set; } = null;
}