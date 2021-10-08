using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    public class Projectile : MonoBehaviour
    {
        // launch variables
        [SerializeField] private Transform target;
        float shootAngle;
        float currentHitFrequencyIncrease=0;
        float increaseHitFrequency;
        float currentHitFrequency = 0;
        float maxHitFrequency;

        bool isLeft;
        private Rigidbody2D rigid;
        DefaulSpecialtWeapon defaultComponent;
        // Use this for initialization
        void Start()
        {
            defaultComponent = gameObject.GetComponent<DefaulSpecialtWeapon>();
            increaseHitFrequency = GameManager.Instance.hitFrequencyIncrease;
            maxHitFrequency = GameManager.Instance.maxHitFrequency;
            rigid = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// launches the object towards the TargetObject with a given LaunchAngle
        /// </summary>
        public virtual void Launch(Transform targetTransform,float launchAngle,float hitFrequency,AILevel characterLevel)
        {
            if (targetTransform.position.x < transform.position.x)
                isLeft = true;
            else
                isLeft = false;
            transform.rotation = Quaternion.identity;
            target = targetTransform;
            RandomHitAndMissShot(hitFrequency);
            shootAngle = launchAngle;

            // think of it as top-down view of vectors: 
            //   we don't care about the y-component(height) of the initial and target position.
            Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
            Vector3 targetXZPos = new Vector3(target.position.x, 0.0f, target.position.z);

            // shorthands for the formula
            float R = Vector3.Distance(projectileXZPos, targetXZPos);
            float G = Physics.gravity.y;
            float tanAlpha = Mathf.Tan(shootAngle * Mathf.Deg2Rad);

            float H = target.position.y+0.5f - transform.position.y;
            if (float.IsNaN(tanAlpha) || float.IsNaN(Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)))))
            {
                if (float.IsNaN(tanAlpha))
                   tanAlpha = -0.01f;
            }
            // calculate the local space components of the velocity 
            // required to land the projectile on the target object 
            float Vx = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));
            float Vy = tanAlpha * Vx;

            if (transform.position.x > target.position.x)
                Vx *= -1;

            // create the velocity vector in local space and get it in global space
            Vector3 localVelocity = new Vector3(Vx, Vy, 0);
            Vector3 globalVelocity = transform.TransformDirection(localVelocity);
            Vector2 global = globalVelocity;
            // launch the object by setting its initial velocity and flipping its state
            if(global!=Vector2.zero)
            rigid.velocity = global;
            if(currentHitFrequency<maxHitFrequency)
                currentHitFrequencyIncrease += increaseHitFrequency;
        }

        /// <summary>
        /// Random between hit and miss by hitFrequency
        /// </summary>
        /// <param name="hitFrequency">The frequency that computer players can hit the player</param>
        protected virtual void RandomHitAndMissShot(float hitFrequency)
        {
            float hitRandomFrequency = UnityEngine.Random.Range(0.0f, 1.0f);
            currentHitFrequency = hitFrequency + currentHitFrequencyIncrease;
            if (currentHitFrequency > maxHitFrequency)
                currentHitFrequency = maxHitFrequency;

            if (GetComponent<SpecialAction>() != null)
                RandomWithAction(hitRandomFrequency);
            else
                RandomWithoutAction(hitRandomFrequency);
        }
        
        void RandomWithoutAction(float hitRandomFrequency)
        {
            if (hitRandomFrequency > currentHitFrequency)
            {
                RandomMiss(false);
            }
            else
            {
               //Hit without action
            }
        }

        void RandomWithAction(float hitRandomFrequency)
        {
            bool isActiveAction = false;
            float useActionRandomFrequency = UnityEngine.Random.Range(0.0f, 1.0f);
            if (useActionRandomFrequency < defaultComponent.useActionFrequency)
            {
                isActiveAction = true;
            }
            if (hitRandomFrequency > currentHitFrequency)
            {
                RandomMiss(isActiveAction);
            }
            else if (isActiveAction)
            {
                HitWithAction();
            }
        }

        protected virtual void HitWithAction()
        {
            defaultComponent.AutoActiveSpecialAction(target.position,ActiveActionMode.ByPosition, true);
            if (defaultComponent.actionPositionMode == ActionPositionMode.AbovePlayer)
                MoveToAbovePlayer();
            else if (defaultComponent.actionPositionMode == ActionPositionMode.BehindPlayer)
                MoveToBehindPlayer();
            else if (defaultComponent.actionPositionMode == ActionPositionMode.NearPlayer)
                MoveToNearPlayer();
            else if (defaultComponent.actionPositionMode == ActionPositionMode.InFrontOfPlayer)
                MoveToFrontPlayer();
        }

        void MoveToAbovePlayer()
        {
            float randomOffset;
            randomOffset = UnityEngine.Random.Range(5.0f, 10.0f);
            target.position += new Vector3(0, randomOffset, 0);
        }

        void MoveToBehindPlayer()
        {
            float randomOffset;
            randomOffset = UnityEngine.Random.Range(5.0f, 10.0f);
            if(isLeft)
                target.position += new Vector3(-randomOffset, randomOffset, 0);
            else
                target.position += new Vector3(randomOffset, randomOffset, 0);
        }

        void MoveToNearPlayer()
        {
            float randomWidthOffset;
            randomWidthOffset = UnityEngine.Random.Range(0.5f, 2.0f);
            float randomHeightOffset= UnityEngine.Random.Range(0.5f, 2.0f);
            if (isLeft)
                target.position += new Vector3(randomWidthOffset, randomHeightOffset, 0);
            else
                target.position += new Vector3(-randomWidthOffset, randomHeightOffset, 0);
        }

        void MoveToFrontPlayer()
        {
            float randomWidthOffset;
            randomWidthOffset = UnityEngine.Random.Range(0.5f, 2.0f);
            if (isLeft)
                target.position += new Vector3(randomWidthOffset, 0, 0);
            else
                target.position += new Vector3(-randomWidthOffset, 0, 0);
        }

        protected virtual void RandomMiss(bool isActiveAction)
        {
            float randomOffset;
            if (UnityEngine.Random.Range(0, 2) == 1)
                randomOffset = UnityEngine.Random.Range(5.0f, 10.0f);
            else
                randomOffset = UnityEngine.Random.Range(-5.0f, -10.0f);

            target.position += new Vector3(randomOffset, 0, 0);
            if (isActiveAction)
                defaultComponent.AutoActiveSpecialAction(target.position,ActiveActionMode.ByDistance,false);
        }
    }
}
