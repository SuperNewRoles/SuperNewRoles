using System;
using System.Collections.Generic;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;

class JumboModifier : ModifierBase<JumboModifier>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.JumboModifier;

    public override Color32 RoleColor => Color.white;

    public override List<Func<AbilityBase>> Abilities => [() =>
        new JumboAbility(new(JumboMaxSize / 10, JumboSizeUpSpeed, JumboWalkSoundSize, JumboSizeUpOnMeeting))
    ];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => GetAssignedTeams();

    public override WinnerTeamType WinnerTeam => WinnerTeamType.None;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;

    public override Func<ExPlayerControl, string> ModifierMark => _ => "{0}" + ModHelpers.Cs(RoleColor, "Ⓙ");

    public override bool AssignFilter => true;

    [CustomOptionBool("JumboAssignToImpostor", true)]
    public static bool JumboAssignToImpostor;
    [CustomOptionBool("JumboAssignToNeutral", true)]
    public static bool JumboAssignToNeutral;
    [CustomOptionFloat("JumboMaxSize", 1f, 72f, 1f, 24f)]
    public static float JumboMaxSize;
    [CustomOptionFloat("JumboSizeUpSpeed", 0f, 360f, 2.5f, 60f)]
    public static float JumboSizeUpSpeed;
    [CustomOptionFloat("JumboWalkSoundSize", 1f, 100f, 1f, 10f)]
    public static float JumboWalkSoundSize;
    [CustomOptionBool("JumboSizeUpOnMeeting", true)]
    public static bool JumboSizeUpOnMeeting;

    private List<AssignedTeamType> GetAssignedTeams()
    {
        int capacity = 1 + (JumboAssignToImpostor ? 1 : 0) + (JumboAssignToNeutral ? 1 : 0);
        var teams = new List<AssignedTeamType>(capacity)
        {
            AssignedTeamType.Crewmate
        };
        if (JumboAssignToImpostor)
            teams.Add(AssignedTeamType.Impostor);
        if (JumboAssignToNeutral)
            teams.Add(AssignedTeamType.Neutral);
        // 全て
        if (teams.Count == 0 || teams.Count >= 3)
            return [];
        return teams;
    }
}

public record JumboData(float MaxSize, float SizeUpSpeed, float WalkSoundSize, bool SizeUpOnMeeting);
public class JumboAbility : AbilityBase
{
    private JumboData Data { get; }
    private EventListener _fixedUpdateListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateListener;
    private EventListener<WrapUpEventData> _wrapUpListener;
    private EventListener<MurderEventData> _murderListener;
    private EventListener<ExileControllerEventData> _exileControllerListener;
    private EventListener<MeetingCalledAnimationInitializeEventData> _meetingCalledAnimationInitializeListener;
    public float _currentSize { get; private set; } = 0f;
    private Vector2 _oldPosition;
    private bool _hasOldPosition = false;
    private float _playSoundTimer = 0f;
    private float syncTimer = 0f;
    private int _lastSizeThreshold = 0;
    public JumboAbility(JumboData jumboData)
    {
        Data = jumboData;
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
        _murderListener = MurderEvent.Instance.AddListener(OnMurder);
        _exileControllerListener = ExileControllerEvent.Instance.AddListener(OnExileController);
        _meetingCalledAnimationInitializeListener = MeetingCalledAnimationInitializeEvent.Instance.AddListener(OnMeetingCalledAnimationInitialize);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _fixedUpdateListener?.RemoveListener();
        _nameTextUpdateListener?.RemoveListener();
        _wrapUpListener?.RemoveListener();
        _murderListener?.RemoveListener();
        _exileControllerListener?.RemoveListener();
        _meetingCalledAnimationInitializeListener?.RemoveListener();
    }
    private void OnWrapUp(WrapUpEventData data)
    {
        if (!Player.AmOwner) return;
        if (data.exiled == null) return;
        if (data.exiled != Player) return;
        if (Player.IsCrewmate())
            EndGamer.RpcEndGameWithWinner(Patches.CustomGameOverReason.NoWinner, WinType.Default, [], Color.white, "NoWinner", "");
    }
    private void OnMurder(MurderEventData data)
    {
        if (data.target != Player) return;
        DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].ParentId == Player.PlayerId)
                array[i].transform.localScale = Vector3.one * (_currentSize + 1f);
        }
    }
    private void OnExileController(ExileControllerEventData data)
    {
        if (data.instance.initData.networkedPlayer == null || data.instance.initData.networkedPlayer.PlayerId != Player.PlayerId)
            return;
        data.instance.Player.transform.localScale = Vector3.one * (_currentSize + 1f);
    }
    private void OnMeetingCalledAnimationInitialize(MeetingCalledAnimationInitializeEventData data)
    {
        if (data.outfit.PlayerName != Player.Data.DefaultOutfit.PlayerName) return;
        data.animation.playerParts.transform.localScale = Vector3.one * (_currentSize + 1f);
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player != Player) return;
        // 現在のサイズを0～100の範囲に換算（Data.MaxSizeが最大値）
        int normalizedSize = Mathf.RoundToInt((_currentSize / Data.MaxSize) * 100f);
        NameText.AddNameText(data.Player, $" ({normalizedSize})");
    }
    [CustomRPC]
    private void RpcSyncJumboSize(float size)
    {
        _currentSize = size;
    }
    private void OnFixedUpdate()
    {
        if ((!Data.SizeUpOnMeeting && (MeetingHud.Instance != null || ExileController.Instance != null)) || FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed) return;
        var p = Player.Player;
        if (p == null) return;
        // 初期位置の設定
        if (!_hasOldPosition)
        {
            _hasOldPosition = true;
            _oldPosition = p.GetTruePosition();
        }
        // サイズアップ
        if (_currentSize <= Data.MaxSize)
        {
            _currentSize += Time.fixedDeltaTime * (Data.MaxSize / Data.SizeUpSpeed);
            if (_currentSize > Data.MaxSize) _currentSize = Data.MaxSize;
            // サイズが整数で1増えたタイミングで名前表示を更新
            int currentThreshold = Mathf.RoundToInt((_currentSize / Data.MaxSize) * 100f);
            if (currentThreshold > _lastSizeThreshold)
            {
                NameText.UpdateNameInfo(Player);
                _lastSizeThreshold = currentThreshold;
            }
        }
        // 歩行音再生判定
        if (Data.MaxSize / (_currentSize > 0f ? _currentSize : Data.MaxSize) >= Data.WalkSoundSize)
        {
            _playSoundTimer -= Time.fixedDeltaTime;
            if (_oldPosition != p.GetTruePosition() && _playSoundTimer <= 0f)
            {
                Transform audioTransform = p.transform.Find("JumboAudio");
                if (audioTransform == null)
                {
                    var go = new GameObject("JumboAudio");
                    audioTransform = go.transform;
                    audioTransform.parent = p.transform;
                }
                audioTransform.localPosition = Vector3.zero;
                var audioSource = audioTransform.gameObject.GetComponent<AudioSource>() ?? audioTransform.gameObject.AddComponent<AudioSource>();
                var clip = SuperNewRoles.Modules.AssetManager.GetAsset<AudioClip>("JumboWalkSound");
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.loop = false;
                    audioSource.spatialBlend = 1f;
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    audioSource.Play();
                }
                _playSoundTimer = 0.35f;
            }
        }
        _oldPosition = p.GetTruePosition();
        // スケール更新
        if (p.cosmetics != null)
            p.cosmetics.transform.localScale = Vector3.one * ((_currentSize + 1f) * 0.5f);
        var bodyForms = p.transform.Find("BodyForms");
        if (bodyForms != null)
            bodyForms.localScale = Vector3.one * (_currentSize + 1f);
        var animations = p.transform.Find("Animations");
        if (animations != null)
            animations.localScale = Vector3.one * (_currentSize + 1f);

        if (Player.AmOwner)
        {
            syncTimer += Time.fixedDeltaTime;
            if (syncTimer >= 5f && _currentSize < Data.MaxSize)
            {
                syncTimer = 0f;
                RpcSyncJumboSize(_currentSize);
            }
        }
    }
}