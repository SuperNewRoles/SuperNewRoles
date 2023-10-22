using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Modules;

public class PlayerData<T>
{
    private Dictionary<byte, T> _data;
    private Dictionary<PlayerControl, T> _playerdata;
    private T defaultvalue = default;
    private bool nonsetinit = false;
    public T Local
    {
        get
        {
            return this[PlayerControl.LocalPlayer.PlayerId];
        }
        set
        {
            this[PlayerControl.LocalPlayer.PlayerId] = value;
        }
    }
    public int Count
    {
        get
        {
            return _data != null ? _data.Count : 0;
        }
    }
    public Dictionary<byte, T>.ValueCollection Values
    {
        get
        {
            return _data.Values;
        }
    }
    private T _result;
    public T this[byte key]
    {
        get
        {
            if (_data == null || !_data.TryGetValue(key, out _result))
            {
                if (nonsetinit) this[key] = defaultvalue;
                return defaultvalue;
            }
            return _result;
        }
        set
        {
            (_data ?? (_data = new(1)))[key] = value;
            if (_playerdata != null)
            {
                PlayerControl player = ModHelpers.PlayerById(key);
                if (player != null)
                    _playerdata[player] = value;
            }
        }
    }
    public T this[PlayerControl key]
    {
        get
        {
            if (key == null) return defaultvalue;
            if (_data == null || !_data.TryGetValue(key.PlayerId, out _result))
            {
                if (nonsetinit) this[key] = defaultvalue;
                return defaultvalue;
            }
            return _result;
        }
        set
        {
            if (key != null)
            {
                (_data ?? (_data = new(1)))[key.PlayerId] = value;
                if (_playerdata != null)
                    _playerdata[key] = value;
            }
        }
    }
    public static implicit operator Dictionary<byte, T>(PlayerData<T> obj)
    {
        if (obj == null)
            return new();
        return obj._data ?? (obj._data = new());
    }
    public static implicit operator Dictionary<PlayerControl, T>(PlayerData<T> obj)
    {
        if (obj == null)
            return new();
        if (obj._playerdata == null)
        {
            Logger.Info("needplayerlistが無効なのにも関わらず、PlayerControlをKeyにしたDictionaryが要求されました。needplayerlistを有効に変更してください。");
            if (obj._data == null)
            {
                obj._data = new();
                obj._playerdata = new();
            }
            else
            {
                obj._playerdata = new(obj._data.Count);
                foreach (var value in obj._data)
                {
                    PlayerControl p = ModHelpers.PlayerById(value.Key);
                    if (p != null)
                        obj._playerdata[p] = value.Value;
                }
            }
        }
        return obj._playerdata;
    }
    public void Reset()
    {
        _data = null;
        if (_playerdata != null) 
            _playerdata = new();
    }
    public bool Any(Func<KeyValuePair<byte, T>, bool> func)
    {
        if (_data == null)
            return false;
        foreach (KeyValuePair<byte, T> obj in _data)
            if (func(obj)) return true;
        return false;
    }

    public bool TryGetValue(PlayerControl key, out T result)
    {
        if (_data == null || key == null)
        {
            result = default;
            return false;
        }
        return TryGetValue(key.PlayerId, out result);
    }

    public bool TryGetValue(byte key, out T result)
    {
        if (_data == null)
        {
            result = default;
            return false;
        }
        return _data.TryGetValue(key, out result);
    }

    public PlayerControl GetPCByValue(T value)
    {
        if (_playerdata != null)
        {
            return _playerdata.GetKeyByValue(value);
        }
        else
        {
            byte pid = _data.GetKeyByValue<byte, T>(value, defaultvalue: 255);
            return pid == 255 ? null : ModHelpers.PlayerById(pid);
        }
    }
    public bool ContainsValue(T value)
    {
        return _data == null ? false : _data.ContainsValue(value);
    }
    public bool Contains(byte player)
    {
        return _data == null ? false : _data.ContainsKey(player);
    }
    public bool Contains(PlayerControl player) => player == null ? false : Contains(player.PlayerId);
    public void Remove(PlayerControl player)
    {
        if (_data != null)
        {
            _data.Remove(player.PlayerId);
            if (_playerdata != null)
                _playerdata.Remove(player);
        }
    }
    public void Remove(byte player)
    {
        if (_data != null)
        {
            _data.Remove(player);
            if (_playerdata != null)
                _playerdata.Remove(ModHelpers.PlayerById(player));
        }
    }

    public Dictionary<byte, T> GetDicts()
    {
        return (Dictionary<byte, T>)this;
    }

    /// <summary>
    /// プレイヤーの情報を保存できるクラス。
    /// 例(intを保存したい場合)：PlayerData<int>
    /// </summary>
    /// <param name="needplayerlist">Dictionary<PlayerControl,T>型が必要かどうか</param>
    public PlayerData(bool needplayerlist = false, T defaultvalue = default, bool nonsetinit = false)
    {
        //使用する際に初期化して、メモリの負担を軽く
        _data = null;
        _playerdata = needplayerlist ? new(1) : null;
        this.defaultvalue = defaultvalue;
        this.nonsetinit = nonsetinit;
    }
    public IEnumerator GetEnumerator()
    {
        if (_data == null)
            _data = new();
        return _data.GetEnumerator();
    }
}