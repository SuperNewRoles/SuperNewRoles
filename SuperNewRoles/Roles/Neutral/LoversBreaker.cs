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
            if (AmongUsClient.Instance != null && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) return;
            if (DateTime.UtcNow >= new DateTime(2022, 12, 23, 12, 0, 0))
            {
                IsViewd = true;
                GenericPopup Popup = GameObject.Instantiate(DiscordManager.Instance.discordPopup, Camera.main.transform);
                Popup.gameObject.SetActive(true);
                Popup.transform.FindChild("Background").localScale = new(2, 2.8f, 1);
                Popup.transform.FindChild("ExitGame").localPosition = new(0f, -2f, -0.5f);
                Popup.transform.FindChild("ExitGame").GetComponentInChildren<TextMeshPro>().text = "了解！";
                TextMeshPro Title = GameObject.Instantiate(Popup.TextAreaTMP, Popup.transform);
                Title.text = "メリークリスマス！";
                Title.transform.localPosition = new(0.15f, 2, -0.5f);
                Title.transform.localScale = Vector3.one * 4.5f;
                Popup.TextAreaTMP.transform.localPosition = new(0, 0.5f, -0.5f);
                Popup.TextAreaTMP.transform.localScale = Vector3.one * 2.3f;
                Popup.TextAreaTMP.text = "メリークリスマス！\n皆さんに2つの新役職のプレゼントを用意しました！\n「爆ぜ師」と「ジャンボ」です！\n詳しくはTwitterをチェック！\n\n\n<link=\"https://twitter.com/SNRDevs\">@SNRDevs</link>";
                Popup.destroyOnClose = true;
            }
        }
    }
}