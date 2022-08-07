using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance;
    public Vector2 RangeX;
    public Vector2 RangeZ;
    public GameObject powerUpTemplate;
    public float spawnFrequency = 0.5f;
    public int MaxPowerUpAmout;
    private static int PowerUpAmount;
    // Start is called before the first frame update
    void Start()
    {
        PowerUpAmount = 0;
        StartCoroutine(SpawnPowerUpCoroutine());   
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public static void IncreaseSpawnFrequency(float delta)
    {
        Instance.spawnFrequency += delta;
    }

    public static void DecreasePowerUpAmount()
    {
        PowerUpAmount--;
    }
    IEnumerator SpawnPowerUpCoroutine()
    {
        while (true)
        {
            if (PowerUpAmount < MaxPowerUpAmout)
            {
                GameObject newPowerUp = Instantiate(powerUpTemplate);
                newPowerUp.SetActive(true);
                newPowerUp.transform.position = Vector3.right * Mathf.Round(Random.Range(RangeX.x, RangeX.y)) + Vector3.forward * Mathf.Round(Random.Range(RangeZ.x, RangeZ.y));
                PowerUpAmount++;
            }
            yield return new WaitForSeconds(1 / spawnFrequency);
        }
    }
}
