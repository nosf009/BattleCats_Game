using UnityEngine;
using System.Collections.Generic;
using System.Collections;
namespace _ThrowBattle
{
    public enum MoveWeaponType
    {
        Static,
        Rotate,
        RotateWithPhysics,
        PhysicsCollider
    }

    public enum ActionPositionMode
    {
        AtPlayerPosition,
        NearPlayer,
        InFrontOfPlayer,
        BehindPlayer,
        AbovePlayer
    }

    public enum ActiveActionMode
    {
        ByTime,
        ByDistance,
        ByPosition
    }

    public interface CreateCloneWeapon
    {
        void DestroyCloneWeapon();
        void CreateCloneWeapon();
        /// <summary>
        /// Moving camera after create clone weapon
        /// Must check if this mode is birdhunt or not
        /// </summary>
        void MoveCameraAfterClone();
    }

    public interface SpecialAction
    {
        ActionPositionMode actionPositionMode { get; set; }
        bool hasCreateSpecialAction { get; set; }
        float useActionFrequency { get; set; }
        /// <summary>
        /// Activate special action
        /// </summary>
        void ActiveSpecialAction();

        void HandleDataForAction(byte[] data);
        /// <summary>
        ///Send signal and Position to active special action
        /// </summary>
        void SendDataOnAction();
        /// <summary>
        /// Define which method will check to activate special action for computer player
        /// </summary>
        void AutoActiveSpecialAction(Vector2 targetPosition,ActiveActionMode activeMode,bool isHit);
        /// <summary>
        /// Make setting before check in update function
        /// </summary>
        void SettingOnAutoActive();
        /// <summary>
        /// Check to see if activating special actions in the current position can hit the player
        /// </summary>
        void PredictImpactPosition();
    }
}
