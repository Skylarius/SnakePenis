using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public Vector2 RangeX;
    public Vector2 RangeZ;
    public GameObject powerUpTemplate;
    public float spawnFrequency = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnPowerUpCoroutine());   
    }

    IEnumerator SpawnPowerUpCoroutine()
    {
        while (true)
        {
            GameObject newPowerUp = Instantiate(powerUpTemplate);
            newPowerUp.SetActive(true);
            newPowerUp.transform.position = Vector3.right * Mathf.Round(Random.Range(RangeX.x, RangeX.y)) + Vector3.forward * Mathf.Round(Random.Range(RangeZ.x, RangeZ.y));
            yield return new WaitForSeconds(1 / spawnFrequency);
        }
    }
}
