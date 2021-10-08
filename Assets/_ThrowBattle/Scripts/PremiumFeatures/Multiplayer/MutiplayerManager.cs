using System;
using System.Collections;
using UnityEngine;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace _ThrowBattle
{
    /// <summary>
    /// Base class for turnbased and realtime multiplayer managers.
    /// </summary>
    public abstract class MutiplayerManager : MonoBehaviour
    {
        #region Tooltips

        public const string MinPlayersTooltip = "The minimum number of players that may join the match, " +
            "includingbthe player who is making the match request. Must be at least 2 (default).";

        public const string MaxPlayersTooltip = "The maximum number of players that may join the match, " +
            "including the player who is making the match request. " +
            "Must be equal or greater than \"minPlayers\" " +
            "and may be no more than the maximum number of players allowed for the type of " +
            "match being requested, as returned by \"GetMaxPlayersAllowed()\". Default value is 2.";

        public const string VariantTooltip = "The match variant. The meaning of this parameter is defined by the game. " +
            "It usually indicates a particular game type or mode (for example \"capture the flag\", " +
            "\"first to 10 points\", etc). It allows the player to match only with players whose " +
            "match request shares the same variant number. " +
            "This value must be between 0 and 511 (inclusive). Default value is 0.";

        public const string ExclusiveBitmaskTooltip = "If your game has multiple player roles (such as farmer, archer, and wizard) " +
            "and you want to restrict auto-matched games to one player of each role, " +
            "add an exclusive bitmask to your match request. When auto-matching with this option, " +
            "players will only be considered for a match when the logical AND of their exclusive " +
            "bitmasks is equal to 0. In other words, this value represents the exclusive role the " +
            "player making this request wants to play in the created match. If this value is 0 (default) " +
            "it will be ignored. If you're creating a match with the standard matchmaker UI, this value will also be ignored.";

        #endregion
          
        #if EASY_MOBILE

        [Header("Match Request")]
        [SerializeField, Tooltip(MinPlayersTooltip)]
        private uint minPlayers = 2;

        [SerializeField, Tooltip(MaxPlayersTooltip)]
        private uint maxPlayers = 2;

        [SerializeField, Tooltip(VariantTooltip)]
        private uint variant = 0;

        [SerializeField, Tooltip(ExclusiveBitmaskTooltip)]
        private uint exclusiveBitmask = 0;

        public abstract MatchType MatchType { get; }
        public abstract void CreateQuickMatch();
        public abstract void CreateWithMatchmaker();
        public abstract void AcceptInvitation(Invitation invitation);
        public abstract void DeclineInvitation(Invitation invitation);
        protected abstract void InvitationReceivedCallback(Invitation invitation, bool shouldAutoAccept);

        /// <summary>
        /// This method will be called every frame in <see cref="Start"/> until user has been logged in.
        /// </summary>
        protected abstract void ShowLoginWaitingUI();

        /// <summary>
        /// This method will be called once in <see cref="Start"/> when user logged in.
        /// </summary>
        protected abstract void HideLoginWaitingUI();

        /// <summary>
        /// Raise when an invitation has been received.
        /// </summary>
        public event GameServices.InvitationReceivedDelegate OnInvitationReceived;

        public uint MaxPlayersAllowed { get; private set; }

        /// <summary>
        /// Check if this object is destroyed, but hasn't been collected by the garbage collector.
        /// Use this to avoid NullReferenceException, especially in callbacks.
        /// </summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// See <see cref="MinPlayersTooltip"/>.
        /// </summary>
        public uint MinPlayers
        {
            get { return minPlayers; }
            set { minPlayers = ValidateMinPlayers(value); }
        }

        /// <summary>
        /// See <see cref="MaxPlayersTooltip"/>.
        /// </summary>
        public uint MaxPlayers
        {
            get { return maxPlayers; }
            set { maxPlayers = ValidateMaxPlayers(value); }
        }

        /// <summary>
        /// See <see cref="VariantTooltip"/>.
        /// </summary>
        public uint Variant
        {
            get { return variant; }
            set { variant = ValidateVariant(value); }
        }

        /// <summary>
        /// See <see cref="ExclusiveBitmaskTooltip"/>.
        /// </summary>
        public uint ExclusiveBitmask
        {
            get { return exclusiveBitmask; }
            set { exclusiveBitmask = value; }
        }

        protected virtual IEnumerator Start()
        {
            /// Wait until user has been logged in.
            while (!GameServices.IsInitialized() || Application.isEditor)
            {
                ShowLoginWaitingUI();
                yield return null;
            }
            HideLoginWaitingUI();

            MaxPlayersAllowed = MatchRequest.GetMaxPlayersAllowed(MatchType);
            OnInvitationReceived += InvitationReceivedCallback;
            GameServices.RegisterInvitationDelegate(OnInvitationReceived);
        }

        protected virtual void OnValidate()
        {
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
            Variant = variant;
        }

        protected virtual void OnDestroy()
        {
            IsDestroyed = true;
            OnInvitationReceived -= InvitationReceivedCallback;
        }

        /// <summary>
        /// Contruct new <see cref="MatchRequest"/>.
        /// </summary>
        public MatchRequest CreateMatchRequest()
        {
            return new MatchRequest
            {
                MinPlayers = minPlayers,
                MaxPlayers = maxPlayers,
                Variant = variant,
                ExclusiveBitmask = exclusiveBitmask
            };
        }

        private uint ValidateMinPlayers(uint value)
        {
            if (value < 2)
            {
                Debug.LogWarning("MatchRequest.MinPlayers can't be smaller than 2.");
                return 2;
            }

            if (maxPlayers < value)
            {
                Debug.LogWarning("MatchRequest.MaxPlayers can't be smaller than MatchRequest.MinPlayers.");
                maxPlayers = (uint)Mathf.Clamp(value, 2, MaxPlayersAllowed > 0 ? MaxPlayersAllowed : int.MaxValue);
            }

            if (MaxPlayersAllowed > 0 && value > MaxPlayersAllowed)
            {
                Debug.LogWarning("MatchRequest.MinPlayers can't be bigger than " + MaxPlayersAllowed);
                return MaxPlayersAllowed;
            }

            return value;
        }

        private uint ValidateMaxPlayers(uint value)
        {
            if (value < 2)
            {
                Debug.LogWarning("MatchRequest.MaxPlayers can't be smaller than 2.");
                return 2;
            }

            if (MaxPlayersAllowed > 0 && value > MaxPlayersAllowed)
            {
                Debug.LogWarning("MatchRequest.MaxPlayers can't be bigger than " + MaxPlayersAllowed);
                return MaxPlayersAllowed;
            }

            if (value < minPlayers)
            {
                Debug.LogWarning("MatchRequest.MaxPlayers can't be smaller than MatchRequest.MinPlayers.");
                return minPlayers;
            }

            return value;
        }

        private uint ValidateVariant (uint value)
        {
            if (value < MatchRequest.MinVariant)
                return MatchRequest.MinVariant;

            if (variant > MatchRequest.MaxVariant)
                return MatchRequest.MaxVariant;

            return value;
        }

        #endif
    }
}
