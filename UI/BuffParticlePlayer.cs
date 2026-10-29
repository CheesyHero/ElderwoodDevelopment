using UnityEngine;

public class BuffParticlePlayer : MonoBehaviour
{
    [Header("Buff Effects")]
    [SerializeField] private ParticleSystem power;
    [SerializeField] private ParticleSystem toughness;
    [SerializeField] private ParticleSystem magic;
    [SerializeField] private ParticleSystem resist;
    [SerializeField] private ParticleSystem speed;
    [SerializeField] private ParticleSystem accuracy;
    [SerializeField] private ParticleSystem evasion;

    public void Play(Enum_HeroAttributes stat)
    {
        ParticleSystem particle = stat switch
        {
            Enum_HeroAttributes.power => power,
            Enum_HeroAttributes.armor => toughness,
            Enum_HeroAttributes.magic => magic,
            Enum_HeroAttributes.resist => resist,
            Enum_HeroAttributes.speed => speed,
            Enum_HeroAttributes.accuracy => accuracy,
            Enum_HeroAttributes.evasion => evasion,
            _ => null
        };

        Debug.LogWarning(
            $"[BUFF PARTICLE PLAY] Stat: {stat} | " +
            $"Particle Assigned: {particle != null} | " +
            $"Particle Name: {(particle ? particle.name : "NULL")} | " +
            $"Active: {(particle ? particle.gameObject.activeInHierarchy : false)}"
        );

        if (!particle)
            return;

        particle.gameObject.SetActive(true);
        particle.Clear(true);
        particle.Play(true);
    }
}