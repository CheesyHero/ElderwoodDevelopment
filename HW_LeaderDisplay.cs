using UnityEngine;

public class HW_LeaderDisplay : MonoBehaviour
{
    HexWalker parent;
    SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        parent = GetComponentInParent<HexWalker>(); 
    }

    private void LateUpdate()
    {
        if (!parent || parent.Leader() == null || !parent.Leader().heroClass)
            return;

        Sprite image = parent.Leader().heroClass.actorImage;
        if (parent && sprite && sprite.sprite != image)
            sprite.sprite = image;
    }

}
