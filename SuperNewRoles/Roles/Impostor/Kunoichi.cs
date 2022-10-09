using System;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;

using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    internal static class Kunoichi
    {
        public static PlayerControl GetShootPlayer(float shotSize = 0.75f)
        {
            PlayerControl result = null;
            float num = 7;
            Vector3 pos;
            Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
            var mouseAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x);

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                //自分自身は撃ち抜かれない
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;

                if (player.IsDead()) continue;

                pos = player.transform.position - PlayerControl.LocalPlayer.transform.position;
                pos = new Vector3(
                    pos.x * MathF.Cos(mouseAngle) + pos.y * MathF.Sin(mouseAngle),
                    pos.y * MathF.Cos(mouseAngle) - pos.x * MathF.Sin(mouseAngle));
                if (Math.Abs(pos.y) < shotSize && (!(pos.x < 0)) && pos.x < num)
                {
                    num = pos.x;
                    result = player;
                }
            }
            return result;
        }
        public static void KillButtonClick()
        {
            if (!RoleClass.Kunoichi.Kunai.kunai.active) return;
            if (!RoleClass.Kunoichi.HideKunai && RoleClass.NiceScientist.IsScientistPlayers.ContainsKey(CachedPlayer.LocalPlayer.PlayerId) && GameData.Instance && RoleClass.NiceScientist.IsScientistPlayers[CachedPlayer.LocalPlayer.PlayerId]) return;
            PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Kunoichi.KillCoolTime, RoleClass.Kunoichi.KillCoolTime);
            RoleClass.Kunoichi.SendKunai = RoleClass.Kunoichi.Kunai;
            RoleClass.Kunoichi.Kunai = new Kunai();
            RoleClass.Kunoichi.Kunai.kunai.transform.position = CachedPlayer.LocalPlayer.transform.position;
            RoleClass.Kunoichi.KunaiSend = true;
            RoleClass.Kunoichi.Kunais.Add(RoleClass.Kunoichi.SendKunai);
            // クリックした座標の取得（スクリーン座標からワールド座標に変換）
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector3 shotForward = Vector3.Scale(mouseWorldPos - RoleClass.Kunoichi.SendKunai.kunai.transform.position, new Vector3(1, 1, 0)).normalized;

            // 弾に速度を与える
            var body = RoleClass.Kunoichi.SendKunai.kunai.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.velocity = shotForward * 10f;
        }
        public static void Update()
        {
            Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
            var MouseAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x);
            var targetPosition = CachedPlayer.LocalPlayer.transform.position + new Vector3(0.8f * (float)Math.Cos(MouseAngle), 0.8f * (float)Math.Sin(MouseAngle));
            RoleClass.Kunoichi.Kunai.kunai.transform.position += (targetPosition - RoleClass.Kunoichi.Kunai.kunai.transform.position) * 0.4f;
            RoleClass.Kunoichi.Kunai.image.transform.eulerAngles = new Vector3(0f, 0f, (float)(MouseAngle * 360f / Math.PI / 2f));
            if (Math.Cos(MouseAngle) < 0.0)
            {
                if (RoleClass.Kunoichi.Kunai.image.transform.localScale.y > 0)
                    RoleClass.Kunoichi.Kunai.image.transform.localScale = new Vector3(1f, -1f);
            }
            else
            {
                if (RoleClass.Kunoichi.Kunai.image.transform.localScale.y < 0)
                    RoleClass.Kunoichi.Kunai.image.transform.localScale = new Vector3(1f, 1f);
            }

            if (PlayerControl.LocalPlayer.inVent)
                RoleClass.Kunoichi.Kunai.kunai.active = false;
            foreach (Kunai kunai in RoleClass.Kunoichi.Kunais.ToArray())
            {
                if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, kunai.kunai.transform.position) > 6f)
                {
                    GameObject.Destroy(kunai.kunai);
                    RoleClass.Kunoichi.Kunais.Remove(kunai);
                }
                else
                {
                    var kunaipos = kunai.kunai.transform.position;
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.IsDead()) continue;
                        if (p.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
                        if (Vector2.Distance(p.GetTruePosition() + new Vector2(0, 0.4f), kunaipos) < 0.4f)
                        {
                            if (!RoleClass.Kunoichi.HitCount.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId] = new();
                            if (!RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId].ContainsKey(p.PlayerId)) RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId][p.PlayerId] = 0;
                            RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId][p.PlayerId]++;
                            if (RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId][p.PlayerId] >= RoleClass.Kunoichi.KillKunai)
                            {
                                ModHelpers.CheckMuderAttemptAndKill(PlayerControl.LocalPlayer, p, showAnimation: false);
                                RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId][p.PlayerId] = 0;
                            }
                            RoleClass.Kunoichi.Kunais.Remove(kunai);
                            GameObject.Destroy(kunai.kunai);
                            break;
                        }
                    }
                }
            }
            // 透明化に必要な待機時間の取得と処理 (ボタン動作ではない時)
            if (!RoleClass.Kunoichi.IsWaitAndPressTheButtonToHide && RoleClass.Kunoichi.HideTime != -1)
            {
                if (!HudManager.Instance.IsIntroDisplayed)
                {
                    if (RoleClass.Kunoichi.OldPosition == CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition()) //止まっている時 ((*1)で取得した位置情報と現在の位置情報が同じ時)
                    {
                        RoleClass.Kunoichi.StopTime += Time.fixedDeltaTime;
                        // 止まっている時間が、透明化に必要な時間を越えた時
                        if (RoleClass.Kunoichi.StopTime >= RoleClass.Kunoichi.HideTime)
                        {
                            HideOn(); // 透明化する
                        }
                    }
                    else // 動き始めた時 & 動き続けている時は
                    {
                        if (RoleClass.Kunoichi.StopTime >= RoleClass.Kunoichi.HideTime)//透明化していた場合
                        {
                            HideOff(); // 透明化を解除する
                        }
                        RoleClass.Kunoichi.StopTime = 0;//止まっている時間を 0 にする
                    }
                    RoleClass.Kunoichi.OldPosition = CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(); // 現在の位置を記録する(*1)
                }
            }
            // 透明化に必要な待機時間の取得と処理 (ボタン動作の時)
            if (RoleClass.Kunoichi.IsWaitAndPressTheButtonToHide && RoleClass.Kunoichi.HideTime != -1)
            {
                if (!HudManager.Instance.IsIntroDisplayed)
                {
                    if (RoleClass.Kunoichi.OldPosition == CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition()) //止まっている時 ((*2)で取得した位置情報と現在の位置情報が同じ時)
                    {
                        RoleClass.Kunoichi.StopTime += Time.fixedDeltaTime;
                        // 止まっている時間が、透明化に必要な時間を越えた時 且つ ボタンが押された時
                        if (RoleClass.Kunoichi.StopTime >= RoleClass.Kunoichi.HideTime && RoleClass.Kunoichi.IsHideButton)
                        {
                            HideOn(); // 透明化する
                        }
                    }
                    else // 動き始めた時 & 動き続けている時は
                    {
                        ResetCoolDown(); // 動いている時は「隠れる」ボタンのクールダウンを常にリセットする
                        if (RoleClass.Kunoichi.StopTime >= RoleClass.Kunoichi.HideTime)//透明化していた場合
                        {
                            HideOff(); // 透明化を解除する
                        }
                        RoleClass.Kunoichi.StopTime = 0;//止まっている時間を 0 にする
                    }
                    RoleClass.Kunoichi.OldPosition = CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition(); // 現在の位置を記録する(*2)
                }
            }
        }
        public static void HideOn()
        {
            // 透明化する
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetScientistRPC, SendOption.Reliable, -1);
                writer.Write(true);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.SetScientistRPC(true, CachedPlayer.LocalPlayer.PlayerId);
            }
        }
        public static void HideOff()
        {
            // 透明化を解除する
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetScientistRPC, SendOption.Reliable, -1);
                writer.Write(false);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.SetScientistRPC(false, CachedPlayer.LocalPlayer.PlayerId);
            }
            //ボタン動作で行っている場合はクールダウンのリセットも行う
            if (RoleClass.Kunoichi.IsHideButton)
            {
                ResetCoolDown();
            }
        }
        public static void ResetCoolDown()
        {
            // [隠れる]ボタンのクールダウンをリセットする
            RoleClass.Kunoichi.IsHideButton = false;
            HudManagerStartPatch.KunoichiHideButton.MaxTimer = RoleClass.Kunoichi.HideTime;
            HudManagerStartPatch.KunoichiHideButton.Timer = RoleClass.Kunoichi.HideTime;
        }

        public static void SetOpacity(PlayerControl player, float opacity, bool cansee)
        {
            // Sometimes it just doesn't work?
            var color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
            try
            {
                if (player.MyRend() != null)
                    player.MyRend().color = color;

                if (player.GetSkin().layer != null)
                    player.GetSkin().layer.color = color;

                if (player.cosmetics.hat != null)
                    player.cosmetics.hat.SpriteColor = color;

                if (player.GetPet()?.rend != null)
                    player.GetPet().rend.color = color;

                if (player.GetPet()?.shadowRend != null)
                    player.GetPet().shadowRend.color = color;

                if (player.VisorSlot() != null)
                    player.VisorSlot().Image.color = color;

                if (player.cosmetics.colorBlindText != null)
                    player.cosmetics.colorBlindText.color = color;

                if (player.NameText != null)
                    if (opacity == 0.1f)
                    {
                        player.NameText().text = "";
                    }
            }
            catch { }
        }
        public static void WrapUp()
        {
            RoleClass.Kunoichi.StopTime = 0;
            foreach (PlayerControl p in RoleClass.Kunoichi.KunoichiPlayer)
            {
                RoleClass.NiceScientist.IsScientistPlayers[p.PlayerId] = false;
            }
        }
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsScientist
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
                if (!ModeHandler.IsMode(ModeId.Default)) return;
                if (__instance.myPlayer.IsRole(RoleId.Kunoichi))
                {
                    var Scientist = __instance.myPlayer;
                    if (Scientist == null || Scientist.IsDead()) return;
                    var ison = RoleClass.NiceScientist.IsScientistPlayers.ContainsKey(__instance.myPlayer.PlayerId) && GameData.Instance && RoleClass.NiceScientist.IsScientistPlayers[__instance.myPlayer.PlayerId];
                    bool canSee = !ison || PlayerControl.LocalPlayer.IsDead() || __instance.myPlayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId;

                    var opacity = canSee ? 0.1f : 0.0f;
                    if (ison)
                    {
                        opacity = Math.Max(opacity, 0);
                        Scientist.MyRend().material.SetFloat("_Outline", 0f);
                    }
                    else
                    {
                        opacity = Math.Max(opacity, 1.5f);
                    }
                    SetOpacity(Scientist, opacity, canSee);
                }
            }
        }
    }
}