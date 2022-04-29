using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{/**
    [HarmonyPatch(typeof(ChatController),nameof(ChatController.AddChat))]
    class ChatHandler
    {
        public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] ref PlayerControl sourcePlayer, [HarmonyArgument(1)] ref string chatText)
        {

            if (!(bool)(UnityEngine.Object)sourcePlayer || !(bool)(UnityEngine.Object)PlayerControl.LocalPlayer)
                return false;
            GameData.PlayerInfo data1 = PlayerControl.LocalPlayer.Data;
            GameData.PlayerInfo data2 = sourcePlayer.Data;
            if (data2 == null || data1 == null || data2.IsDead && (!PlayerControl.LocalPlayer.isDead() || PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceRedRidingHood)))
                return false;
            if (__instance.chatBubPool.NotInUse == 0)
                __instance.chatBubPool.ReclaimOldest();
            ChatBubble bubble = HudManager.Instance.Chat.chatBubPool.Get<ChatBubble>();
            try
            {
                bubble.transform.SetParent(__instance.scroller.Inner);
                bubble.transform.localScale = Vector3.one;
                int num = (UnityEngine.Object)sourcePlayer == (UnityEngine.Object)PlayerControl.LocalPlayer ? 1 : 0;
                if (num != 0)
                    bubble.SetRight();
                else
                    bubble.SetLeft();
                bool flag = (bool)(UnityEngine.Object)data1.Role && (bool)(UnityEngine.Object)data2.Role && data1.Role.NameColor == data2.Role.NameColor;
                bool didVote = (bool)(UnityEngine.Object)MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
     https://media.discordapp.net/attachments/965644999578513450/967642315541856286/2022-04-24_3.png?width=875&height=492           bubble.SetCosmetics(data2);
                __instance.SetChatBubbleName(bubble, data2, data2.IsDead, didVote, flag ? data2.Role.NameColor : Color.white);
                if (SaveManager.CensorChat)
                    chatText = BlockedWords.CensorWords(chatText);
                bubble.SetText(chatText);
                bubble.AlignChildren();
                __instance.AlignAllBubbles();
                if (!HudManager.Instance.Chat.IsOpen && HudManager.Instance.Chat.notificationRoutine == null)
                    HudManager.Instance.Chat.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
                if (num != 0)
                    return false;
                SoundManager.Instance.PlaySound(__instance.MessageSound, false).pitch = (float)(0.5 + (double)sourcePlayer.PlayerId / 15.0);
            }
            catch (Exception ex)
            {
                SuperNewRolesPlugin.Logger.LogError((object)ex);
                HudManager.Instance.Chat.chatBubPool.Reclaim((PoolableBehavior)bubble);
            }
            return false;
        }
    }
    **/
}
