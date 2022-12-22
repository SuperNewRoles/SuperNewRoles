using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral
{
    public static class LoversBreaker
    {
        //ここにコードを書きこんでください
        static bool IsViewd = false;
        static bool IsNotSetted = false;
        public static void LateUpdate()
        {
            if (!IsNotSetted && DateTime.UtcNow >= new DateTime(2022, 12, 23, 12, 0, 0))
            {
                IsNotSetted = true;
                IsViewd = true;
                return;
            }
            IsNotSetted = true;
            if (IsViewd) return;
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) return;
            if (DateTime.UtcNow >= new DateTime(2022, 12, 23, 12, 0, 0))
            {
                IsViewd = true;
                GenericPopup Popup = GameObject.Instantiate(DiscordManager.Instance.discordPopup, Camera.main.transform);
                Popup.gameObject.SetActive(true);
                Popup.transform.FindChild("Background").localScale = new(2, 2.8f, 1);
                Popup.transform.FindChild("ExitGame").localPosition = new(0f, -2f, -0.5f);
                TextMeshPro Title = GameObject.Instantiate(Popup.TextAreaTMP, Popup.transform);
                Popup.TextAreaTMP.transform.localPosition = new(-2, 1.8f, -0.5f);
                Popup.TextAreaTMP.transform.localScale = Vector3.one * 1.5f;
                Popup.TextAreaTMP.text = "メリークリスマス！\n明日はクリスマスイブですね！\n皆さんにクリスマスプレゼントを用意しました！\n";
            }
        }
    }
}