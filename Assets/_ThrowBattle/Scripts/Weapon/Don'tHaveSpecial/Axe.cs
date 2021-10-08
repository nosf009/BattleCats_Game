using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    public class Axe : Weapon
    {
        bool isCollideWithPlane=false;
        bool hasDisable;
        protected override void Init()
        {
            hasDisable = false;
        }
        protected override void SettingWeaponToShoot()
        {
            isCollideWithPlane = false;
        }

      /*  private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && !hasTriggerPlayer && GameManager.gameMode != GameMode.BirdHunt)
            {
                hasTriggerPlayer = true;
                ShowPlayerBone(collision.gameObject);
                MakeDamgeToPlayer(collision.collider, rigidbodyWeapon.velocity, GetPlayerInjured(collision.gameObject));
            }
            else if (GameManager.gameMode == GameMode.BirdHunt)
                DisableWeapon();
        }
        */
        /*
        protected override void CollideWithPlayer(bool impactWithBlade, Vector2 velocity, Collider2D collision, bool isMakeDamage = true)
        {
            hasTriggerPlayer = true;
            ShowPlayerBone(collision.gameObject);
            if (impactWithBlade)
            {
                transform.SetParent(collision.gameObject.transform);
                DisableWeapon();
            }
            else
            {
                Vector2 normal;
                if (rigidbodyWeapon.velocity.x > 0)
                    normal = Vector2.left;
                else
                    normal = Vector2.right;
                Vector2 direction = Vector2.Reflect(rigidbodyWeapon.velocity, normal);
                float magnitude = rigidbodyWeapon.velocity.magnitude;
                rigidbodyWeapon.velocity = direction.normalized* magnitude;
            }
            MakeDamgeToPlayer(collision, velocity, GetPlayerInjured(collision.gameObject),isMakeDamage);
        }
        */

        protected override void CollideWithPlane()
        {
            isCollideWithPlane = true;
            isShoot = true;

            Invoke("ResetCamera", 0.6f);
        }

        public override void StartShoot()
        {
            DisableCollider(true);
        }

        void Update()
        {
            if (isShoot)
            {
                MoveWeapon();
                if (rigidbodyWeapon.velocity == Vector2.zero && isCollideWithPlane && !hasTriggerPlayer)
                {
                    hasDisable = true;
                    isShoot = false;
                    DisableWeapon();
                    canShoot = true;
                    //ResetCamera();
                }
            }
            if (!hasDisable && rigidbodyWeapon.velocity == Vector2.zero && isCollideWithPlane)
            {
                hasDisable = true;
                DisableWeapon();
                if(isShoot)
                {
                    isShoot = false;
                }
            }
        }
    }
}
