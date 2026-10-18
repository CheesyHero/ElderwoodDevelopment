using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleParticleTiming
{
    OnStart, //
    OnAnimate,
    OnHit,
    OnComplete
}

public enum BattleParticleTarget
{
    Source, // The user of the move
    Target, // The target of the move 
    Unit, // The center of the entire enemy unit
    Field // In the center of both units
}

public class BattleParticleController : MonoBehaviour
{
    public float triggerTime = 0.25f;
    public float delay = 0f;
    [Space(8)]
    public BattleParticleTiming timing;
    public BattleParticleTarget target;

    private ParticleSystem particles;
    private Action onComplete;
    private bool completed;

    private static readonly List<BattleParticleController> activeParticles = new();

    private void Awake()
    {
        if (!particles) particles = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (!activeParticles.Contains(this))
            activeParticles.Add(this); 
    }

    private void OnDisable()
    {
        activeParticles.Remove(this);
    }
    private void OnDestroy()
    {
        activeParticles.Remove(this);
    }

    public void Play(Action complete = null)
    {
        if (particles) particles.Play();

        onComplete = complete;
        completed = false;

        CancelInvoke(nameof(Complete));
        Invoke(nameof(Complete), triggerTime);
    }

    private void Complete()
    {
        if (completed) return;

        completed = true;

        Action callback = onComplete;
        onComplete = null;

        callback?.Invoke();

        Destroy(gameObject);
    } 

    public static bool HasActiveParticles()
    {
        return activeParticles.Count > 0;
    }
}