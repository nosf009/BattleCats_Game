using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace _ThrowBattle
{
    public class UIManager : MonoBehaviour
    {
        [Header("Object References")]
        public GameObject pauseBtn;
        public GameObject pauseUI;
        public GameObject charactePriceUI;
        public GameObject characterSelection;
        public GameObject modeMenu;
        public GameObject playWithComBtn;
        public GameObject playOnlineBtn;
        public GameObject mainCanvas;
        public GameObject characterSelectionUI;
        public GameObject header;
        public GameObject title;
        public GameObject homeBtn;
        public Text score;
        public Text bestScore;
        public Text coinText;
        public Image player1HealthBar;
        public Image player2HealthBar;
        public GameObject newBestScore;
        public GameObject restartBtn;
        public GameObject menuButtons;
        public GameObject backBtn;
        public GameObject dailyRewardBtn;
        public Text dailyRewardBtnText;
        public GameObject rewardUI;
        public GameObject settingsUI;
        public GameObject soundOnBtn;
        public GameObject soundOffBtn;
        public GameObject musicOnBtn;
        public GameObject musicOffBtn;
        public GameObject sampleBg;
        public RectTransform charDescriptionTrans;

        [Header("Premium Features Buttons")]
        public GameObject watchRewardedAdBtn;
        public GameObject leaderboardBtn;
        public GameObject achievementBtn;
        public GameObject shareBtn;
        public GameObject iapPurchaseBtn;
        public GameObject removeAdsBtn;
        public GameObject restorePurchaseBtn;


        Animator scoreAnimator;
        Animator dailyRewardAnimator;
        bool isWatchAdsForCoinBtnActive;
        bool isMultiplayerRematch;


        private Vector3 origPosCharSelect;

        void OnEnable()
        {
            GameManager.GameStateChanged += GameManager_GameStateChanged;
            //ScoreManager.ScoreUpdated += OnScoreUpdated;
#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom += OnPlayerLeft;
#endif
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= GameManager_GameStateChanged;
            //ScoreManager.ScoreUpdated -= OnScoreUpdated;
#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom -= OnPlayerLeft;
#endif
        }

        // Use this for initialization
        void Start()
        {
            origPosCharSelect = charDescriptionTrans.anchoredPosition;
            float ratio = Camera.main.aspect;
            if (ratio > 1)
                UseLandscapeLeftLayout();
            else
                UsePortraitLayout();
            //scoreAnimator = score.GetComponent<Animator>();
            //dailyRewardAnimator = dailyRewardBtn.GetComponent<Animator>();

            Reset();
            ShowStartUI();
        }

        void OnPlayerLeft()
        {
            pauseBtn.SetActive(false);
            pauseUI.SetActive(false);
            player1HealthBar.fillAmount = 1;
            player2HealthBar.fillAmount = 1;
            if (!isMultiplayerRematch)
            {
                isMultiplayerRematch = false;
                Reset();
                ShowStartUI();
            }
        }

        void UseLandscapeLeftLayout()
        {
            Vector3 temp = origPosCharSelect;
            temp.x = temp.x + 500;
            charDescriptionTrans.anchoredPosition = temp;
            charDescriptionTrans.localScale = new Vector3(1.5f, 1.5f, 1);

            //modeMenu.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.Flexible;
            menuButtons.GetComponent<RectTransform>().localScale = new Vector3(1.5f, 1.5f, 1);
            if (GameManager.Instance.zoomFactor == 1)
            {
                GameManager.Instance.SetZoomValue(0.5f);
                Camera.main.orthographicSize = GameManager.Instance.minZoom;
            }
            else if (GameManager.Instance.zoomFactor != 0.5)
            {
                GameManager.Instance.SetZoomValue(0.5f);
            }
        }

        void UsePortraitLayout()
        {
            charDescriptionTrans.anchoredPosition = origPosCharSelect;
            charDescriptionTrans.localScale = new Vector3(1, 1, 1);
            menuButtons.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            //modeMenu.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            if (GameManager.Instance.zoomFactor == 0.5)
            {
                GameManager.Instance.SetZoomValue(1);
                Camera.main.orthographicSize = GameManager.Instance.minZoom;
            }
            else if (GameManager.Instance.zoomFactor != 1)
            {
                GameManager.Instance.SetZoomValue(1);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance.GameState != GameState.Playing && GameManager.Instance.GameState != GameState.Paused && GameManager.Instance.GameState != GameState.GameOver)
            {
                if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
                {
                    GameManager.Instance.curOrientation = ScreenOrientation.LandscapeLeft;
                    UseLandscapeLeftLayout();
                }
                else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
                {
                    GameManager.Instance.curOrientation = ScreenOrientation.LandscapeRight;
                    UseLandscapeLeftLayout();
                }
                else if (Input.deviceOrientation == DeviceOrientation.Portrait)
                {
                    GameManager.Instance.curOrientation = ScreenOrientation.Portrait;
                    UsePortraitLayout();
                }
                else if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
                {
                    GameManager.Instance.curOrientation = ScreenOrientation.PortraitUpsideDown;
                    UsePortraitLayout();
                }
            }
            if (score.gameObject.activeSelf)
                score.text = ScoreManager.Instance.Score.ToString();
            //bestScore.text = ScoreManager.Instance.HighScore.ToString();
            coinText.text = CoinManager.Instance.Coins.ToString();

           

            if (settingsUI.activeSelf)
            {
                UpdateSoundButtons();
                UpdateMusicButtons();
            }
        }

        void GameManager_GameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                ShowGameUI();
            }
            else if (newState == GameState.PreGameOver)
            {
                // Before game over, i.e. game potentially will be recovered
            }
            else if (newState == GameState.GameOver)
            {
                Invoke("ShowGameOverUI", 1f);
            }
        }

        void OnScoreUpdated(int newScore)
        {
            scoreAnimator.Play("NewScore");
        }

        void Reset()
        {
            mainCanvas.SetActive(true);
            charactePriceUI.SetActive(false);
            characterSelectionUI.SetActive(false);
            header.SetActive(false);
            title.SetActive(false);
            score.gameObject.SetActive(false);
            newBestScore.SetActive(false);
            menuButtons.SetActive(false);
            //dailyRewardBtn.SetActive(false);

            // Enable or disable premium stuff
            //leaderboardBtn.SetActive(enablePremium);

            // Hidden by default
            settingsUI.SetActive(false);

            // These premium feature buttons are hidden by default
            // and shown when certain criteria are met (e.g. rewarded ad is loaded)
            //watchRewardedAdBtn.gameObject.SetActive(false);
        }

        public void StartGame()
        {
            if (GameManager.gameMode == GameMode.MultiPlayer)
            {
                GameManager.Instance.ExitCharacterSelection();
            }
            else if (GameManager.gameMode == GameMode.PlayWithFriend)
            {
                if (CharacterManager.Instance.player2Index != -1)
                    GameManager.Instance.StartGame();
            }
            else
                GameManager.Instance.StartGame();
        }

        public void EndGame()
        {
            GameManager.Instance.GameOver();
        }

        public void GoHome()
        {
            if (!GameManager.Instance.IsMultiplayerMode())
            {
                GameManager.Instance.GoHome(0.2f);
            }
        }

        public void RestartGame()
        {
            if (GameManager.Instance.IsMultiplayerMode())
            {
#if EASY_MOBILE_PRO
                if (GameManager.Instance.multiplayerManager.GetConnectedParticipants().Count >= 2)
                {
                    isMultiplayerRematch = true;
                    GameManager.Instance.multiplayerManager.RaiseLeaveRoomEvent();
                    GameManager.Instance.MultiplayerRematch();
                    GameManager.Instance.playerController.HideRematchInviTation();
                }
                else
                {
                    isMultiplayerRematch = false;
                    PopUpController.Instance.SetMessage("Other player have disconnected,can't rematch");
                    PopUpController.Instance.ShowPopUp();
                }
#endif
            }
            else
                GameManager.Instance.RestartGame(0.2f);
        }

        public void ShowPlayOnlineBtns(bool isShow)
        {
            playWithComBtn.SetActive(!isShow);
            playOnlineBtn.SetActive(!isShow);
        }

        public void ShowCharSelectionBtns(bool isShow)
        {
            playWithComBtn.SetActive(!isShow);
            playOnlineBtn.SetActive(!isShow);
            //backBtn.SetActive(true);
        }

        public void BackAction()
        {
            ShowPlayOnlineBtns(false);
            backBtn.SetActive(false);
        }

        public void ShowStartUI()
        {
            sampleBg.SetActive(true);
            pauseBtn.SetActive(false);
            characterSelection.SetActive(true);
            settingsUI.SetActive(false);
            modeMenu.SetActive(true);
            ShowPlayOnlineBtns(false);
            player1HealthBar.transform.parent.gameObject.SetActive(false);
            player2HealthBar.transform.parent.gameObject.SetActive(false);
            header.SetActive(true);
            title.SetActive(true);
            restartBtn.SetActive(false);
            homeBtn.SetActive(false);
            menuButtons.SetActive(true);
            //backBtn.SetActive(false);
            shareBtn.SetActive(false);

            // If first launch: show "WatchForCoins" and "DailyReward" buttons if the conditions are met
            if (GameManager.GameCount == 0)
            {
                ShowWatchForCoinsBtn();
            }
        }

        public void ShowGameUI()
        {
            sampleBg.SetActive(false);
            pauseBtn.SetActive(true);
            isMultiplayerRematch = false;
            modeMenu.SetActive(false);
            charactePriceUI.SetActive(false);
            characterSelection.SetActive(false);
            if (GameManager.gameMode != GameMode.AppleShoot && GameManager.gameMode != GameMode.BirdHunt)
            {
                score.gameObject.SetActive(false);
                ShowPlayerHealthUI(true);
            }
            else
                score.gameObject.SetActive(true);
            header.SetActive(true);
            title.SetActive(false);
            homeBtn.SetActive(false);
            restartBtn.SetActive(false);
            menuButtons.SetActive(false);
            //dailyRewardBtn.SetActive(false);
            //watchRewardedAdBtn.SetActive(false);
        }

        public void ShowPauseUI(bool isShow)
        {
            homeBtn.SetActive(isShow);
            pauseUI.SetActive(isShow);
        }

        void ShowPlayerHealthUI(bool isShow)
        {
            player1HealthBar.transform.parent.gameObject.SetActive(isShow);
            player2HealthBar.transform.parent.gameObject.SetActive(isShow);
        }

        public void ShowGameOverUI()
        {
            ShowPlayerHealthUI(false);

            pauseBtn.SetActive(false);
            homeBtn.SetActive(true);
            header.SetActive(true);
            title.SetActive(false);
            score.gameObject.SetActive(false);
            //newBestScore.SetActive(ScoreManager.Instance.HasNewHighScore);

            restartBtn.SetActive(true);
            menuButtons.SetActive(true);
            //backBtn.SetActive(false);
            settingsUI.SetActive(false);


        }

        void ShowWatchForCoinsBtn()
        {
            // Only show "watch for coins button" if a rewarded ad is loaded and premium features are enabled
#if EASY_MOBILE
        if (IsPremiumFeaturesEnabled() && AdDisplayer.Instance.CanShowRewardedAd() && AdDisplayer.Instance.watchAdToEarnCoins)
        {
            watchRewardedAdBtn.SetActive(true);
            watchRewardedAdBtn.GetComponent<Animator>().SetTrigger("activate");
        }
        else
        {
            watchRewardedAdBtn.SetActive(false);
        }
#endif
        }

      

        public void ShowSettingsUI()
        {
            settingsUI.SetActive(true);
        }

        public void HideSettingsUI()
        {
            settingsUI.SetActive(false);
        }

      

     

        public void ShowCharacterSelectionScene()
        {
            mainCanvas.SetActive(false);
            characterSelectionUI.SetActive(true);
        }

        public void CloseCharacterSelectionScene()
        {
            mainCanvas.SetActive(true);
            characterSelectionUI.SetActive(false);
        }

        public void WatchRewardedAd()
        {
#if EASY_MOBILE
        // Hide the button
        watchRewardedAdBtn.SetActive(false);

        AdDisplayer.CompleteRewardedAdToEarnCoins += OnCompleteRewardedAdToEarnCoins;
        AdDisplayer.Instance.ShowRewardedAdToEarnCoins();
#endif
        }

        void OnCompleteRewardedAdToEarnCoins()
        {
#if EASY_MOBILE
        // Unsubscribe
        AdDisplayer.CompleteRewardedAdToEarnCoins -= OnCompleteRewardedAdToEarnCoins;

        // Give the coins!
        ShowRewardUI(AdDisplayer.Instance.rewardedCoins);
#endif
        }


        public void ToggleSound()
        {
            SoundManager.Instance.ToggleSound();
        }

        public void ToggleMusic()
        {
            SoundManager.Instance.ToggleMusic();
        }
        void UpdateSoundButtons()
        {
            if (SoundManager.Instance.IsSoundOff())
            {
                soundOnBtn.gameObject.SetActive(false);
                soundOffBtn.gameObject.SetActive(true);
            }
            else
            {
                soundOnBtn.gameObject.SetActive(true);
                soundOffBtn.gameObject.SetActive(false);
            }
        }

        void UpdateMusicButtons()
        {
            if (SoundManager.Instance.IsMusicOff())
            {
                musicOffBtn.gameObject.SetActive(true);
                musicOnBtn.gameObject.SetActive(false);
            }
            else
            {
                musicOffBtn.gameObject.SetActive(false);
                musicOnBtn.gameObject.SetActive(true);
            }
        }

    }
}