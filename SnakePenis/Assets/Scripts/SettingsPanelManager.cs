using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: All the colours specials should be applied to the SnakeMeshCopy

public class BaseBonusData
{
    public bool bEnabled;
    public string Title;
    public string Info;
    public int BonusPercent;
    public Text ButtonText;

    public BaseBonusData()
    {
        bEnabled = false;
        Title = "";
        Info = "";
        BonusPercent = 0;
        ButtonText = null;
    }
}

public class SettingsPanelManager : MonoBehaviour, IDataPersistence
{
    public ScoreManager scoreManager;
    public InputField inputNameField;
    public Text infoText;

    [System.Serializable]
    public struct Unlockable
    {
        public GameObject Menu;
        public int Level;
    }

    [System.Serializable]
    public struct Bonus
    {
        public string Name;
        public int Percent;
        public int XPAmount;
        public int TotalBonus;
    }
    [Header("Daily Bonus")]
    public string DailyBonusTimeStamp;
    public int DailyBonusXPAmount;
    public string DailyBonusTitle;

    [Header("Sound")]
    public bool isSoundEnabled;
    public Button SoundButton;
    public Sprite SoundOn, SoundOff;
    public int SoundBonusPercent;
    public string SoundBonusTitle;

    [Header("Field Of View Settings")]
    public bool isFieldOfViewShowing;
    private float currentFieldOfView;
    public Slider FieldOfViewSlider;
    public Button CameraButton;
    public Sprite CameraOn, CameraOff;

    public List<Unlockable> Unlockables;

    [Header("Square Ball unlockable")]
    public bool isRoundedBallsEnabled = false;
    public Text RoundedBallsButtonText;
    public GameObject RoundedBalls;
    private GameObject newRoundedBalls = null;
    public GameObject SnakeHead;
    public GameObject OriginalBalls;
    public string roundedBallInfo;
    public int RoundedBallsPercent = 0;
    public string RoundedBallsBonusTitle;

    [Header("Afro Style unlockable")]
    public bool isAfroStyleEnabled = false;
    public GameObject OriginalPenisMesh;
    public Material Brown, DarkBrown;
    public Material OriginalPink, OriginalDarkPink;
    public Text AfroStyleButtonText;
    public string afroStyleInfo;
    public int AfroStyleBonusPercent = 0;
    public string AfroStyleBonusTitle;

    [Header("Jump unlockable")]
    public bool isJumpEnabled = false;
    public Text DickingJumpButtonText;
    public string jumpInfo;
    public int JumpBonusPercent = 0;
    public int JumpBonusXPAmount = 0;
    public string JumpBonusTitle;

    [Header("Rainbow unlockable")]
    public bool isRainbowEnabled = false;
    public Material RainbowMaterial;
    public ParticleSystem snakeParticleSystem;
    public Material particleSystemOriginalMaterial;
    public Text RainbowStyleButtonText;
    public string rainbowInfo;
    public int RainbowBonusPercent = 0;
    public string RainbowBonusTitle;

    [Header("Moving Walls")]
    public bool isMovingWallsEnabled = false;
    public GameObject MovingWall1;
    public GameObject MovingWall2;
    public int MovingWallsBonusPercent = 12;
    public string MovingWallsBonusTitle;
    private GameObject newMovingWall1 = null, newMovingWall2 = null;
    public Text MovingWallsButtonText;
    public string movingWallsInfo;

    [Header("Free 360 Movement")]
    public bool isFree360Enabled = false;
    public Text Free360ButtonText;
    public string Free360Info;
    public int Free360BonusPercent = 0;
    public string Free360BonusTitle;

    [Header("Pills Blower")] 
    public bool isPillsBlowerEnabled = false;
    public GameObject PillsAttractor;
    private GameObject newPillsAttractor;
    public Text PillsBlowerButtonText;
    public string PillsBlowerInfo;
    public int PillsBlowerBonusPercent = 0;
    public string PillsBlowerBonusTitle;

    [System.Serializable]
    public struct TeleportCouple
    {
        public GameObject Portal1;
        public GameObject Portal2;
        public string BonusTitle;

        public TeleportCouple(GameObject portal1, GameObject portal2) {
            this.Portal1 = portal1;
            this.Portal2 = portal2;
            this.BonusTitle = "";
        }

        public bool IsNull()
        {
            return Portal1 == null && Portal2 == null;
        }

        public void DestroyCouple()
        {
            Destroy(Portal1);
            Destroy(Portal2);
            Portal1 = null;
            Portal2 = null;
        }
    }

    [Header("Teleport")]
    public bool isTeleportEnabled = false;
    public TeleportCouple[] TeleportCouples;
    private TeleportCouple instancedCouple;
    public GameObject[] WallsToAnchor;
    public float SpawnRadius;
    public float WallOffset;
    public Vector3 WallRotationOffset;
    public Text TeleportButtonText;
    public string TeleportInfo;
    public int TeleportBonusPercent = 0;
    public int TeleportBonusXPAmount = 0;
    public string TeleportBonusTitle;

    [Header("A Cappella")]
    public bool isACappellaEnabled = false;
    private AudioClip InitialSoundtrack;
    public AudioClip ACappellaSoundtrack;
    public Text ACappellaButtonText;
    public string ACappellaInfo;
    public int ACappellaBonusPercent = 0;
    public string ACappellaBonusTitle;




    private List<Bonus> _bonuses = null;
    public List<Bonus> Bonuses
    {
        get
        {
            if (_bonuses == null)
            {
                _bonuses = new List<Bonus>();
                CalculateBonuses();
            }
            return _bonuses;
        }
    }
    
    void StartUpSettingsPanelManager()
    {
        if (ScoreManager.CurrentScoreName!="" && ScoreManager.CurrentID != "")
        {
            inputNameField.text = ScoreManager.CurrentScoreName;
        }
        foreach (Unlockable unlockable in Unlockables)
        {
            unlockable.Menu.SetActive(LevelProgressionManager.CurrentLevel >= unlockable.Level);
        }
        infoText.text = "";
    }

    internal bool IsBonusAtLevel(int level)
    {
        Unlockable unlockable = Unlockables.Find(elem => elem.Level == level);
        return unlockable.Level == level;
    }

    public void SwitchSoundEnabled()
    {
        SwitchSoundEnabled(!isSoundEnabled);
    }

    void SwitchSoundEnabled(bool condition)
    {
        isSoundEnabled = condition;
        AudioListener.volume = condition ? 1f : 0f;
        SoundButton.image.sprite = (condition) ? SoundOn : SoundOff;
    }

    public void SwitchCameraFieldOfViewButton()
    {
        SwitchCameraFieldOfViewButton(!isFieldOfViewShowing);
    }

    void SwitchCameraFieldOfViewButton(bool condition)
    {
        isFieldOfViewShowing = condition;
        FieldOfViewSlider.gameObject.SetActive(condition);
        CameraButton.image.sprite = (condition) ? CameraOn : CameraOff;
    }

    public void SetCameraFieldOfView()
    {
        float value = FieldOfViewSlider.value;
        SetCameraFieldOfView(value);
    }

    void SetCameraFieldOfView(float value)
    {
        currentFieldOfView = value;
        Camera.main.fieldOfView = value;
        FieldOfViewSlider.value = value;
    }

    public IEnumerable<Bonus> BonusGenerator()
    {
        Bonus bonus;
        System.DateTime dt1 = System.DateTime.Today;
        System.DateTime dt2 = System.DateTime.ParseExact(DailyBonusTimeStamp, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
        if (dt1 > dt2)
        {
            bonus = new Bonus();
            bonus.Name = DailyBonusTitle;
            bonus.XPAmount = DailyBonusXPAmount;
            yield return bonus;
            DailyBonusTimeStamp = System.DateTime.Today.ToString("dd-MM-yyyy");
        }
        if (isSoundEnabled == false)
        {
            bonus = new Bonus();
            bonus.Name = SoundBonusTitle;
            bonus.Percent = SoundBonusPercent;
            yield return bonus;
        }
        if (isRoundedBallsEnabled)
        {
            bonus = new Bonus();
            bonus.Name = RoundedBallsBonusTitle;
            bonus.Percent = RoundedBallsPercent;
            yield return bonus;
        }
        if (isJumpEnabled)
        {
            bonus = new Bonus();
            bonus.Name = JumpBonusTitle;
            bonus.Percent = JumpBonusPercent;
            bonus.XPAmount = JumpBonusXPAmount * InputHandler.JumpsAmount * LevelProgressionManager.CurrentLevel;
            yield return bonus;
        }
        if (isAfroStyleEnabled)
        {
            bonus = new Bonus();
            bonus.Name = AfroStyleBonusTitle;
            bonus.Percent = AfroStyleBonusPercent;
            yield return bonus;
        }
        if (isRainbowEnabled)
        {
            bonus = new Bonus();
            bonus.Name = RainbowBonusTitle;
            bonus.Percent = RainbowBonusPercent;
            yield return bonus;
        }
        if (isMovingWallsEnabled)
        {
            bonus = new Bonus();
            bonus.Name = MovingWallsBonusTitle;
            bonus.Percent = MovingWallsBonusPercent;
            yield return bonus;
        }
        if (isFree360Enabled)
        {
            bonus = new Bonus();
            bonus.Name = Free360BonusTitle;
            bonus.Percent = Free360BonusPercent;
            yield return bonus;
        }
        if (isPillsBlowerEnabled)
        {
            bonus = new Bonus();
            bonus.Name = PillsBlowerBonusTitle;
            bonus.Percent = PillsBlowerBonusPercent;
            yield return bonus;
        }
        if (isTeleportEnabled)
        {
            bonus = new Bonus();
            bonus.Name = TeleportBonusTitle;
            bonus.Percent = TeleportBonusPercent;
            bonus.XPAmount = TeleportBonusXPAmount * PortalManager.PortalUsage;
            yield return bonus;
        }
        if (isACappellaEnabled && isSoundEnabled)
        {
            bonus = new Bonus();
            bonus.Name = ACappellaBonusTitle;
            bonus.Percent = ACappellaBonusPercent;
            yield return bonus;
        }

        //if (true)
        //{
        //    bonus = new Bonus();
        //    bonus.Name = "BONUS Test";
        //    bonus.XPAmount = 125000 * 5;
        //    yield return bonus;
        //}
    }

    private void CalculateBonuses()
    {
        Bonuses.Clear();
        if (ScoreManager.CurrentScore == "")
        {
            return;
        }
        foreach (SettingsPanelManager.Bonus bonus in BonusGenerator())
        {
            int bonusScore = (int)(bonus.XPAmount + bonus.Percent * 0.01f * int.Parse(ScoreManager.CurrentScore));
            Bonus newBonus = bonus;
            newBonus.TotalBonus = bonusScore;
            _bonuses.Add(newBonus);
        }
    }


    public void ChangeName() {
        StartCoroutine(ChangeNameOnHighScores());
    }

    IEnumerator ChangeNameOnHighScores()
    {
        if (inputNameField.text != "")
        {
            ScoreManager.CurrentScoreName = StringFormatter.FormatKeyboardName(inputNameField.text);
            yield return StartCoroutine(scoreManager.ChangeName(infoText));
            DataPersistenceManager.Instance.SaveGame();
        }
    }

    public void SwitchBallsWithRoundedBalls()
    {
        SwitchBallsWithRoundedBalls(!isRoundedBallsEnabled);
        infoText.text = roundedBallInfo;
    }

    void SwitchBallsWithRoundedBalls(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        RealSnakeBinder realSnakeBinder = SnakeHead.GetComponent<RealSnakeBinder>();
        Renderer[] initialBallsRenderers = snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1].GetComponentsInChildren<Renderer>();
        if (condition == true && isRoundedBallsEnabled==false) {
            newRoundedBalls = Instantiate(RoundedBalls);
            Renderer[] newBallsRenderers = newRoundedBalls.GetComponentsInChildren<Renderer>();
            for (int i=0; i<newBallsRenderers.Length; i++)
            {
                newBallsRenderers[i].material = initialBallsRenderers[i].material;
            }
            newRoundedBalls.transform.position = OriginalBalls.transform.position;
            newRoundedBalls.transform.rotation = OriginalBalls.transform.rotation;
            snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1] = newRoundedBalls;
            snakeMovement.Tail = newRoundedBalls;
            realSnakeBinder.UpdateBinder();
            OriginalBalls.SetActive(false);
            isRoundedBallsEnabled = true;
        } else if (condition == false && isRoundedBallsEnabled == true)
        {
            if (OriginalBalls.activeSelf == false)
            {
                OriginalBalls.SetActive(true);
            }
            if (newRoundedBalls)
            {
                OriginalBalls.transform.position = newRoundedBalls.transform.position;
                OriginalBalls.transform.rotation = newRoundedBalls.transform.rotation;
                Destroy(newRoundedBalls);
            }
            snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1] = OriginalBalls;
            snakeMovement.Tail = OriginalBalls;
            Renderer[] OriginalBallsRenderers = OriginalBalls.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < OriginalBallsRenderers.Length; i++)
            {
                OriginalBallsRenderers[i].material = initialBallsRenderers[i].material;
            }
            realSnakeBinder.UpdateBinder();
            isRoundedBallsEnabled = false;
        }
        RoundedBallsButtonText.text = (isRoundedBallsEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchAfroStyle()
    {
        if (isRainbowEnabled)
        {
            SwitchRainbowStyle(false);
        }
        SwitchAfroStyle(!isAfroStyleEnabled);
        infoText.text = afroStyleInfo;
    }

    void SwitchAfroStyle(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        Renderer snakeRenderer = OriginalPenisMesh.GetComponent<Renderer>();
        Renderer[] ballsRenderers = snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1].GetComponentsInChildren<Renderer>();
        if (condition == true && isAfroStyleEnabled == false)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = DarkBrown;
            }
            snakeRenderer.material = Brown;
            isAfroStyleEnabled = true;
        }
        else if (condition == false && isAfroStyleEnabled == true)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = OriginalDarkPink;
            }
            snakeRenderer.material = OriginalPink;
            isAfroStyleEnabled = false;
        }
        AfroStyleButtonText.text = (isAfroStyleEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchDickingJump()
    {
        SwitchDickingJump(!isJumpEnabled);
        infoText.text = jumpInfo;
    }

    void SwitchDickingJump(bool condition)
    {
        InputHandler inputHandler = SnakeHead.GetComponent<InputHandler>();
        if (condition == true && isJumpEnabled == false)
        {
            inputHandler.EnableStandardJump();
            isJumpEnabled = true;
        }
        else if (condition == false && isJumpEnabled == true)
        {
            inputHandler.DisableStandardJump();
            isJumpEnabled = false;
        }
        DickingJumpButtonText.text = (isJumpEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchRainbowStyle()
    {
        if (isAfroStyleEnabled)
        {
            SwitchAfroStyle(false);
        }
        SwitchRainbowStyle(!isRainbowEnabled);
        infoText.text = rainbowInfo;
    }

    void SwitchRainbowStyle(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        Renderer snakeRenderer = OriginalPenisMesh.GetComponent<Renderer>();
        Renderer[] ballsRenderers = snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1].GetComponentsInChildren<Renderer>();
        if (condition == true && isRainbowEnabled == false)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = RainbowMaterial;
            }
            snakeRenderer.material = RainbowMaterial;
            snakeParticleSystem.GetComponent<Renderer>().material = RainbowMaterial;
            isRainbowEnabled = true;
        }
        else if (condition == false && isRainbowEnabled == true)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = OriginalDarkPink;
            }
            snakeRenderer.material = OriginalPink;
            snakeParticleSystem.GetComponent<Renderer>().material = particleSystemOriginalMaterial;
            isRainbowEnabled = false;
        }
        RainbowStyleButtonText.text = (isRainbowEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchMovingWalls()
    {
        SwitchMovingWalls(!isMovingWallsEnabled);
        infoText.text = movingWallsInfo;
    }

    void SwitchMovingWalls(bool condition)
    {
        if (condition == true && isMovingWallsEnabled == false)
        {
            newMovingWall1 = Instantiate(MovingWall1);
            newMovingWall2 = Instantiate(MovingWall2);
            isMovingWallsEnabled = true;
        }
        else if (condition == false && isMovingWallsEnabled == true)
        {
            if (newMovingWall1)
            {
                Destroy(newMovingWall1);
            }
            if (newMovingWall2)
            {
                Destroy(newMovingWall2);
            }
            isMovingWallsEnabled = false;
        }
        MovingWallsButtonText.text = (isMovingWallsEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchFree360()
    {
        SwitchFree360(!isFree360Enabled);
        infoText.text = Free360Info;
    }

    void SwitchFree360(bool condition)
    {
        InputHandler inputHandler = SnakeHead.GetComponent<InputHandler>();
        if (condition == true && isFree360Enabled == false)
        {
            inputHandler.SetFree360MovementType();
            isFree360Enabled = true;
        }
        else if (condition == false && isJumpEnabled == true)
        {
            inputHandler.SetStandardMovementType();
            SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
            if (snakeMovement)
            {
                snakeMovement.SetDirectionToClosestHortogonal();
            }
            isFree360Enabled = false;
        }
        Free360ButtonText.text = (isFree360Enabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchPillsBlower()
    {
        SwitchPillsBlower(!isPillsBlowerEnabled);
        infoText.text = PillsBlowerInfo;
    }

    void SwitchPillsBlower(bool condition)
    {
        if (condition == true && isPillsBlowerEnabled == false)
        {
            newPillsAttractor = Instantiate(PillsAttractor, SnakeHead.transform);
            isPillsBlowerEnabled = true;
        }
        else if (condition == false && isPillsBlowerEnabled == true)
        {
            if (newPillsAttractor)
            {
                Destroy(newPillsAttractor);
            }
            isPillsBlowerEnabled = false;
        }
        PillsBlowerButtonText.text = (isPillsBlowerEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchTeleport()
    {
        SwitchTeleport(!isTeleportEnabled);
        infoText.text = TeleportInfo;
    }

    void SwitchTeleport(bool condition)
    {
        if (condition == true && isTeleportEnabled == false)
        {
            //Select teleport couple
            instancedCouple = new TeleportCouple(null, null);
            int randomIndex = UnityEngine.Random.Range(0, TeleportCouples.Length);
            TeleportCouple selectedTeleportCouple = TeleportCouples[randomIndex];
            TeleportBonusTitle = selectedTeleportCouple.BonusTitle;

            //Select walls
            randomIndex = UnityEngine.Random.Range(0, WallsToAnchor.Length);
            GameObject Wall1 = WallsToAnchor[randomIndex];
            randomIndex = UnityEngine.Random.Range(0, WallsToAnchor.Length);
            GameObject Wall2 = WallsToAnchor[randomIndex];

            //Select Spawn Position Value on Walls
            Vector3 spawnPositionPortal1, spawnPositionPortal2;
            float spawnValuePositionPortal1, spawnValuePositionPortal2;
            if (Wall1 == Wall2) // if sa
            {
                randomIndex = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
                spawnValuePositionPortal1 = randomIndex * SpawnRadius;
                spawnValuePositionPortal2 = -randomIndex * SpawnRadius;

            } else
            {
                spawnValuePositionPortal1 = UnityEngine.Random.Range(-SpawnRadius, SpawnRadius);
                spawnValuePositionPortal2 = UnityEngine.Random.Range(-SpawnRadius, SpawnRadius);
            }

            //Instantiate Portal and set them in instancedCouple
            if (instancedCouple.IsNull() == false)
            {
                Debug.LogWarning("There is an instanced couple of teleport objects. They should not be here. Will try to proceed anyway...");
                instancedCouple.DestroyCouple();
            }
            instancedCouple.Portal1 = Instantiate(selectedTeleportCouple.Portal1);
            instancedCouple.Portal2 = Instantiate(selectedTeleportCouple.Portal2);
            instancedCouple.Portal1.transform.rotation = Wall1.transform.rotation * Quaternion.Euler(WallRotationOffset);
            instancedCouple.Portal2.transform.rotation = Wall2.transform.rotation * Quaternion.Euler(WallRotationOffset);
            spawnPositionPortal1 = Wall1.transform.position + instancedCouple.Portal1.transform.right * spawnValuePositionPortal1 + instancedCouple.Portal1.transform.forward * WallOffset;
            spawnPositionPortal2 = Wall2.transform.position + instancedCouple.Portal2.transform.right * spawnValuePositionPortal2 + instancedCouple.Portal2.transform.forward * WallOffset;
            instancedCouple.Portal1.transform.position = spawnPositionPortal1;
            instancedCouple.Portal2.transform.position = spawnPositionPortal2;

            //Set OUT point of each portal, as specified in the TeleportCouple prefab
            //Remember: the OUT point of a portal object is in the OTHER portal object
            PortalManager portalManager1 = instancedCouple.Portal1.GetComponent<PortalManager>();
            PortalManager portalManager2 = instancedCouple.Portal2.GetComponent<PortalManager>();
            portalManager1.PortalOUTPoint = portalManager2.SelfPortalOUTPoint;
            portalManager2.PortalOUTPoint = portalManager1.SelfPortalOUTPoint;

            isTeleportEnabled = true;
        }
        else if (condition == false && isTeleportEnabled == true)
        {
            instancedCouple.DestroyCouple();
            isTeleportEnabled = false;
        }
        TeleportButtonText.text = (isTeleportEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchACappella()
    {
        SwitchACappella(!isACappellaEnabled);
        infoText.text = ACappellaInfo;
    }

    void SwitchACappella(bool condition)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (condition == true && isACappellaEnabled == false)
        {
            float t = audioSource.time;
            audioSource.Stop();
            InitialSoundtrack = audioSource.clip;
            audioSource.clip = ACappellaSoundtrack;
            if (t < audioSource.clip.length)
            {
                audioSource.time = t;
            }
            else
            {
                audioSource.time = 0f;
            }
            audioSource.Play();
            isACappellaEnabled = true;
        }
        else if (condition == false && isACappellaEnabled == true)
        {
            if (InitialSoundtrack)
            {
                float t = audioSource.time;
                audioSource.Stop();
                audioSource.clip = InitialSoundtrack;
                if (t < audioSource.clip.length)
                {
                    audioSource.time = t;
                } else
                {
                    audioSource.time = 0f;
                }
                audioSource.Play();
            }
            isACappellaEnabled = false;
        }
        ACappellaButtonText.text = (isACappellaEnabled) ? "Disattiva" : "Attiva";
    }

    public void LoadData(GameData data)
    {
        if (data.DailyBonusTimeStamp != "")
        {
            DailyBonusTimeStamp = data.DailyBonusTimeStamp;
        } else
        {
            DailyBonusTimeStamp = System.DateTime.Today.AddDays(-1).ToString("dd-MM-yyyy");
        }
        if (data.Sound == false)
        {
            print("SOUND OFF");
            SwitchSoundEnabled(false);
        }
        else
        {
            print("SOUND ON");
            SwitchSoundEnabled(true);
        }
        if (data.FieldOfView > 0f)
        {
            SetCameraFieldOfView(data.FieldOfView);
            FieldOfViewSlider.value = data.FieldOfView;
        }
        if (data.RoundedBalls == true)
        {
            SwitchBallsWithRoundedBalls(true);
            RoundedBallsButtonText.text = "Disattiva";
        } else
        {
            print("NO ROUND BALLS");
            isRoundedBallsEnabled = false;
            RoundedBallsButtonText.text = "Attiva";
        }

        if (data.AfroStyle == true)
        {
            SwitchAfroStyle(true);
            if (isRainbowEnabled)
            {
                SwitchRainbowStyle(false);
                RainbowStyleButtonText.text = "Attiva";
            }
            AfroStyleButtonText.text = "Disattiva";
        } else
        {
            print("NO AFRO");
            isAfroStyleEnabled = false;
            AfroStyleButtonText.text = "Attiva";
        }

        if (data.DickingJump == true)
        {
            SwitchDickingJump(true);
            DickingJumpButtonText.text = "Disattiva";
        }
        else
        {
            print("NO JUMP");
            isJumpEnabled = false;
            DickingJumpButtonText.text = "Attiva";
        }

        if (data.RainbowStyle == true)
        {
            SwitchRainbowStyle(true);
            if (isAfroStyleEnabled)
            {
                SwitchAfroStyle(false);
                AfroStyleButtonText.text = "Attiva";
            }
            RainbowStyleButtonText.text = "Disattiva";
        }
        else
        {
            print("NO RAINBOW");
            isRainbowEnabled = false;
            RainbowStyleButtonText.text = "Attiva";
        }

        if (data.MovingWalls == true)
        {
            SwitchMovingWalls(true);
            MovingWallsButtonText.text = "Disattiva";
        }
        else
        {
            print("NO MOVING WALLS");
            isMovingWallsEnabled = false;
            MovingWallsButtonText.text = "Attiva";
        }

        if (data.Free360Movement == true)
        {
            SwitchFree360(true);
            Free360ButtonText.text = "Disattiva";
        }
        else
        {
            print("NO FREE 360");
            isFree360Enabled = false;
            Free360ButtonText.text = "Attiva";
        }

        if (data.PillsBlower == true)
        {
            SwitchPillsBlower(true);
            PillsBlowerButtonText.text = "Disattiva";
        }
        else
        {
            print("NO PILLS BLOWER");
            isPillsBlowerEnabled = false;
            PillsBlowerButtonText.text = "Attiva";
        }

        if (data.Teleport == true)
        {
            SwitchTeleport(true);
            TeleportButtonText.text = "Disattiva";
        }
        else
        {
            print("NO TELEPORT");
            isTeleportEnabled = false;
            TeleportButtonText.text = "Attiva";
        }
        if (data.ACappella == true)
        {
            SwitchACappella(true);
            ACappellaButtonText.text = "Disattiva";
        }
        else
        {
            print("NO A CAPPELLA");
            isACappellaEnabled = false;
            ACappellaButtonText.text = "Attiva";
        }
    }

    void OnLoadComplete()
    {
        StartUpSettingsPanelManager();
    }

    public void SaveData(ref GameData data)
    {
        data.DailyBonusTimeStamp = System.DateTime.ParseExact(
            DailyBonusTimeStamp, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture
            ).ToString("dd-MM-yyyy");
        data.ACappella = isACappellaEnabled;
        data.Sound = isSoundEnabled;
        data.FieldOfView = currentFieldOfView;
        data.RoundedBalls = isRoundedBallsEnabled;
        data.AfroStyle = isAfroStyleEnabled;
        data.DickingJump = isJumpEnabled;
        data.RainbowStyle = isRainbowEnabled;
        data.MovingWalls = isMovingWallsEnabled;
        data.Free360Movement = isFree360Enabled;
        data.PillsBlower = isPillsBlowerEnabled;
        data.Teleport = isTeleportEnabled;
    }
}
