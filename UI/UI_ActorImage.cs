using UnityEngine;

public class UI_ActorImage : ComponentLoader
{
    [SerializeField] private Material teamColorMaterial;

    private Material actorMaterial;

    private void Start()
    {
        if (image && teamColorMaterial)
        {
            actorMaterial = new Material(teamColorMaterial);
            image.material = actorMaterial;
        }
    }

    public override void PassObject(object obj)
    {
        if (obj is Hero hero && hero.heroClass != null)
        {
            SetImageSprite(hero.heroClass.actorImage);
            SetImageColor(Color.white);

            if (actorMaterial)
            {
                actorMaterial.SetTexture(
                    "_MaskTex",
                    hero.heroClass.colorMask != null
                        ? hero.heroClass.colorMask
                        : Texture2D.blackTexture
                );

                actorMaterial.SetColor(
                    "_TeamColor",
                    hero.team != null
                        ? hero.team.teamColor
                        : Color.white
                );

                actorMaterial.SetColor(
                    "_ArmorColor",
                    hero.equipment?.armor != null
                        ? hero.equipment.armor.color
                        : Color.white
                );

                actorMaterial.SetColor(
                    "_WeaponColor",
                    hero.equipment?.weapon != null
                        ? hero.equipment.weapon.color
                        : Color.white
                );
            }
        }
        else
        {
            SetImageColor(Color.clear);
        }
    }

    private void OnDestroy()
    {
        if (actorMaterial)
            Destroy(actorMaterial);
    }
}