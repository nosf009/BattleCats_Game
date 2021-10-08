using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    public class Bommerang : DefaulSpecialtWeapon
    {
        public float moveBackAngle = 60;
        bool isRotate;
        Vector2 reflect=Vector3.zero;
        Vector2 currentCharacterPosition;
        Vector2 directionMoveBack;
        protected override void OnActiveSpecialAction()
        {
            SettingOnAutoActive();
            isRotate = true;
                hasCreateSpecialAction = true;
                rigidbodyWeapon.velocity = Vector2.zero;
                rigidbodyWeapon.gravityScale = 0;
                StartCoroutine(WaitMoveBack());
        }

        public override void SettingOnAutoActive()
        {
            targetPosition = GameManager.Instance.playerController.otherCharacter.transform.position;
            currentCharacterPosition = GameManager.Instance.playerController.currentCharacter.transform.position;
            float angle = -moveBackAngle;
            directionMoveBack = Vector2.right;
            if (targetPosition.x > currentCharacterPosition.x)
            {
                angle = moveBackAngle;
                directionMoveBack = Vector2.left;
            }
            reflect = Quaternion.Euler(0, 0, angle) * directionMoveBack;
        }

        IEnumerator WaitMoveBack()
        {
            yield return new WaitForSeconds(0.5f);
            isRotate = false;
            rigidbodyWeapon.gravityScale = 1;
            rigidbodyWeapon.AddForce(reflect * 4000);
            Invoke("ShowVelocity", 0.01f);
        }

        /// <summary>
        /// Check to see if activating special actions in the current position can hit the player
        /// </summary>
        public override void PredictImpactPosition()
        {
            Vector2 currentDirection = targetPosition - (Vector2)transform.position;
            if (!hasCreateSpecialAction && isHit && Vector2.Dot(reflect.normalized,currentDirection.normalized)>0.98)
            {
                ActiveSpecialAction();
            }
        }

        protected override void Loop()
        {
            if (isRotate)
                transform.Rotate(0, 0, 15);
        }
    }
}