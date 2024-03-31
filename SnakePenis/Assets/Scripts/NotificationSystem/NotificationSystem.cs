using System;
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
    public float TimeQueueCheck = 3f;
    private Queue<INotification> NotificationQueue;
    private WaitForSeconds waitForSeconds;

    public event Action<INotification> OnNotificationExecuted = delegate { };


    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(DequeueNotificationCoroutine());
    }

    private void Start()
    {
        waitForSeconds = new WaitForSeconds(TimeQueueCheck);
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
                OnNotificationExecuted.Invoke(n);

            } else
            {
                yield return waitForSeconds;
            }
        }
    }

    public void Enqueue(INotification notification)
    {
        NotificationQueue.Enqueue(notification);
    }
}
