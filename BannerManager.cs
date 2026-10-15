using System.Collections;
using UnityEngine;

public class BannerManager : ComponentLoader
{ 
    public bool IsPrinting 
    { get { return routine != null; } }
    public CanvasGroup group;
    public float speed = 1f;
    public float facetime = 2f;
    public float initialDelay = 2f;
     
    private void Start()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        group.alpha = 0;
    }
    private void Update()
    {
        if (initialDelay > 0) initialDelay -= Time.deltaTime;
    } 
    public void PrintReport(string report)
    {
        if (initialDelay > 0) return;

        if (routine != null)
            StopCoroutine(routine);

        SetText(report);
        To();
    }
    protected void To()
    {
        group.alpha = 0;
        routine = StartCoroutine(Animation());
    }
    Coroutine routine;
    IEnumerator Animation()
    {
        while (group.alpha < 1)
        {
            group.alpha += Time.deltaTime * speed;
            yield return null;
        } 

        float delay = 0;
        while (delay < facetime)
        {
            delay += Time.deltaTime;
            yield return null;
        }

        while (group.alpha > 0)
        { 
            group.alpha -= Time.deltaTime * speed;
            yield return null;
        } 
    }
}
