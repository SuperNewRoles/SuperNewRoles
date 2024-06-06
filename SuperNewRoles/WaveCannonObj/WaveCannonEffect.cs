using System;
using UnityEngine;

public class WaveCannonEffect : MonoBehaviour
{
    public WaveCannonEffect(IntPtr intPtr) : base(intPtr)
    {
    }

    public Animator headanimator;
    public Animator roopanimator;
    public Animator chargeanimator;
    public Collider2D[] WaveColliders;

    private void Awake()
    {
        headanimator = transform.FindChild("Cannon_Head").GetComponent<Animator>();
        roopanimator = transform.FindChild("Cannon_Roop").GetComponent<Animator>();
        chargeanimator = transform.FindChild("Cannon_Charge").GetComponent<Animator>();
        WaveColliders = GetComponentsInChildren<Collider2D>();
        SetChargeState(true);
    }

    public void SetChargeState(bool isCharge)
    {
        headanimator.gameObject.SetActive(!isCharge);
        roopanimator.gameObject.SetActive(!isCharge);
        chargeanimator.gameObject.SetActive(isCharge);
    }
}