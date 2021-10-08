using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    public class Card : Bomb, CreateCloneWeapon
    {
        int directionCount = 6;
        int cardRemain = 0;
        public GameObject cardWeapon;
        bool isFollowWeapon;
        protected override void Init()
        {
            cardWeapon = bladeColliders[0].gameObject;
            original = Quaternion.Euler(0, 0, 0);
            orgY = cardWeapon.transform.localPosition.y;
        }

        protected override void GetWeaponCollider()
        {
            if (!hasCreateSpecialAction)
                allCollider = bladeColliders.ToArray();
        }

        public void CreateCloneWeapon()
        {
            cardRemain = directionCount;
            Vector2 cardDirection = cardWeapon.transform.right;
            Vector2 currentDirection = cardDirection;
            GameObject currentCard;
            bladeColliders.Clear();
            bladeColliders.Add(cardWeapon.GetComponent<Collider2D>());
            for (int i = 0; i < directionCount; i++)
            {
                currentDirection = Quaternion.Euler(0, 0, (360 / directionCount) * i) * cardDirection;
                currentCard = Instantiate(cardWeapon, cardWeapon.transform.position, Quaternion.Euler(0, 0, -30 * i) * cardWeapon.transform.rotation);
                currentCard.tag = "Weapon";
                TriggerCallBack objectComponent = currentCard.GetComponent<TriggerCallBack>();
                objectComponent.isClone = true;
                objectComponent.objectCallBack = gameObject;
                objectComponent.destroyOntrigger = true;
                objectComponent.DestroyObjectByTime(3);
                currentCard.AddComponent<Rigidbody2D>().AddForce(currentDirection.normalized * 1000);
                bladeColliders.Add(currentCard.GetComponent<Collider2D>());
            }
        }

        public void DestroyCloneWeapon()
        {
            cardRemain--;
            if (cardRemain <= 0)
            {
                isShoot = false;
                if (!hasChangeTurn)
                {
                    GameManager.Instance.playerController.ChangeTurn();
                }
                if (!hasTriggerPlayer)
                    ResetCamera();
            }
        }

        protected override void ImpacWithBladePart(Vector2 oldVelocity)
        {
            rigidbodyWeapon.angularVelocity = 0;
            rigidbodyWeapon.velocity = Vector2.zero;
            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
            transform.position += (Vector3)oldVelocity.normalized * 0.01f;
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
            cardWeapon.SetActive(false);
        }

        protected override void ProtectLocalTransform()
        {
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            Camera.main.transform.localScale = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
            if (isFollowWeapon && GameManager.gameMode!=GameMode.BirdHunt)
            {
                Vector3 position = transform.position;
                position.z = Camera.main.transform.position.z;
                Camera.main.transform.position = position;
            }
        }

        protected override void SettingWeaponToShoot()
        {
            isFollowWeapon = true;
            isActiveAction = false;
            isDestroyImediate = true;
            if (cardWeapon == null)
                cardWeapon = bladeColliders[0].gameObject;
            cardWeapon.SetActive(true);
            cardWeapon.transform.localRotation = original;
            angleSin = 0;
            Vector3 position = cardWeapon.transform.localPosition;
            position.y = orgY;
            cardWeapon.transform.localPosition = position;
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

        public void MoveCameraAfterClone()
        {
            if (GameManager.gameMode != GameMode.BirdHunt)
            {
                isFollowWeapon = false;
                Camera.main.GetComponent<CameraController>().injuredPlayerTransform = GameManager.Instance.playerController.otherCharacter.transform;
                Camera.main.GetComponent<CameraController>().followInjuredPlayer = true;
            }
        }
    }
}
