using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BodySizeWave
{
    public int BodyIndex;
    public List<float> ContributingWaves;
    public BodySizeWave(int BodyIndex)
    {
        this.BodyIndex = BodyIndex;
        this.ContributingWaves = new List<float>();
    }
}

public class SwallowWaveGenerator : MonoBehaviour
{
    // Start is called before the first frame update

    public List<BodySizeWave> BodySizeWaveList;
    public BodySizeWave TailSizeWave;
    private SnakeMovement snakeMovement;

    [Header("Wave Size Settings")]
    public float WaveSize = 2f;
    public float WaveSizeDelta = 1f;

    const float NULL_WAVE_VALUE = -5f;

    void Start()
    {
        BodySizeWaveList = new List<BodySizeWave>();
        snakeMovement = GetComponent<SnakeMovement>();
        StartCoroutine(SwallowAnimationCoroutine());
    }

    /// <summary>
    /// Animation of the whole body swallowing a pill
    /// </summary>
    /// <returns></returns>
    IEnumerator SwallowAnimationCoroutine()
    {
        GameObject SnakeBodyPart;
        Transform SnakeBoneTransform;
        while (true)
        {
            // Apply swallow scale to all snake except for Tail
            if (BodySizeWaveList.Count > 0)
            {
                foreach (BodySizeWave bodySizeWave in BodySizeWaveList)
                {
                    SnakeBodyPart = snakeMovement.SnakeBody[bodySizeWave.BodyIndex];
                    if (SnakeBodyPart == null)
                    {
                        continue;
                    }
                    SnakeBoneTransform = snakeMovement.GetSnakeBoneTransform(SnakeBodyPart);
                    if (SnakeBoneTransform)
                    {
                        SumWavesToTransform(bodySizeWave, SnakeBoneTransform);
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
                bodySizeWave = new BodySizeWave(index);
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



    
}