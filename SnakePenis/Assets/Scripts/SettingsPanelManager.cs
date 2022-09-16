using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: All the colours specials should be applied to the SnakeMeshCopy


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

    [Header("Touch Sensitivity Settings")]
    public bool isTouchSensitivityShowing;
    private float currentTouchSensitivity;
    public Slider TouchSensitivitySlider;
    public Button TouchSensitivityButton;
    public Sprite TouchSensitivityOn, TouchSensitivityOff;

    [Header("Globals")]
    public GameObject SnakeHead;
    public GameObject OriginalPenisMesh;
    public Material OriginalPink, OriginalDarkPink;

    [System.Serializable]
    public class BaseSpecial
    {
        public bool enabled;
        public Unlockable Unlockable;
        public string Title;
        public string Info;
        public int BonusPercent;
        public int BonusXPAmount;
        public Button Button
        {
            get
            {
                return this.Unlockable.Menu.GetComponentInChildren<Button>();
            }
        }

        public Text ButtonText
        {
            get
            {
                Button b = Button;
                if (b!=null)
                {
                    return b.GetComponentInChildren<Text>();
                } else
                {
                    return null;
                }
            }
        }

        public Text UnlockableText
        {
            get
            {
                return this.Unlockable.Menu.GetComponentInChildren<Text>();
            }
        }

        //public delegate void SwitchSpecialFunction();
        //public SwitchSpecialFunction Switch;

        public delegate void SwitchSpecialFunctionConditioned(bool condition);
        public SwitchSpecialFunctionConditioned SwitchConditioned;

        internal SettingsPanelManager settingsPanelManager;

        public BaseSpecial()
        {
            enabled = false;
            Title = "";
            Info = "";
            BonusPercent = 0;
            BonusXPAmount = 0;
        }

        public virtual int GetXPAmount()
        {
            return BonusXPAmount * LevelProgressionManager.CurrentLevel;
        }

        public virtual bool IsEnabled()
        {
            return enabled;
        }

        public virtual void Switch()
        {
            SwitchConditioned(!this.enabled);
            settingsPanelManager.infoText.text = this.Info;
        }

        public void SetSettingsPanelManager(SettingsPanelManager settingsPanelManager)
        {
            this.settingsPanelManager = settingsPanelManager;
        }

    }

    private List<BaseSpecial> _specials;
    public List<BaseSpecial> Specials
    {
        get
        {
            return _specials;
        }
    }

    [System.Serializable]
    public class SquareBalls : BaseSpecial
    {
        public GameObject SquareBallsPrefab;
        internal GameObject newSquareBalls = null;
        public GameObject OriginalBalls;
    }

    [System.Serializable]
    public class AfroStyle : BaseSpecial
    {
        public Material Brown, DarkBrown;

        public override void Switch()
        {
            if (settingsPanelManager.RainbowSpecial.IsEnabled())
            {
                settingsPanelManager.RainbowSpecial.SwitchConditioned(false);
            }
            base.Switch();
        }
    }

    [System.Serializable]
    public class JumpingDick : BaseSpecial
    {
        public override int GetXPAmount()
        {
            return BonusXPAmount * InputHandler.JumpsAmount;
        }
    }

    [System.Serializable]
    public class Rainbow : BaseSpecial
    {
        public Material RainbowMaterial;
        public ParticleSystem snakeParticleSystem;
        public Material particleSystemOriginalMaterial;

        public override void Switch()
        {
            if (settingsPanelManager.AfroStyleSpecial.IsEnabled())
            {
                settingsPanelManager.AfroStyleSpecial.SwitchConditioned(false);
            }
            base.Switch();
        }
    }

    [System.Serializable]
    public class MovingWalls : BaseSpecial
    {
        public GameObject MovingWall1;
        public GameObject MovingWall2;
        internal GameObject newMovingWall1 = null, newMovingWall2 = null;
    }

    [System.Serializable]
    public class Free360Movement : BaseSpecial
    {

    }

    [System.Serializable]
    public class PillsBlower : BaseSpecial
    {
        public GameObject PillsAttractor;
        internal GameObject newPillsAttractor;
    }

    [System.Serializable]
    public class Teleport : BaseSpecial
    {
        [System.Serializable]
        public struct TeleportCouple
        {
            public GameObject Portal1;
            public GameObject Portal2;
            public string BonusTitle;

            public TeleportCouple(GameObject Portal1, GameObject Portal2)
            {
                this.Portal1 = Portal1;
                this.Portal2 = Portal2;
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

        public TeleportCouple[] TeleportCouples;
        internal List<TeleportCouple> instancedCouples;
        public float SpawnRadius;
        public float WallOffset;
        public Vector3 WallRotationOffset;

        public override int GetXPAmount()
        {
            return BonusXPAmount * PortalManager.PortalUsage;
        }
    }

    [System.Serializable]
    public class ACappella : BaseSpecial
    {
        internal AudioClip InitialSoundtrack;
        public AudioClip ACappellaSoundtrack;

        public override bool IsEnabled()
        {
            return enabled && settingsPanelManager.isSoundEnabled;
        }
    }

    [System.Serializable]
    public class FirstPerson : BaseSpecial
    {
        public Camera FirstPersonCamera;
        internal Camera OldCamera = null;
        internal Camera newFirstPersonCamera = null;
        public float CameraHeight = 20f;

        public override int GetXPAmount()
        {
            return (ScoreManager.ScoreBeforeBonuses == "" || int.Parse(ScoreManager.ScoreBeforeBonuses) == 0) ? 0 : BonusXPAmount;
        }
    }

    [System.Serializable]
    public class Labyrinth : BaseSpecial
    {
        public Camera LabyrinthCamera;
        internal Camera OldCamera = null;
        internal Camera newLabyrinthCamera = null;
        public bool disableNextGame = false;
    }

    [Header("Square Ball unlockable")]
    public SquareBalls SquareBallsSpecial;

    [Header("Afro Style unlockable")]
    public AfroStyle AfroStyleSpecial;

    [Header("Jump unlockable")]
    public JumpingDick JumpingDickSpecial;

    [Header("Rainbow unlockable")]
    public Rainbow RainbowSpecial;

    [Header("Moving Walls")]
    public MovingWalls MovingWallsSpecial;

    [Header("Free 360 Movement")]
    public Free360Movement Free360MovementSpecial;

    [Header("Pills Blower")] 
    public PillsBlower PillsBlowerSpecial;

    [Header("Teleport")]
    public Teleport TeleportSpecial;

    [Header("A Cappella")]
    public ACappella ACappellaSpecial;

    [Header("First Person View")]
    public FirstPerson FirstPersonSpecial;

    [Header("Labyrinth")]
    public Labyrinth LabyrinthSpecial;


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
        SnakeHead = GameGodSingleton.SnakeMovement.gameObject;
        if (ScoreManager.CurrentScoreName != "" && ScoreManager.CurrentID != "")
        {
            inputNameField.text = ScoreManager.CurrentScoreName;
        }
        infoText.text = "";

        _specials = new List<BaseSpecial>() {
            SquareBallsSpecial,
            AfroStyleSpecial,
            JumpingDickSpecial,
            RainbowSpecial,
            MovingWallsSpecial,
            Free360MovementSpecial,
            PillsBlowerSpecial,
            TeleportSpecial,
            ACappellaSpecial,
            FirstPersonSpecial,
            LabyrinthSpecial
        };

        foreach (BaseSpecial special in _specials)
        {
            special.SetSettingsPanelManager(this);
            special.Unlockable.Menu.SetActive(LevelProgressionManager.CurrentLevel >= special.Unlockable.Level);
        }

        SetupButtonsAction();
    }

    void SetupSpecialsDelegates()
    {
        SquareBallsSpecial.SwitchConditioned = SwitchBallsWithRoundedBalls;
        AfroStyleSpecial.SwitchConditioned = SwitchAfroStyle;
        JumpingDickSpecial.SwitchConditioned = SwitchDickingJump;
        RainbowSpecial.SwitchConditioned = SwitchRainbowStyle;
        MovingWallsSpecial.SwitchConditioned = SwitchMovingWalls;
        Free360MovementSpecial.SwitchConditioned = SwitchFree360;
        PillsBlowerSpecial.SwitchConditioned = SwitchPillsBlower;
        TeleportSpecial.SwitchConditioned = SwitchTeleport;
        ACappellaSpecial.SwitchConditioned = SwitchACappella;
        FirstPersonSpecial.SwitchConditioned = SwitchFirstPerson;
        LabyrinthSpecial.SwitchConditioned = SwitchLabyrinth;
    }

    void SetupButtonsAction()
    {
        foreach(BaseSpecial special in _specials)
        {
            special.Button.onClick.AddListener(special.Switch);
        }
    }

    internal BaseSpecial GetSpecialAtLevel(int level)
    {
        return _specials.Find(elem => elem.Unlockable.Level == level);
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
        if (isTouchSensitivityShowing)
        {
            SwitchTouchSensitivityButton(false);
        }
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

    public void SwitchTouchSensitivityButton()
    {
        if (isFieldOfViewShowing)
        {
            SwitchCameraFieldOfViewButton(false);
        }
        SwitchTouchSensitivityButton(!isTouchSensitivityShowing);
    }

    void SwitchTouchSensitivityButton(bool condition)
    {
        isTouchSensitivityShowing = condition;
        TouchSensitivitySlider.gameObject.SetActive(condition);
        TouchSensitivityButton.image.sprite = (condition) ? TouchSensitivityOn : TouchSensitivityOff;
    }

    public void SetTouchSensitivity()
    {
        float value = TouchSensitivitySlider.value;
        SetTouchSensitivity(value);
    }

    void SetTouchSensitivity(float value)
    {
        currentTouchSensitivity = value;
        GameGodSingleton.InputHandler.AndroidRotationFirstPersonSpeed = value;
        TouchSensitivitySlider.value = value;
    }

    public IEnumerable<Bonus> BonusGenerator()
    {
        Bonus bonus;
        System.DateTime dt1 = System.DateTime.Today;
        System.DateTime dt2 = System.DateTime.ParseExact(DailyBonusTimeStamp, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

        // Start yielding Bonuses
        if (dt1 > dt2)
        {
            bonus = new Bonus();
            bonus.Name = DailyBonusTitle;
            bonus.XPAmount = DailyBonusXPAmount * LevelProgressionManager.CurrentLevel;
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

        foreach(BaseSpecial special in _specials)
        {
            if (special.IsEnabled())
            {
                bonus = new Bonus();
                bonus.Name = special.Title;
                bonus.Percent = special.BonusPercent;
                bonus.XPAmount = special.GetXPAmount();
                yield return bonus;
            }
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
        foreach (Bonus bonus in BonusGenerator())
        {
            int bonusScore = (int)(bonus.XPAmount + bonus.Percent * 0.01f * int.Parse(ScoreManager.CurrentScore));
            Bonus newBonus = bonus;
            newBonus.TotalBonus = bonusScore;
            _bonuses.Add(newBonus);
        }
    }

    public void RecalculateBonuses()
    {
        CalculateBonuses();
    }

    public void ResetBonuses()
    {
        if (_bonuses != null)
        {
            _bonuses.Clear();
        }
        _bonuses = null;
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

    void SwitchBallsWithRoundedBalls(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        RealSnakeBinder realSnakeBinder = SnakeHead.GetComponent<RealSnakeBinder>();
        Renderer[] initialBallsRenderers = snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1].GetComponentsInChildren<Renderer>();
        if (condition == true && SquareBallsSpecial.enabled == false) {
            SquareBallsSpecial.newSquareBalls = Instantiate(SquareBallsSpecial.SquareBallsPrefab);
            Renderer[] newBallsRenderers = SquareBallsSpecial.newSquareBalls.GetComponentsInChildren<Renderer>();
            for (int i=0; i<newBallsRenderers.Length; i++)
            {
                newBallsRenderers[i].material = initialBallsRenderers[i].material;
            }
            SquareBallsSpecial.newSquareBalls.transform.position = SquareBallsSpecial.OriginalBalls.transform.position;
            SquareBallsSpecial.newSquareBalls.transform.rotation = SquareBallsSpecial.OriginalBalls.transform.rotation;
            snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1] = SquareBallsSpecial.newSquareBalls;
            snakeMovement.Tail = SquareBallsSpecial.newSquareBalls;
            realSnakeBinder.UpdateBinder();
            SquareBallsSpecial.OriginalBalls.SetActive(false);
            SquareBallsSpecial.enabled = true;
        } else if (condition == false && SquareBallsSpecial.enabled == true)
        {
            if (SquareBallsSpecial.OriginalBalls.activeSelf == false)
            {
                SquareBallsSpecial.OriginalBalls.SetActive(true);
            }
            if (SquareBallsSpecial.newSquareBalls)
            {
                SquareBallsSpecial.OriginalBalls.transform.position = SquareBallsSpecial.newSquareBalls.transform.position;
                SquareBallsSpecial.OriginalBalls.transform.rotation = SquareBallsSpecial.newSquareBalls.transform.rotation;
                Destroy(SquareBallsSpecial.newSquareBalls);
            }
            snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1] = SquareBallsSpecial.OriginalBalls;
            snakeMovement.Tail = SquareBallsSpecial.OriginalBalls;
            Renderer[] OriginalBallsRenderers = SquareBallsSpecial.OriginalBalls.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < OriginalBallsRenderers.Length; i++)
            {
                OriginalBallsRenderers[i].material = initialBallsRenderers[i].material;
            }
            realSnakeBinder.UpdateBinder();
            SquareBallsSpecial.enabled = false;
        }
        SquareBallsSpecial.ButtonText.text = (SquareBallsSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    void SwitchAfroStyle(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        Renderer snakeRenderer = OriginalPenisMesh.GetComponent<Renderer>();
        Renderer[] ballsRenderers = snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1].GetComponentsInChildren<Renderer>();
        if (condition == true && AfroStyleSpecial.enabled == false)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = AfroStyleSpecial.DarkBrown;
            }
            snakeRenderer.material = AfroStyleSpecial.Brown;
            AfroStyleSpecial.enabled = true;
        }
        else if (condition == false && AfroStyleSpecial.enabled == true)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = OriginalDarkPink;
            }
            snakeRenderer.material = OriginalPink;
            AfroStyleSpecial.enabled = false;
        }
        AfroStyleSpecial.ButtonText.text = (AfroStyleSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    void SwitchDickingJump(bool condition)
    {
        InputHandler inputHandler = SnakeHead.GetComponent<InputHandler>();
        if (condition == true && JumpingDickSpecial.enabled == false)
        {
            if (FirstPersonSpecial.IsEnabled())
            {
                inputHandler.EnableFirstPersonJump();
            } else
            {
                inputHandler.EnableStandardJump();
            }
            JumpingDickSpecial.enabled = true;
        }
        else if (condition == false && JumpingDickSpecial.enabled == true)
        {
            if (FirstPersonSpecial.IsEnabled())
            {
                inputHandler.DisableFirstPersonJump();
            }
            else
            {
                inputHandler.DisableStandardJump();
            }
            JumpingDickSpecial.enabled = false;
        }
        JumpingDickSpecial.ButtonText.text = (JumpingDickSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    void SwitchRainbowStyle(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        Renderer snakeRenderer = OriginalPenisMesh.GetComponent<Renderer>();
        Renderer[] ballsRenderers = snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1].GetComponentsInChildren<Renderer>();
        if (condition == true && RainbowSpecial.enabled == false)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = RainbowSpecial.RainbowMaterial;
            }
            snakeRenderer.material = RainbowSpecial.RainbowMaterial;
            RainbowSpecial.snakeParticleSystem.GetComponent<Renderer>().material = RainbowSpecial.RainbowMaterial;
            RainbowSpecial.enabled = true;
        }
        else if (condition == false && RainbowSpecial.enabled == true)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = OriginalDarkPink;
            }
            snakeRenderer.material = OriginalPink;
            RainbowSpecial.snakeParticleSystem.GetComponent<Renderer>().material = RainbowSpecial.particleSystemOriginalMaterial;
            RainbowSpecial.enabled = false;
        }
        RainbowSpecial.ButtonText.text = (RainbowSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    void SwitchMovingWalls(bool condition)
    {
        if (condition == true && MovingWallsSpecial.enabled == false)
        {
            MovingWallsSpecial.newMovingWall1 = Instantiate(MovingWallsSpecial.MovingWall1);
            MovingWallsSpecial.newMovingWall2 = Instantiate(MovingWallsSpecial.MovingWall2);
            MovingWallsSpecial.enabled = true;
        }
        else if (condition == false && MovingWallsSpecial.enabled == true)
        {
            if (MovingWallsSpecial.newMovingWall1)
            {
                Destroy(MovingWallsSpecial.newMovingWall1);
            }
            if (MovingWallsSpecial.newMovingWall2)
            {
                Destroy(MovingWallsSpecial.newMovingWall2);
            }
            MovingWallsSpecial.enabled = false;
        }
        MovingWallsSpecial.ButtonText.text = (MovingWallsSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    void SwitchFree360(bool condition)
    {
        InputHandler inputHandler = SnakeHead.GetComponent<InputHandler>();
        if (condition == true && Free360MovementSpecial.enabled == false)
        {
            if (FirstPersonSpecial.IsEnabled())
            {
                inputHandler.SetFirstPersonFree360MovementType();
            } else
            {
                inputHandler.SetFree360MovementType();
            }
            Free360MovementSpecial.enabled = true;
        }
        else if (condition == false && Free360MovementSpecial.enabled == true)
        {
            if (FirstPersonSpecial.IsEnabled())
            {
                inputHandler.SetFirstPerson90RotationMovementType();
            } else
            {
                inputHandler.SetStandardMovementType();
            }
            SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
            if (snakeMovement)
            {
                snakeMovement.SetDirectionToClosestHortogonal();
            }
            Free360MovementSpecial.enabled = false;
        }
        Free360MovementSpecial.ButtonText.text = (Free360MovementSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    void SwitchPillsBlower(bool condition)
    {
        if (condition == true && PillsBlowerSpecial.enabled == false)
        {
            PillsBlowerSpecial.newPillsAttractor = Instantiate(PillsBlowerSpecial.PillsAttractor, SnakeHead.transform);
            PillsBlowerSpecial.enabled = true;
        }
        else if (condition == false && PillsBlowerSpecial.enabled == true)
        {
            if (PillsBlowerSpecial.newPillsAttractor)
            {
                Destroy(PillsBlowerSpecial.newPillsAttractor);
            }
            PillsBlowerSpecial.enabled = false;
        }
        PillsBlowerSpecial.ButtonText.text = (PillsBlowerSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    void SwitchTeleport(bool condition)
    {
        if (condition == true && TeleportSpecial.enabled == false)
        {
            //Select teleport couple
            int randomIndex = UnityEngine.Random.Range(0, TeleportSpecial.TeleportCouples.Length);
            Teleport.TeleportCouple selectedTeleportCouple = TeleportSpecial.TeleportCouples[randomIndex];
            TeleportSpecial.Title = selectedTeleportCouple.BonusTitle;

            GameObject Wall1, Wall2;
            List<GameObject> Walls = GetAllWalls();
            List<Tuple<GameObject, GameObject>> WallsCouples = new List<Tuple<GameObject, GameObject>>();
            for (int i = 0; i < Walls.Count / 2; i += 2)
            {
                Wall1 = Walls[UnityEngine.Random.Range(0, Walls.Count)];
                Wall2 = Walls[UnityEngine.Random.Range(0, Walls.Count)];
                WallsCouples.Add(new Tuple<GameObject, GameObject>(Wall1, Wall2));
                Walls.RemoveAll(w => w == Wall1 || w == Wall2);
            }

            foreach (Tuple<GameObject, GameObject> wallCouple in WallsCouples)
            {
                InstanceTeleportCoupleToWalls(selectedTeleportCouple, wallCouple.Item1, wallCouple.Item2);
            }


            TeleportSpecial.enabled = true;
        }
        else if (condition == false && TeleportSpecial.enabled == true)
        {
            foreach (Teleport.TeleportCouple instancedCouple in TeleportSpecial.instancedCouples)
            {
                instancedCouple.DestroyCouple();
            }
            TeleportSpecial.instancedCouples.Clear();
            TeleportSpecial.enabled = false;
        }
        TeleportSpecial.ButtonText.text = (TeleportSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    private List<GameObject> GetAllWalls()
    {
        List<GameObject> AllWalls = new List<GameObject>();
        if (LabyrinthSpecial.enabled)
        {
            foreach (Tile t in GameGodSingleton.PlaygroundGenerator.GeneratedTiles)
            {
                AllWalls.AddRange(t.GetWalls());
            }
            return AllWalls;
        } else
        {
            TileData td = GameGodSingleton.MainGround.GetComponent<TileData>();
            if (td)
            {
               AllWalls.AddRange(td.Walls);
            }
        }
        return AllWalls;
    }

    void InstanceTeleportCoupleToWalls(Teleport.TeleportCouple selectedTeleportCouple, GameObject Wall1, GameObject Wall2)
    {
        //Select Spawn Position Value on Walls
        Vector3 spawnPositionPortal1, spawnPositionPortal2;
        float spawnValuePositionPortal1, spawnValuePositionPortal2;
        if (Wall1 == Wall2) // if sa
        {
            int randomIndex = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
            spawnValuePositionPortal1 = randomIndex * TeleportSpecial.SpawnRadius;
            spawnValuePositionPortal2 = -randomIndex * TeleportSpecial.SpawnRadius;

        }
        else
        {
            spawnValuePositionPortal1 = UnityEngine.Random.Range(-TeleportSpecial.SpawnRadius, TeleportSpecial.SpawnRadius);
            spawnValuePositionPortal2 = UnityEngine.Random.Range(-TeleportSpecial.SpawnRadius, TeleportSpecial.SpawnRadius);
        }

        //Instantiate Portal and set them in instancedCouple
        Teleport.TeleportCouple instancedCouple = new Teleport.TeleportCouple(null, null);
        if (instancedCouple.IsNull() == false)
        {
            Debug.LogWarning("There is an instanced couple of teleport objects. They should not be here. Will try to proceed anyway...");
            instancedCouple.DestroyCouple();
        }
        instancedCouple.Portal1 = Instantiate(selectedTeleportCouple.Portal1);
        instancedCouple.Portal2 = Instantiate(selectedTeleportCouple.Portal2);
        instancedCouple.Portal1.transform.rotation = Wall1.transform.rotation * Quaternion.Euler(TeleportSpecial.WallRotationOffset);
        instancedCouple.Portal2.transform.rotation = Wall2.transform.rotation * Quaternion.Euler(TeleportSpecial.WallRotationOffset);
        spawnPositionPortal1 = Wall1.transform.position +
            instancedCouple.Portal1.transform.right * spawnValuePositionPortal1 +
            instancedCouple.Portal1.transform.forward * TeleportSpecial.WallOffset;
        spawnPositionPortal2 = Wall2.transform.position +
            instancedCouple.Portal2.transform.right * spawnValuePositionPortal2 +
            instancedCouple.Portal2.transform.forward * TeleportSpecial.WallOffset;
        instancedCouple.Portal1.transform.position = spawnPositionPortal1;
        instancedCouple.Portal2.transform.position = spawnPositionPortal2;

        //Set OUT point of each portal, as specified in the TeleportCouple prefab
        //Remember: the OUT point of a portal object is in the OTHER portal object
        PortalManager portalManager1 = instancedCouple.Portal1.GetComponent<PortalManager>();
        PortalManager portalManager2 = instancedCouple.Portal2.GetComponent<PortalManager>();
        portalManager1.PortalOUTPoint = portalManager2.SelfPortalOUTPoint;
        portalManager2.PortalOUTPoint = portalManager1.SelfPortalOUTPoint;

        // Add instanced couple of portals to the List
        if (TeleportSpecial.instancedCouples == null)
        {
            TeleportSpecial.instancedCouples = new List<Teleport.TeleportCouple>();
        }
        TeleportSpecial.instancedCouples.Add(instancedCouple);
    }

    void SwitchACappella(bool condition)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (condition == true && ACappellaSpecial.enabled == false)
        {
            float t = audioSource.time;
            audioSource.Stop();
            ACappellaSpecial.InitialSoundtrack = audioSource.clip;
            audioSource.clip = ACappellaSpecial.ACappellaSoundtrack;
            if (t < audioSource.clip.length)
            {
                audioSource.time = t;
            }
            else
            {
                audioSource.time = 0f;
            }
            audioSource.Play();
            ACappellaSpecial.enabled = true;
        }
        else if (condition == false && ACappellaSpecial.enabled == true)
        {
            if (ACappellaSpecial.InitialSoundtrack)
            {
                float t = audioSource.time;
                audioSource.Stop();
                audioSource.clip = ACappellaSpecial.InitialSoundtrack;
                if (t < audioSource.clip.length)
                {
                    audioSource.time = t;
                } else
                {
                    audioSource.time = 0f;
                }
                audioSource.Play();
            }
            ACappellaSpecial.enabled = false;
        }
        ACappellaSpecial.ButtonText.text = (ACappellaSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    void SwitchFirstPerson(bool condition)
    {
        InputHandler inputHandler = SnakeHead.GetComponent<InputHandler>();
        if (condition == true && FirstPersonSpecial.enabled == false)
        {
            //FirstPersonSpecial.OldCamera = Camera.main;
            //FirstPersonSpecial.newFirstPersonCamera = Instantiate(FirstPersonSpecial.FirstPersonCamera);
            //Camera.SetupCurrent(FirstPersonSpecial.newFirstPersonCamera);
            //FirstPersonSpecial.newFirstPersonCamera.GetComponent<FirstPersonCameraController>().offset = Vector3.up * FirstPersonSpecial.CameraHeight;
            //if (LabyrinthSpecial.enabled && LabyrinthSpecial.newLabyrinthCamera != null)
            //{
            //    LabyrinthSpecial.OldCamera = Camera.main;
            //    LabyrinthSpecial.newLabyrinthCamera = Instantiate(LabyrinthSpecial.LabyrinthCamera);
            //    LabyrinthSpecial.newLabyrinthCamera.gameObject.SetActive(true);
            //    Camera.SetupCurrent(LabyrinthSpecial.newLabyrinthCamera);
            //    LabyrinthSpecial.OldCamera.gameObject.SetActive(false);
            //}
            //else
            //{
            //    FirstPersonSpecial.OldCamera.gameObject.SetActive(false);
            //}
            UseCamera(CameraType.FirstPerson);
            if (JumpingDickSpecial.IsEnabled())
            {
                inputHandler.DisableStandardJump();
                inputHandler.EnableFirstPersonJump();
            }
            if (Free360MovementSpecial.IsEnabled())
            {
                inputHandler.SetFirstPersonFree360MovementType();
            } else
            {
                inputHandler.SetFirstPerson90RotationMovementType();
            }
            FirstPersonSpecial.enabled = true;
        }
        else if (condition == false && FirstPersonSpecial.enabled == true)
        {
            if (LabyrinthSpecial.enabled)
            {
                UseCamera(CameraType.Labyrinth);
            }
            else
            {
                UseCamera(CameraType.Main);
            }
            if (FirstPersonSpecial.newFirstPersonCamera)
            {
                Destroy(FirstPersonSpecial.newFirstPersonCamera.gameObject);
            }
            if (Free360MovementSpecial.IsEnabled())
            {
                inputHandler.SetFree360MovementType();
            } else
            {
                inputHandler.SetStandardMovementType();
            }
            
            if (JumpingDickSpecial.IsEnabled())
            {
                inputHandler.DisableFirstPersonJump();
                inputHandler.EnableStandardJump();
            }
            SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
            if (snakeMovement)
            {
                snakeMovement.SetDirectionToClosestHortogonal();
            }
            FirstPersonSpecial.enabled = false;
        }
        FirstPersonSpecial.ButtonText.text = (FirstPersonSpecial.enabled) ? "Disattiva" : "Attiva";
    }

    void SwitchLabyrinth(bool condition)
    {
        if (condition == true && LabyrinthSpecial.enabled == false)
        {
            GameGodSingleton.MainGround.SetActive(false);
            GameGodSingleton.PlaygroundGenerator.GeneratePlayground();
            if (GameGodSingleton.PlaygroundGenerator.GeneratedTiles != null)
            {
                GameGodSingleton.PowerUpSpawner.SetSpawnAreasFromTiles(GameGodSingleton.PlaygroundGenerator.GeneratedTiles);
            }
            if (FirstPersonSpecial.enabled)
            {
                UseCamera(CameraType.FirstPerson);
            } else
            {
                UseCamera(CameraType.Labyrinth);
            }
            LabyrinthSpecial.enabled = true;

            // Replace all the portals
            if (TeleportSpecial.enabled)
            {
                SwitchTeleport(false);
                SwitchTeleport(true);
            }
        }
        else if (condition == false && LabyrinthSpecial.enabled == true)
        {
            if (LabyrinthSpecial.disableNextGame == false)
            {
                //TODO: create alert system
                Debug.Log("Will disable feature next game");
                LabyrinthSpecial.disableNextGame = true;
            } else
            {
                LabyrinthSpecial.disableNextGame = false;
            }
            if (FirstPersonSpecial.enabled)
            {
                UseCamera(CameraType.FirstPerson);
            } else
            {
                UseCamera(CameraType.Main);
            }
        }
        LabyrinthSpecial.ButtonText.text = (LabyrinthSpecial.enabled && !LabyrinthSpecial.disableNextGame) ? "Disattiva" : "Attiva";
    }

    void UseLabyrinthCamera(bool condition)
    {
        if (condition)
        {
            if (LabyrinthSpecial.newLabyrinthCamera == null)
            {
                LabyrinthSpecial.newLabyrinthCamera = Instantiate(LabyrinthSpecial.LabyrinthCamera);
            }
            LabyrinthSpecial.newLabyrinthCamera.gameObject.SetActive(true);
            Camera.SetupCurrent(LabyrinthSpecial.newLabyrinthCamera);
        } else
        {
            if (LabyrinthSpecial.newLabyrinthCamera != null)
            {
                Destroy(LabyrinthSpecial.newLabyrinthCamera.gameObject);
            }
        }
    }

    void UseMainCamera(bool condition)
    {
        if (condition)
        {
            GameGodSingleton.MainCamera.gameObject.SetActive(true);
            Camera.SetupCurrent(GameGodSingleton.MainCamera);
        } else
        {
            GameGodSingleton.MainCamera.gameObject.SetActive(false);
        }
    }

    void UseFirstPersonCamera(bool condition)
    {
        if (condition)
        {
            if (FirstPersonSpecial.newFirstPersonCamera == null)
            {
                FirstPersonSpecial.newFirstPersonCamera = Instantiate(FirstPersonSpecial.FirstPersonCamera);
            }
            Camera.SetupCurrent(FirstPersonSpecial.newFirstPersonCamera);
            FirstPersonSpecial.newFirstPersonCamera.GetComponent<FirstPersonCameraController>().offset = Vector3.up * FirstPersonSpecial.CameraHeight;
        } else
        {
            if (FirstPersonSpecial.newFirstPersonCamera != null)
            {
                Destroy(FirstPersonSpecial.newFirstPersonCamera.gameObject);
            }
        }
    }

    public enum CameraType
    {
        Main, FirstPerson, Labyrinth
    }

    public void UseCamera(CameraType type)
    {
        switch (type)
        {
            case CameraType.Main:
                UseFirstPersonCamera(false);
                UseLabyrinthCamera(false);
                UseMainCamera(true);
                break;
            case CameraType.FirstPerson:
                UseMainCamera(false);
                UseLabyrinthCamera(false);
                UseFirstPersonCamera(true);
                break;
            case CameraType.Labyrinth:
                UseFirstPersonCamera(false);
                UseMainCamera(false);
                UseLabyrinthCamera(true);
                break;
        }
    }

    void LoadSpecial(BaseSpecial special, bool condition)
    {
        if (condition == true)
        {
            special.SwitchConditioned(true);
            special.ButtonText.text = "Disattiva";
        }
        else
        {
            print($"NO {special.Title}");
            special.enabled = false;
            special.ButtonText.text = "Attiva";
        }
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
        if (data.TouchSensitivity > 0f)
        {
            SetTouchSensitivity(data.TouchSensitivity);
            TouchSensitivitySlider.value = data.TouchSensitivity;
        }
        SetupSpecialsDelegates();

        if (data.AfroStyle == true && data.RainbowStyle == true)
        {
            data.RainbowStyle = false;
        }
        LoadSpecial(SquareBallsSpecial, data.RoundedBalls);
        LoadSpecial(AfroStyleSpecial, data.AfroStyle);
        LoadSpecial(JumpingDickSpecial, data.DickingJump);
        LoadSpecial(RainbowSpecial, data.RainbowStyle);
        LoadSpecial(MovingWallsSpecial, data.MovingWalls);
        LoadSpecial(Free360MovementSpecial, data.Free360Movement);
        LoadSpecial(PillsBlowerSpecial, data.PillsBlower);
        LoadSpecial(TeleportSpecial, data.Teleport);
        LoadSpecial(ACappellaSpecial, data.ACappella);
        LoadSpecial(FirstPersonSpecial, data.FirstPerson);
        LoadSpecial(LabyrinthSpecial, data.Labyrinth);
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

        data.Sound = isSoundEnabled;
        data.FieldOfView = currentFieldOfView;
        data.TouchSensitivity = currentTouchSensitivity;
        data.RoundedBalls = SquareBallsSpecial.enabled;
        data.AfroStyle = AfroStyleSpecial.enabled;
        data.DickingJump = JumpingDickSpecial.enabled;
        data.RainbowStyle = RainbowSpecial.enabled;
        data.MovingWalls = MovingWallsSpecial.enabled;
        data.Free360Movement = Free360MovementSpecial.enabled;
        data.PillsBlower = PillsBlowerSpecial.enabled;
        data.Teleport = TeleportSpecial.enabled;
        data.ACappella = ACappellaSpecial.enabled;
        data.FirstPerson = FirstPersonSpecial.enabled;
        data.Labyrinth = LabyrinthSpecial.enabled && !LabyrinthSpecial.disableNextGame;
    }
}
