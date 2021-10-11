using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

namespace _ThrowBattle
{
    public enum GameState
    {
        Prepare,
        Playing,
        Paused,
        PreGameOver,
        GameOver
    }

    public enum GameMode
    {
        PlayWithFriend,
        MultiPlayer,
        PlayWithCom,
        AppleShoot,
        BirdHunt
    }

    public enum AILevel
    {
        Easy,
        Normal,
        Hard,
        Expert
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event System.Action<GameState, GameState> GameStateChanged;

        public static event System.Action EnterCharSelectionEvent;

        public static event System.Action ExitCharSelectionEvent;

        private static bool isRestart;

        public GameState GameState
        {
            get
            {
                return _gameState;
            }
            private set
            {
                if (value != _gameState)
                {
                    GameState oldState = _gameState;
                    _gameState = value;

                    if (GameStateChanged != null)
                        GameStateChanged(_gameState, oldState);
                }
            }
        }

        public static int GameCount
        {
            get { return _gameCount; }
            private set { _gameCount = value; }
        }

        private static int _gameCount = 0;
        public static GameMode gameMode;
        [HideInInspector]
        public bool isMultiplayerRematch;
        public bool isActivelyDisconnected;

        [Header("Set the target frame rate for this game")]
        [Tooltip("Use 60 for games requiring smooth quick motion, set -1 to use platform default frame rate")]
        public int targetFrameRate = 30;

        [Header("Current game state")]
        [SerializeField]
        private GameState _gameState = GameState.Prepare;

        // List of public variable for gameplay tweaking
        [Header("Gameplay Config")]
        public float sendDataDelayTime = 0.1f;
        public int numberPlayerWeaponLimit = 2;
        public int appleCoin = 20;
        public float appleDistance = 10;
        public float appleDistanceIncrease = 10;
        public int appleShootLife = 3;
        public float timeWaitOtherPlayer = 15;
        [Range(0f, 1f)]
        public float easyBotHitFrequency = 0.3f;
        [Range(0f, 1f)]
        public float normalBotHitFrequency = 0.5f;
        [Range(0f, 1f)]
        public float hardBotHitFrequency = 0.7f;
        [Range(0f, 1f)]
        public float expertBotHitFrequency = 0.9f;
        [Range(0f, 1f)]
        public float hitFrequencyIncrease = 0.02f;
        [Range(0.7f, 0.95f)]
        public float maxHitFrequency = 0.95f;
        public int winCoin = 100;
        public int drawCoin = 10;

        public float birdHuntTime = 60;

        public float timeCreateBird = 1.25f;

        [Range(0, 1)]
        public float percentageIncreaseSpeed = 0.2f;

        public float minZoom = 8;

        public float maxZoom = 12;
        [HideInInspector]
        public float originalMinZoom;
        [HideInInspector]
        public float originalMaxZoom;
        [HideInInspector]
        public float zoomFactor = 0;
        [HideInInspector]
        public ScreenOrientation curOrientation;

        [HideInInspector]
        public bool isCharSelectionEnter;
        //[Range(0f, 1f)]
        //public float coinFrequency = 0.1f;

        // List of public variables referencing other objects
        [Header("Object References")]
        public PlayerController playerController;
        public PopUpController popUpController;

        void OnEnable()
        {
            PlayerController.PlayerDied += PlayerController_PlayerDied;
            CharacterScroller.ChangeCharacter += CreateNewCharacter;
#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom += PrepareGame;
#endif
        }

        void OnDisable()
        {
            PlayerController.PlayerDied -= PlayerController_PlayerDied;
            CharacterScroller.ChangeCharacter -= CreateNewCharacter;
#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom -= PrepareGame;
#endif
        }

        public void SetZoomValue(float zoomValue)
        {
            minZoom = originalMinZoom * zoomValue;
            maxZoom = originalMaxZoom * zoomValue;
            zoomFactor = zoomValue;
        }

        public void SetOriginalZoom()
        {
            originalMinZoom = minZoom;
            originalMaxZoom = maxZoom;
        }

        public void SetOriginalZoomForAppleShoot(float widthToBeSeen)
        {
            widthToBeSeen += 3f;
            float newZoom = widthToBeSeen * (float)Screen.height / (float)Screen.width * 0.5f;
            if (newZoom > minZoom)
            {
                minZoom = newZoom;
                maxZoom = newZoom + 2;
            }
        }

        void Awake()
        {
            float ratio = Camera.main.aspect;
            Screen.orientation = ScreenOrientation.Portrait;
            SetOriginalZoom();
            SetZoomValue(zoomFactor);
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(Instance.gameObject);
                Instance = this;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void MultiplayerRematch()
        {
            byte[] rematchSignal = { 3 };
            isMultiplayerRematch = true;
        }

        public bool IsMultiplayerMode()
        {
            if (gameMode == GameMode.MultiPlayer)
                return true;
            else
                return false;
        }

        // Use this for initialization
        void Start()
        {
            // Initial setup
            minZoom = originalMinZoom;
            maxZoom = originalMaxZoom;
            PopUpController.Instance = popUpController;
            Application.targetFrameRate = targetFrameRate;
            ScoreManager.Instance.Reset();
            popUpController.HidePopUp();
            PrepareGame();
        }

        // Update is called once per frame
        void Update()
        {

        }

        // Listens to the event when player dies and call GameOver
        void PlayerController_PlayerDied()
        {
            GameOver();
        }

        // Make initial setup and preparations before the game can be played
        public void PrepareGame()
        {
            if (SoundManager.Instance.background != null)
            {
                SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
            }

            GameState = GameState.Prepare;
            Camera.main.orthographicSize = minZoom;
            Screen.orientation = ScreenOrientation.AutoRotation;
            // Automatically start the game if this is a restart.
            if (isRestart)
            {
                isRestart = false;
                Invoke("StartGame", 0.5f);
            }
        }

        public void StartPlayWithFriendMode()
        {
            gameMode = GameMode.PlayWithFriend;
        }

        public void StartPlayWithComMode()
        {
            gameMode = GameMode.PlayWithCom;
        }

        public void StartPlayAppleShootMode()
        {
            gameMode = GameMode.AppleShoot;
        }

        public void StartPlayOnlineMode()
        {
            gameMode = GameMode.MultiPlayer;
        }

        public void StartPlayBirdsHuntMode()
        {
            minZoom *= 2;
            maxZoom *= 2;
            gameMode = GameMode.BirdHunt;
        }

        public void EnterCharacterSelection()
        {
            isCharSelectionEnter = true;
            if (EnterCharSelectionEvent != null)
                EnterCharSelectionEvent();
        }

        public void ExitCharacterSelection()
        {
            if (ExitCharSelectionEvent != null)
                ExitCharSelectionEvent();
        }

        public void PauseGame()
        {
            Time.timeScale = 0;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
        }

        private void OnApplicationQuit()
        {
#if EASY_MOBILE_PRO
            if (multiplayerManager.IsPlayWithOtherPlayer())
            {
                byte[] data = { 12 };
                playerController.SendDataToOtherPlayer(data);
            }
#endif
        }

        // A new game official starts
        public void StartGame()
        {
            Screen.orientation = curOrientation;
            DelayStartGame();
        }

        void DelayStartGame()
        {
            GameState = GameState.Playing;
            if (SoundManager.Instance.background != null)
            {
                SoundManager.Instance.StopMusic();
            }
            if (SoundManager.Instance.inGameBg != null)
            {
                SoundManager.Instance.PlayMusic(SoundManager.Instance.inGameBg);
            }
            //CreateNewCharacter(CharacterManager.Instance.CurrentCharacterIndex);
        }

        // Called when the player died
        public void GameOver()
        {

            if (SoundManager.Instance.inGameBg != null)
            {
                SoundManager.Instance.StopMusic();
            }

            SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
            GameState = GameState.GameOver;
            GameCount++;

            // Add other game over actions here if necessary
        }

        // Start a new game
        public void RestartGame(float delay = 0)
        {
            isRestart = true;
            StartCoroutine(CRRestartGame(delay));
        }

        public void GoHome(float delay = 0)
        {
            minZoom = originalMinZoom / zoomFactor;
            maxZoom = originalMaxZoom / zoomFactor;
            zoomFactor = 1;
            isRestart = false;
            StartCoroutine(CRRestartGame(delay));
        }

        IEnumerator CRRestartGame(float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void HidePlayer()
        {
            if (playerController != null)
                playerController.gameObject.SetActive(false);
        }

        public void ShowPlayer()
        {
            if (playerController != null)
                playerController.gameObject.SetActive(true);
        }

        void CreateNewCharacter(int curChar)
        {
            if (playerController != null)
            {
                DestroyImmediate(playerController.gameObject);
                playerController = null;
            }
            //StartCoroutine(CR_DelayCreateNewCharacter(curChar));
        }

        //IEnumerator CR_DelayCreateNewCharacter(int curChar)
        //{
        //    yield return new WaitForEndOfFrame();
        //    GameObject player = Instantiate(CharacterManager.Instance.characters[curChar]);
        //    player.transform.position = startPlayerPosition;
        //    playerController = player.GetComponent<PlayerController>();
        //}
    }
}