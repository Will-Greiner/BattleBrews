using UnityEngine;
using System.Collections;

public class FighterAnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float walkInDuration = 1.5f;
    [SerializeField] float walkOutDuration = 1.5f;
    [SerializeField] GameObject body;

    public IEnumerator WalkIn()
    {
        body.SetActive(true);
        animator.SetTrigger("WalkIn");
        yield return new WaitForSeconds(walkInDuration);
    }

    public IEnumerator WalkOut()
    {
        animator.SetTrigger("WalkOut");
        yield return new WaitForSeconds(walkOutDuration);
        body.SetActive(false);
    }

}
