using System;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.RequestInGame;

public class LoadingUI
{
    public static void ShowLoadingUI(Transform parent, Func<string> loadingTextFunc, Func<bool> isActiveFunc = null)
    {
        // AssetManaget from LoadingUI
        GameObject loadingUI = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("LoadingUI"), parent);
        loadingUI.transform.localPosition = new(0, 0, -12);
        loadingUI.transform.localScale = Vector3.one;
        loadingUI.transform.localRotation = Quaternion.identity;
        loadingUI.AddComponent<LoadingUIComponent>().Init(loadingTextFunc, isActiveFunc);

        PassiveButton passiveButton = loadingUI.AddComponent<PassiveButton>();
        passiveButton.Colliders = new[] { loadingUI.GetComponentInChildren<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOver = new();
    }
}
public class LoadingUIComponent : MonoBehaviour
{
    public TextMeshPro text;
    public SpriteRenderer spriteRenderer;
    private Func<string> loadingTextFunc;
    private Func<bool> isActiveFunc;
    public void Init(Func<string> loadingTextFunc, Func<bool> isActiveFunc)
    {
        this.loadingTextFunc = loadingTextFunc;
        this.isActiveFunc = isActiveFunc;
    }
    public void Awake()
    {
        text = GetComponentInChildren<TextMeshPro>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    public void Update()
    {
        if (loadingTextFunc != null)
            text.text = loadingTextFunc();
        spriteRenderer.transform.Rotate(0, 0, 175 * Time.deltaTime);
        if (isActiveFunc != null && !isActiveFunc())
            Destroy(gameObject);
    }
}
