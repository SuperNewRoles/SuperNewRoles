using UnityEngine;

namespace SuperNewRoles.WaveCannonObj;
public class WCSantaHandler : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public static readonly float SantaSpeed = 6.5f;
    public void Start()
    {
        Renderer = gameObject.AddComponent<SpriteRenderer>();
        Renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannon.Santa.png", 115f);
    }
    public void Update()
    {
        if (transform.localScale.y < 0.725f)
            transform.localScale += new Vector3(-0.05f, 0.05f, 0.05f);
        transform.localPosition += new Vector3(SantaSpeed * Time.deltaTime,0,0);
        if (Vector2.Distance(transform.position, PlayerControl.LocalPlayer.transform.position) > 30)
            Destroy(gameObject);
    }
}