using System;
using System.Collections.Generic;
using UnityEngine;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace _ThrowBattle
{
    public class MultiplayerRealtimeManager : MutiplayerManager, OkBtnClickAction
#if EASY_MOBILE_PRO
        , IRealTimeMultiplayerListener
#endif
    {
        public GameObject waitingUI;
        public GameObject cannotInteractImg;
        public bool isCreateQuickMatch;

        public void RaiseLeaveRoomEvent()
        {
#if EASY_MOBILE_PRO
            GameManager.Instance.isActivelyDisconnected = true;
            if (OnLeaveRoom != null)
                OnLeaveRoom();
#endif
        }

        public void Disconnect()
        {
            if (GameManager.Instance.playerController.isPlay && GameManager.gameMode == GameMode.MultiPlayer)
            {
                PopUpController.Instance.SetMessage("Are you sure you want to exit the game ?");
                PopUpController.Instance.ShowPopUp(gameObject, true, false);
            }
            else
                OnClickOkBtn();
        }

        public void OnClickOkBtn()
        {
#if EASY_MOBILE_PRO
            GameManager.Instance.isActivelyDisconnected = true;
            if (OnLeaveRoom != null)
                OnLeaveRoom();
            if (IsRoomConnected())
                LeaveRoom();
            PopUpController.Instance.HidePopUp();
#endif
        }

#if EASY_MOBILE_PRO
        [Space]
        [SerializeField, Tooltip("[Game Center only]\nShould we reinvite disconnected player ?")]
        private bool reinviteDisconnectedPlayer = true;

        [SerializeField, Tooltip("Should we show waiting UI when accepting invitation ?")]
        private bool showInvitationWaitingRoomUI = true;

        public static System.Action OnLeaveRoom;

        public override MatchType MatchType { get { return MatchType.RealTime; } }

        /// <summary>
        /// [Game Center only] Should we reinvite disconnected player ?
        /// </summary>
        public bool ReinviteDisconnectedPlayer
        {
            get { return reinviteDisconnectedPlayer; }
            set { reinviteDisconnectedPlayer = value; }
        }

        /// <summary>
        /// Should we show waiting UI when accepting invitation ?
        /// </summary>
        public bool ShowInvitationWaitingRoomUI
        {
            get { return showInvitationWaitingRoomUI; }
            set { showInvitationWaitingRoomUI = value; }
        }

        private void Awake()
        {
            cannotInteractImg.SetActive(false);
            waitingUI.SetActive(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (GameServices.IsInitialized() && IsRoomConnected())
                LeaveRoom();
        }

        /// <summary>
        /// This method will be called every frame in <see cref="Start"/> until user has been logged in.
        /// </summary>
        protected override void ShowLoginWaitingUI()
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                if (!GameServices.IsInitialized())
                {
#if UNITY_ANDROID
                    GameServices.Init();    // start a new initialization process
#elif UNITY_IOS
    
                    Debug.Log("Cannot show leaderboard UI: The user is not logged in to Game Center.");
#endif
                }
                /// Write your code here...
            }
        }
#if UNITY_ANDROID
        private void OnApplicationPause(bool pause)
        {
            if (pause && GameManager.Instance.IsMultiplayerMode() && GameManager.Instance.playerController.isPlay)
            {
                Disconnect();
                PopUpController.Instance.SetMessage("You have lost the connection!");
                PopUpController.Instance.ShowPopUp();
            }
        }
#endif
        /// <summary>
        /// This method will be called once in <see cref="Start"/> when user logged in.
        /// </summary>
        protected override void HideLoginWaitingUI()
        {
            /// Write your code here...
        }

        System.Collections.IEnumerator WaitOtherPlayer()
        {
            yield return new WaitForSeconds(GameManager.Instance.timeWaitOtherPlayer);
            if (isCreateQuickMatch)
            {
                PopUpController.Instance.SetMessage("Can't find any available players");
                PopUpController.Instance.ShowPopUp();
                LeaveRoom();
            }
        }

        #region Main APIs

        /// <summary>
        /// Creates a game with random automatch opponents using exclusiveBitMask. No UI will be shown.
        /// </summary>
        /// <remarks> 
        /// The participants will be automatically selected among users who are currently
        /// looking for opponents.
        /// After calling this method, your listener's
        /// <see cref="RealTimeMultiplayerListener.OnRoomSetupProgress" />
        /// method will be called to indicate room setup progress. Eventually,
        /// <see cref="RealTimeMultiplayerListener.OnRoomConnected" />
        /// will be called to indicate that the room setup is either complete or has failed
        /// (check the <b>success</b> parameter of the callback). If you wish to
        /// cancel room setup, call <see cref="LeaveRoom"/>.
        /// </remarks>
        public override void CreateQuickMatch()
        {
            if (GameServices.IsInitialized())
            {
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    StartCoroutine(WaitOtherPlayer());
                    waitingUI.SetActive(true);
                    GameServices.RealTime.CreateQuickMatch(CreateMatchRequest(), this);
                    isCreateQuickMatch = true;
                }
                else
                {
                    PopUpController.Instance.SetMessage("Please connect to the internet and login to play online");
                    PopUpController.Instance.ShowPopUp();
                }
            }
            else
            {
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    GameServices.Init();
                }else
                {
                    PopUpController.Instance.SetMessage("Please connect to the internet and login to play online");
                    PopUpController.Instance.ShowPopUp();
                }

            }
        }

        /// <summary>
        /// Create a game with standard built-in UI.
        /// </summary>
        public override void CreateWithMatchmaker()
        {
            if (GameServices.IsInitialized())
            {
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    cannotInteractImg.SetActive(true);
                    GameServices.RealTime.CreateWithMatchmakerUI(CreateMatchRequest(), this);
                }
                else
                {
                    PopUpController.Instance.SetMessage("Please connect to the internet and login to play online");
                    PopUpController.Instance.ShowPopUp();
                }
            }
            else
            {
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    GameServices.Init();
                }
                else
                {
                    PopUpController.Instance.SetMessage("Please connect to the internet and login to play online");
                    PopUpController.Instance.ShowPopUp();
                }
            }
        }

        /// <summary>
        /// Accepts an invitation.
        /// </summary>
        /// <remarks>This will not show any UI. The listener's
        /// <see cref="RealTimeMultiplayerListener.OnRoomSetupProgress"/> will be called
        /// to report room setup progress, and eventually
        /// <see cref="RealTimeMultiplayerListener.OnRoomConnected"/> will be called to
        /// indicate that the room setup is either complete or has failed (check the
        /// <b>success</b> parameter of the callback).
        /// </remarks>
        public override void AcceptInvitation(Invitation invitation)
        {
            GameServices.RealTime.AcceptInvitation(invitation, showInvitationWaitingRoomUI, this);
        }

        /// <summary>
        /// Declines the invitation.
        /// </summary>
        public override void DeclineInvitation(Invitation invitation)
        {
            GameServices.RealTime.DeclineInvitation(invitation);
        }

        /// <summary>
        /// Shows the default UI where the user can accept or decline an invitation.
        /// This is only available on Google Play Games platform.
        /// </summary>
        /// <remarks>On the invitations screen,
        /// the player can select an invitation to accept, in which case the room setup
        /// process will start. The listener's
        /// <see cref="RealTimeMultiplayerListener.OnRoomSetupProgress"/> will be called
        /// to report room setup progress, and eventually
        /// <see cref="RealTimeMultiplayerListener.OnRoomConnected"/> will be called to
        /// indicate that the room setup is either complete or has failed (check the
        /// <b>success</b> parameter of the callback).
        /// </remarks>
        public void ShowInvitationsUI()
        {
            GameServices.RealTime.ShowInvitationsUI(this);
        }

        /// <summary>Sends a message to all other participants.</summary>
        /// <param name="reliable">If set to <c>true</c>, mesasge is reliable; if not,
        /// it is unreliable. Unreliable messages are faster, but are not guaranteed to arrive
        /// and may arrive out of order.</param>
        public void SendMessageToAll(bool reliable, byte[] data)
        {
            GameServices.RealTime.SendMessageToAll(reliable, data);
        }

        /// <summary>
        /// Send a message to a particular participant.
        /// </summary>
        /// <param name="reliable">If set to <c>true</c>, message is reliable; if not,
        /// it is unreliable. Unreliable messages are faster, but are not guaranteed to arrive
        /// and may arrive out of order.</param>
        public void SendMessage(bool reliable, string participantId, byte[] data)
        {
            GameServices.RealTime.SendMessage(reliable, participantId, data);
        }

        /// <summary>
        /// Gets the connected participants, including self.
        /// </summary>
        public List<Participant> GetConnectedParticipants()
        {
            return GameServices.RealTime.GetConnectedParticipants();
        }

        /// <summary>
        /// Gets the participant that represents the current player.
        /// </summary>
        public Participant GetSelf()
        {
            return GameServices.RealTime.GetSelf();
        }

        /// <summary>
        /// Get a participant by ID.
        /// </summary>
        public Participant GetParticipant(string participantId)
        {
            return GameServices.RealTime.GetParticipant(participantId);
        }

        /// <summary>
        /// Leaves the room.
        /// </summary>
        /// <remarks>
        /// Call this method to leave the room after you have
        /// started room setup. Leaving the room is not an immediate operation -- you
        /// must wait for <see cref="RealTimeMultplayerListener.OnLeftRoom"/>
        /// to be called. If you leave a room before setup is complete, you will get
        /// a call to
        /// <see cref="RealTimeMultiplayerListener.OnRoomConnected"/> with <b>false</b>
        /// parameter instead. If you attempt to leave a room that is shutting down or
        /// has shutdown already, you will immediately receive the
        /// <see cref="RealTimeMultiplayerListener.OnLeftRoom"/> callback.
        /// </remarks>
        public void LeaveRoom()
        {
            cannotInteractImg.SetActive(false);
            GameServices.RealTime.LeaveRoom();
        }

        /// <summary>
        /// Returns whether or not the room is connected (ready to play).
        /// </summary>
        public bool IsRoomConnected()
        {
            return GameServices.RealTime.IsRoomConnected();
        }

        public bool IsPlayWithOtherPlayer()
        {
            if (IsRoomConnected() && GetConnectedParticipants().Count >= 2)
            {
                return true;
            }
            else
                return false;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Called when a real-time message is received.
        /// </summary>
        /// <param name="senderId">Sender identifier.</param>
        /// <param name="data">Data.</param>
        public void OnRealTimeMessageReceived(string senderId, byte[] data)
        {
            if (IsDestroyed)
                return;
            GameManager.Instance.playerController.HandleDataReceive(data);
            /// Write your code here...
            /// Parse the data with MultiplayerGameData.FromByteArray to use in your game...
        }

        /// <summary>
        /// Notifies that room setup is finished. If <c>success == true</c>, you should
        /// react by starting to play the game; otherwise, show an error screen.
        /// </summary>
        /// <param name="success">Whether setup was successful.</param>
        public void OnRoomConnected(bool success)
        {
            isCreateQuickMatch = false;
            waitingUI.SetActive(false);
            if (IsDestroyed)
                return;
            if(success)
            {
                cannotInteractImg.SetActive(true);
                List<Participant> participants = GetConnectedParticipants();
                Participant currentPlayer = GetSelf();
                GameManager.Instance.playerController.thisPlayerID = currentPlayer.ParticipantId;
                GameManager.Instance.playerController.currentOnlinePlayerID = participants[0].ParticipantId;
                GameManager.gameMode = GameMode.MultiPlayer;
                for (int i = 0; i < participants.Count; i++)
                {
                    if (participants[i].ParticipantId != currentPlayer.ParticipantId)
                    {
                        GameManager.Instance.playerController.otherPlayerID = participants[i].ParticipantId;
                        break;
                    }
                }
                Invoke("SendSignalStartGame", 0.5f);
            }else
            {
                PopUpController.Instance.SetMessage("Cancel the match creation");
                PopUpController.Instance.ShowPopUp();
                cannotInteractImg.SetActive(false);
                waitingUI.SetActive(false);
            }
            /// Write your code here...
            /// This is where you should start your game...
        }

        void SendSignalStartGame()
        {
            byte[] thisPlayerCharacterIndex = { 9, (byte)CharacterManager.Instance.CurrentCharacterIndex };
            GameManager.Instance.playerController.SendDataToOtherPlayer(thisPlayerCharacterIndex);
            GameManager.Instance.playerController.SettingBeforePlay();
        }

        /// <summary>
        /// See <see cref="OnInvitationAccepted"/> comment.
        /// </summary>
        protected override void InvitationReceivedCallback(Invitation invitation, bool shouldAutoAccept)
        {
            if (IsDestroyed)
                return;
            if (GameManager.Instance.GameState == GameState.Playing)
                DeclineInvitation(invitation);
            else
            {
                cannotInteractImg.SetActive(true);
                ShowInvitationsUI();
            }
            /// Write your code here...
            
            //if (shouldAutoAccept)
            //    AcceptInvitation(invitation);
            //else
            //    /// Show UI for user to accept or decline invitation...
        }

        /// <summary>
        /// Notifies that the current player has left the room. This may have happened
        /// because you called LeaveRoom, or because an error occurred and the player
        /// was dropped from the room. You should react by stopping your game and
        /// possibly showing an error screen (unless leaving the room was the player's
        /// request, naturally).
        /// </summary>
        public void OnLeftRoom()
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// Raises the participant left event.
        /// This is called during room setup if a player declines an invitation
        /// or leaves. The status of the participant can be inspected to determine
        /// the reason. If all players have left, the room is closed automatically.
        /// </summary>
        /// <param name="participant">Participant that left</param>
        public void OnParticipantLeft(Participant participant)
        {
            if (participant.Status == Participant.ParticipantStatus.Declined)
            {
                cannotInteractImg.SetActive(true);
                //GameManager.Instance.playerController.ShowDeclined();
                PopUpController.Instance.SetMessage("Other player have declined the invitation");
                PopUpController.Instance.ShowPopUp();
            }
            else
            {
                PopUpController.Instance.SetMessage("Other player have disconnected");
                PopUpController.Instance.ShowPopUp();
                if (OnLeaveRoom != null)
                    OnLeaveRoom();
            }

            if (IsDestroyed)
                return;
            /// Write your code here...
        }

        /// <summary>
        /// Called when peers connect to the room.
        /// </summary>
        /// <param name="participantIds">All connected participants' id.</param>
        public void OnPeersConnected(string[] participantIds)
        {
            if (IsDestroyed)
                return;
            /// Write your code here...
        }

        /// <summary>
        /// Called when peers disconnect from the room.
        /// </summary>
        /// <param name="participantIds">All disconnected participants' id.</param>
        public void OnPeersDisconnected(string[] participantIds)
        {
            if (!GameManager.Instance.playerController.isDie)
            {
                PopUpController.Instance.SetMessage("Other player have disconnected");
                PopUpController.Instance.ShowPopUp();
                if(OnLeaveRoom != null)
                OnLeaveRoom();
                LeaveRoom();
            }
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// Called during room setup to notify of room setup progress.
        /// </summary>
        /// <param name="percent">The room setup progress in percent (0.0 to 100.0).</param>
        public void OnRoomSetupProgress(float percent)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// [Game Center only] Called when a player in a two-player match was disconnected.
        /// </summary>
        /// <returns>Your game should return <c>true</c> if it wants Game Kit 
        /// to attempt to reconnect the player, <c>false</c> if it wants to terminate the match.</returns>
        /// <param name="participant">Participant that disconnected.</param>
        public bool ShouldReinviteDisconnectedPlayer(Participant participant)
        {
            if (IsDestroyed)
                return false;

            return ReinviteDisconnectedPlayer;
        }

        #endregion

#endif
    }
}
