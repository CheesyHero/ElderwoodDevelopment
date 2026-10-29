
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Combatant_StatusParticleHandler : MonoBehaviour
{
    [SerializeField] BattleCombatant parent;
    public Dictionary<StatusCondition, ParticleSystem> Links = new();

    private void Awake()
    {
        if (!parent)
            parent = GetComponentInParent<BattleCombatant>();

        AutoBattleManager.OnBattleEvent += UpdateChildren;
    }

    private void OnDestroy()
    {
        AutoBattleManager.OnBattleEvent -= UpdateChildren;

        foreach (var particle in Links.Values)
        {
            if (particle)
                Destroy(particle.gameObject);
        }

        Links.Clear();
    }

    public void UpdateChildren(object _)
    {
        if (!parent || parent.hero == null) return;

        bool defeated = parent.hero.health.IsDead;

        // Add particles for new status conditions
        foreach (var status in parent.StatusConditions)
        {
            if (!Links.ContainsKey(status) && status.status.idleParticle)
            {
                ParticleSystem particle = Instantiate(
                    status.status.idleParticle,
                    parent.transform.position,
                    parent.transform.rotation,
                    parent.transform
                );

                Links.Add(status, particle);
            }
        }

        // Remove particles for expired status conditions
        List<StatusCondition> toRemove = new();

        foreach (var link in Links)
        {
            if (!parent.StatusConditions.Contains(link.Key))
                toRemove.Add(link.Key);
        }

        foreach (var status in toRemove)
        {
            if (Links.TryGetValue(status, out ParticleSystem particle))
            {
                if (particle)
                    Destroy(particle.gameObject);

                Links.Remove(status);
            }
        }

        // Disable all particles when defeated, re-enable on revival
        foreach (var particle in Links.Values)
        {
            if (!particle) continue;

            if (defeated)
            {
                if (particle.isPlaying)
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            else if (!particle.isPlaying)
            {
                particle.Play();
            }
        }
    }
}
