
using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleParticleTiming
{
    OnStart,
    OnAnimate,
    OnHit,
    OnComplete
}

public enum BattleParticleTarget
{
    Source,
    Target,
    Unit,
    Field
}

public class BattleParticleController : MonoBehaviour
{
    public Vector3 offset;

    [Space(8)]
    public float triggerTime = 0.25f;
    public float delay = 0f;

    [Space(8)]
    public BattleParticleTiming timing;
    public BattleParticleTarget target;

    [Header("Components")]
    public AudioSource audioSource;

    private ParticleSystem particles;
    private Action onComplete;
    private bool completed;

    private static readonly List<BattleParticleController> activeParticles = new();

    private void Awake()
    {
        if (!particles) particles = GetComponent<ParticleSystem>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        transform.position += offset;
    }

    private void OnEnable()
    {
        if (!activeParticles.Contains(this))
            activeParticles.Add(this);
    }

    private void OnDisable()
    {
        CancelInvoke();
        activeParticles.Remove(this);
    }

    private void OnDestroy()
    {
        activeParticles.Remove(this);
    }

    public void Play(Action complete = null)
    {
        CancelInvoke();

        onComplete = complete;
        completed = false;

        if (delay > 0f)
            Invoke(nameof(StartParticles), delay);
        else
            StartParticles();
    }

    private void StartParticles()
    {
        if (particles) particles.Play();
        if (audioSource && audioSource.clip) audioSource.Play();

        Invoke(nameof(Complete), Mathf.Max(0f, triggerTime));
    }

    private void Complete()
    {
        if (completed) return;

        completed = true;

        Action callback = onComplete;
        onComplete = null;

        callback?.Invoke();

        //Destroy(gameObject);
    }

    public static bool HasActiveParticles()
    {
        return activeParticles.Count > 0;
    }
}
