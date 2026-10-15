using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Projectile : MonoBehaviour
{
    public Vector3 spawnOffset = new(0, 1);
    public Vector3 targetOffset = new(0, 1);
    public float speed = 5f;
    public float rise = 0.5f;
    public float duration;
    private float lift; 
    private float distance;
    public float MoveToAndComplete(Vector3 position, Action function)
    {
        transform.position += spawnOffset;
        distance = Vector3.Distance(transform.position, position);
        duration = distance / speed;

        LeanTween.moveX(gameObject, position.x + targetOffset.x, duration).setOnComplete(function).setDestroyOnComplete(true);
        LeanTween.moveZ(gameObject, position.z + targetOffset.z, duration);

        lift = distance * rise;
        LeanTween.moveY(gameObject, transform.position.y + lift + targetOffset.y, duration * 0.5f).setEaseOutSine().setOnComplete(Complete);

        return duration;
    }
    private void Complete()
    { 
        LeanTween.moveY(gameObject, transform.position.y - lift + targetOffset.y, duration * 0.5f).setEaseInSine();
    }

}
