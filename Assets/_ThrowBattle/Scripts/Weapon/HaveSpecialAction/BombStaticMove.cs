using System.Collections;
using UnityEngine;
namespace _ThrowBattle
{
    public class BombStaticMove : DefaulSpecialtWeapon
    {
        ParticleSystem explosion;
        protected Quaternion original;
        public float amplitude=1;
        public float radius = 3;

        protected override void Init()
        {
            original = Quaternion.Euler(0,0,180);
            explosion = GameManager.Instance.playerController.explosion;
            force = 0;
        }

        public static void AddExplosionForce(Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier)
        {
            var dir = (body.transform.position - explosionPosition);
            float wearoff = 1 - (dir.magnitude / explosionRadius);
            Vector3 baseForce = dir.normalized * explosionForce * wearoff;
            body.AddForce(baseForce);

            float upliftWearoff = 1 - upliftModifier / explosionRadius;
            Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
            body.AddForce(upliftForce);
        }

        protected override void OnActiveSpecialAction()
        {
            isActiveAction = true;
            explosion.gameObject.SetActive(true);
            explosion.transform.position = bladeColliders[0].transform.position;
            var shape = explosion.shape;
            shape.radius = radius;
            explosion.Play();
                bool hasCollideWithPlayer = false;
                hasCreateSpecialAction = true;
                rigidbodyWeapon.velocity = Vector2.zero;
                //rigidbodyWeapon.AddForce(Vector2.down * 1000);
                Collider2D[] colliders =Physics2D.OverlapCircleAll(bladeColliders[0].transform.position, radius);
                if (colliders != null)
                {
                    foreach (Collider2D hit in colliders)
                    {
                        Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();

                        if (rb != null)
                            AddExplosionForce(rb,4000, bladeColliders[0].transform.position, radius, 1000);
                    }
                    foreach (Collider2D collider in colliders)
                    {
                        if (collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("Target"))
                        {
                            if(collider.gameObject.CompareTag("Player"))
                                hasCollideWithPlayer = true;
                            OnTriggerCallBack(collider);
                        }
                    }
                }
                if (!hasCollideWithPlayer)
                {
                    isHitPlane = true;
                    OnTriggerCallBack(GameManager.Instance.playerController.ground.GetComponent<Collider2D>());
                }
                    gameObject.SetActive(false);
        }

        protected override void ImpacWithBladePart(Vector2 oldVelocity)
        {
            ActiveSpecialAction();
        }

        /// <summary>
        /// Check to see if activating special actions in the current position can hit the player
        /// </summary>
        public override void PredictImpactPosition()
        {
            if (!hasCreateSpecialAction && isHit && Vector2.Distance(targetPosition,bladeColliders[0].transform.position)< radius)
            {
                ActiveSpecialAction();
            }
        }
        protected override void GetWeaponCollider()
        {
            if(!hasCreateSpecialAction)
            allCollider = bladeColliders.ToArray();
        }

        protected override void MoveAction()
        {
            if (weaponType == MoveWeaponType.Static)
            {
                if (rigidbodyWeapon.velocity.x > 0)
                {
                    bladeColliders[0].gameObject.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    bladeColliders[0].gameObject.transform.localRotation = original;
                }
                float angle = Vector2.SignedAngle(Vector2.right, rigidbodyWeapon.velocity);
                transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }

        public override void CollisionWithPlane(Vector2 positionHit, Vector2 direction)
        {
            transform.position = positionHit;
            bladeColliders[0].transform.position = positionHit;
            hasTrigger = false;
            isActiveAction = true;
            isShoot = true;
            ActiveSpecialAction();
        }

        public override void StartShoot()
        {
            
            StartCoroutine(DelayEnableCollider(0.08f));
        }

        public IEnumerator DelayEnableCollider(float time)
        {
            yield return new WaitForSeconds(time);
            DisableCollider(true);
            rigidbodyWeapon.bodyType = RigidbodyType2D.Dynamic;

        }

        protected override void SettingWeaponToShoot()
        {
            bladeColliders[0].transform.localRotation = original;
        }
    }
}
