using System;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    public BattleCombatant parent;
    public Animator animator; 

    public void PlayAnimation(string id)
    {
        if (!animator) return;
        animator.Play(id, 0, 0f);
    }
}
