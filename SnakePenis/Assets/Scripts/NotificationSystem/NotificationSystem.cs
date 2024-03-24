using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INotification
{
    public IEnumerator ExecuteCoroutine();
}

public class NotificationSystem : MonoBehaviour
{
    public GameObject HintTemplate;
    public GameObject SubtitleHintTemplate;
    public GameObject SmallVisualHintTemplate;
    public GameObject SaveGameHintTemplate;
    private Queue<INotification> NotificationQueue;


    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(DequeueNotificationCoroutine());
    }

    IEnumerator DequeueNotificationCoroutine()
    {
        NotificationQueue = new Queue<INotification>();
        while (true)
        {
            if (NotificationQueue.Count > 0)
            {
                INotification n = NotificationQueue.Dequeue();
                yield return StartCoroutine(n.ExecuteCoroutine());
            } else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public void Enqueue(INotification notification)
    {
        NotificationQueue.Enqueue(notification);
    }
}
