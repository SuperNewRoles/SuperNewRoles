using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

namespace SuperNewRoles;
public static class ModHelpers
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    public static string GetRandomFormat(string format, int min, int max)
    {
        return format.Replace("{0}", GetRandomInt(max, min).ToString());
    }

    public static bool IsCustomServer()
    {
        if (FastDestroyableSingleton<ServerManager>.Instance == null) return false;
        StringNames n = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
        return n is not StringNames.ServerNA and not StringNames.ServerEU and not StringNames.ServerAS;
    }
    /// <summary>
    /// ランダムを取得します。max = 10だと0～10まで取得できます
    /// </summary>
    /// <param name="max"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public static int GetRandomInt(int max, int min = 0)
    {
        if (min > max)
        {
            // minとmaxが逆の場合は入れ替える
            (min, max) = (max, min);
        }
        // RandomNumberGenerator.GetInt32は `toExclusive` なので max + 1 する
        return RandomNumberGenerator.GetInt32(min, max + 1);
    }

    /// <summary>
    /// 指定された確率（パーセンテージ）で成功を判定する
    /// </summary>
    /// <param name="percentage">成功確率（0-100）</param>
    /// <returns>成功した場合true</returns>
    public static bool IsSuccessChance(int percentage)
    {
        if (percentage <= 0) return false;
        if (percentage >= 100) return true;
        return GetRandomInt(100) < percentage;
    }
    private static MD5 md5 = MD5.Create();
    private static SHA256 sha256 = SHA256.Create();
    public static string HashMD5(string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        return HashMD5(bytes);
    }
    public static string HashMD5(byte[] bytes)
    {
        return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }
    public static string HashSHA256(string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        return HashSHA256(bytes);
    }
    public static string HashSHA256(byte[] bytes)
    {
        return BitConverter.ToString(sha256.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }

    // MeetingHudのMaskAreaを更新するヘルパーメソッド
    public static void UpdateMeetingHudMaskAreas(bool active)
    {
        if (MeetingHud.Instance == null) return;

        foreach (var playerState in MeetingHud.Instance.playerStates)
        {
            var maskArea = playerState.transform.Find("MaskArea");
            if (maskArea != null && maskArea.gameObject.activeSelf != active)
                maskArea.gameObject.SetActive(active);
        }
    }
    public static ReadOnlySpan<T> AsSpan<T>(this List<T> list) => CollectionsMarshal.AsSpan(list);
    public static ReadOnlySpan<T> AsSpan<T>(this T[] array) => array;
    /*public static bool Any<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        foreach (var item in span)
        {
            if (predicate(item)) return true;
        }
        return false;
    }
    public static ReadOnlySpan<T> Where<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        var list = new List<T>();
        foreach (var item in span)
        {
            if (predicate(item)) list.Add(item);
        }
        return list.AsSpan();
    }
    public static HashSet<T> ToHashSet<T>(this ReadOnlySpan<T> span)
    {
        var hashSet = new HashSet<T>();
        foreach (var item in span)
        {
            hashSet.Add(item);
        }
        return hashSet;
    }*/
    public static bool IsComms()
    {
        try
        {
            if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out ISystemType system)) return false;
            HudOverrideSystemType hudOverride;
            HqHudSystemType hqHud;
            if ((hudOverride = system.TryCast<HudOverrideSystemType>()) != null) return hudOverride.IsActive;
            if ((hqHud = system.TryCast<HqHudSystemType>()) != null) return hqHud.IsActive;

            return false;
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString(), "IsComms");
            return false;
        }
    }
    public static bool IsElectrical()
    {
        try
        {
            if (ExPlayerControl.ExPlayerControls.Any(x => x.GetAbility<OwlSpecialBlackoutAbility>()?.IsSpecialBlackout ?? false)) return true;
            if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType system)) return false;
            SwitchSystem electrical;
            if ((electrical = system.TryCast<SwitchSystem>()) != null) return electrical.IsActive;
            return false;
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString(), "IsElectrical");
            return false;
        }
    }
    public static bool IsSabotageAvailable(bool isMushroomMixAsSabotage = true)
    {
        try
        {
            if (isMushroomMixAsSabotage && PlayerControl.LocalPlayer.IsMushroomMixupActive())
            {
                return true;
            }
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                {
                    return true;
                }
            }
        }
        catch { }
        return false;
    }
    public static string Cs(Color c, string s)
    {
        return $"<color=#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}{ToByte(c.a):X2}>{s}</color>";
    }
    public static string CsWithTranslation(Color c, string s)
    {
        return Cs(c, ModTranslation.GetString(s));
    }
    public static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
    public static string WrapText(string text, int width, bool halfIshalf = true)
    {
        if (string.IsNullOrEmpty(text) || width <= 0)
        {
            return text;
        }

        StringBuilder overallResult = new StringBuilder();
        // 改行を CRLF/LF/R すべてで分割
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (string singleInputLine in lines)
        {
            if (singleInputLine.Length == 0)
            {
                // 空行はそのまま（実際にはAppendLineが改行を追加する）
                overallResult.AppendLine();
                continue;
            }

            StringBuilder wrappedLineBuilder = new StringBuilder(); // 現在の入力行をラップした結果を保持
            StringBuilder currentSegmentForWrapping = new StringBuilder(); // ラップ処理中の一部分
            float currentVisualWidth = 0f;

            foreach (char c in singleInputLine)
            {
                float charEffectiveWidth;
                if (halfIshalf)
                {
                    // 半角文字の判定：ASCII印字可能文字 (U+0020～U+007E) または半角カタカナ (U+FF61～U+FF9F)
                    bool isHankaku = (c >= '\u0020' && c <= '\u007E') || (c >= '\uFF61' && c <= '\uFF9F');
                    charEffectiveWidth = isHankaku ? 0.5f : 1.0f;
                }
                else
                {
                    charEffectiveWidth = 1.0f;
                }

                // 現在のセグメントに文字を追加すると幅を超える場合、改行を挿入
                if (currentVisualWidth + charEffectiveWidth > width && currentSegmentForWrapping.Length > 0)
                {
                    wrappedLineBuilder.Append(currentSegmentForWrapping.ToString());
                    wrappedLineBuilder.Append('\n'); // 折り返しによる内部的な改行
                    currentSegmentForWrapping.Clear();
                    currentVisualWidth = 0f;
                }
                currentSegmentForWrapping.Append(c);
                currentVisualWidth += charEffectiveWidth;
            }

            // 行の最後のセグメントを追加
            wrappedLineBuilder.Append(currentSegmentForWrapping.ToString());

            // 元のRegex.Replace(line, $".{{{width}}}", "$0\n").AppendLine(); の動作を模倣:
            // ラップされた行（内部改行を含む可能性あり）を全体の結果に追加し、その後にも改行を追加。
            overallResult.Append(wrappedLineBuilder.ToString());
            overallResult.AppendLine();
        }

        // 最後に、全体の末尾の余分な改行を削除
        return overallResult.ToString().TrimEnd('\r', '\n');
    }
    public static bool Il2CppIs<T1, T2>(this T1 before, out T2 after) where T1 : Il2CppObjectBase where T2 : Il2CppObjectBase
    {
        after = before?.TryCast<T2>();
        return after != null;
    }
    public static (int completed, int total) TaskCompletedData(NetworkedPlayerInfo playerInfo)
    {
        if (playerInfo?.Tasks == null)
            return (-1, -1);

        int TotalTasks = 0;
        int CompletedTasks = 0;

        for (int j = 0; j < playerInfo.Tasks.Count; j++)
        {
            if (playerInfo.Tasks[j] == null)
                continue;
            TotalTasks++;
            if (playerInfo.Tasks[j].Complete)
            {
                CompletedTasks++;
            }
        }
        return (CompletedTasks, TotalTasks);
    }
    public static long ReadInt64(this MessageReader reader)
    {

        long output = (long)reader.FastByte()
            | (long)reader.FastByte() << 8
            | (long)reader.FastByte() << 16
            | (long)reader.FastByte() << 24
            | (long)reader.FastByte() << 32
            | (long)reader.FastByte() << 40
            | (long)reader.FastByte() << 48
            | (long)reader.FastByte() << 56;
        return output;
    }
    /// <summary>keyCodesが押されているか</summary>
    public static bool GetManyKeyDown(KeyCode[] keyCodes) =>
        keyCodes.All(x => Input.GetKey(x)) && keyCodes.Any(x => Input.GetKeyDown(x));
    public static ExileController.InitProperties GenerateExileInitProperties(NetworkedPlayerInfo player, bool voteTie)
    {
        ExileController.InitProperties initProperties = new();
        if (player != null)
        {
            initProperties.outfit = player.Outfits[PlayerOutfitType.Default];
            initProperties.networkedPlayer = player;
            initProperties.isImpostor = player.Role.IsImpostor;
        }
        initProperties.voteTie = voteTie;
        initProperties.confirmImpostor = GameManager.Instance.LogicOptions.GetConfirmImpostor();
        initProperties.totalImpostorCount = GameData.Instance.AllPlayers.Count((NetworkedPlayerInfo p) => p.Role.IsImpostor);
        initProperties.remainingImpostorCount = GameData.Instance.AllPlayers.Count((NetworkedPlayerInfo p) => p.Role.IsImpostor && !p.IsDead && !p.Disconnected);
        if (player != null && player.Role.IsImpostor && !player.Disconnected)
            initProperties.remainingImpostorCount--;
        return initProperties;
    }
    public static bool IsMap(this MapNames map)
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == (byte)map;
    }
    public static void SetActiveAllObject(this IEnumerable<GameObject> gameObjects, bool active)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject == null) continue;
            gameObject.SetActive(active);
        }
    }
    public static void SetActiveAllObject(this IEnumerable<GameObject> gameObjects, string name, bool active)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject == null) continue;
            if (gameObject.name != name) continue;
            gameObject.SetActive(active);
        }
    }
    public static IEnumerable<GameObject> GetChildren(this GameObject gameObject)
    {
        List<GameObject> list = new();
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            var child = gameObject.transform.GetChild(i);
            if (child.gameObject == null) continue;
            list.Add(child.gameObject);
        }
        return list;
    }
    public static void Shuffle<T>(this List<T> list)
    {
        // Fisher–Yatesアルゴリズムを使ってリストをシャッフル
        for (int i = list.Count - 1; i > 0; i--)
        {
            // UnityEngine.Random.Range(0, i + 1) を GetRandomInt(0, i) に置き換え
            int j = GetRandomInt(i); // 0 から i までのランダムなインデックスを取得
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    public static List<T> Shuffled<T>(this List<T> list)
    {
        list.Shuffle();
        return list;
    }
    public static void Write(this MessageWriter writer, short value)
    {
        writer.Buffer[writer.Position++] = (byte)value;
        writer.Buffer[writer.Position++] = (byte)(value >> 8);
        if (writer.Position > writer.Length) writer.Length = writer.Position;
    }
    public static void Write(this MessageWriter writer, long value)
    {
        writer.Buffer[writer.Position++] = (byte)value;
        writer.Buffer[writer.Position++] = (byte)(value >> 8);
        writer.Buffer[writer.Position++] = (byte)(value >> 16);
        writer.Buffer[writer.Position++] = (byte)(value >> 24);
        writer.Buffer[writer.Position++] = (byte)(value >> 32);
        writer.Buffer[writer.Position++] = (byte)(value >> 40);
        writer.Buffer[writer.Position++] = (byte)(value >> 48);
        writer.Buffer[writer.Position++] = (byte)(value >> 56);
        if (writer.Position > writer.Length) writer.Length = writer.Position;
    }
    public static int GetRandomIndex<T>(List<T> list)
    {
        if (list == null || list.Count == 0) return -1; // リストが空の場合は -1 を返すなどのエラーハンドリングを追加
        return GetRandomInt(list.Count - 1); // 0 から list.Count - 1 までのインデックス
    }
    /// <summary>
    /// 2つの位置が指定された距離以内かどうかを確認します
    /// </summary>
    /// <param name="pos">位置1</param>
    /// <param name="pos2">位置2</param>
    /// <param name="distance">最大距離</param>
    /// <returns>距離以内の場合はtrue</returns>
    public static bool IsPositionDistance(Vector2 pos, Vector2 pos2, float distance)
    {
        float dis = Vector2.Distance(pos, pos2);
        return dis <= distance;
    }

    /// <summary>
    /// リストからランダムな要素を取得します
    /// </summary>
    /// <typeparam name="T">リストの型</typeparam>
    /// <param name="list">リスト</param>
    /// <returns>ランダムな要素</returns>
    public static T GetRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0) return default;
        // GetRandomIndex を使用するように変更
        return list[GetRandomIndex(list)];
    }

    /// <summary>
    /// リストからランダムなインデックスを取得します
    /// </summary>
    /// <typeparam name="T">リストの型</typeparam>
    /// <param name="list">リスト</param>
    /// <returns>ランダムなインデックス</returns>
    public static int GetRandomIndex<T>(this IEnumerable<T> list)
    {
        var array = list as T[] ?? list.ToArray();
        if (array.Length == 0) return -1; // 配列が空の場合は -1 を返す
        // GetRandomInt を使用するように変更
        return GetRandomInt(array.Length - 1); // 0 から array.Length - 1 までのインデックス
    }
    public static IEnumerable<T> GetSystemEnumerable<T>(this Il2CppSystem.Collections.IEnumerable list)
    {
        var castedList = list.WrapToManaged().OfType<T>();
        foreach (var item in castedList)
        {
            yield return item;
        }
    }
    public static IEnumerable<T> GetSystemEnumerable<T>(this Il2CppSystem.Collections.Generic.IEnumerable<T> list)
    {
        return GetSystemEnumerable<T>(list.TryCast<Il2CppSystem.Collections.IEnumerable>());
    }
    public static Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> Wrap<TKey, TValue>(this Dictionary<TKey, TValue> dict)
    {
        var newDict = new Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue>();
        foreach (var item in dict)
        {
            newDict.Add(item.Key, item.Value);
        }
        return newDict;
    }
    public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
    {
        if (dict.TryGetValue(key, out TValue value))
            return value;
        return defaultValue;
    }
    public static void AddOrUpdate<TKey>(this Dictionary<TKey, int> dict, TKey key, int value)
    {
        if (dict.TryGetValue(key, out int oldValue))
            dict[key] = oldValue + value;
        else
            dict[key] = value;
    }
    public static T TryGetIndex<T>(this List<T> list, int index)
    {
        if (index < 0 || index >= list.Count) return default;
        return list[index];
    }
    public static T TryGetIndex<T>(this T[] array, int index)
    {
        if (index < 0 || index >= array.Length) return default;
        return array[index];
    }
    public static byte ParseToByte(this string str)
    {
        return byte.Parse(str);
    }

    private static bool IsSameOutfit(NetworkedPlayerInfo.PlayerOutfit shifterOutfit, NetworkedPlayerInfo.PlayerOutfit targetOutfit) =>
            !(shifterOutfit.ColorId == targetOutfit.ColorId
            && shifterOutfit.HatId == targetOutfit.HatId
            && shifterOutfit.PetId == targetOutfit.PetId
            && shifterOutfit.SkinId == targetOutfit.SkinId
            && shifterOutfit.VisorId == targetOutfit.VisorId
            && shifterOutfit.PlayerName == targetOutfit.PlayerName);

    public static void setOutfit(this PlayerControl pc, NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        pc.Data.Outfits[PlayerOutfitType.Shapeshifted] = outfit;
        if (IsSameOutfit(pc.Data.DefaultOutfit, outfit)) pc.CurrentOutfitType = PlayerOutfitType.Shapeshifted;
        pc.RawSetName(outfit.PlayerName);
        pc.RawSetHat(outfit.HatId, outfit.ColorId);
        pc.RawSetVisor(outfit.VisorId, outfit.ColorId);
        pc.RawSetColor(outfit.ColorId);
        pc.RawSetPet(outfit.PetId, outfit.ColorId);
        pc.RawSetSkin(outfit.SkinId, outfit.ColorId);
    }

    [CustomRPC]
    public static void RpcOpenToilet(PlayerControl pc)
    {
        foreach (byte i in new[] { 79, 80, 81, 82 })
        {
            ShipStatus.Instance.UpdateSystem(SystemTypes.Doors, pc, i);
        }
    }

    [CustomRPC]
    public static void RpcFixingSabotage(TaskTypes taskType)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        FixingSabotage(taskType);
    }

    [CustomRPC]
    public static void RpcFixLight()
    {
        if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType system))
            return;
        SwitchSystem switchSystem = system.TryCast<SwitchSystem>();
        switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
    }

    public static bool IsSabotage(TaskTypes taskType)
    {
        return taskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.StopCharles or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.MushroomMixupSabotage;
    }
    public static bool IsSabotage(SystemTypes systemType)
    {
        return systemType is SystemTypes.Electrical or SystemTypes.LifeSupp or SystemTypes.Reactor or SystemTypes.HeliSabotage or SystemTypes.Laboratory or SystemTypes.Comms;
    }

    private static void FixingSabotage(TaskTypes taskType)
    {
        switch (taskType)
        {
            case TaskTypes.FixLights:
                RpcFixLight();
                break;
            case TaskTypes.RestoreOxy: // 酸素 (Skeld, Mira)
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 0 | 64);
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 1 | 64);
                break;
            case TaskTypes.ResetReactor: // リアクター (Skeld, Mira, Fungle)
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 16);
                break;
            case TaskTypes.StopCharles: // 衝突回避 (Airship)
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, 0 | 16);
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, 1 | 16);
                break;
            case TaskTypes.ResetSeismic: // 耐震 (Polus)
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Laboratory, 16);
                break;
            case TaskTypes.FixComms:
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                break;
            case TaskTypes.MushroomMixupSabotage:
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.MushroomMixupSabotage, 16 | 1);
                break;
            default:
                Logger.Info($"リペア処理が異常な呼び出しを受けました。", "Repair Process");
                break;
        }
    }
    // ユークリッドの互除法で最大公約数を求める
    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int tmp = b;
            b = a % b;
            a = tmp;
        }
        return a;
    }

    // 画面サイズからアスペクト比を計算
    public static (int, int) GetAspectRatio(int width, int height)
    {
        int gcd = Gcd(width, height);
        return (width / gcd, height / gcd);
    }

    // SendWebRequest が返す UnityWebRequestAsyncOperation を await 可能にする
    public static TaskAwaiter<UnityWebRequest> GetAwaiter(
        this UnityWebRequestAsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();

        asyncOp.add_completed((Il2CppSystem.Action<AsyncOperation>)((op) =>
        {
            var req = asyncOp.webRequest;
            if (req.result == UnityWebRequest.Result.Success)
                tcs.SetResult(req);
            else
                tcs.SetException(new Exception(req.error));
        }));

        return tcs.Task.GetAwaiter();
    }

    public static bool IsWaitingSpawn(this PlayerControl player)
    {
        return Vector2.Distance(player.transform.position, new Vector2(3, 6)) <= 0.5f ||
               Vector2.Distance(player.transform.position, new Vector2(-25, 40)) <= 0.5f ||
               Vector2.Distance(player.transform.position, new Vector2(-1.4f, 2.3f)) <= 0.5f;
    }
    public static bool IsWaitingSpawn(this ExPlayerControl player)
        => player.Player.IsWaitingSpawn();

    public static bool Not(bool b) => !b;

    public static bool IsHnS()
    {
        return GameOptionsManager.Instance.CurrentGameOptions.GameMode is AmongUs.GameOptions.GameModes.HideNSeek or AmongUs.GameOptions.GameModes.SeekFools;
    }
    public static bool TryCastOut<T>(this Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase obj, out T result) where T : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {
        result = obj.TryCast<T>();
        return result != null;
    }
    public static void SetOpacity(PlayerControl player, float opacity)
    {
        // Sometimes it just doesn't work?
        var color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
        try
        {
            if (player.cosmetics.currentBodySprite.BodySprite != null)
                player.cosmetics.currentBodySprite.BodySprite.color = color;

            if (player.cosmetics.skin.layer != null)
                player.cosmetics.skin.layer.color = color;

            if (player.cosmetics.hat != null)
                player.cosmetics.hat.SpriteColor = color;

            if (player.cosmetics.GetPet() != null)
                player.cosmetics.GetPet().ForEachRenderer(true, (Il2CppSystem.Action<SpriteRenderer>)((render) => render.color = color));

            if (player.cosmetics.visor != null)
                player.cosmetics.visor.Image.color = color;

            CustomCosmeticsLayer layer = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics);
            if (layer != null)
            {
                layer.hat1.SpriteColor = color;
                layer.hat2.SpriteColor = color;
                layer.visor1.Image.color = color;
                layer.visor2.Image.color = color;
            }
            if (player.cosmetics.colorBlindText != null && opacity < 0.1f) // 完全に透明化している場合のみ, 色覚補助テキストを非表示にする。
                player.cosmetics.colorBlindText.color = Palette.ClearWhite;
            else if (player.cosmetics.colorBlindText != null)
                player.cosmetics.colorBlindText.color = Palette.White;

            if (player.cosmetics.nameText != null && opacity < 0.1f)
                player.cosmetics.nameText.enabled = false; // 完全に透明化している場合は, プレイヤー名を非表示にする。
            else
                player.cosmetics.nameText.enabled = true; // 少しでも姿を見られるなら, プレイヤー名を表示する。
        }
        catch { }
    }
    public static AudioSource PlaySound(Transform parent, AudioClip clip, bool loop, float volume = 1f, AudioMixerGroup audioMixer = null)
    {
        if (audioMixer == null)
        {
            audioMixer = (loop ? SoundManager.Instance.MusicChannel : SoundManager.Instance.SfxChannel);
        }
        AudioSource value = parent.GetComponent<AudioSource>() ?? parent.gameObject.AddComponent<AudioSource>();
        value.outputAudioMixerGroup = audioMixer;
        value.playOnAwake = false;
        value.volume = volume;
        value.loop = loop;
        value.clip = clip;
        value.Play();
        return value;
    }

    // shhhhhh.....
    public static bool IsAndroid()
    {
        return Constants.GetPlatformType() == Platforms.Android;
    }

    public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
    {
        KeyValuePair<byte, int> max = new(byte.MaxValue, int.MinValue);
        tie = false;
        foreach (var pair in self)
        {
            if (pair.Value > max.Value)
            {
                max = pair;
                tie = false;
            }
            else if (pair.Value == max.Value)
                tie = true;
        }
        return max;
    }

    /// <summary>
    /// 指定された位置がコライダーと重なっているかを確認します
    /// </summary>
    /// <param name="collider2D">チェック対象のコライダー</param>
    /// <param name="position">チェックする位置</param>
    /// <returns>重なっている場合はtrue</returns>
    public static bool CheckCollision(Collider2D collider2D, Vector3 position)
    {
        if (collider2D == null) return false;
        return collider2D.OverlapPoint(position);
    }
}