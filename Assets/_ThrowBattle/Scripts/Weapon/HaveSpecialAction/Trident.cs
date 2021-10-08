using UnityEngine;
namespace _ThrowBattle
{
    public class Trident : DefaulSpecialtWeapon
    {
        Vector3 localScale;
        protected override void Init()
        {
            localScale = transform.localScale;
        }

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

        protected override void GetWeaponCollider()
        {
            allCollider = bladeColliders.ToArray();
        }

        protected override void OnActiveSpecialAction()
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            Vector2 tempVelocity = rigidbodyWeapon.velocity.normalized;
                rigidbodyWeapon.velocity = Vector2.zero;
                rigidbodyWeapon.AddForce(tempVelocity * 50,ForceMode2D.Impulse);
        }

        public override void PredictImpactPosition()
        {
            if (isHit && Vector2.Dot(rigidbodyWeapon.velocity,targetPosition- (Vector2)transform.position)>0.975f 
                && Vector2.Distance(targetPosition,transform.position)<15)
            {
                ActiveSpecialAction();
            }
        }

        protected override void ProtectLocalTransform()
        {
            if (transform.localScale.x > 0)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            Camera.main.transform.localScale = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
            Vector3 position = transform.position;
            position.z = Camera.main.transform.position.z;
            Camera.main.transform.position = position;
        }

        protected override void SettingWeaponToShoot()
        {            
            transform.localScale = localScale;
        }

    }
}
