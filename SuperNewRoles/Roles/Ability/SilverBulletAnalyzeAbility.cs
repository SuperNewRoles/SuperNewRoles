using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability
{
    internal class SilverBulletAnalyzeAbility : VentTargetCustomButtonBase
    {
        public override string buttonText => ModTranslation.GetString("SilverBulletAnalyzeButtonName");
        public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SilverBulletAnalyzeButton.png");
        public override float DefaultTimer => cooltime;
        private float cooltime;
        protected override KeyType keytype => KeyType.Ability1;
        public override Color32 OutlineColor => SilverBullet.Instance.RoleColor;

        public override ShowTextType showTextType => ShowTextType.ShowWithCount;

        private static List<int> _willSendChat = new();
        private static Dictionary<int, List<byte>> _lastUsedVentData = new();

        private EventListener<MeetingStartEventData> _meetingStartListener;
        private EventListener<VentEnterEventData> _ventEnterListener;


        public SilverBulletAnalyzeAbility(int count, float cooltime)
        {
            Count = count;
            this.cooltime = cooltime;
        }

        public override void AttachToLocalPlayer()
        {
            base.AttachToLocalPlayer();
            _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
            _ventEnterListener = VentEnterEvent.Instance.AddListener(OnVentEnter);
        }

        public override void DetachToLocalPlayer()
        {
            base.DetachToLocalPlayer();
            _meetingStartListener?.RemoveListener();
            _ventEnterListener?.RemoveListener();
        }

        private void OnVentEnter(VentEnterEventData data)
        {
            if (!_lastUsedVentData.ContainsKey(data.vent.Id))
            {
                _lastUsedVentData[data.vent.Id] = new List<byte>();
            }
            _lastUsedVentData[data.vent.Id].Add(data.player.PlayerId);
        }

        public override void OnClick()
        {
            if (Count <= 0 || Target == null) return;

            _willSendChat.Add(Target.Id);
            Count--;
            ResetTimer();
        }

        public override bool CheckIsAvailable()
        {
            return Count > 0 && TargetIsExist && Player.Player.CanMove;
        }

        public override bool CheckHasButton()
        {
            return Player.IsAlive();
        }

        private void OnMeetingStart(MeetingStartEventData data)
        {
            if (_willSendChat.Count <= 0) return;

            string textLine = "|-------------------------------------------------------------|";
            string baseText = ModHelpers.Cs(SilverBullet.Instance.RoleColor, $"{textLine}\n|{ModTranslation.GetString("SilverBulletName")}|\n{textLine}") + "\n\n";

            foreach (int ventId in _willSendChat)
            {
                StringBuilder text = new(baseText);
                if (!_lastUsedVentData.TryGetValue(ventId, out var usedPlayers) || usedPlayers.Count == 0)
                {
                    text.Append(ModTranslation.GetString("SilverBulletVentNotUsed"));
                }
                else
                {
                    text.Append(string.Format(ModTranslation.GetString("SilverBulletVentUsed"), usedPlayers.Count) + "\n\n");
                    if (SilverBullet.SilverBulletAnalysisLight)
                    {
                        text.AppendLine("|-----------------------");
                        int index = 1;
                        foreach (byte playerId in usedPlayers)
                        {
                            var player = ExPlayerControl.ById(playerId);
                            if (player == null) continue;
                            text.AppendLine(string.Format(
                                ModTranslation.GetString($"SilverBulletVentUsedColor{(CustomColors.IsLighter(player) ? "Light" : "Dark")}"), index));
                            index++;
                        }
                        text.AppendLine("|-----------------------");
                    }
                }
                HudManager.Instance.Chat.AddChat(Player.Player, text.ToString());
            }
            _willSendChat.Clear();
            _lastUsedVentData.Clear();
        }
    }
}