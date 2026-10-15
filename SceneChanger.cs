using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    private static int nextIndex = -1;
    private static TransitionManager transitionPrefab;

    private static TransitionManager Transition
    {
        get
        {
            if (transitionPrefab == null)
            {
                transitionPrefab = Resources.Load<TransitionManager>("TransitionScreen");
                Debug.LogWarning($"Loaded prefab: {transitionPrefab}");
            }


            return transitionPrefab;
        }
    }

    public static void ChangeScenes(int buildIndex)
    {
        if (buildIndex == SceneManager.GetActiveScene().buildIndex)
            return;

        if (Transition != null)
        {
            LoadTransition(buildIndex);
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }

    private static void LoadTransition(int index)
    {
        nextIndex = index;
        Object.Instantiate(Transition);
    }

    public static void CompleteChangeRequest()
    { 
        if (nextIndex < 0)
        {
            Debug.LogWarning("SceneChanger: No scene change request is pending.");
            return;
        }

        SceneManager.LoadScene(nextIndex);
        nextIndex = -1;
    }

    public static void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}