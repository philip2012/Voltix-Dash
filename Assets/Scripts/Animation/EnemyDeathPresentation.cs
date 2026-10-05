using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class EnemyDeathPresentation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer visual;
    private static readonly int Dead = Animator.StringToHash("Dead");
    private static readonly int Death = Animator.StringToHash("Base Layer.Death");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        visual = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        animator.SetBool(Dead, true);
        animator.Play(Death, 0, 0f);
        animator.Update(0f);
    }
    public void SetFacing(bool left) => visual.flipX = left;
    private void Update()
    {
        if (Time.timeScale > 0f && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            Destroy(gameObject);
    }
}
