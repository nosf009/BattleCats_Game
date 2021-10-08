using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace _ThrowBattle
{
    public class PlayerController : MonoBehaviour
    {
        public static event System.Action FinishPlayer;
        public static event System.Action MissFinishShot;
        public static event System.Action PlayerDied;
        public static event System.Action PlayerShoot;
        public static event System.Action PlayerWin;
        public static event System.Action PlayerLose;
        public static event System.Action DrawGame;

        public bool isTestPlayer;
        public int otherPlayerIndexTest;

        public ParticleSystem explosion;
        [HideInInspector]
        public MatchResult resultMode;

        public GameObject lifeText;
        public GameObject lifeTextObj;
        public Text textTimeCountDown;
        public GameObject textTimeCountDownObj;

        public GameObject apple;
        public GameObject ground;
        public GameObject rematchUI;
        public GameObject aimUI;
        [HideInInspector]
        public GameObject weapon;
        [HideInInspector]
        public GameObject currentCharacter;
        [HideInInspector]
        public GameObject otherCharacter;
        public GameObject playerTurnNotification;
        [HideInInspector]
        public GameObject midPlayers;
        public GameObject resultTextObj;
        public Text resultText;

        public Transform pivotOtherPlayerDistanceUI;
        public Transform rotatorArrowOtherPlayerDirection;

        public Image firstClickImg;
        public Image currentClickImg;
        public Image player1HealthImg;
        public Image player2HealthImg;
        public Image otherPlayerDistanceUI;

        public Text angleText;
        public Text powerText;


        [HideInInspector]
        public string currentOnlinePlayerID = null;
        [HideInInspector]
        public string otherPlayerID = null;
        [HideInInspector]
        public string thisPlayerID = null;
        [HideInInspector]
        public string lastPlayerID = null;

        public Transform leftPosition;
        public Transform rightPosition;


        Dictionary<int, GameObject> players = new Dictionary<int, GameObject>(2);
        Dictionary<int, GameObject> playerWeapons = new Dictionary<int, GameObject>(2);

        public float force = 1500;
        [HideInInspector]
        public float angle;
        [HideInInspector]
        public float currentForce;
        public float timeWaitResendMapData = 3;
        public float torqueMoveWeapon;
        public float timeCountDown;

        [HideInInspector]
        public int thisPlayerIndex;
        [HideInInspector]
        public int otherPlayerCharacterIndex;
        public int resendDataLimit = 3;
        [HideInInspector]
        public int totalCoin = 0;
        [HideInInspector]
        public int birdCountValue = 0;
        public int birdCount
        {
            get { return birdCountValue; }
            set
            {
                if (birdCountValue != value)
                {
                    birdCountValue = value;
                    if (birdCountValue == 0 && isPlay)
                        StartCoroutine(CreateBird(true));
                }
            }
        }

        public bool IsLastShot
        {
            get
            {
                return isLastShot;
            }
        }

        public bool canShoot
        {
            get { return canShootValue; }
            set
            {
                if (canShootValue != value)
                {
                    canShootValue = value;
                    if (canShootValue && isThisPlayerTurn && isPlay && gameMode != GameMode.BirdHunt)
                        NoticePlayerTurn();
                }
            }
        }
        [HideInInspector]
        public bool isThisPlayerShoot;
        [HideInInspector]
        public bool canShootValue = false;
        [HideInInspector]
        public bool isPlay;
        [HideInInspector]
        public bool isThisPlayerTurn;
        [HideInInspector]
        public bool isShotFromThisPlayer;
        [HideInInspector]
        public bool isDie;
        [HideInInspector]
        public bool generateMapComplete;

#if EASY_MOBILE_PRO
        private MultiplayerRealtimeManager multiplayer;
#endif
        private MultiplayerGameData gameData;
        private AILevel enemyLevel;
        private LineRenderer aimLine;
        private GameMode gameMode;
        private Text textOtherPlayerDistance;
        private Weapon weaponComponent;

        private GameObject bodyAim;
        private GameObject leftPlayerWeapon;
        private GameObject rightPlayerWeapon;
        private Quaternion orgRotationDistanceUI;

        private Vector2 shootDirection;
        private Vector2 originalTouchPosition;
        private Vector2 currentTouchPosition;
        private Vector2 imgScreenPosition;
        private Vector2 otherPlayerScreenPosition;
        private Vector2 centerScreenPosition;
        private Vector2 birdSpawnPosition1;
        private Vector2 birdSpawnPosition2;


        //private Vector3 worldPositionOriginalTouch;
        //private Vector3 worldPositionCurrentTouch;

        private float aimDistanceLimit = 0.35f;
        private float currentWorldAimDistance;
        private float angleForUI;
        private float sendDataDelayTime;
        private float characterHeigh;
        private float pixelWidth;
        private float pixelHeight;
        private float previousAngle = 0;
        private float enemyHitFrequency;

        [HideInInspector]
        public int currentPlayerIndex = 1;
        private int leftPlayerWeaponCount = 0;
        private int rightPlayerWeaponCount = 0;
        private int numberWeaponLimit;
        private int currentAimDirection;
#if EASY_MOBILE_PRO
        private int multiplayerRematchCount = 0;
#endif
        private int resendDataCount = 0;
        private int count = 0;
        private int limitAimCount = 0;
        private int life = 3;

        private bool isShootPhase = false;
        private bool isFirstAim = true;
#if EASY_MOBILE_PRO
        private bool isSendData = true;
#endif
        private bool haveCreateMap = false;
        private bool waitEndTurn;
        private bool firstLogin = true;
        private bool isTurnBegin = true;
        private bool isFirstComAim = true;
        private bool holdShoot = false;
        private bool isCheckPower = false;
        private bool isFinishPlayer;
        private bool isLastShot = false;
        private bool isFirst = true;
        private bool isReceivedOtherChar = false;

        private Rigidbody2D rigidbodyWeapon;

        void OnEnable()
        {
            resultTextObj.SetActive(false);
            aimLine = GetComponent<LineRenderer>();
            GameManager.GameStateChanged += OnGameStateChanged;
            isFinishPlayer = false;
            isLastShot = false;

#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom += OnPlayerLeft;
#endif
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;

#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom -= OnPlayerLeft;
#endif
        }

        void OnUserLoginSucceeded()
        {
            Debug.Log("User logged in successfully.");
        }

        void OnUserLoginFailed()
        {
            if (firstLogin)
            {
                firstLogin = false;
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    PopUpController.Instance.SetMessage("Unable to connect to the internet, you are in offline mode");
                    PopUpController.Instance.ShowPopUp();
                }
                Debug.Log("User login failed.");
            }
        }

        public void LostLife()
        {
            if (life > 1)
            {
                life--;
                lifeText.GetComponent<Text>().text = "Life : " + life.ToString();
            }
            else
            {
                lifeTextObj.SetActive(false);
                resultMode = MatchResult.AppleShoot;
                Die();
                CoinManager.Instance.AddCoins(ScoreManager.Instance.Score * GameManager.Instance.appleCoin);
            }
        }

        public void MissFinish()
        {
            if (MissFinishShot != null)
                MissFinishShot();
            currentCharacter.GetComponent<PlayerManager>().TurnOnPhysics();
            Invoke("Die", 0.5f);
        }



        public void ResetCamera()
        {

            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerController.canShoot = true;
            }

            if (Camera.main != null)
            {
                StartCoroutine(ZoomLerp(GameManager.Instance.maxZoom, 0.25f));
                Camera.main.transform.SetParent(null);
                Camera.main.GetComponent<CameraController>().followInjuredPlayer = false;
                Camera.main.GetComponent<CameraController>().fixPosition = true;
            }
        }



        public void RaiseFinishPlayer()
        {

            isFinishPlayer = true;
            isLastShot = true;
            if (GameManager.gameMode == GameMode.PlayWithFriend)
            {
                if (currentCharacter.GetComponent<Character>().health > 0)
                    ChangeTurn();
            }
            if (resultMode == MatchResult.Draw)
            {
                Invoke("Die", 0.2f);
                return;
            }
            else if (resultMode != MatchResult.Suicide)
            {
                ChangeTurn();
            }
            if (GameManager.gameMode == GameMode.MultiPlayer && resultMode == MatchResult.Lose)
            {
                if (lastPlayerID == thisPlayerID)
                {
                    byte[] suicideAlert = { 15 };
                    GameManager.Instance.playerController.SendDataToOtherPlayer(suicideAlert);
                    ChangeTurn();
                }
            }

            Invoke("ResetCamera", 1f);
            if (FinishPlayer != null)
            {
                FinishPlayer();
            }

        }

        //Show popup with message depend on game mode
        void ShowResult()
        {
            if (resultMode == MatchResult.Win)
            {
                CoinManager.Instance.AddCoins(GameManager.Instance.winCoin);
                GameManager.Instance.playerController.RaiseEventPlayerWin();
                PopUpController.Instance.SetMessage("You Win!");
                resultText.text = "You win!";
            }
            if (resultMode == MatchResult.Draw)
            {
                CoinManager.Instance.AddCoins(GameManager.Instance.drawCoin);
                GameManager.Instance.playerController.RaiseEventDrawGame();
                PopUpController.Instance.SetMessage("Draw!");
                resultText.text = "Draw!";
            }
            if (resultMode == MatchResult.Lose)
            {
                GameManager.Instance.playerController.RaiseEventPlayerLose();
                PopUpController.Instance.SetMessage("You Lose!");
                resultText.text = "You lose!";
            }
            if (resultMode == MatchResult.Suicide)
            {
                GameManager.Instance.playerController.RaiseEventPlayerLose();
                PopUpController.Instance.SetMessage("You committed suicide!");
                resultText.text = "You committed suicide!";
            }
            if (resultMode == MatchResult.Player2Win)
            {
                PopUpController.Instance.SetMessage("Player 2 win!");
                resultText.text = "Player 2 win!";
            }
            if (resultMode == MatchResult.Player1Win)
            {
                PopUpController.Instance.SetMessage("Player 1 win");
                resultText.text = "Player 1 win!";
            }
            if (resultMode == MatchResult.AppleShoot)
            {
                PopUpController.Instance.SetMessage("You shot " + ScoreManager.Instance.Score + " apples and get " + ScoreManager.Instance.Score * GameManager.Instance.appleCoin + " coins");
                resultText.text = "You shot " + ScoreManager.Instance.Score + " apples";
                PopUpController.Instance.ShowPopUp();
            }
            if (resultMode == MatchResult.BirdHunt)
            {
                CoinManager.Instance.AddCoins(ScoreManager.Instance.Score * GameManager.Instance.appleCoin);
                PopUpController.Instance.SetMessage("You shot " + ScoreManager.Instance.Score + " birds and get " + totalCoin + " coins");
                resultText.text = "You shot " + ScoreManager.Instance.Score + " birds";
                PopUpController.Instance.ShowPopUp();
            }
            resultTextObj.SetActive(true);

        }

        //Update player health in canvas after shoot
        public void UpdatePlayerHealth()
        {
            Character player1 = players[1].GetComponent<Character>();
            Character player2 = players[-1].GetComponent<Character>();
            float factor1 = (float)decimal.Divide(player1.health, player1.originalHealth);
            float factor2 = (float)decimal.Divide(player2.health, player2.originalHealth);
            StartCoroutine(LerpPlayer1HealthImg(factor1));
            StartCoroutine(LerpPlayer2HealthImg(factor2));
        }

        IEnumerator LerpPlayer1HealthImg(float targetFill)
        {
            var startTime = Time.time;
            float runTime = 0.5f;
            float timePast = 0;
            float originalFill = player1HealthImg.fillAmount;

            while (Time.time < startTime + runTime)
            {
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                player1HealthImg.fillAmount = Mathf.Lerp(originalFill, targetFill, factor);
                yield return null;
            }
            player1HealthImg.fillAmount = targetFill;
        }

        IEnumerator LerpPlayer2HealthImg(float targetFill)
        {
            var startTime = Time.time;
            float runTime = 0.5f;
            float timePast = 0;
            float originalFill = player2HealthImg.fillAmount;

            while (Time.time < startTime + runTime)
            {
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                player2HealthImg.fillAmount = Mathf.Lerp(originalFill, targetFill, factor);
                yield return null;
            }
            player2HealthImg.fillAmount = targetFill;
        }

        void Start()
        {
            textTimeCountDownObj.SetActive(false);
            lifeTextObj.SetActive(false);
            numberWeaponLimit = GameManager.Instance.numberPlayerWeaponLimit;
            orgRotationDistanceUI = otherPlayerDistanceUI.transform.rotation;
            centerScreenPosition = new Vector2(pixelWidth / 2, pixelHeight / 2);
            pivotOtherPlayerDistanceUI.gameObject.SetActive(false);
            textOtherPlayerDistance = otherPlayerDistanceUI.transform.GetComponentInChildren<Text>();
            pixelHeight = Camera.main.pixelHeight;
            pixelWidth = Camera.main.pixelWidth;
            gameData = new MultiplayerGameData();
#if EASY_MOBILE_PRO
            multiplayer = GameManager.Instance.multiplayerManager;
#endif
            sendDataDelayTime = GameManager.Instance.sendDataDelayTime;
            firstClickImg.gameObject.SetActive(false);
            aimLine.enabled = false;
            currentClickImg.gameObject.SetActive(false);
            // Setup
        }

        // Update is called once per frame
        void Update()
        {
            if (isPlay && isThisPlayerTurn && otherCharacter != null)
            {
                ShowOtherPlayerDistance();

            }
            if (isPlay && otherPlayerDistanceUI.gameObject.activeSelf && !isThisPlayerTurn)
                pivotOtherPlayerDistanceUI.gameObject.SetActive(false);
            if (isPlay && isThisPlayerTurn && canShoot && !Camera.main.GetComponent<CameraController>().isCamMoving)
            {
                TouchHandle();
            }
            else if (isPlay && canShoot && gameMode == GameMode.PlayWithCom && !isThisPlayerTurn)
            {
                ComPlayerHandling();
            }
            if (midPlayers)
            {
                if (currentCharacter)
                {
                    Vector3 tempPos = midPlayers.transform.position;
                    tempPos.y = currentCharacter.transform.position.y;
                    midPlayers.transform.position = tempPos;
                }
            }
        }

        void ShowOtherPlayerDistance()
        {
            otherPlayerScreenPosition = Camera.main.WorldToScreenPoint(otherCharacter.transform.position);
            Vector2 viewPort = Camera.main.WorldToViewportPoint(otherCharacter.transform.position);
            float angle = Vector2.SignedAngle(Vector2.right, otherPlayerScreenPosition - centerScreenPosition);
            rotatorArrowOtherPlayerDirection.transform.localEulerAngles = new Vector3(0, 0, angle);
            if (!(viewPort.x <= 1 && viewPort.y <= 1 && viewPort.x >= 0 && viewPort.y >= 0))
            {
                if (!pivotOtherPlayerDistanceUI.gameObject.activeSelf)
                    pivotOtherPlayerDistanceUI.gameObject.SetActive(true);
                if (otherPlayerScreenPosition.x > pixelWidth)
                    otherPlayerScreenPosition.x = pixelWidth;
                if (otherPlayerScreenPosition.x < 0)
                    otherPlayerScreenPosition.x = 0;
                if (otherPlayerScreenPosition.y > pixelHeight)
                    otherPlayerScreenPosition.y = pixelHeight;
                if (otherPlayerScreenPosition.y < 0)
                    otherPlayerScreenPosition.y = 0;
                pivotOtherPlayerDistanceUI.GetComponent<RectTransform>().position = otherPlayerScreenPosition;
                otherPlayerDistanceUI.transform.rotation = orgRotationDistanceUI;
                textOtherPlayerDistance.text = ((int)Vector2.Distance(Camera.main.ScreenToWorldPoint(centerScreenPosition), otherCharacter.transform.position)).ToString() + "m";
            }
            else if (pivotOtherPlayerDistanceUI.gameObject.activeSelf)
            {
                pivotOtherPlayerDistanceUI.gameObject.SetActive(false);
            }
        }



        #region Multiplayer Handle

        public void StartWaitOtherGenerateMap()
        {
            StartCoroutine(WaitOtherGenerateMap());
        }

        //Loop check if other player receive data to generated the map yet,if not then send map's data again
        IEnumerator WaitOtherGenerateMap()
        {
            yield return new WaitForSeconds(timeWaitResendMapData);
            if (!generateMapComplete)
            {
                if (resendDataCount < resendDataLimit)
                {
                    byte[] thisPlayerCharacterIndex = { 9, (byte)CharacterManager.Instance.CurrentCharacterIndex };
                    SendDataToOtherPlayer(thisPlayerCharacterIndex);
                    ground.GetComponent<MapGenerate>().SendMapData();
                    resendDataCount++;
                }
                else
                {
                    GameManager.Instance.multiplayerManager.Disconnect();
                    PopUpController.Instance.SetMessage("Can't connect with other player");
                    PopUpController.Instance.ShowPopUp();
                }
            }
        }

        //When other player or this player leave the room
        void OnPlayerLeft()
        {
            if (players.ContainsKey(1))
                Destroy(players[1]);
            if (players.ContainsKey(-1))
                Destroy(players[-1]);
            if (playerWeapons.ContainsKey(1))
                Destroy(playerWeapons[1]);
            if (playerWeapons.ContainsKey(-1))
                Destroy(playerWeapons[-1]);

            textTimeCountDownObj.SetActive(false);
            isPlay = false;
            players.Clear();
            playerWeapons.Clear();
            leftPlayerWeaponCount = 0;
            rightPlayerWeaponCount = 0;
            if (!GameManager.Instance.isActivelyDisconnected)
            {
                PopUpController.Instance.SetMessage("Other player have disconnected!");
                PopUpController.Instance.ShowPopUp();
            }
            pivotOtherPlayerDistanceUI.gameObject.SetActive(false);
            aimUI.gameObject.SetActive(false);
            TurnAimLine(false);
        }

        //Send data to other player for multiplayer mode
        public void SendDataToOtherPlayer(byte[] data)
        {
#if EASY_MOBILE_PRO
            multiplayer.SendMessage(true, otherPlayerID, data);
#endif
        }

        IEnumerator WaitAnotherCharacterIndex()
        {
            float timeout = 0;
            while (!isReceivedOtherChar)
            {
                timeout += Time.deltaTime;
                if (timeout > 8f)
                {
                    break;
                }
                yield return null;
            }
            if (isReceivedOtherChar)
            {
                generateMapComplete = true;
                GameManager.Instance.StartGame();
                StopCoroutine(WaitOtherGenerateMap());
            }
            else
            {
                GameManager.Instance.multiplayerManager.Disconnect();
                PopUpController.Instance.SetMessage("Can't connect with other player");
                PopUpController.Instance.ShowPopUp();
            }
        }

        //Handle received data for multiplayer game mode
        public void HandleDataReceive(byte[] data)
        {

            //The received data has the first byte as the signal to determine which data it is
            //The real data is data array without first byte then store real data into finalData by remove first byte
            byte[] finalData = new byte[data.Length - 1];
            finalData = GetByteArrayByRange(data, 1, data.Length);

            //0.this is current player's health data
            if (data[0] == 0)
            {
                if (data[1] == 1)
                    players[1].GetComponent<Character>().ReceiveHealthData(finalData);
                else
                    players[-1].GetComponent<Character>().ReceiveHealthData(finalData);
                return;
            }
            //1.this is the first click signal.
            if (data[0] == 1)
            {
                FirstTouchHandling();
                return;
            }
            //2.this is the signal to start the game
            if (data[0] == 2)
            {
                if (isReceivedOtherChar)
                {
                    generateMapComplete = true;
                    GameManager.Instance.StartGame();
                    StopCoroutine(WaitOtherGenerateMap());
                    return;
                }
                else
                {
                    StartCoroutine(WaitAnotherCharacterIndex());
                }

            }
            // 3.this is the signal to rematch
            if (data[0] == 3)
            {
                if (!GameManager.Instance.isMultiplayerRematch)
                    ShowRematchInvitation();
                else
                {
                    AcceptRematch();
                }
                return;
            }
            //4.this is the signal other player has rejected the invitation
            if (data[0] == 4)
            {
                GameManager.Instance.multiplayerManager.waitingUI.SetActive(false);
                PopUpController.Instance.SetMessage("Invitation rejected");
                PopUpController.Instance.ShowPopUp();
                return;
            }
            //5.the received data is signal for player to shoot
            if (data[0] == 5)
            {
                HandleReceivedShootData(finalData);
                return;
            }
            //6.the received data is map's data
            if (!haveCreateMap && data[0] == 6)
            {
                ground.GetComponent<MapGenerate>().ByteArrayToMapData(finalData);
                haveCreateMap = false;
                return;
            }
            //7.Received data is shoot data include player's position,health
            if (data[0] == 7)
            {
                isShotFromThisPlayer = false;
                if (finalData[0] == 1)
                {
                    players[1].GetComponent<Character>().SetShootData(finalData);
                    players[1].GetComponent<Character>().hasReceivedShootData = true;
                    players[1].GetComponent<Character>().StartCheckHandleShootData();
                }
                else
                {
                    players[-1].GetComponent<Character>().SetShootData(finalData);
                    players[-1].GetComponent<Character>().hasReceivedShootData = true;
                    players[1].GetComponent<Character>().StartCheckHandleShootData();
                }
                return;
            }
            //8.that is the angle to aim
            if (data[0] == 8)
            {
                HandleReceivedAngle(finalData);
                return;
            }
            //9.this is other player's character index which will use to select character
            if (data[0] == (byte)9)
            {
                isReceivedOtherChar = true;
                otherPlayerCharacterIndex = finalData[0];
                return;
            }
            //10.this is signal for current weapon to do special action
            if (data[0] == 10)
            {
                weapon.GetComponent<SpecialAction>().HandleDataForAction(finalData);
            }
            //11.this is other player's character data
            if (data[0] == 11)
            {
                otherPlayerCharacterIndex = finalData[0];
            }
            //12.this is signal other player have left the game
            if (data[0] == 12)
            {
                GameManager.Instance.multiplayerManager.Disconnect();
                PopUpController.Instance.SetMessage("Other player have disconnected!");
                PopUpController.Instance.ShowPopUp();
            }
            //13.this is request this player to send back health,position of current character
            if (data[0] == 13)
            {
                byte[] dataSendBack = { 14 };
                dataSendBack = dataSendBack.Concat(BitConverter.GetBytes(currentCharacter.GetComponent<Character>().health)).Concat(BitConverter.GetBytes(currentCharacter.transform.position.x))
                    .Concat(BitConverter.GetBytes(currentCharacter.transform.position.y)).ToArray();
                SendDataToOtherPlayer(dataSendBack);
            }
            //14.this is data send back from step above include health,position of current character
            if (data[0] == 14)
            {
                currentCharacter.GetComponent<Character>().ReceiveDataRequest(data);
            }

            //15. Received suicide alert: the other had suicided
            if (data[0] == 15)
            {
                ChangeTurn();
            }
        }

        //If the data received is shoot data
        //then sync the current shot angle of the player with the firing angle of this data and then start firing
        void HandleReceivedShootData(byte[] data)
        {
            isShotFromThisPlayer = false;
            gameData = MultiplayerGameDataExtension.ToMultiplayerGameData(data);
            AimByReceiveAngle(gameData.angleForUI);
            angle = gameData.angle;
            currentForce = gameData.force;
            shootDirection.x = gameData.shootDirectionX;
            shootDirection.y = gameData.shootDirectionY;
            Vector3 shootPosition = new Vector2();
            shootPosition.x = gameData.shootPositionX;
            shootPosition.y = gameData.shootPositionY;
            weapon.transform.position = shootPosition;
            Vector3 otherPlayerPosition = new Vector2();
            otherPlayerPosition.x = gameData.otherPlayerPositionX;
            otherPlayerPosition.y = gameData.otherPlayerPositionY;
            otherCharacter.transform.position = otherPlayerPosition;
            lastPlayerID = gameData.playerID;
            Shoot();
        }


        //Get the aiming angle every amount of time then rotate the current player's body with this angle
        void HandleReceivedAngle(byte[] data)
        {
            float receiveAngle = BitConverter.ToSingle(data, 0);
            AimByReceiveAngle(receiveAngle);
        }

        //when player shoot,it will send signal to other device include shoot direction,power and aim angle
        void SendDataOnShoot()
        {
#if EASY_MOBILE_PRO
            gameData.playerID = thisPlayerID;
            gameData.force = currentForce;
            gameData.angle = angle;
            gameData.angleForUI = angleForUI;
            gameData.shootDirectionX = shootDirection.x;
            gameData.shootDirectionY = shootDirection.y;
            gameData.shootPositionX = weapon.transform.position.x;
            gameData.shootPositionY = weapon.transform.position.y;
            gameData.otherPlayerPositionX = otherCharacter.transform.position.x;
            gameData.otherPlayerPositionY = otherCharacter.transform.position.y;
            StopAllCoroutines();
            byte[] shootData = { 5 };
            shootData = shootData.Concat(gameData.ToByteArray()).ToArray();
            isSendData = false;
            multiplayer.SendMessage(true, otherPlayerID, shootData);
#endif
        }


        //Generate terrain before playing
        public void SettingBeforePlay()
        {
            isLastShot = false;
            isFinishPlayer = false;
            if (currentOnlinePlayerID == thisPlayerID)
            {
                ground.GetComponent<MapGenerate>().GenerateMap();
            }
        }
        public void AcceptRematch()
        {
            GameManager.Instance.multiplayerManager.waitingUI.SetActive(false);
            GameManager.Instance.multiplayerManager.RaiseLeaveRoomEvent();
            MultiplayerRematch();
        }

        void ShowRematchInvitation()
        {
            rematchUI.SetActive(true);
        }

        public void HideRematchInviTation()
        {
            rematchUI.SetActive(false);
        }

        public void DeclinedRematchInvitation()
        {
            rematchUI.SetActive(false);
            byte[] declinedSignal = { 4 };
            SendDataToOtherPlayer(declinedSignal);

#if EASY_MOBILE_PRO
            GameManager.Instance.multiplayerManager.LeaveRoom();
#endif
        }

        //This method will be called on all device to start rematch after the invitation of the rematch is accepted
        void MultiplayerRematch()
        {
#if EASY_MOBILE_PRO
            GameManager.Instance.multiplayerManager.waitingUI.SetActive(false);
            GameManager.Instance.isMultiplayerRematch = false;
            List<EasyMobile.Participant> participantsArray = GameManager.Instance.multiplayerManager.GetConnectedParticipants();
            if (participantsArray.Count < 2)
                return;

            if (multiplayerRematchCount % 2 == 0)
                currentOnlinePlayerID = participantsArray[1].ParticipantId;
            else
                currentOnlinePlayerID = participantsArray[0].ParticipantId;
            multiplayerRematchCount++;
            SettingBeforePlay();
#endif
        }


        //Send this angle for other device to show in gameplay each period of time
        IEnumerator WaitSendAimAngle()
        {

            yield return new WaitForSeconds(sendDataDelayTime);
#if EASY_MOBILE_PRO
            if (isSendData)
            {
                byte[] byteArrayAngle = { 8 };
                byteArrayAngle = byteArrayAngle.Concat(BitConverter.GetBytes(angleForUI)).ToArray();
                multiplayer.SendMessage(true, otherPlayerID, byteArrayAngle);
                StartCoroutine(WaitSendAimAngle());
            }
#endif
        }

        //Get a byte array from another byte array by range
        byte[] GetByteArrayByRange(byte[] array, int fromIndex, int toIndex)
        {
            byte[] result = new byte[toIndex - fromIndex];
            int resultIndex = 0;
            for (int i = fromIndex; i < toIndex; i++)
            {
                result[resultIndex] = array[i];
                resultIndex++;
            }
            return result;
        }
        #endregion
        #region Main GamePlay

        public void NoticePlayerTurn(string message = null)
        {
            if (isFinishPlayer)
                playerTurnNotification.GetComponentInChildren<Text>().text = "Finish Him";
            else
                playerTurnNotification.GetComponentInChildren<Text>().text = "Your turn";
            if (message != null)
                playerTurnNotification.GetComponentInChildren<Text>().text = message;
            playerTurnNotification.SetActive(true);
            Invoke("TurnOffNoticePlayerTurn", 1.5f);
        }

        void TurnOffNoticePlayerTurn()
        {
            playerTurnNotification.SetActive(false);
        }
        //Handle for touch input
        void TouchHandle()
        {
            if (Input.GetMouseButtonDown(0) && !isPointerOverUIObject())
            {
                FirstTouchHandling();
            }
            if (isShootPhase && Input.GetMouseButton(0))
            {
                CalculatorAngleAndDirection();
                Aim();
            }
            if (isShootPhase && Input.GetMouseButtonUp(0) && !holdShoot)
            {
                Shoot();
            }
        }

        //first click will Create weapon,send a signal for other device to starting aim and get mouse position
        void FirstTouchHandling()
        {
            isShotFromThisPlayer = true;
            isShootPhase = true;

#if EASY_MOBILE_PRO
            isSendData = true;
#endif

            if (isThisPlayerTurn)
                GetFirstTouch();

            CreateWeapon();

            SettingCurrentCharacter();
            if (isThisPlayerTurn && gameMode == GameMode.MultiPlayer)
            {
#if EASY_MOBILE_PRO
                byte[] firstTouchFlag = { 1 };
                multiplayer.SendMessage(true, otherPlayerID, firstTouchFlag);
#endif
            }
        }

        //Get body of current character and play animation
        void SettingCurrentCharacter()
        {
            bodyAim = currentCharacter.GetComponent<PlayerManager>().body.gameObject;
            currentCharacter.GetComponent<PlayerManager>().SetAimAnimation(true);
        }

        //Get first click position to calculate power and direction
        void GetFirstTouch()
        {
            aimUI.SetActive(true);
            Invoke("StartCheckPower", 0.25f);
            TurnAimLine(true);
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = -2;
            aimLine.SetPosition(0, mousePosition);
            originalTouchPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            mousePosition.z = 0;
            firstClickImg.rectTransform.position = Input.mousePosition;
            currentClickImg.rectTransform.position = Input.mousePosition;
            //worldPositionOriginalTouch = Camera.main.ScreenToWorldPoint(originalTouchPosition);
        }

        void StartCheckPower()
        {
            isCheckPower = true;
        }

        void TurnAimLine(bool isOn)
        {
            firstClickImg.gameObject.SetActive(isOn);
            currentClickImg.gameObject.SetActive(isOn);
            aimLine.enabled = isOn;
            if (isOn)
                aimLine.gameObject.transform.position = Vector3.zero;
        }

        //Create current player's weapon at local position
        void CreateWeapon()
        {
            GameObject currentCharacterWeapon = playerWeapons[currentPlayerIndex];
            CheckCreateWeapon(currentCharacterWeapon);
            weaponComponent = weapon.GetComponent<Weapon>();
            weaponComponent.SettingToShoot();
            rigidbodyWeapon = weapon.GetComponent<Rigidbody2D>();

            weapon.transform.position = currentCharacterWeapon.transform.position;
            weapon.transform.SetParent(currentCharacterWeapon.transform.parent);
            weapon.transform.localRotation = currentCharacterWeapon.transform.localRotation;
            weapon.transform.localScale = currentCharacterWeapon.transform.localScale;
        }

        void CheckCreateWeapon(GameObject currentCharacterWeapon)
        {
            if (currentPlayerIndex == 1)
            {
                if (leftPlayerWeaponCount < numberWeaponLimit)
                {
                    weapon = Instantiate(currentCharacterWeapon);
                    leftPlayerWeapon = weapon;
                    leftPlayerWeaponCount++;
                }
                else
                    weapon = leftPlayerWeapon;
            }
            else
            {
                if (rightPlayerWeaponCount < numberWeaponLimit)
                {
                    weapon = Instantiate(currentCharacterWeapon);
                    rightPlayerWeapon = weapon;
                    rightPlayerWeaponCount++;
                }
                else
                    weapon = rightPlayerWeapon;
            }
        }

        void Aim()
        {
            //Depent on left or right player to show Aim UI
            angleForUI = angle;
            if (currentAimDirection == -1)
            {
                if (angleForUI > 0)
                    angleForUI = 180 - angleForUI;
                else
                    angleForUI = -180 - angleForUI;

                angleForUI *= -1;
            }

            RotatingBody();

            if (isFirstAim && gameMode == GameMode.MultiPlayer)
            {
                isFirstAim = false;
                StartCoroutine(WaitSendAimAngle());
            }
            ShowAimUI(angleForUI, shootDirection);
        }

        //Depend on left or right player's position to make different rotate
        void RotatingBody()
        {
            if (currentPlayerIndex == 1)
            {
                if (angleForUI < -90 || angleForUI > 90)
                {
                    if (currentCharacter.transform.localScale != Vector3.one)
                    {
                        currentCharacter.transform.localScale = Vector3.one;
                        currentCharacter.GetComponent<PlayerManager>().UpdateFootJoinAnchor();
                    }
                    bodyAim.transform.eulerAngles = new Vector3(0, 0, -(180 - angleForUI));
                }
                else
                {
                    if (currentCharacter.transform.localScale != new Vector3(-1, 1, 1))
                    {
                        currentCharacter.transform.localScale = new Vector3(-1, 1, 1);
                        currentCharacter.GetComponent<PlayerManager>().UpdateFootJoinAnchor();
                    }
                    bodyAim.transform.eulerAngles = new Vector3(0, 0, angleForUI);
                }
            }
            else
            {
                if (angleForUI < -90 || angleForUI > 90)
                {
                    if (currentCharacter.transform.localScale != new Vector3(-1, 1, 1))
                    {
                        currentCharacter.transform.localScale = new Vector3(-1, 1, 1);
                        currentCharacter.GetComponent<PlayerManager>().UpdateFootJoinAnchor();
                    }
                    bodyAim.transform.eulerAngles = new Vector3(0, 0, -(180 - angleForUI));
                }
                else
                {
                    if (currentCharacter.transform.localScale != Vector3.one)
                    {
                        currentCharacter.transform.localScale = Vector3.one;
                        currentCharacter.GetComponent<PlayerManager>().UpdateFootJoinAnchor();
                    }
                    bodyAim.transform.eulerAngles = new Vector3(0, 0, angleForUI);
                }
            }
        }

        void AimByReceiveAngle(float receiveAngle)
        {
            if (currentPlayerIndex == 1)
            {
                if (receiveAngle < -90 || receiveAngle > 90)
                {
                    if (currentCharacter.transform.localScale != Vector3.one)
                    {
                        currentCharacter.transform.localScale = Vector3.one;
                        currentCharacter.GetComponent<PlayerManager>().UpdateFootJoinAnchor();
                    }
                    bodyAim.transform.eulerAngles = new Vector3(0, 0, -(180 - receiveAngle));
                }
                else
                {
                    if (currentCharacter.transform.localScale != new Vector3(-1, 1, 1))
                    {
                        currentCharacter.transform.localScale = new Vector3(-1, 1, 1);
                        currentCharacter.GetComponent<PlayerManager>().UpdateFootJoinAnchor();
                    }
                    bodyAim.transform.eulerAngles = new Vector3(0, 0, receiveAngle);
                }
            }
            else
            {
                if (receiveAngle < -90 || receiveAngle > 90)
                {
                    if (currentCharacter.transform.localScale != new Vector3(-1, 1, 1))
                    {
                        currentCharacter.transform.localScale = new Vector3(-1, 1, 1);
                        currentCharacter.GetComponent<PlayerManager>().UpdateFootJoinAnchor();
                    }
                    bodyAim.transform.eulerAngles = new Vector3(0, 0, -(180 - receiveAngle));
                }
                else
                {
                    if (currentCharacter.transform.localScale != Vector3.one)
                    {
                        currentCharacter.transform.localScale = Vector3.one;
                        currentCharacter.GetComponent<PlayerManager>().UpdateFootJoinAnchor();
                    }
                    bodyAim.transform.eulerAngles = new Vector3(0, 0, receiveAngle);
                }
            }
        }

        //Get angle and direction to aim and shoot base on input
        void CalculatorAngleAndDirection()
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            //Get current mouse position and compare with first click position
            currentTouchPosition = Camera.main.ScreenToViewportPoint(mouseScreenPosition);
            currentClickImg.rectTransform.position = mouseScreenPosition;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            mousePosition.z = -2;
            aimLine.SetPosition(1, mousePosition);
            firstClickImg.rectTransform.position = Camera.main.WorldToScreenPoint(aimLine.GetPosition(0));
            shootDirection = currentTouchPosition - originalTouchPosition;

            //calculate power by distance between current and first mouse position
            //worldPositionCurrentTouch = Camera.main.ScreenToWorldPoint(currentTouchPosition);
            currentWorldAimDistance = (currentTouchPosition - originalTouchPosition).magnitude;
            currentForce = currentWorldAimDistance / aimDistanceLimit;
            if (currentForce > 1)
                currentForce = 1;

            //calculate angle by direction between current and first mouse position
            angle = Vector2.SignedAngle(Vector2.right, -shootDirection);
        }

        //Show Power and Angle Aim above player's head
        void ShowAimUI(float angleForUI, Vector2 direction, bool isShow = true)
        {
            Vector2 dir = (Vector2)(Quaternion.Euler(0, 0, 90) * -shootDirection.normalized * currentAimDirection);
            Vector3 position = Camera.main.WorldToScreenPoint(currentCharacter.transform.position + (Vector3)dir.normalized * characterHeigh * 1.5f);

            aimUI.transform.position = position;
            if (gameMode != GameMode.BirdHunt)
                Camera.main.orthographicSize = Mathf.Lerp(GameManager.Instance.minZoom, GameManager.Instance.maxZoom, currentForce);
            int forceText = (int)(currentForce * 100);
            if (isCheckPower)
            {
                if (forceText < 10 && !holdShoot)
                {
                    TurnAimLine(false);
                    holdShoot = true;
                    aimUI.SetActive(false);
                    currentCharacter.GetComponent<PlayerManager>().SetAimAnimation(false);
                    weapon.SetActive(false);
                }
                else if (forceText > 10 && holdShoot)
                {
                    TurnAimLine(true);
                    holdShoot = false;
                    aimUI.SetActive(true);
                    weapon.SetActive(true);
                    currentCharacter.GetComponent<PlayerManager>().SetAimAnimation(true);
                }
            }
            powerText.text = forceText.ToString() + "%";
            if (currentAimDirection == -1)
                angleForUI = -angleForUI;
            angleText.text = ((int)angleForUI).ToString();
            if (gameMode == GameMode.BirdHunt)
                aimUI.SetActive(false);
        }

        //Shoot when the touch is released
        void Shoot()
        {
            weaponComponent.StartShoot();
            if (gameMode != GameMode.BirdHunt)
                players[-currentPlayerIndex].GetComponent<PlayerManager>().ResetAnimator();
            isCheckPower = false;
            TurnAimLine(false);
            if (isThisPlayerTurn && gameMode == GameMode.MultiPlayer)
            {
                isShotFromThisPlayer = true;
                lastPlayerID = thisPlayerID;
                SendDataOnShoot();
            }
            if (isThisPlayerTurn)
                isThisPlayerShoot = true;
            else
                isThisPlayerShoot = false;
            ResetShootSetting();
            SettingWeaponToShoot();
            bodyAim.transform.eulerAngles = Vector3.zero;
            Camera.main.GetComponent<CameraController>().fixPosition = false;
            //Camera.main.transform.SetParent(weapon.transform);
            Vector3 fixPosition = weapon.transform.position;
            fixPosition.z = Camera.main.transform.position.z;
            Camera.main.transform.position = fixPosition;
            currentCharacter.GetComponent<PlayerManager>().SetAimAnimation(false);

            if (SoundManager.Instance.shot != null)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.shot);
            }
            if (PlayerShoot != null)
                PlayerShoot();
        }

        void ResetShootSetting()
        {
            isShootPhase = false;
            aimUI.SetActive(false);
            canShoot = false;
            isFirstAim = true;
        }

        //before shoot it will unparent this weapon from current character 
        //and make rigidbody to dynamic to get collision,then add force to this weapon to make it move
        void SettingWeaponToShoot()
        {
            weapon.transform.SetParent(null);
            Vector3 rayDirection = -shootDirection.normalized;
            //Check if it is player with computer mode then use auto shoot
            if (!isThisPlayerTurn && gameMode == GameMode.PlayWithCom)
            {
                float angleShoot = angleForUI;
                if (angleForUI < 0)
                    angleShoot = -angleForUI;
                rayDirection = Quaternion.Euler(0, 0, -angleShoot) * Vector3.left;
            }

            if (isFinishPlayer)
            {
                isFinishPlayer = false;
                weaponComponent.isFinishShot = true;
            }
            //Use raycast to detect if hit plane from the first shot then crash weapon into it
            RaycastHit2D hit = Physics2D.Raycast(weapon.transform.position - rayDirection * 0.25f, rayDirection,
                8, LayerMask.GetMask("Plane"));
            if (hit.collider != null)
            {
                currentCharacter.GetComponent<PlayerManager>().SetAimAnimation(false);
                weaponComponent.CollisionWithPlane(hit.point, -shootDirection.normalized);
            }
            else
            {
                weaponComponent.isShoot = true;
                weaponComponent.startTime = Time.time;
                rigidbodyWeapon.bodyType = RigidbodyType2D.Dynamic;

                //Check if it is player with computer mode then use auto shoot
                if (currentAimDirection == -1 && gameMode == GameMode.PlayWithCom)
                {
                    float angleShoot = angleForUI;
                    if (angleForUI < 0)
                        angleShoot = -angleForUI;
                    weaponComponent.isComWeapon = true;
                    isThisPlayerShoot = false;
                    weapon.GetComponent<Projectile>().Launch(otherCharacter.GetComponent<PlayerManager>().body, angleShoot, enemyHitFrequency, enemyLevel);
                    if (weaponComponent.weaponType == MoveWeaponType.Rotate || weaponComponent.weaponType == MoveWeaponType.RotateWithPhysics)
                    {
                        rigidbodyWeapon.AddTorque(torqueMoveWeapon);
                        rigidbodyWeapon.bodyType = RigidbodyType2D.Dynamic;
                    }
                }
                //or manual shoot
                else
                {
                    weaponComponent.isComWeapon = false;
                    rigidbodyWeapon.AddForce(force * currentForce * (-shootDirection.normalized));
                    if (weaponComponent.weaponType == MoveWeaponType.Rotate || weaponComponent.weaponType == MoveWeaponType.RotateWithPhysics)
                    {
                        rigidbodyWeapon.AddTorque(torqueMoveWeapon);
                        rigidbodyWeapon.bodyType = RigidbodyType2D.Dynamic;
                    }
                }
            }
        }

        // Listens to changes in game state
        void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                // Do whatever necessary when a new game starts
                StartGame();
            }
        }

        //Check if click in any UI element
        private bool isPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        //Denpend on game mode to select player turn
        void SetTurnWithGameMode()
        {
            gameMode = GameManager.gameMode;
            if (gameMode == GameMode.MultiPlayer)
            {
                if (currentOnlinePlayerID == thisPlayerID)
                {
                    isThisPlayerTurn = true;
                    thisPlayerIndex = 1;
                }
                else
                {
                    isThisPlayerTurn = false;
                    thisPlayerIndex = -1;
                }
            }
            else
            {
                if (gameMode == GameMode.PlayWithFriend || gameMode == GameMode.BirdHunt)
                {
                    isThisPlayerTurn = true;
                    if (CharacterManager.Instance.player2Index != -1)
                        otherPlayerCharacterIndex = CharacterManager.Instance.player2Index;
                    else
                        otherPlayerCharacterIndex = CharacterManager.Instance.CurrentCharacterIndex;
                }
                else if (gameMode == GameMode.AppleShoot)
                {
                    isThisPlayerTurn = true;
                    otherPlayerCharacterIndex = CharacterManager.Instance.CurrentCharacterIndex;
                }
                else
                {
                    if (isTestPlayer)
                        otherPlayerCharacterIndex = otherPlayerIndexTest;
                    else
                        otherPlayerCharacterIndex = UnityEngine.Random.Range(0, CharacterManager.Instance.characters.Length);
                    isThisPlayerTurn = false;
                }
                thisPlayerIndex = 1;
                if (gameMode == GameMode.BirdHunt)
                {
                    ground.GetComponent<MapGenerate>().segmentHeigh = 0;
                    ground.GetComponent<MapGenerate>().EnableTopCollider();
                }
                else if (gameMode == GameMode.AppleShoot)
                {
                    ground.GetComponent<MapGenerate>().segmentHeigh = 1;
                    ground.GetComponent<MapGenerate>().mapWidth *= 3;
                }
                else
                    ground.GetComponent<MapGenerate>().segmentHeigh = ground.GetComponent<MapGenerate>().originalSegmenHeight;
                ground.GetComponent<MapGenerate>().GenerateMap();
            }
        }

        //First setting before playing to select which player will play first then set aim direction.
        void StartGame()
        {
            resultTextObj.SetActive(false);
            player1HealthImg.fillAmount = 1;
            player2HealthImg.fillAmount = 1;
            GameManager.Instance.multiplayerManager.cannotInteractImg.SetActive(false);
            currentPlayerIndex = 1;
            SetTurnWithGameMode();
            if (gameMode != GameMode.BirdHunt)
                currentAimDirection = -1;
            else
            {
                resultMode = MatchResult.BirdHunt;
                currentAimDirection = 1;
                textTimeCountDownObj.SetActive(true);
                timeCountDown = GameManager.Instance.birdHuntTime;
                UpdateTimeText();
            }
            CreatePlayer();
            //There is only one player in bird hunting mode
            if (gameMode != GameMode.BirdHunt)
            {
                if (gameMode == GameMode.AppleShoot)
                {
                    currentPlayerIndex *= 1;
                    currentAimDirection *= -1;
                    currentCharacter = players[currentPlayerIndex];
                    otherCharacter = players[-currentPlayerIndex];
                    Camera.main.GetComponent<CameraController>().isChangeTurn = true;
                }
                else
                {
                    currentPlayerIndex *= -1;
                    currentCharacter = players[currentPlayerIndex];
                    otherCharacter = players[-currentPlayerIndex];
                    Camera.main.GetComponent<CameraController>().isChangeTurn = true;
                }
            }
            else
                StartCoroutine(CountDownStartBirdHunt());


            if (gameMode == GameMode.AppleShoot)
            {
                midPlayers = new GameObject();
                Vector3 tempPos = (currentCharacter.transform.localPosition + otherCharacter.transform.localPosition) / 2;
                tempPos.y = currentCharacter.transform.localPosition.y;
                midPlayers.transform.localPosition = tempPos;
                GameManager.Instance.SetOriginalZoomForAppleShoot(Vector2.Distance(leftPosition.position, rightPosition.position));
                Camera.main.GetComponent<CameraController>().playerTransform = midPlayers.transform;
                Camera.main.orthographicSize = GameManager.Instance.minZoom;
            }
            else
            {
                Camera.main.GetComponent<CameraController>().playerTransform = currentCharacter.transform;
            }
            Camera.main.GetComponent<CameraController>().fixPosition = true;
        }

        IEnumerator CountDownStartBirdHunt()
        {
            yield return new WaitForSeconds(1);
            canShoot = true;
            if (isThisPlayerTurn)
                NoticePlayerTurn("Start");
            GameManager.Instance.SetZoomValue(2);
            StartCoroutine(ZoomLerp(GameManager.Instance.maxZoom, 0.5f, true));
        }

        //Create two players in left and right position
        void CreatePlayer()
        {

            CreatePlayerOne();

            //There is only one player in bird hunting mode
            if (gameMode != GameMode.BirdHunt)
                CreatePlayerTwo();


        }

        //Create player 1 in left position
        void CreatePlayerOne()
        {
            if (isThisPlayerTurn)
                currentCharacter = Instantiate(CharacterManager.Instance.characters[otherPlayerCharacterIndex]);
            else
                currentCharacter = Instantiate(CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex]);
            if (thisPlayerIndex == 1)
                currentCharacter.GetComponent<Character>().isThisPlayerCharacter = true;
            else
                currentCharacter.GetComponent<Character>().isThisPlayerCharacter = false;
            if (gameMode == GameMode.BirdHunt)
                currentCharacter.transform.position = new Vector3(0, leftPosition.transform.position.y, 0);
            else
                currentCharacter.transform.position = leftPosition.position;

            currentCharacter.transform.localScale = new Vector3(-1, 1, 1);
            currentCharacter.transform.GetChild(0).GetComponent<Animator>().enabled = false;
            currentCharacter.name = "Player1";
            currentCharacter.GetComponent<Character>().playerIndex = 1;
            playerWeapons.Add(1, currentCharacter.GetComponent<Character>().weapon);
            players.Add(1, currentCharacter);

        }

        //Create player 2 in right position
        void CreatePlayerTwo()
        {
            if (isThisPlayerTurn)
                currentCharacter = Instantiate(CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex]);
            else
                currentCharacter = Instantiate(CharacterManager.Instance.characters[otherPlayerCharacterIndex]);

            if (thisPlayerIndex == -1)
                currentCharacter.GetComponent<Character>().isThisPlayerCharacter = true;
            else
                currentCharacter.GetComponent<Character>().isThisPlayerCharacter = false;

            if (GameManager.gameMode == GameMode.AppleShoot)
            {
                life = GameManager.Instance.appleShootLife;
                lifeTextObj.SetActive(true);
                lifeText.GetComponent<Text>().text = "Life : " + life.ToString();
                rightPosition.position = leftPosition.position + new Vector3(GameManager.Instance.appleDistance, 0, 0);
                currentCharacter.GetComponent<PlayerManager>().isEnemy = true;
            }
            currentCharacter.transform.position = rightPosition.position;
            currentCharacter.transform.GetChild(0).GetComponent<Animator>().enabled = false;
            currentCharacter.name = "Player2";
            currentCharacter.GetComponent<Character>().playerIndex = -1;
            playerWeapons.Add(-1, currentCharacter.GetComponent<Character>().weapon);
            players.Add(-1, currentCharacter);

        }

        public void CreateRightPlayer()
        {
            GameObject rightPlayer = players[-1];
            players.Remove(-1);
            Destroy(rightPlayer);
            rightPlayer = Instantiate(CharacterManager.Instance.characters[otherPlayerCharacterIndex]);
            rightPosition.position = leftPosition.position + new Vector3((ScoreManager.Instance.Score + 1) * GameManager.Instance.appleDistanceIncrease, 0, 0);
            rightPlayer.GetComponent<PlayerManager>().isEnemy = true;
            rightPlayer.transform.position = rightPosition.position;
            rightPlayer.transform.GetChild(0).GetComponent<Animator>().enabled = false;
            rightPlayer.name = "Player2";
            rightPlayer.GetComponent<Character>().playerIndex = -1;
            otherCharacter = rightPlayer;
            players.Add(-1, rightPlayer);
            if (midPlayers)
            {
                Vector3 tempPos = (currentCharacter.transform.localPosition + otherCharacter.transform.localPosition) / 2;
                tempPos.y = currentCharacter.transform.localPosition.y;
                midPlayers.transform.localPosition = tempPos;
            }
            GameManager.Instance.SetOriginalZoomForAppleShoot(Vector2.Distance(leftPosition.position, rightPosition.position));
            //otherCharacter.GetComponent<PlayerManager>().SetPositionOnPlane();
        }

        public void ReLocateEnemy()
        {
            rightPosition.position = leftPosition.position + new Vector3((ScoreManager.Instance.Score + 1) * GameManager.Instance.appleDistanceIncrease, 0, 0);
            Vector3 tempPos = otherCharacter.transform.position;
            tempPos.x = rightPosition.position.x;
            tempPos.y += 5;
            otherCharacter.transform.position = tempPos;
            otherCharacter.GetComponent<PlayerManager>().SetPositionOnPlane();
            Vector3 tempPos2 = (currentCharacter.transform.localPosition + otherCharacter.transform.localPosition) / 2;
            tempPos2.y = currentCharacter.transform.localPosition.y;
            midPlayers.transform.localPosition = tempPos2;
            GameManager.Instance.SetOriginalZoomForAppleShoot(Vector2.Distance(leftPosition.position, rightPosition.position));
            Camera.main.orthographicSize = GameManager.Instance.minZoom;
        }

        public void RecreateApple()
        {
            otherCharacter.GetComponent<PlayerManager>().GenerateApple();
        }

        public void AddLife()
        {
            if (life < GameManager.Instance.appleShootLife)
            {
                life += 1;
                lifeText.GetComponent<Text>().text = "Life : " + life.ToString();
            }
        }

        //after current player shoot it will change turn to other player
        public void ChangeTurn()
        {
            if (gameMode != GameMode.AppleShoot && gameMode != GameMode.BirdHunt)
            {
                if (gameMode == GameMode.MultiPlayer || gameMode == GameMode.PlayWithCom)
                    isThisPlayerTurn = !isThisPlayerTurn;

                currentPlayerIndex *= -1;
                currentAimDirection *= -1;
                currentCharacter = players[currentPlayerIndex];
                otherCharacter = players[-currentPlayerIndex];
                characterHeigh = currentCharacter.GetComponent<PlayerManager>().characterHeight;
                Camera.main.GetComponent<CameraController>().playerTransform = currentCharacter.transform;
            }

        }

        // Calls this when the player dies and game over
        public void Die()
        {
            isFinishPlayer = false;
            isLastShot = false;
            ShowResult();
            pivotOtherPlayerDistanceUI.gameObject.SetActive(false);
            textTimeCountDownObj.SetActive(false);
            firstClickImg.gameObject.SetActive(false);
            currentClickImg.gameObject.SetActive(false);
            aimLine.enabled = false;
            isDie = true;
            isPlay = false;
            // Fire event
            if (PlayerDied != null)
                PlayerDied();
        }

        public void RaiseEventPlayerWin()
        {
            if (PlayerWin != null)
                PlayerWin();
        }

        public void RaiseEventDrawGame()
        {
            if (DrawGame != null)
                DrawGame();
        }


        public void RaiseEventPlayerLose()
        {
            if (PlayerLose != null)
                PlayerLose();
        }

        #endregion
        #region AI handling
        void ComPlayerHandling()
        {
            if (isTurnBegin)
            {
                if (isFirst)
                {
                    currentCharacter.GetComponent<PlayerManager>().ResetTransform();
                    isFirst = false;
                }
                else
                {
                    limitAimCount = UnityEngine.Random.Range(0, 3);
                    isTurnBegin = false;
                    FirstTouchHandling();
                    GetAILevel();
                }
            }
            else
                ComPlayerAiming();
        }

        void GetAILevel()
        {
            enemyLevel = currentCharacter.GetComponent<Character>().BotLevel;
            if (enemyLevel == AILevel.Easy)
                enemyHitFrequency = GameManager.Instance.easyBotHitFrequency;
            if (enemyLevel == AILevel.Normal)
                enemyHitFrequency = GameManager.Instance.normalBotHitFrequency;
            if (enemyLevel == AILevel.Hard)
                enemyHitFrequency = GameManager.Instance.hardBotHitFrequency;
            if (enemyLevel == AILevel.Expert)
                enemyHitFrequency = GameManager.Instance.expertBotHitFrequency;
        }

        void ComPlayerAiming()
        {
            if (isFirstComAim)
            {
                isFirstComAim = false;
                float randomAngle = -UnityEngine.Random.Range(1, 20f);
                StartCoroutine(RotateBodyLerp(randomAngle));
                StartCoroutine(RandomAim());
                previousAngle = randomAngle;
            }
        }

        IEnumerator RandomAim()
        {
            yield return new WaitForSeconds(0.5f);
            if (count > limitAimCount)
            {
                Shoot();
                count = 0;
                isFirstComAim = true;
                isTurnBegin = true;
                isFirst = true;
            }
            else
            {
                float randomAngle = UnityEngine.Random.Range(20.0f, 60.0f);
                StartCoroutine(RotateBodyLerp(-randomAngle));
                StartCoroutine(RandomAim());
                previousAngle = -randomAngle;
                angleForUI = randomAngle;
                count++;
            }
        }

        IEnumerator ZoomLerp(float targetZoom, float runTime, bool isBirdHuntZoom = false)
        {
            var startTime = Time.time;
            float timePast = 0;
            float originalZoom = Camera.main.orthographicSize;

            while (Time.time < startTime + runTime)
            {
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                Camera.main.orthographicSize = Mathf.Lerp(originalZoom, targetZoom, factor);
                yield return null;
            }
            Camera.main.orthographicSize = targetZoom;
            if (isBirdHuntZoom)
            {
                birdSpawnPosition1 = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
                birdSpawnPosition1.x += 3;
                birdSpawnPosition1.y -= 7;

                birdSpawnPosition2 = Camera.main.ViewportToWorldPoint(new Vector2(0, 1));
                birdSpawnPosition2.x -= 3;
                birdSpawnPosition2.y -= 7;
                isPlay = true;
                StartCoroutine(CreateBird());
                StartCoroutine(TimeCountDown());
            }
        }

        IEnumerator CreateBird(bool isIncreaseSpeed = false)
        {
            yield return new WaitForSeconds(GameManager.Instance.timeCreateBird);
            GameObject bird = Instantiate(SpriteController.Instance.RandomBird());
            if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f)
            {
                bird.transform.position = birdSpawnPosition1;
                bird.GetComponent<TargetObject>().isLeftPosition = false;
                bird.GetComponent<TargetObject>().height = birdSpawnPosition1.y;
                Vector3 scale = bird.transform.localScale;
                scale.x *= -1;
                bird.transform.localScale = scale;
            }
            else
            {
                bird.transform.position = birdSpawnPosition2;
                bird.GetComponent<TargetObject>().isLeftPosition = true;
                bird.GetComponent<TargetObject>().height = birdSpawnPosition2.y;
            }
            if (isIncreaseSpeed)
                bird.GetComponent<TargetObject>().speed *= 1 + GameManager.Instance.percentageIncreaseSpeed;
            birdCount++;
            if (birdCount < 12 && isPlay)
                StartCoroutine(CreateBird());
        }

        IEnumerator TimeCountDown()
        {
            yield return new WaitForSeconds(1);
            if (timeCountDown > 0)
            {
                timeCountDown--;
                UpdateTimeText();
                StartCoroutine(TimeCountDown());
            }
            else
            {
                StopAllCoroutines();
                Die();
            }
        }

        public void UpdateTimeText()
        {
            textTimeCountDown.text = "Time : " + timeCountDown.ToString();
        }

        IEnumerator RotateBodyLerp(float angle)
        {
            var startTime = Time.time;
            float runTime = 0.5f;
            float timePast = 0;
            float originalAngle = previousAngle;

            while (Time.time < startTime + runTime)
            {
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                float angleRotate = Mathf.Lerp(originalAngle, angle, factor);
                AimByReceiveAngle(angleRotate);
                yield return null;
            }
            AimByReceiveAngle(angle);
        }

        #endregion
    }
}