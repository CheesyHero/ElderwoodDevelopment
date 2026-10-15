using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAgent : MonoBehaviour
{ 
    public int walkDistance = 3;
    public int jumpHeight = 1;
    public float walkSpeed = 0.4f; 

    private void Start()
    {
        HighlightArea.HighlightMovement(GetCurrentTile(), this);
    }
    public bool Walking
    { get { return routine != null; } }

    Coroutine routine;
    public void Walk(Queue<WalkSpace> queue)
    {
        Debug.LogWarning(name + " recieved queue with count = " + queue.Count);
        routine ??= StartCoroutine(WalkCycle(queue));
    }
    private IEnumerator WalkCycle(Queue<WalkSpace> queue)
    { 
        Debug.LogWarning(name +  " : Starting Walk Cycle: " + queue.Count + " spaces");

        while(queue.Count > 0)
        {
            WalkSpace next = queue.Dequeue();
            LookAt(next.WalkPoint());
            LeanTween.move(gameObject, next.WalkPoint(), walkSpeed);
            yield return new WaitForSeconds(walkSpeed);
        }

        routine = null;
    }
    public WalkSpace GetCurrentTile()
    {
        Ray ray = new(transform.position + Vector3.up / 2, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2))
        {
            var find = hit.transform.GetComponent<WalkSpace>();
            if(find) return find;
        }

        return null;
    }

    public void LookAt(Vector3 target)
    { 
        transform.LookAt(target);
        transform.eulerAngles = new(0, transform.eulerAngles.y, 0);
    }
}
