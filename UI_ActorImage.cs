using UnityEngine;

public class UI_ActorImage : ComponentLoader
{
    public override void PassObject(object obj)
    { 
        if(obj is Hero hero && hero.heroClass != null)
        {
            SetImageSprite(hero.heroClass.actorImage);
            SetImageColor(Color.white);
        }
        else
        {
            SetImageColor(Color.clear);
        }
    }
}
