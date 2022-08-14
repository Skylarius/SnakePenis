using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnakeMovement : MonoBehaviour
{
    public Vector2 direction;
    public Vector2 targetRealPosition;
    public float realSpeed = 0.1f;
    public Vector3 targetPositionOnGrid;
    public float speedPercentage = 1f;
    public float segmentSpeedPercent = 0.75f;
    public float segmentRotationSpeedPercent = 0.5f;
    public float rotationSpeed;
    public int gridScale = 1;
    public GameObject RightBall, LeftBall;
    private ParticleSystem JoyParticleSystem;
    public List<GameObject> SnakeBody;
    private List<Vector3> SnakeBodyTargetPositions;

    [Header("GameGod")]
    public GameObject GameGod;

    [Header("Game Over")]
    private AudioSystem audioSystem;
    private RealSnakeBinder realSnakeBinder;
    private float levelTime = 0f;
    public static bool isGameOver = false;

    [Header("Animation")]
    public float TimeForPickUpToReachTheTail = 3f;
    // Start is called before the first frame update
    void Start()
    {
        isGameOver = false;
        direction = Vector2.left;
        targetRealPosition = Vector2.right * transform.position.x + Vector2.up * transform.position.z;
        SnakeBodyTargetPositions = new List<Vector3>();
        for (int i = SnakeBody.Count; i > 0; i--)
        {
            SnakeBodyTargetPositions.Add(SnakeBody[i - 1].transform.position);
        }
        audioSystem = GetComponent<AudioSystem>();
        JoyParticleSystem = GetComponentInChildren<ParticleSystem>();
        realSnakeBinder = GetComponent<RealSnakeBinder>();
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

        targetRealPosition = Vector2.right * (targetRealPosition.x + direction.x * realSpeed * Time.deltaTime) + Vector2.up * (targetRealPosition.y + direction.y * realSpeed * Time.deltaTime);
#if UNITY_EDITOR_WIN
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
#else
        float x = 0f;
        float z = 0f;
#endif
        if (Input.touchCount > 0) 
        {
            Touch touch = Input.GetTouch(0);
            switch(touch.phase)
            {
                case TouchPhase.Began:
                    //TODO: Check if it's in the AVOIDING ZONE (pause, exit)...
                    Vector3 tapPoint = ConvertTouchToPositionInWorld(touch);
                    x = tapPoint.x - transform.position.x;
                    z = tapPoint.z - transform.position.z;
                    if (direction.y != 0)
                    {
                        z = 0f;
                    }
                    if (direction.x != 0)
                    {
                        x = 0f;
                    }
                    break;
            }
        }
        //float x = Input.GetAxis("Horizontal");
        //float z = Input.GetAxis("Vertical");
        if (x == 0f && z == 0f || direction.x * x < 0 || direction.y * z < 0)
        {
            return;
        }
        if (x !=0 && z != 0) //Avoid multiple input
        {
            z = 0f;
        }
        if (x != 0f)
        {
            x = x > 0 ? 1 : -1;
        }
        if (z != 0f)
        {
            z = z > 0 ? 1 : -1;
        }
        direction = Vector2.right * x + Vector2.up * z;
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
        audioSystem.PlayGrowSounds();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;
        if (other.gameObject.CompareTag("PowerUp"))
        {
            //Destroy(other.gameObject);
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
        Vector3 originalTailScale = SnakeBody[SnakeBody.Count - 1].transform.localScale;
        pickup.GetComponent<Collider>().enabled = false;
        pickup.GetComponentInChildren<Animator>().enabled = false;
        float t;
        for (int i = 1; i < SnakeBody.Count; i++)
        {
            t = 0;
            SnakeBody[i].GetComponent<Collider>().enabled = false;
            Transform SnakeBoneTransform;
            if (i != SnakeBody.Count - 1)
            {
                SnakeBoneTransform = GetSnakeBoneTransform(SnakeBody[i]);
            } else
            {
                SnakeBoneTransform = SnakeBody[i].transform;
            }
            while (t * SnakeBody.Count < TimeForPickUpToReachTheTail)
            {
                pickup.transform.position = Vector3.Lerp(pickup.transform.position, SnakeBody[i].transform.position, t * SnakeBody.Count / TimeForPickUpToReachTheTail);
                pickup.transform.localScale = Vector3.Lerp(pickup.transform.localScale, pickup.transform.localScale * 0.7f, t * SnakeBody.Count / TimeForPickUpToReachTheTail);
                if (SnakeBoneTransform)
                {
                    if (t * SnakeBody.Count < TimeForPickUpToReachTheTail / 2)
                    {
                        SnakeBoneTransform.localScale = Vector3.Slerp(
                            Vector3.one,
                            Vector3.one * 2,
                            t * SnakeBody.Count * 2 / TimeForPickUpToReachTheTail
                            );
                    }
                    else
                    {
                        SnakeBoneTransform.localScale = Vector3.Slerp(
                            Vector3.one * 2,
                            Vector3.one,
                            (2* t * SnakeBody.Count - TimeForPickUpToReachTheTail)/ TimeForPickUpToReachTheTail
                            );
                    }
                    yield return new WaitForEndOfFrame();
                }
                t += Time.deltaTime;
            }
            SnakeBody[i].GetComponent<Collider>().enabled = true;
            SnakeBoneTransform.localScale = Vector3.one;
        }
        Destroy(pickup);
    }

    Transform GetSnakeBoneTransform(GameObject SnakeBodyPart)
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
        audioSystem.PlayDeathSounds();
        ScoreManager.SetLengthAndScore(SnakeBody.Count, (SnakeBody.Count - 5) * (int)levelTime / 10 * (int)realSpeed);
        LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));
        StartCoroutine(
            GameGod.GetComponent<GameOverProcedures>().StartGameOverProcedure()
            );
        yield return new WaitForSeconds(5f);
    }

    public Vector3 ConvertToPositionOnGrid(Vector2 position)
    {
        return Vector3.right* Mathf.Round(position.x / gridScale) * gridScale + Vector3.forward * Mathf.Round(position.y / gridScale) * gridScale;
    }

    Vector3 ConvertTouchToPositionInWorld(Touch touch)
    {
        Vector3 returnVector = Vector3.zero;
        // create ray from the camera and passing through the touch position:
        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        // create a logical plane at this object's position
        // and perpendicular to world Y:
        Plane plane = new Plane(Vector3.up, transform.position);
        float distance = 0; // this will return the distance from the camera
        if (plane.Raycast(ray, out distance))
        { // if plane hit...
            returnVector = ray.GetPoint(distance); // get the point
        }
        return returnVector;
    }
}
