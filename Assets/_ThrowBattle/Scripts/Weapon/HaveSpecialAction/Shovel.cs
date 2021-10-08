using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    public class Shovel : Trident
    {
        protected override void OnObjectDisable()
        {
            if (!hasTriggerPlayer)
            {
                if (GameManager.gameMode == GameMode.AppleShoot)
                {
                }
                else
                    ResetCamera();
            }
                
        }
        protected override void OnActiveSpecialAction()
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            Vector2 tempVelocity = rigidbodyWeapon.velocity.normalized;
            Vector2 direction=Vector2.Reflect(rigidbodyWeapon.velocity,Vector2.up);
            rigidbodyWeapon.velocity = direction;
            //rigidbodyWeapon.AddForce(direction.normalized * 50, ForceMode2D.Impulse);
        }

        protected override void ProtectLocalTransform()
        {
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            Camera.main.transform.localScale = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
            Vector3 position = transform.position;
            position.z = Camera.main.transform.position.z;
            Camera.main.transform.position = position;
        }

        public override void PredictImpactPosition()
        {
            if(isHit && (targetPosition.y> transform.position.y) && Vector2.Distance(targetPosition, transform.position) < 3)
            {
                ActiveSpecialAction();
            }
        }
    }
}
