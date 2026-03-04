using System.Collections;
using UnityEngine;

public class HaulJobQueueMB : MonoBehaviour
{
   public JobQueue<Job> queue;

    private void Awake()
    {
        queue = new JobQueue<Job>();
        StartCoroutine(CleanUp(1f));
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
