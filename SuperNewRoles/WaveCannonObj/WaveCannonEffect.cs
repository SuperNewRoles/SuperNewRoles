using UnityEngine;

public class WaveCannonEffect : MonoBehaviour
{
    public Animator head_animator;
    public Animator roop_animator;
    public Collider2D[] WaveColliders;

    public void SetChargeState(bool isCharge)
    {
        head_animator.SetBool("IsCharging", isCharge);
        roop_animator.gameObject.SetActive(!isCharge);
    }
}