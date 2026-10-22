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
        if (!parent || !sprite || parent.Leader()?.heroClass == null)
            return;

        Hero hero = parent.Leader();

        if (sprite.sprite != hero.heroClass.actorImage)
            sprite.sprite = hero.heroClass.actorImage;

        MaterialPropertyBlock properties = new();
        sprite.GetPropertyBlock(properties);

        properties.SetTexture("_MaskTex",
            hero.heroClass.colorMask != null
                ? hero.heroClass.colorMask
                : Texture2D.blackTexture);

        properties.SetColor("_TeamColor", parent.Parent.team.teamColor);
        properties.SetColor("_ArmorColor", hero.equipment.armor.color);
        properties.SetColor("_WeaponColor", hero.equipment.weapon.color);

        sprite.SetPropertyBlock(properties);
    }
}