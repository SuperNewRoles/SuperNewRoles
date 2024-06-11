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
    /// チャットコマンドが実行されたときに呼ばれる
    /// </summary>
    /// <param name="args">引数</param>
    /// <returns>チャットをキャンセルするか</returns>
    public bool OnChatCommand(string[] args);
}