using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnArea
{
    public List<Vector2> Vertices;
    private float minX, maxX, minY, maxY;
    private bool isInit = false;
    
    public virtual void Init()
    {
        List<float> Xlist = Vertices.ConvertAll(v => v.x);
        List<float> Ylist = Vertices.ConvertAll(v => v.y);
        minX = GetMin(Xlist);
        maxX = GetMax(Xlist);
        minY = GetMin(Ylist);
        maxY = GetMax(Ylist);
        isInit = true;
    }

    private float GetMin(List<float> floatList)
    {
        if (floatList.Count == 0)
        {
            return -Mathf.Infinity;
        }
        return Mathf.Min(floatList.ToArray());
    }

    private float GetMax(List<float> floatList)
    {
        if (floatList.Count == 0)
        {
            return Mathf.Infinity;
        }
        return Mathf.Max(floatList.ToArray());
    }

    public Vector2 GetRandomPointInConvexArea()
    {
        if (Vertices.Count == 0)
        {
            return Vector2.negativeInfinity;
        }
        if (isInit == false)
        {
            Init();
        }
        float x = Random.Range(minX, maxX);
        List<Vector2> Points;
        // Get All the points at the left of x
        Points = Vertices.FindAll(v => v.x <= x);
        float ymin0 = GetMin(Points.ConvertAll(v => v.y));
        float ymax0 = GetMax(Points.ConvertAll(v => v.y));
        Points.Clear();

        //Get all the points at the right of x
        Points = Vertices.FindAll(v => v.x > x);
        float ymin1 = GetMin(Points.ConvertAll(v => v.y));
        float ymax1 = GetMax(Points.ConvertAll(v => v.y));

        float ymin = Mathf.Max(ymin0, ymin1);
        float ymax = Mathf.Min(ymax0, ymax1);
        float y = Random.Range(ymin, ymax);
        return new Vector2() { x = x, y = y };
    }

}

[System.Serializable]
public class SpawnAreaRect : SpawnArea
{
    public Vector2 RangeX, RangeY;
    public override void Init()
    {
        Vertices = new List<Vector2>();
        Vertices.Add(new Vector2() { x = RangeX.x, y = RangeY.x });
        Vertices.Add(new Vector2() { x = RangeX.x, y = RangeY.y });
        Vertices.Add(new Vector2() { x = RangeX.y, y = RangeY.x });
        Vertices.Add(new Vector2() { x = RangeX.y, y = RangeY.y });
        base.Init();
    }
}

[System.Serializable]
public class PowerUpsArea
{
    public Vector3 Position;
    public SpawnArea spawnArea;
    public List<GameObject> PowerUps;

    public void SpawnPowerUp(GameObject powerUpTemplate)
    {
        Vector2 spawnPoint = spawnArea.GetRandomPointInConvexArea();
        if (PowerUps == null)
        {
            PowerUps = new List<GameObject>();
        }
        GameObject powerUp = Object.Instantiate(powerUpTemplate);
        powerUp.SetActive(true);
        powerUp.transform.position = Position + new Vector3(spawnPoint.x, 0f, spawnPoint.y);
        PowerUps.Add(powerUp);
    }
}

public class PowerUpSpawner : MonoBehaviour
{
    public List<PowerUpsArea> PowerUpsAreas;
    public GameObject powerUpTemplate;
    public float spawnFrequency = 0.5f;
    public int MaxPowerUpAmout;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnPowerUpCoroutine());   
    }

    public void IncreaseSpawnFrequency(float delta)
    {
        spawnFrequency += delta;
    }

    public void DecreasePowerUpAmount(GameObject powerUpPickUp)
    {
        lock (PowerUpsAreas)
        {
            foreach (PowerUpsArea powerUpArea in PowerUpsAreas)
            {
                int i = powerUpArea.PowerUps.IndexOf(powerUpPickUp);
                if (i > -1)
                {
                    powerUpArea.PowerUps.RemoveAt(i);
                }
            }
        }
    }
    IEnumerator SpawnPowerUpCoroutine()
    {
        while (SnakeMovement.isGameOver == false)
        {
            lock (PowerUpsAreas)
            {
                for (int i=0; i< PowerUpsAreas.Count; i++)
                {
                    if (PowerUpsAreas[i].PowerUps.Count < MaxPowerUpAmout)
                    {
                        PowerUpsAreas[i].SpawnPowerUp(powerUpTemplate);
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return new WaitForSeconds(1 / spawnFrequency);
        }
    }

    public void SetSpawnAreasFromTiles(List<Tile> tiles)
    {
        lock (PowerUpsAreas)
        {
            if (PowerUpsAreas == null)
            {
                PowerUpsAreas = new List<PowerUpsArea>();
            } else
            {
                PowerUpsAreas.Clear();
            }
            foreach (Tile t in tiles)
            {
                PowerUpsArea p = new PowerUpsArea();
                p.Position = t.GetRealPosition();
                p.Position.y = 0;
                p.PowerUps = new List<GameObject>();
                p.spawnArea = t.SpawnArea;
                PowerUpsAreas.Add(p);
            }
        }
    }
}
