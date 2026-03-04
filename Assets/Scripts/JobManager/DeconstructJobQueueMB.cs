using System.Collections;
using UnityEngine;

public class DeconstructJobQueueMB : MonoBehaviour
{
    public JobQueue<Job> queue;

    private void Awake()
    {
        queue = new JobQueue<Job>();
        StartCoroutine(CleanUp(1));
    }

    IEnumerator CleanUp(float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            queue.CleanUp();
        }
    }
}
