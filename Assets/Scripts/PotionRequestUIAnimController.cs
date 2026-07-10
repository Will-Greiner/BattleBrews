using UnityEngine;
using System.Collections;

public class PotionRequestUIAnimController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float duration = 0.25f;


    public IEnumerator OpenRequest()
    {
        animator.SetTrigger("ShowUI");
        yield return new WaitUntil(() =>
        !animator.IsInTransition(0) &&
        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
    }

    public IEnumerator CloseRequest()
    {
        yield return new WaitUntil(() =>
        !animator.IsInTransition(0) &&
        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
    }
}
