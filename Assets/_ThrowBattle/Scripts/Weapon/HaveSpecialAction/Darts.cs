using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    public class Darts : DefaulSpecialtWeapon,CreateCloneWeapon
    {
        public float angle=30;
        int directionCount = 3;
        GameObject dartsWeapon;
        int dartsRemain = 0;
        bool isFollowWeapon;
        protected override void Init()
        {
            dartsWeapon = bladeColliders[0].gameObject;
        }


        public void DestroyCloneWeapon()
        {
            dartsRemain--;
            if (dartsRemain <= 0)
            {
                if (!hasChangeTurn)
                {
                    GameManager.Instance.playerController.ChangeTurn();
                }
                if (!hasTriggerPlayer)
                {
                    isShoot = false;
                    ResetCamera();
                }
            }
        }

        protected override void GetWeaponCollider()
        {
            if (!hasCreateSpecialAction)
                allCollider = bladeColliders.ToArray();
        }
        protected override void ImpacWithBladePart(Vector2 oldVelocity)
        {
            rigidbodyWeapon.angularVelocity = 0;
            rigidbodyWeapon.velocity = Vector2.zero;
            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
            transform.position += (Vector3)oldVelocity.normalized * 0.01f;
        }

        public void CreateCloneWeapon()
        {
            dartsRemain = directionCount;
            Vector2 dartsDirection = rigidbodyWeapon.velocity.normalized;
            Vector2 currentDirection = dartsDirection;
            GameObject currentDarts;
            bladeColliders.Clear();
            bladeColliders.Add(dartsWeapon.GetComponent<Collider2D>());
            int currentAim = 1;
            for (int i = 0; i < directionCount; i++)
            {
                if (i == 0)
                {
                    currentDirection = dartsDirection;
                    currentDarts = Instantiate(dartsWeapon, dartsWeapon.transform.position, dartsWeapon.transform.rotation);
                    currentDarts.transform.localScale =  Vector3.one;
                    currentDarts.tag = "Weapon";
                    TriggerCallBack objectComponent = currentDarts.GetComponent<TriggerCallBack>();
                    objectComponent.isClone = true;
                    objectComponent.objectCallBack = gameObject;
                    objectComponent.destroyOntrigger = true;
                    objectComponent.DestroyObjectByTime(3);
                    currentDarts.AddComponent<Rigidbody2D>().velocity = rigidbodyWeapon.velocity;
                    bladeColliders.Add(currentDarts.GetComponent<Collider2D>());
                }
                else
                {
                    currentDirection = Quaternion.Euler(0, 0, angle * currentAim) * dartsDirection;
                    currentDarts = Instantiate(dartsWeapon, dartsWeapon.transform.position, Quaternion.Euler(0, 0, angle * currentAim) * dartsWeapon.transform.rotation);
                    currentDarts.transform.localScale = Vector3.one;
                    currentDarts.tag = "Weapon";
                    TriggerCallBack objectComponent = currentDarts.GetComponent<TriggerCallBack>();
                    objectComponent.isClone = true;
                    objectComponent.objectCallBack = gameObject;
                    objectComponent.destroyOntrigger = true;
                    objectComponent.DestroyObjectByTime(3);
                    currentDarts.AddComponent<Rigidbody2D>().velocity = currentDirection.normalized * rigidbodyWeapon.velocity.magnitude;
                    bladeColliders.Add(currentDarts.GetComponent<Collider2D>());
                    currentAim *= -1;
                }
            }
        }

        protected override void OnActiveSpecialAction()
        {
            MoveCameraAfterClone();
            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
            isShoot = true;
            isActiveAction = true;
            hasCreateSpecialAction = true;
            isDestroyImediate = false;
            CreateCloneWeapon();
            rigidbodyWeapon.velocity = Vector2.zero;
            dartsWeapon.SetActive(false);
        }

        public override void PredictImpactPosition()
        {
            if (!hasCreateSpecialAction && isHit && Vector2.Distance(targetPosition, bladeColliders[0].transform.position) < 5)
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
            if (isFollowWeapon && GameManager.gameMode != GameMode.BirdHunt)
            {
                Vector3 position = transform.position;
                position.z = Camera.main.transform.position.z;
                Camera.main.transform.position = position;
            }
        }

        public void MoveCameraAfterClone()
        {
            if (GameManager.gameMode != GameMode.BirdHunt)
            {
                isFollowWeapon = false;
                Camera.main.GetComponent<CameraController>().injuredPlayerTransform = GameManager.Instance.playerController.otherCharacter.transform;
                Camera.main.GetComponent<CameraController>().followInjuredPlayer = true;
            }
        }

        protected override void SettingWeaponToShoot()
        {
            isFollowWeapon = true;
            isActiveAction = false;
            isDestroyImediate = true;
            if (dartsWeapon == null)
                dartsWeapon = bladeColliders[0].gameObject;
            dartsWeapon.SetActive(true);
        }
        /// <summary>
        /// Make weapon crash into plane if start shooting and the weapon go through the plane collider
        /// </summary>
        /// <param name="positionHit"></param>
        /// <param name="direction"></param>
        public override void CollisionWithPlane(Vector2 positionHit, Vector2 direction)
        {
            isActiveAction = false;
            canShoot = true;
            Invoke("ResetCamera", 0.6f);
            DisableWeapon();
            rigidbodyWeapon.velocity = Vector2.zero;
            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
            transform.position = (Vector3)(positionHit + direction.normalized * 0.01f);
            GameManager.Instance.playerController.ChangeTurn();
        }
    }
}