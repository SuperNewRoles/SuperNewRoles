using UnityEngine;

public class DestroyOnAnimationEndObject : MonoBehaviour
{
    private Animator animator;
    private bool hasDestroyed = false;
    private bool animationStarted = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator != null && !hasDestroyed)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // アニメーションが開始されたことを記録
            if (!animationStarted && stateInfo.normalizedTime > 0.0f)
            {
                animationStarted = true;
            }

            // アニメーションが開始されており、かつ一周完了した場合に破棄
            // ループしないアニメーションの場合は normalizedTime >= 1.0f で終了
            // ループするアニメーションの場合は、一度1.0fを超えてから再び小さい値に戻った時に終了
            if (animationStarted && stateInfo.normalizedTime >= 1.0f)
            {
                hasDestroyed = true;
                Destroy(gameObject);
            }
        }
    }
}
