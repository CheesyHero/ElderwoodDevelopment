using UnityEngine;

public class OneOf : MonoBehaviour
{
    private static OneOf instance;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);

        else instance = this;
    }
}
