using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnakeMovement : MonoBehaviour
{
    private InputHandler inputHandler;
    private SwallowWaveGenerator swallowWaveGenerator;
    public Vector2 direction;
    public Vector2 targetRealPosition;
    public float realSpeed = 0.1f;
    public Vector3 targetPositionOnGrid;
    public float speedPercentage = 1f;
    public float segmentSpeedPercent = 0.75f;
    public float segmentRotationSpeedPercent = 0.5f;
    public float rotationSpeed;
    public int gridScale = 1;
    private bool Block = false;

    private ParticleSystem JoyParticleSystem;
    public List<GameObject> SnakeBody;
    private List<Vector3> SnakeBodyTargetPositions;

    [Header("Tail")]
    public GameObject Tail;
    public GameObject RightBall, LeftBall;

    [Header("GameGod")]
    public GameObject GameGod;

    [Header("Game Over")]
    private AudioSystem audioSystem;
    private RealSnakeBinder realSnakeBinder;
    public float levelTime = 0f;
    public static bool isGameOver = false;

    [Header("Animation")]
    public float TimeForPickUpToReachTheTail = 10f;

    // Start is called before the first frame update
    void Start()
    {
        isGameOver = false;
        targetRealPosition = Vector2.right * transform.position.x + Vector2.up * transform.position.z;
        SnakeBodyTargetPositions = new List<Vector3>();
        for (int i = SnakeBody.Count; i > 0; i--)
        {
            SnakeBodyTargetPositions.Add(SnakeBody[i - 1].transform.position);
        }
        audioSystem = GetComponent<AudioSystem>();
        JoyParticleSystem = GetComponentInChildren<ParticleSystem>();
        realSnakeBinder = GetComponent<RealSnakeBinder>();
        swallowWaveGenerator = GetComponent<SwallowWaveGenerator>();
        inputHandler = GetComponent<InputHandler>();
        levelTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        levelTime += Time.deltaTime;
        for (int i = SnakeBodyTargetPositions.Count - 1; i > 0; i--)
        {
            SnakeBodyTargetPositions[i] = SnakeBody[i - 1].transform.position;
        }

        targetRealPosition = Vector2.right * (
            targetRealPosition.x + direction.x * realSpeed * Time.deltaTime
            ) + Vector2.up * (
            targetRealPosition.y + direction.y * realSpeed * Time.deltaTime
            );

        // Handle inputs (movement and additional actions)
        // If additional action is executed in this round DON'T execute movement
        bool actionExecuted = false;
        if (inputHandler.enabled && !Block)
        {
            foreach (InputHandler.Action action in inputHandler.actions)
            {
                actionExecuted = action();
            }
            if (!actionExecuted)
            {
                inputHandler.move(ref direction);
            }
        }

    }

    private void FixedUpdate()
    {
        if (isGameOver)
        {
            return;
        }

        for (int i = SnakeBody.Count - 1; i > 0f; i--)
        {
            SnakeBody[i].transform.position = Vector3.Lerp(
                SnakeBody[i].transform.position,
                SnakeBodyTargetPositions[i],
                Time.deltaTime * realSpeed * speedPercentage * segmentSpeedPercent);
            SnakeBody[i].transform.rotation = Quaternion.Lerp(
                SnakeBody[i].transform.rotation,
                SnakeBody[i - 1].transform.rotation,
                Time.deltaTime * rotationSpeed * segmentRotationSpeedPercent);
        }

        targetPositionOnGrid = ConvertToPositionOnGrid(targetRealPosition);
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.right * direction.x + Vector3.forward * direction.y, Vector3.up);
        transform.position = Vector3.Lerp(transform.position, targetPositionOnGrid, Time.deltaTime * realSpeed * speedPercentage);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void Grow()
    {
        if (SnakeBody.Count < 200)
        {
            realSnakeBinder.ResetOldStructure();
            GameObject newBodySegment = Instantiate(SnakeBody[SnakeBody.Count - 2]);
            newBodySegment.name = "newBody_" + (SnakeBody.Count - 1).ToString();
            SnakeBody.Insert(SnakeBody.Count - 1, newBodySegment);
            SnakeBodyTargetPositions.Insert(SnakeBodyTargetPositions.Count - 1, newBodySegment.transform.position);
            realSnakeBinder.UpdateBinder();
        }
        if (SnakeBody.Count%10==0)
        {
            JoyParticleSystem.Play();
        }
        realSpeed += 0.6f;
        PowerUpSpawner.IncreaseSpawnFrequency(0.01f);
        PowerUpSpawner.DecreasePowerUpAmount();
        RightBall.transform.localScale += Vector3.one * 0.04f;
        LeftBall.transform.localScale += Vector3.one * 0.03f;
        if (audioSystem.enabled)
        {
            audioSystem.PlayGrowSounds();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;
        if (other.gameObject.CompareTag("PowerUp"))
        {

            Grow();
            StartCoroutine(PickupAnimationCoroutine(other.gameObject));
        }
        if (other.gameObject.CompareTag("Body") || other.gameObject.CompareTag("Wall"))
        {
            StartCoroutine(DeathCoroutine());
        }
    }

    private IEnumerator PickupAnimationCoroutine(GameObject pickup)
    {
        pickup.GetComponent<Collider>().enabled = false;
        pickup.GetComponentInChildren<Animator>().enabled = false;
        float t;
        for (int i = 1; i < SnakeBody.Count; i++)
        {
            t = 0;
            swallowWaveGenerator.SwallowAtBodyPart(i);
            while (t * SnakeBody.Count < TimeForPickUpToReachTheTail)
            {
                pickup.transform.position = Vector3.Lerp(pickup.transform.position, SnakeBody[i].transform.position, t * SnakeBody.Count / TimeForPickUpToReachTheTail);
                pickup.transform.localScale = Vector3.Lerp(pickup.transform.localScale, pickup.transform.localScale * 0.7f, t * SnakeBody.Count / TimeForPickUpToReachTheTail);
                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        Destroy(pickup);
    }

    IEnumerator AnimateSnakeBodyPart(Transform boneTransform)
    {
        float t = 0;
        float T = 1f;
        while (t < T)
        {
            boneTransform.localScale = Vector3.Lerp(
                Vector3.one,
                Vector3.one * 2,
                Mathf.Sin(Mathf.PI * t / T)
                );
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public Transform GetSnakeBoneTransform(GameObject SnakeBodyPart)
    {
        foreach (Transform tr in SnakeBodyPart.GetComponentsInChildren<Transform>())
        {
            if (tr != SnakeBodyPart.transform)
            {
                return tr;
            }
        }
        return null;
    }

    public void ResetSnakeBody()
    {
        for (int i = 1; i < SnakeBody.Count - 1; i++)
        {
            SnakeBody[i].GetComponentInChildren<Transform>().localScale = Vector3.one;
        }
    }

    IEnumerator DeathCoroutine()
    {
        isGameOver = true;
        for (int i=0; i<SnakeBody.Count; i++)
        {
            Rigidbody rb = SnakeBody[i].GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddForce(Vector3.right * Random.Range(10, 20) + Vector3.up * 20 + Vector3.forward * Random.Range(-10, 10));
        }
        print("GAMEOVER");
        //Time.timeScale = 0;
        if (audioSystem.enabled)
        {
            audioSystem.PlayDeathSounds();
        }
        // Add Score
        ScoreManager.SetLengthAndScore(SnakeBody.Count, (SnakeBody.Count - 5) * (int)levelTime / 10 * (int)realSpeed);

        ScoreManager.ScoreBeforeBonuses = ScoreManager.CurrentScore;
        // Add Bonuses Score
        foreach (SettingsPanelManager.Bonus bonus in GameGod.GetComponent<SettingsPanelManager>().Bonuses)
        {
            ScoreManager.AddBonusScore(bonus.TotalBonus);
        }

        // Add XP
        LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));

        //Save Game (if user has name)
        DataPersistenceManager.Instance.SaveGame();
        //Start GameOver Animations
        StartCoroutine(
            GameGod.GetComponent<GameOverProcedures>().StartGameOverProcedure()
            );
        yield return new WaitForSeconds(5f);
    }

    public Vector3 ConvertToPositionOnGrid(Vector2 position)
    {
        return Vector3.right * Mathf.Round(position.x / gridScale) * gridScale + Vector3.forward * Mathf.Round(position.y / gridScale) * gridScale + Vector3.up * targetPositionOnGrid.y;
    }

    public void SetDirectionToClosestHortogonal()
    {
        Vector2 tmpDir = direction;
        if (Mathf.Abs(tmpDir.x) > Mathf.Abs(tmpDir.y))
        {
            tmpDir.y = 0f;
        }
        tmpDir.Normalize();
        direction = tmpDir;
    }

    public void SetBodyTargetPositionsToValue(Vector3 value)
    {
        for(int i = 0; i< SnakeBodyTargetPositions.Count; i++)
        {
            SnakeBodyTargetPositions[i] = value;
        }
    }

    public void BlockInputForSnake(bool condition)
    {
        Block = condition;
    }
}
