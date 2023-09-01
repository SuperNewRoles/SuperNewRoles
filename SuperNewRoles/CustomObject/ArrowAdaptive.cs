using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class ArrowAdaptive
{
    public float perc = 0.925f;
    public SpriteRenderer image;
    public GameObject arrow;
    private Vector3 oldTarget;

    public static Sprite GetAdaptiveSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Arrow_adaptive.png", 200f);
    private static bool Between(float value, float min, float max) => value > min && value < max;

    public ArrowAdaptive(int bodyColorId = 0)
    {
        arrow = new GameObject("ArrowAdaptive")
        {
            layer = 5
        };
        image = arrow.AddComponent<SpriteRenderer>();
        image.sprite = GetAdaptiveSprite();
        image.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        image.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetColors(bodyColorId, image);
        PlayerMaterial.Properties Properties = new()
        {
            MaskLayer = 0,
            MaskType = PlayerMaterial.MaskType.None,
            ColorId = bodyColorId,
        };
        image.material.SetInt(PlayerMaterial.MaskLayer, Properties.MaskLayer);
    }

    public void Update(Vector3 target, int bodyColorId = 0)
    {
        if (arrow == null) return;
        oldTarget = target;

        image.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        image.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetColors(bodyColorId, image);
        PlayerMaterial.Properties Properties = new()
        {
            MaskLayer = 0,
            MaskType = PlayerMaterial.MaskType.None,
            ColorId = bodyColorId,
        };
        image.material.SetInt(PlayerMaterial.MaskLayer, Properties.MaskLayer);

        Camera main = Camera.main;
        Vector2 vector = target - main.transform.position;
        float num = vector.magnitude / (main.orthographicSize * perc);
        image.enabled = (double)num > 0.3;
        Vector2 vector2 = main.WorldToViewportPoint(target);
        if (Between(vector2.x, 0f, 1f) && Between(vector2.y, 0f, 1f))
        {
            arrow.transform.position = target - (Vector3)vector.normalized * 0.6f;
            float num2 = Mathf.Clamp(num, 0f, 1f);
            arrow.transform.localScale = new Vector3(num2, num2, num2);
        }
        else
        {
            Vector2 vector3 = new(Mathf.Clamp(vector2.x * 2f - 1f, -1f, 1f), Mathf.Clamp(vector2.y * 2f - 1f, -1f, 1f));
            float orthographicSize = main.orthographicSize;
            float num3 = main.orthographicSize * main.aspect;
            Vector3 vector4 = new(Mathf.LerpUnclamped(0f, num3 * 0.88f, vector3.x), Mathf.LerpUnclamped(0f, orthographicSize * 0.79f, vector3.y), 0f);
            arrow.transform.position = main.transform.position + vector4;
            arrow.transform.localScale = Vector3.one;
        }

        LookAt2d(arrow.transform, target);
    }

    private void LookAt2d(Transform transform, Vector3 target)
    {
        Vector3 vector = target - transform.position;
        vector.Normalize();
        float num = Mathf.Atan2(vector.y, vector.x);
        if (transform.lossyScale.x < 0f)
            num += 3.1415927f;
        transform.rotation = Quaternion.Euler(0f, 0f, num * 57.29578f);
    }
}