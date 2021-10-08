using System;
using System.Collections;
using UnityEngine;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace _ThrowBattle
{
    public class MultiplayerTurnbasedManager : MutiplayerManager
    {
        #if EASY_MOBILE

        #region Events

        /// <summary>
        /// Raises when a turnbased match has been received.
        /// </summary>
        public event MatchDelegate OnMatchReceived;

        /// <summary>
        /// Raises when <see cref="CreateQuickMatch"/> has been called.
        /// </summary>
        public event Action<bool, TurnBasedMatch> OnQuickMatchCreated;

        /// <summary>
        /// Raises when the matchmaker has been cancelled by user.
        /// </summary>
        public event Action OnMatchmakerCancelled;

        /// <summary>
        /// Raises when an error orcurs during matchmaker's matching progress.
        /// </summary>
        public event Action<string> OnMatchmakerError;

        /// <summary>
        /// Raises when <see cref="AcceptInvitation(Invitation)"/> has been called. 
        /// </summary>
        public event Action<bool, TurnBasedMatch> OnInvitationAccepted;

        /// <summary>
        /// Raises when <see cref="GetAllMatches"/> has been called.
        /// </summary>
        public event Action<TurnBasedMatch[]> OnGetAllMatches;

        /// <summary>
        /// Raises when <see cref="TakeTurn(TurnBasedMatch, byte[], string)"/> has been called.
        /// </summary>
        public event Action<bool> OnTurnTaken;

        /// <summary>
        /// Raises when <see cref="Finish(TurnBasedMatch, byte[], MatchOutcome)"/> has been called.
        /// </summary>
        public event Action<bool> OnFinishMatch;

        /// <summary>
        /// Raises when <see cref="LeaveMatch(TurnBasedMatch)"/> has been called.
        /// </summary>
        public event Action<bool> OnLeaveMatch;

        /// <summary>
        /// Raises when <see cref="LeaveMatchInTurn(TurnBasedMatch, string)"/> 
        /// or <see cref="LeaveMatchInTurn(TurnBasedMatch, Participant)"/> has been called.
        /// </summary>
        public event Action<bool> OnLeaveMatchInTurn;

        /// <summary>
        /// Raises when <see cref="AcknowledgeFinishedMatch(TurnBasedMatch)"/> has been called.
        /// </summary>
        public event Action<bool> OnAcknowledgeMatch;

        /// <summary>
        /// Raises when <see cref="Rematch(TurnBasedMatch)"/> has been called.
        /// </summary>
        public event Action<bool, TurnBasedMatch> OnRematch;

        #endregion 

        /// <summary>
        /// Newest <see cref="TurnBasedMatch"/> received via <see cref="OnMatchReceived"/> event.
        /// </summary>
        public TurnBasedMatch NewestMatch { get; protected set; }

        public override MatchType MatchType { get { return MatchType.TurnBased; } }

        protected override IEnumerator Start()
        {
            yield return base.Start();

            /// Register all events.
            OnMatchReceived += MatchReceivedCallback;
            GameServices.TurnBased.RegisterMatchDelegate(OnMatchReceived);
            OnQuickMatchCreated += CreateQuickMatchCallback;
            OnMatchmakerCancelled += MatchmakerCancelledCallback;
            OnMatchmakerError += MatchmakerErrorCallback;
            OnInvitationAccepted += AcceptInvitationCallback;
            OnGetAllMatches += GetAllMatchesCallback;
            OnTurnTaken += TakeTurnCallback;
            OnFinishMatch += FinishMatchCallback;
            OnLeaveMatch += LeaveMatchCallback;
            OnLeaveMatchInTurn += LeaveMatchInTurnCallback;
            OnAcknowledgeMatch += AcknowledgeMatchCallback;
            OnRematch += RematchCallback;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            /// Unregister all events.
            OnMatchReceived -= MatchReceivedCallback;
            OnQuickMatchCreated -= CreateQuickMatchCallback;
            OnMatchmakerCancelled -= MatchmakerCancelledCallback;
            OnMatchmakerError -= MatchmakerErrorCallback;
            OnInvitationAccepted -= AcceptInvitationCallback;
            OnGetAllMatches -= GetAllMatchesCallback;
            OnTurnTaken -= TakeTurnCallback;
            OnFinishMatch -= FinishMatchCallback;
            OnLeaveMatch -= LeaveMatchCallback;
            OnLeaveMatchInTurn -= LeaveMatchInTurnCallback;
            OnAcknowledgeMatch -= AcknowledgeMatchCallback;
            OnRematch -= RematchCallback;
        }

        /// <summary>
        /// This method will be called every frame in <see cref="Start"/> until user has been logged in.
        /// </summary>
        protected override void ShowLoginWaitingUI()
        {
            /// Write your code here...
        }

        /// <summary>
        /// This method will be called once in <see cref="Start"/> when user logged in.
        /// </summary>
        protected override void HideLoginWaitingUI()
        {
            /// Write your code here...
        }

        #region Main APIs

        /// <summary>
        /// Starts a game with randomly selected opponent(s). No UI will be shown.
        /// </summary>
        public override void CreateQuickMatch()
        {
            GameServices.TurnBased.CreateQuickMatch(CreateMatchRequest(), OnQuickMatchCreated);
        }

        /// <summary>
        /// Creates the match with standard built-in UI.
        /// </summary>
        public override void CreateWithMatchmaker()
        {
            GameServices.TurnBased.CreateWithMatchmakerUI(CreateMatchRequest(),
                OnMatchmakerCancelled,
                OnMatchmakerError);
        }

        /// <summary>
        /// Accepts the given invitation.
        /// </summary>
        public override void AcceptInvitation(Invitation invitation)
        {
            GameServices.TurnBased.AcceptInvitation(invitation, OnInvitationAccepted);
        }

        /// <summary>
        /// Declines the given invitation.
        /// </summary>
        public override void DeclineInvitation(Invitation invitation)
        {
            GameServices.TurnBased.DeclineInvitation(invitation);
        }

        /// <summary>
        /// Return all logged in user's matches.
        /// </summary>
        public void GetAllMatches()
        {
            GameServices.TurnBased.GetAllMatches(OnGetAllMatches);
        }

        /// <summary>
        /// Show the standard UI where player can pick a match or accept an invitations.
        /// </summary>
        public void ShowMatchesUI()
        {
            GameServices.TurnBased.ShowMatchesUI();
        }

        /// <summary>
        /// Take a turn.
        /// </summary>
        /// <param name="nextParticipantId">ID of participant who is next to play. If
        /// this is null and there are automatch slots open, the turn will be passed
        /// to one of the automatch players. Passing null when there are no open
        /// automatch slots is an error.</param>
        public void TakeTurn(TurnBasedMatch match, byte[] data, string nextParticipantId)
        {
            GameServices.TurnBased.TakeTurn(match, data, nextParticipantId, OnTurnTaken);
        }

        /// <summary>
        /// Same ass <see cref="TakeTurn(TurnBasedMatch, byte[], string)"/>.
        /// </summary>
        public void TakeTurn(TurnBasedMatch match, byte[] data, Participant nextParticipant)
        {
            GameServices.TurnBased.TakeTurn(match, data, nextParticipant, OnTurnTaken);
        }

        /// <summary>
        /// Finish a match.
        /// </summary>
        public void Finish(TurnBasedMatch match, byte[] data, MatchOutcome matchOutcome)
        {
            GameServices.TurnBased.Finish(match, data, matchOutcome, OnFinishMatch);
        }

        /// <summary>
        /// Leave the match (not during turn). Call this to leave the match when it is not your turn.
        /// </summary>
        public void LeaveMatch(TurnBasedMatch match)
        {
            GameServices.TurnBased.LeaveMatch(match, OnLeaveMatch);
        }

        /// <summary>
        /// Leave the match (during turn). Call this to leave the match when it's your turn.
        /// </summary>
        /// <param name="nextPartipantId">ID of participant who is next to play. If
        /// this is null and there are automatch slots open, the turn will be passed
        /// to one of the automatch players. Passing null when there are no open
        /// automatch slots is an error.</param>
        public void LeaveMatchInTurn(TurnBasedMatch match, string nextPartipantId)
        {
            GameServices.TurnBased.LeaveMatchInTurn(match, nextPartipantId, OnLeaveMatchInTurn);
        }

        /// <summary>
        /// Same as <see cref="LeaveMatchInTurn(TurnBasedMatch, string)"/>.
        /// </summary>
        public void LeaveMatchInTurn(TurnBasedMatch match, Participant nextParticipant)
        {
            GameServices.TurnBased.LeaveMatchInTurn(match, nextParticipant, OnLeaveMatchInTurn);
        }

        /// <summary>
        /// Acknowledges that a match was finished.
        /// Call this on a finished match that you
        /// have just shown to the user, to acknowledge that the user has seen the results
        /// of the finished match. This will remove the match from the user's inbox.
        /// </summary>
        public void AcknowledgeFinishedMatch(TurnBasedMatch match)
        {
            GameServices.TurnBased.AcknowledgeFinished(match, OnAcknowledgeMatch);
        }

        /// <summary>
        /// Request a rematch
        /// This can be used on a finished match in order to start a new
        /// match with the same opponents.
        /// </summary>
        public void Rematch(TurnBasedMatch match)
        {
            GameServices.TurnBased.Rematch(match, OnRematch);
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// See <see cref="OnMatchReceived"/> comment.
        /// </summary>
        protected virtual void MatchReceivedCallback(TurnBasedMatch match, bool shouldAutoLaunch, bool playerWantsToQuit)
        {
            if (IsDestroyed)
                return;

            NewestMatch = match;

            /// Write your code here...
            /// This is where you should start or update your game.
            /// Parse the match.Data with MultiplayerGameData.FromByteArray to use in your game...
        }

        /// <summary>
        /// See <see cref="OnInvitationAccepted"/> comment.
        /// </summary>
        protected override void InvitationReceivedCallback(Invitation invitation, bool shouldAutoAccept)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...

            //if (shouldAutoAccept)
            //    AcceptInvitation(invitation);
            //else
            //    /// Show UI for user to accept or decline invitation...
        }

        /// <summary>
        /// See <see cref="OnQuickMatchCreated"/> comment.
        /// </summary>
        protected virtual void CreateQuickMatchCallback(bool success, TurnBasedMatch match)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
            if (success)
                MatchReceivedCallback(match, true, false);
        }

        /// <summary>
        /// See <see cref="OnMatchmakerCancelled"/> comment.
        /// </summary>
        protected virtual void MatchmakerCancelledCallback()
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// See <see cref="OnMatchmakerError"/> comment.
        /// </summary>
        protected virtual void MatchmakerErrorCallback(string error)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// See <see cref="OnInvitationAccepted"/> comment.
        /// </summary>
        protected virtual void AcceptInvitationCallback(bool success, TurnBasedMatch match)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
            if (success)
                MatchReceivedCallback(match, true, false);
        }

        /// <summary>
        /// See <see cref="OnGetAllMatches"/> comment.
        /// </summary>
        protected virtual void GetAllMatchesCallback(TurnBasedMatch[] matches)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// See <see cref="OnTurnTaken"/> comment.
        /// </summary>
        protected virtual void TakeTurnCallback(bool success)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// See <see cref="OnFinishMatch"/> comment.
        /// </summary>
        protected virtual void FinishMatchCallback(bool success)
        {
            if (IsDestroyed)
                return;

            /// Write your code here.
        }

        /// <summary>
        /// See <see cref="OnLeaveMatch"/> comment.
        /// </summary>
        protected virtual void LeaveMatchCallback(bool success)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// See <see cref="OnLeaveMatchInTurn"/> comment.
        /// </summary>
        protected virtual void LeaveMatchInTurnCallback(bool success)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// See <see cref="OnAcknowledgeMatch"/> comment.
        /// </summary>
        protected virtual void AcknowledgeMatchCallback(bool success)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
        }

        /// <summary>
        /// See <see cref="OnRematch"/> comment.
        /// </summary>
        /// <param name="success"></param>
        protected virtual void RematchCallback(bool success, TurnBasedMatch match)
        {
            if (IsDestroyed)
                return;

            /// Write your code here...
            if (success)
                MatchReceivedCallback(match, true, false);
        }

        #endregion

        #endif
    }
}
