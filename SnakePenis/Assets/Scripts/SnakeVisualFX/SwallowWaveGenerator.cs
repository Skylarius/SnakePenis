using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodySizeWave
{
    public int BodyIndex;
    public List<float> ContributingWaves;

    public BodySizeWave()
    {
        this.BodyIndex = -1;
        this.ContributingWaves = new List<float>();
    }
    public BodySizeWave(int BodyIndex)
    {
        this.BodyIndex = BodyIndex;
        this.ContributingWaves = new List<float>();
    }

    public bool IsUseful()
    {
        return ContributingWaves.Exists(w => w > 0f);
    }
}

public class SwallowWaveGenerator : BaseSnakeComponent
{
    // Start is called before the first frame update

    public List<BodySizeWave> BodySizeWaveList;
    private PoolingSystem<BodySizeWave> BodySizeWavePoolingSystem;
    public BodySizeWave TailSizeWave;
    private bool isSwallowCoroutineRunning = false; 

    [Header("Wave Size Settings")]
    public float WaveSize = 2f;
    public float WaveSizeDelta = 1f;

    const float NULL_WAVE_VALUE = -5f;

    void Start()
    {
        if (BodySizeWaveList == null)
        {
            BodySizeWaveList = new List<BodySizeWave>();
        }
        snakeMovement = GetComponent<SnakeMovement>();
        BodySizeWavePoolingSystem = new PoolingSystem<BodySizeWave>();
        StartCoroutine(SwallowAnimationCoroutine());
    }

    /// <summary>
    /// Animation of the whole body swallowing a pill
    /// </summary>
    /// <returns></returns>
    IEnumerator SwallowAnimationCoroutine()
    {
        if (isSwallowCoroutineRunning)
        {
            yield break;
        }
        isSwallowCoroutineRunning = true;
        StartCoroutine(WaveGarbageCollectorCoroutine());
        GameObject SnakeBodyPart;
        Transform SnakeBoneTransform;
        while (true)
        {
            // Apply swallow scale to all snake except for Tail
            if (BodySizeWaveList.Count > 0)
            {
                lock (BodySizeWaveList)
                { 
                    foreach (BodySizeWave bodySizeWave in BodySizeWaveList)
                    {
                        //SnakeBodyPart = snakeMovement.SnakeBody[bodySizeWave.BodyIndex];
                        //if (SnakeBodyPart == null)
                        //{
                        //    continue;
                        //}
                        //SnakeBoneTransform = snakeMovement.GetSnakeBoneTransform(SnakeBodyPart);
                        SnakeBoneTransform = snakeMovement.GetSnakeBoneTransformFromBodyIndex(bodySizeWave.BodyIndex);
                        if (SnakeBoneTransform)
                        {
                            SumWavesToTransform(bodySizeWave, SnakeBoneTransform);
                        }
                    }
                }
            }
            // Perform action on Tail (this is done to avoid the tail to flickr)
            SnakeBodyPart = snakeMovement.Tail;
            if (SnakeBodyPart == null)
            {
                continue;
            }
            SnakeBoneTransform = SnakeBodyPart.transform;
            if (SnakeBoneTransform)
            {
                SumWavesToTransform(TailSizeWave, SnakeBoneTransform);
            }
            yield return new WaitForEndOfFrame();
        }
        isSwallowCoroutineRunning = false;
        StopCoroutine(WaveGarbageCollectorCoroutine());
    }

    void SumWavesToTransform(BodySizeWave bodySizeWave, Transform SnakeBoneTransform)
    {
        float waveSum = 0f;
        for (int i = 0; i < bodySizeWave.ContributingWaves.Count; i++)
        {
            float delta = bodySizeWave.ContributingWaves[i];
            if (delta > 0)
            {
                waveSum += bodySizeWave.ContributingWaves[i];
            }
        }
        SnakeBoneTransform.localScale = Vector3.ClampMagnitude(Vector3.one * (1 + waveSum), 5);
    }

    public void SwallowAtBodyPart(int index)
    {
        StartCoroutine(BodyPartAnimationCoroutine(index));
    }

    IEnumerator BodyPartAnimationCoroutine(int index)
    {
        snakeMovement.SnakeBody[index].GetComponent<Collider>().enabled = false;
        float t = 0;
        float T = 1f;
        BodySizeWave bodySizeWave;
        if (index == snakeMovement.SnakeBody.Count - 1)
        {
            bodySizeWave = TailSizeWave;
        }
        else
        {
            int bodySizeWaveIndex = BodySizeWaveList.FindIndex(e => e.BodyIndex == index);
            if (bodySizeWaveIndex < 0)
            {
                bodySizeWave = BodySizeWavePoolingSystem.GetPooledObject();
                bodySizeWave.BodyIndex = index;
                BodySizeWaveList.Add(bodySizeWave);
            }
            else
            {
                bodySizeWave = BodySizeWaveList[bodySizeWaveIndex];
            }
        }
        int waveIndex = AddNewContributingWave(bodySizeWave);
        float WaveAmplitude = Random.Range(WaveSize - WaveSizeDelta, WaveSize + WaveSizeDelta);
        while (t < T)
        {
            bodySizeWave.ContributingWaves[waveIndex] = WaveAmplitude*Mathf.Sin(Mathf.PI * t / T);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        bodySizeWave.ContributingWaves[waveIndex] = NULL_WAVE_VALUE;
        snakeMovement.SnakeBody[index].GetComponent<Collider>().enabled = true;
    }

    IEnumerator WaveGarbageCollectorCoroutine()
    {
        while (isSwallowCoroutineRunning)
        {
            if (BodySizeWaveList.Count > 0)
            {
                lock(BodySizeWaveList)
                {
                    int i = 0;
                    while (i < BodySizeWaveList.Count)
                    {
                        if (BodySizeWaveList[i].IsUseful())
                        {
                            i++;
                        } else
                        {
                            BodySizeWavePoolingSystem.StorePooledObject(BodySizeWaveList[i]);
                            BodySizeWaveList.RemoveAt(i);
                        }
                    }

                    //int lastUsefulIndex = BodySizeWaveList.FindLastIndex(e => e.ContributingWaves.Exists(w => w > 0f));
                    //if (lastUsefulIndex == -1) //Every element is useless
                    //{
                    //    BodySizeWavePoolingSystem.StorePooledObjectList(BodySizeWaveList);
                    //    BodySizeWaveList.Clear();
                    //} 
                    //else if (lastUsefulIndex != BodySizeWaveList.Count - 1) //If not every element is useless BUT if the useful one is not the last (in that case, do nothing)
                    //{
                    //    for (int i = lastUsefulIndex + 1; i < BodySizeWaveList.Count; i++)
                    //    {
                    //        BodySizeWavePoolingSystem.StorePooledObject(BodySizeWaveList[i]);
                    //        BodySizeWaveList.RemoveAt(i);
                    //    }
                    //    //BodySizeWaveList.RemoveRange(lastUsefulIndex + 1, BodySizeWaveList.Count - lastUsefulIndex - 1);
                    //}
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Add new contributing wave and returns the handle of it.
    /// </summary>
    /// <returns></returns>
    int AddNewContributingWave(BodySizeWave bodySizeWave)
    {
        lock(bodySizeWave.ContributingWaves)
        {
            for (int index=0; index < bodySizeWave.ContributingWaves.Count; index++)
            {
                if (bodySizeWave.ContributingWaves[index] == NULL_WAVE_VALUE)
                {
                    bodySizeWave.ContributingWaves[index] = 0f;
                    return index;
                }
            }
            bodySizeWave.ContributingWaves.Add(0f);
            return bodySizeWave.ContributingWaves.Count - 1;
        }
    }


    // Never Used
    public void RestartSwallowAnimationCoroutine()
    {
        StopCoroutine(SwallowAnimationCoroutine());
        StopCoroutine(WaveGarbageCollectorCoroutine());
        isSwallowCoroutineRunning = false;
        StartCoroutine(SwallowAnimationCoroutine());
    }





}
