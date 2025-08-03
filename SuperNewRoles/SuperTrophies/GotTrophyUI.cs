using System.Collections.Generic;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.SuperTrophies;

public static class GotTrophyUI
{
    public static void Initialize(List<ISuperTrophy> gotTrophies)
    {
        if (gotTrophies == null)
            return;
        float baseX = 9f;
        float baseY = 1.95f;
        float spacingY = 0.6f;
        float zPosition = -100f;
        float scale = 0.8f;

        // gotTrophies の数に応じて UI を生成し、順次スライドインで表示する
        for (int i = 0; i < gotTrophies.Count; i++)
        {
            // UI オブジェクトを生成
            var trophyUI = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("GetTrophyUI"));

            // 各トロフィーの最終表示位置を計算（縦方向に spacingY ずつずらす）
            float targetY = baseY - spacingY * i;
            // 初期位置は最終位置より少し下から開始
            float initialY = targetY - 0.5f;
            trophyUI.transform.localPosition = new Vector3(baseX, targetY, zPosition);
            trophyUI.transform.localScale = Vector3.one * scale;

            // 必要に応じて、gotTrophies[i] の情報を UI に適用する処理を追加可能
            // 例: trophyUI.GetComponent<TrophyDisplay>()?.SetTrophy(gotTrophies[i]);

            // 各トロフィーが順番にスライドインするよう、遅延時間を調整
            float delay = 0.5f + i * 0.1f;
            DelaySlideIn(trophyUI, targetY, delay);

            SetupTrophyUI(trophyUI, gotTrophies[i]);
        }
    }
    public static void DelaySlideIn(GameObject obj, float y, float delay)
    {
        new LateTask(() =>
        {
            if (obj != null)
                obj.AddComponent<SlideAnimator>().Initialize(new(8.7f, y, -100f), new(3.3f, y, -100f), 1.5f);
        }, delay, "DelaySlideIn");
    }
    public static void SetupTrophyUI(GameObject obj, ISuperTrophy trophy)
    {
        obj.transform.Find("TrophyTitle").GetComponent<TextMeshPro>().text = ModTranslation.GetString("SuperTrophy.trophy." + trophy.TrophyId.ToString());
    }
    public class SlideAnimator : MonoBehaviour
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public float duration;
        private float elapsedTime;

        public void Initialize(Vector3 startPos, Vector3 endPos, float duration)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            this.duration = duration;
            this.elapsedTime = 0f;
            transform.localPosition = startPos; // 初期位置を設定
        }

        public void Update()
        {
            if (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                // ease-out cubic：初めはもっと速く進み、最後はゆっくり止まる
                float easedT = 1f - Mathf.Pow(1f - t, 3);
                transform.localPosition = Vector3.Lerp(startPos, endPos, easedT);
            }
            else
            {
                transform.localPosition = endPos; // 最終位置を明示的に設定
                Destroy(this); // アニメーション完了後、自身を破棄
            }
        }
    }
}
