using System.Collections; 
using UnityEngine;
using UnityEngine.UI;

public class UI_CommandPoints : MonoBehaviour
{
    public static UI_CommandPoints manager;

    public float updateSpeed = 0.2f;

    public Color mainColor = Color.yellowNice;
    public Color previewCostColor = Color.indianRed;
    public Color errorCostColor = Color.darkRed;

    public Image[] children;
    public bool player;

    private void Awake()
    {
        manager = this;

        children = GetComponentsInChildren<Image>();

        Army.OnCommandPointsGained += CheckPlayer;
        Army.OnCommandPointsLost += CheckPlayer;
    }
    private void Start()
    {
        foreach (var child in children)
            child.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        if (manager == this)
            manager = null;

        Army.OnCommandPointsGained -= CheckPlayer;
        Army.OnCommandPointsLost -= CheckPlayer;
    }
    public static void ShowPreview(int cost)
    {
        if (!manager) return;

        manager.Preview(cost);
    }

    public static void EndPreview()
    {
        if (!manager) return;

        manager.CheckPlayer();
        manager.ResetAllColors();
    }

    private void Preview(int cost)
    {
        if (GameManager.Player == null || children == null)
            return;

        int available = GameManager.Player.commandPoints;
        int max = children.Length;
        int shown = Mathf.Clamp(Mathf.Max(cost, available), 0, max);

        for (int i = 0; i < max; i++)
        {
            bool shouldShow = i < shown;
            children[i].gameObject.SetActive(shouldShow);

            if (!shouldShow) continue;

            if (cost > available)
            {
                children[i].color = i >= available ? errorCostColor : mainColor;
            }
            else
            {
                int previewStart = available - cost;
                var child = children[i];
                child.color = i >= previewStart ? previewCostColor : mainColor;

                if(child.color == previewCostColor)
                {
                    child.transform.localScale = Vector3.one * 1.2f;
                    LeanTween.cancel(child.gameObject);
                    LeanTween.scale(child.gameObject, Vector3.one, 0.2f);
                } 
            }
        }
    }


    public void CheckPlayer(Army _) => CheckPlayer();
    public void CheckPlayer()
    { 
        if (ActiveChildren() != GameManager.Player.commandPoints)
            SetActiveImages(GameManager.Player);
    }
    public int ActiveChildren()
    {
        int t = 0;
        foreach (var item in children)
            if (item.gameObject.activeInHierarchy) t++;

        return t;
    }
    private Coroutine updateRoutine;

    public void SetActiveImages(Army target)
    {
        if (target == null) 
            return;

        int desired = target.commandPoints;
        if (desired == ActiveChildren())
            return; 

        if (updateRoutine != null)
            StopCoroutine(updateRoutine);

        if (gameObject.activeInHierarchy)
            updateRoutine = StartCoroutine(SetActiveImagesRoutine(desired));
    } 
    private void ResetAllColors()
    {
        foreach (var item in children)
            item.color = mainColor;
    }
    private IEnumerator SetActiveImagesRoutine(int desired)
    {
        desired = Mathf.Clamp(desired, 0, children.Length);

        while (ActiveChildren() != desired)
        {
            int current = ActiveChildren();

            if (current < desired)
            {
                children[current].gameObject.SetActive(true);
                children[current].color = mainColor;
            }
            else
            {
                children[current - 1].gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(updateSpeed);
        }

        for (int i = 0; i < desired; i++)
            children[i].color = mainColor;

        updateRoutine = null;
    }
}
