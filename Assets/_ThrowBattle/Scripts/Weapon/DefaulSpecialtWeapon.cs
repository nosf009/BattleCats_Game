using System.Collections;
using UnityEngine;
using System;
using System.Linq;

namespace _ThrowBattle
{
    public class DefaulSpecialtWeapon : Weapon, SpecialAction
    {
        public ActionPositionMode actionPositionMode
        {
            get { return _actionPositionMode; }
            set { _actionPositionMode = value; }
        }
        [SerializeField]
        private ActionPositionMode _actionPositionMode;

        public float useActionFrequency
        {
            get { return _useActionFrequency; }
            set { _useActionFrequency = value; }
        }
        [SerializeField]
        [Range(0f, 1f)]
        private float _useActionFrequency;
        [HideInInspector]
        public Vector2 targetPosition;

        public bool hasCreateSpecialAction { set; get; }

        public float minTimeActiveAction = 2;
        public float maxTimeActiveAction = 5;
        public float minDistance = 2;
        public float maxDistance = 10;

        protected float distanceActiveAction;

        protected int funcTionIndex;

        protected Vector2 positionAction;

        protected bool isWaitActiveAction = false;
        protected bool hasWaitToActiveAction = false;
        protected bool isHit = false;
        protected bool isSend = false;
        /// <summary>
        /// Define which method will check to activate special action for computer player
        /// </summary>
        public virtual void AutoActiveSpecialAction(Vector2 targetPosition,ActiveActionMode activeMode,bool isHit)
        {
            this.targetPosition = targetPosition;
            //Use random distance to active special action for computer character
            if (activeMode == ActiveActionMode.ByDistance)
            {
                distanceActiveAction = UnityEngine.Random.Range(minDistance, maxDistance);
                isWaitActiveAction = true;
                funcTionIndex = 0;
            }
            //Use random time to active special action for computer character
            else if (activeMode == ActiveActionMode.ByTime)
                StartCoroutine(RandomTimeActiveSpecial());
            else if(activeMode==ActiveActionMode.ByPosition)
            {
                SettingOnAutoActive();
                this.isHit = isHit;
                isWaitActiveAction = true;
                funcTionIndex = 1;
            }
        }

        /// <summary>
        /// Setting for weapon when it starting to shoot
        /// </summary>
        public virtual void SettingOnAutoActive()
        {

        }

        public void CallFunctionByIndex(int index)
        {
            switch(index)
            {
                case 0:
                    CheckActiveActionByDistance();
                    break;
                case 1:
                    PredictImpactPosition();
                    break;
            }
        }

        /// <summary>
        /// coroutine waits for a few seconds to activate special action when selecting an active mode over time
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator RandomTimeActiveSpecial()
        {
            float timeRandom = UnityEngine.Random.Range(minTimeActiveAction, maxTimeActiveAction);
            yield return new WaitForSeconds(timeRandom);
            if (isShoot && !hasCreateSpecialAction)
            {
                ActiveSpecialAction();
            }
        }

        /// <summary>
        /// Check to see if activating special actions in the current position can hit the player
        /// </summary>
        public virtual void PredictImpactPosition()
        {
            if (isHit && Mathf.Abs(targetPosition.x-transform.position.x)<weaponWidth)
            {
                ActiveSpecialAction();
            }
        }

        /// <summary>
        /// Activate special action
        /// </summary>
        public virtual void ActiveSpecialAction()
        {
            if (!hasTrigger && !hasCreateSpecialAction)
            {
                if (GameManager.Instance.IsMultiplayerMode() && GameManager.Instance.playerController.isThisPlayerTurn)
                {
                    SendDataOnAction();
                }
                GameManager.Instance.playerController.currentCharacter.GetComponent<PlayerManager>().ResetAnimator();
                OnActiveSpecialAction();
                hasCreateSpecialAction = true;
            }
        }

        protected virtual void OnActiveSpecialAction()
        {
            hasCreateSpecialAction = true;
            rigidbodyWeapon.velocity = Vector2.zero;
            rigidbodyWeapon.AddForce(Vector2.down * 1000);
        }

        public void HandleDataForAction(byte[] data)
        {
            positionAction.x = BitConverter.ToSingle(data, 0);
            positionAction.y = BitConverter.ToSingle(data, 4);
            //Check if this weapon has moved through the position where it created the special action
            //If right then fix this weapon position and then active special action
            if ((rigidbodyWeapon.velocity.x > 0 && transform.position.x > positionAction.x) || (rigidbodyWeapon.velocity.x < 0 && transform.position.x < positionAction.x))
            {
                transform.position = positionAction;
                ActiveSpecialAction();
            }
            //If not,then wait for weapon to get close to this position and active special action
            else
            {
                hasWaitToActiveAction = true;
            }
        }
        /// <summary>
        ///Send signal and Position to active special action
        /// </summary>
        public void SendDataOnAction()
        {
            isSend = false;
            byte[] actionData = { 10 };
            actionData = actionData.Concat(BitConverter.GetBytes(transform.position.x)).ToArray();
            actionData = actionData.Concat(BitConverter.GetBytes(transform.position.y)).ToArray();
            GameManager.Instance.playerController.SendDataToOtherPlayer(actionData);
        }

        /// <summary>
        /// Check when the computer player will activate the special action
        /// </summary>
        public virtual void CheckActiveActionByDistance()
        {
            isWaitActiveAction = false;
            if (Vector2.Distance(transform.position, targetPosition) < distanceActiveAction)
                ActiveSpecialAction();
        }

        /// <summary>
        /// Loop when weapon is moving
        /// </summary>
        protected virtual void Loop()
        {

        }



        private void Update()
        {
            Loop();
            if (isShoot)
            {
                MoveWeapon();
                if (isComWeapon && isWaitActiveAction)
                {
                    CallFunctionByIndex(funcTionIndex);
                }
                if (Input.GetMouseButtonDown(0) && GameManager.Instance.playerController.isThisPlayerTurn && !hasCreateSpecialAction)
                {
                    isSend = true;
                    ActiveSpecialAction();
                }
                if (hasWaitToActiveAction)
                {
                    if ((Vector2)transform.position == positionAction || Vector2.Distance(transform.position, positionAction) < 0.001f)
                    {
                        transform.position = positionAction;
                        ActiveSpecialAction();
                    }
                }
            }
        }
    }
}
