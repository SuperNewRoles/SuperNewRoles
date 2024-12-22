using UnityEngine;

namespace SuperNewRoles.WaveCannonObj;
public class WCSantaHandler : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public static readonly float SantaSpeed = 6.5f;
    public static bool IsFlipX;
    public float moveX;
    public static bool reflection = false;
    public static float Angle;
    public static Vector3 WiseManVector;
    public static float Xdiff;

    public void Start()
    {
        Renderer = gameObject.AddComponent<SpriteRenderer>();
        Renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannon.Santa.png", 115f);
    }
    public void Update()
    {
        int flip = transform.parent == null && IsFlipX ? -1 : 1;
        if (transform.localScale.y < 0.725f)
            transform.localScale += new Vector3(flip * -0.05f, 0.05f, 0.05f);

        //回転の残骸
        //moveX += SantaSpeed * Time.deltaTime;
        //float diff = moveX - Xdiff;
        //if (reflection && diff > 0)
        //{
        //    transform.position = new(WiseManVector.x, transform.position.y, transform.position.z);
        //    transform.localRotation = Quaternion.AngleAxis(Angle, Vector3.forward);
        //    transform.localPosition = transform.localRotation * new Vector3(diff, 0, 0);
        //}
        //else
        //{
        //    transform.localPosition = new Vector3(flip * moveX, transform.localPosition.y, transform.localPosition.z);
        //}

        transform.localPosition += new Vector3(flip * SantaSpeed * Time.deltaTime, 0, 0);
        if (Vector2.Distance(transform.position, PlayerControl.LocalPlayer.transform.position) > 30)
            Destroy(gameObject);
    }
}