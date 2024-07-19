using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface ISHRChatCommand
{
    public string CommandName { get; }
    public string? Alias => null;
    /// <summary>
    /// 導入者ゲスト視点でキャンセルするか(実際の処理をするのは未推奨)
    /// </summary>
    /// <param name="args">引数</param>
    /// <returns>チャットをキャンセルするか</returns>
    public bool OnChatCommandClient(string[] args)
        => true;
    /// <summary>
    /// チャットコマンドが実行されたときに呼ばれる
    /// </summary>
    /// <param name="args">引数</param>
    /// <returns>チャットをキャンセルするか</returns>
    public bool OnChatCommand(string[] args);
}