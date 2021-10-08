using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _ThrowBattle
{
    public abstract class Weapon : MonoBehaviour
    {
        public string weaponName;
        public Sprite iconSprite;
        [HideInInspector]
        public Image icon;
        public MoveWeaponType weaponType;
        public CircleCollider2D circle;
        public List<Collider2D> bladeColliders;

        public int normalDamage = 5;
        public int headDamage = 8;

        [HideInInspector]
        public float startTime = 0;

        protected bool isDestroyImediate = true;

        public bool isShoot;
        [HideInInspector]
        public bool hasTrigger;
        [HideInInspector]
        public bool isComWeapon;
        [HideInInspector]
        public bool isFinishShot = false;

        protected bool hasChangeTurn = false;

        public float force = 2000;

        protected Collider2D[] allCollider;

        protected Rigidbody2D rigidbodyWeapon;

        protected GameObject playerInjured;

        protected float weaponWidth = 0;
        [SerializeField]
        protected bool canShoot = true;
        private bool firstEnable = true;
        protected bool isHitPlane = false;
        protected bool isActiveAction = false;
        protected bool hasTriggerPlayer = false;
        protected bool isDisableWeapon = true;
        protected bool hasResetCamera = false;
        protected bool isScore = false;
        private void OnEnable()
        {
            if (firstEnable)
            {
                firstEnable = false;
                Init();
            }
            if (allCollider == null)
                GetWeaponCollider();

#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom += OnPlayerLeft;
#endif
        }

        private void OnDisable()
        {
#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom -= OnPlayerLeft;
#endif
            OnObjectDisable();
        }

        protected virtual void OnObjectDisable()
        {

        }

        void OnPlayerLeft()
        {
            Camera.main.transform.SetParent(null);
            Destroy(gameObject);
        }

        protected virtual void GetWeaponCollider()
        {
            allCollider = GetComponents<Collider2D>();
        }

        // Use this for initialization
        void Start()
        {

            if (rigidbodyWeapon == null)
                rigidbodyWeapon = GetComponent<Rigidbody2D>();
            CalculateLocalBounds();
            circle.gameObject.SetActive(false);
        }

        protected virtual void Init()
        {

        }

        //Reset weapon property to reuse this object
        public virtual void SettingToShoot()
        {
            isScore = false;
            hasResetCamera = false;
            hasTriggerPlayer = false;
            hasChangeTurn = false;
            SettingWeaponToShoot();
            transform.SetParent(null);
            if (rigidbodyWeapon != null)
                rigidbodyWeapon.WakeUp();
            else
                rigidbodyWeapon = GetComponent<Rigidbody2D>();
            if (GetComponent<SpecialAction>() != null)
                GetComponent<SpecialAction>().hasCreateSpecialAction = false;
            gameObject.SetActive(true);
            if (rigidbodyWeapon == null)
                rigidbodyWeapon = GetComponent<Rigidbody2D>();
            hasTrigger = false;
            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
            isShoot = false;
            gameObject.SetActive(true);
            DisableCollider();
            circle.enabled = false;
            for (int i = allCollider.Length - 1; i >= 0; i--)
            {
                allCollider[i].isTrigger = true;
            }

        }

        public virtual void StartShoot()
        {
            DisableCollider(true);
        }


        /// <summary>
        /// Setting weapon before shoot or before reuse this weapon
        /// </summary>
        protected virtual void SettingWeaponToShoot()
        {

        }


        private void OnTriggerEnter2D(Collider2D collision)
        {

            OnTriggerCallBack(collision);
        }

        public void OnTriggerCallBack(Collider2D collision, bool isClone = false, bool isBoom = false)
        {


            if (GameManager.gameMode != GameMode.AppleShoot && GameManager.gameMode != GameMode.BirdHunt)
            {
                if ((collision.gameObject.CompareTag("Plane") && isHitPlane) || !collision.gameObject.CompareTag("Plane"))
                {
                    if (isShoot && collision.gameObject.GetComponent<CircleCollider2D>() == null && !collision.gameObject.CompareTag("Radius") && (!(collision.gameObject.CompareTag("Player") && (Time.time - startTime) < 0.25f) || isActiveAction))
                    {
                        CollisionHandling(collision, isClone);
                        if (weaponType != MoveWeaponType.RotateWithPhysics && isDisableWeapon)
                        {
                            if (collision.gameObject.CompareTag("Player"))
                                DisableWeapon();
                            else
                                DisableWeapon(false);
                        }
                    }
                }
            }
            else
            {
                if (GameManager.gameMode == GameMode.BirdHunt && collision.GetComponent<TargetObject>() != null)
                {
                    collision.GetComponent<TargetObject>().GetReward();
                    collision.GetComponent<TargetObject>().BirdDie();
                    //Destroy(collision.gameObject);
                    if (gameObject.activeSelf)
                        DisableWeapon();
                }
                else
                {
                    AppleShootTrigger(collision);
                }

            }

        }

        void AppleShootTrigger(Collider2D collision)
        {
            hasChangeTurn = true;
            if ((collision.gameObject.CompareTag("Plane") && isHitPlane) || !collision.gameObject.CompareTag("Plane"))
            {
                if (isShoot && collision.gameObject.GetComponent<CircleCollider2D>() == null && !collision.gameObject.CompareTag("Radius") && (!(collision.gameObject.CompareTag("Player") && (Time.time - startTime) < 0.25f) || isActiveAction))
                {
                    if (collision.gameObject.CompareTag("Target") && !hasTriggerPlayer)
                    {
                        if (!isScore)
                        {
                            GameManager.Instance.playerController.AddLife();
                            ScoreManager.Instance.AddScore(1);
                            Destroy(collision.gameObject);
                            isScore = true;
                        }
                    }
                    else
                    {
                        bool isTouchBladePart = false;
                        foreach (Collider2D collider in bladeColliders)
                        {
                            if (collider != null && collider.IsTouching(collision))
                            {
                                isTouchBladePart = true;
                                break;
                            }
                        }
                        Vector2 oldVelocity = rigidbodyWeapon.velocity.normalized;
                        if (!collision.gameObject.CompareTag("Finish"))
                        {
                            bool impactWithBlade = false;
                            //Make weapons crash into collision object
                            if (isTouchBladePart)
                            {
                                impactWithBlade = true;
                                ImpacWithBladePart(oldVelocity);
                            }
                            else
                            {
                                impactWithBlade = false;
                                ImpacWithRollingPart();
                            }
                            if (collision.gameObject.CompareTag("Player") && GameManager.gameMode != GameMode.BirdHunt)
                            {
                                collision.gameObject.GetComponentInParent<Character>().isScore = isScore;
                                CollideWithPlayer(impactWithBlade, oldVelocity, collision, false);

                                isShoot = false;

                            }
                            else
                            {

                                CollideWithPlane();
                                isShoot = false;
                            }
                        }
                        else
                        {
                            ResetCamera();
                            canShoot = true;
                            DisableWeapon();
                            rigidbodyWeapon.velocity = Vector2.zero;
                            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
                            transform.position = Vector3.zero;
                            gameObject.SetActive(false);
                        }
                        if (weaponType != MoveWeaponType.RotateWithPhysics && isDisableWeapon)
                        {
                            if (collision.gameObject.CompareTag("Player"))
                                DisableWeapon();
                            else
                                DisableWeapon(false);
                        }
                    }
                }
            }
        }

        protected void DisableWeapon(bool isImediate = true)
        {
            rigidbodyWeapon.velocity = Vector2.zero;
            hasTrigger = true;
            if (weaponType == MoveWeaponType.Static || isImediate)
            {
                DisableCollider();
                circle.enabled = false;
            }
            else
            {
                if (gameObject.activeSelf)
                    StartCoroutine(WaitDisable());
            }
            if (GameManager.gameMode == GameMode.BirdHunt)
            {
                gameObject.SetActive(false);
                if (GameManager.Instance != null)
                    GameManager.Instance.playerController.canShoot = true;
            }
        }

        IEnumerator WaitDisable()
        {
            yield return new WaitForSeconds(0.6f);
            DisableCollider();
        }

        private void OnBecameInvisible()
        {
            if (GameManager.gameMode == GameMode.BirdHunt && gameObject.activeSelf)
                DisableWeapon();
        }

        /// <summary>
        /// Disable or enable rigidbody and collider to detect impact
        /// </summary>
        /// <param name="isDisable"></param>
        public virtual void DisableCollider(bool isDisable = false)
        {
            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
            rigidbodyWeapon.angularVelocity = 0;
            if (GetComponent<Collider2D>() != null)
                GetComponent<Collider2D>().enabled = isDisable;
            for (int i = 0; i < allCollider.Length; i++)
            {
                allCollider[i].enabled = isDisable;
            }
            if (!isDisable)
                rigidbodyWeapon.Sleep();
            else
                rigidbodyWeapon.WakeUp();
        }

        /// <summary>
        /// Make weapon crash into plane if start shooting and the weapon go through the plane collider
        /// </summary>
        /// <param name="positionHit"></param>
        /// <param name="direction"></param>
        public virtual void CollisionWithPlane(Vector2 positionHit, Vector2 direction)
        {
            canShoot = true;
            Invoke("ResetCamera", 0.6f);
            DisableWeapon();
            rigidbodyWeapon.velocity = Vector2.zero;
            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
            transform.position = (Vector3)(positionHit + direction.normalized * 0.25f);
            GameManager.Instance.playerController.ChangeTurn();
        }

        /// <summary>
        /// Each weapon is divided into two parts including: rolling part and blade part
        /// This function handle for the blade part collision and make it crash into collision object
        /// </summary>
        /// <param name="oldVelocity"></param>
        protected virtual void ImpacWithBladePart(Vector2 oldVelocity)
        {
            rigidbodyWeapon.angularVelocity = 0;
            rigidbodyWeapon.velocity = Vector2.zero;
            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
            transform.position += (Vector3)oldVelocity * 0.25f;
        }

        /// <summary>
        /// This function handles rolling part collisions and makes it continue to simulate physics
        /// </summary>
        protected virtual void ImpacWithRollingPart()
        {
            for (int i = allCollider.Length - 1; i >= 0; i--)
            {
                allCollider[i].isTrigger = false;
            }
            rigidbodyWeapon.bodyType = RigidbodyType2D.Dynamic;
        }

        protected virtual void CollisionHandling(Collider2D collision, bool isClone = false)
        {
            bool isTouchBladePart = false;
            foreach (Collider2D collider in bladeColliders)
            {
                if (collider != null && collider.IsTouching(collision))
                {
                    isTouchBladePart = true;
                    break;
                }
            }
            circle.gameObject.SetActive(true);
            if (isDestroyImediate)
            {
                isShoot = false;
            }
            if (!hasChangeTurn)
            {
                hasChangeTurn = true;
                GameManager.Instance.playerController.ChangeTurn();
            }
            Vector2 oldVelocity = rigidbodyWeapon.velocity.normalized;
            if (!collision.gameObject.CompareTag("Finish"))
            {
                bool impactWithBlade = false;
                //Make weapons crash into collision object
                if (isTouchBladePart)
                {
                    impactWithBlade = true;
                    ImpacWithBladePart(oldVelocity);
                }
                else
                {
                    impactWithBlade = false;
                    ImpacWithRollingPart();
                }
                if (collision.gameObject.CompareTag("Player") && GameManager.gameMode != GameMode.BirdHunt)
                {
                    CollideWithPlayer(impactWithBlade, oldVelocity, collision);
                }
                else
                {
                    CollideWithPlane();
                    if (isFinishShot && !hasTriggerPlayer && !isClone)
                        GameManager.Instance.playerController.MissFinish();
                }
            }
            else
            {
                ResetCamera();
                canShoot = true;
                DisableWeapon();
                rigidbodyWeapon.velocity = Vector2.zero;
                rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
                transform.position = Vector3.zero;
                gameObject.SetActive(false);
                if (isFinishShot && !hasTriggerPlayer && !isClone)
                    GameManager.Instance.playerController.MissFinish();
            }
        }

        protected virtual void CollideWithPlayer(bool impactWithBlade, Vector2 velocity, Collider2D collision, bool isMakeDamage = true)
        {
            hasTriggerPlayer = true;
            ShowPlayerBone(collision.gameObject);
            if (impactWithBlade)
                transform.SetParent(collision.gameObject.transform);
            MakeDamgeToPlayer(collision, velocity, GetPlayerInjured(collision.gameObject), isMakeDamage);
        }

        protected virtual void CollideWithPlane()
        {
            canShoot = true;
            Invoke("ResetCamera", 0.6f);
        }


        protected virtual void ReLocateEnemy()
        {
            GameManager.Instance.playerController.ReLocateEnemy();
        }

        protected virtual void RecreateAppleInvoke()
        {
            Invoke("ReLocateEnemy", 2f);
        }

        protected virtual void MakeDamgeToPlayer(Collider2D collision, Vector2 velocity, Character character, bool isMakeDamage = true)
        {
            if (SoundManager.Instance.hit != null)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.hit);
            }

            bool isThisOpponent = (character.health <= 0);
            if (collision.gameObject.name == "Head")
                character.TakeDamage(headDamage, velocity * force, transform.position, isMakeDamage);
            else
                character.TakeDamage(normalDamage, velocity * force, transform.position, isMakeDamage);
            if (isFinishShot)
            {
                if (isThisOpponent)
                    BreakJointObject(collision.gameObject);
                else
                    GameManager.Instance.playerController.MissFinish();
            }

            if (GameManager.gameMode == GameMode.AppleShoot)
            {
                if (isScore)
                    character.WaitCheckShoot(RecreateAppleInvoke);
                else
                    character.WaitCheckShoot(null);
            }
            else
            {
                character.WaitCheckShoot(null);
            }

        }

        void BreakJointObject(GameObject objectBreak)
        {
            Joint2D[] joints = objectBreak.GetComponents<Joint2D>();
            if (joints.Length > 0)
            {
                foreach (Joint2D joint in joints)
                {
                    joint.breakForce = 10;
                    joint.breakTorque = 10;
                }
            }
        }

        protected Character GetPlayerInjured(GameObject objectGetHit)
        {
            playerInjured = objectGetHit.transform.root.gameObject;
            Character character = playerInjured.GetComponent<Character>();
            character.rigidbody2 = objectGetHit.gameObject.GetComponent<Rigidbody2D>();
            playerInjured.GetComponent<PlayerManager>().AnimatorControll(false);
            character.timeStart = Time.time;
            return character;
        }

        protected virtual void ShowPlayerBone(GameObject objectRender)
        {
            float dirX = objectRender.transform.root.transform.localScale.x;
            circle.enabled = true;
            SpriteController.Instance.DestroySprite(circle, objectRender.GetComponent<SpriteRenderer>(), dirX);
            circle.enabled = false;
        }

        protected void ResetCamera()
        {
            if (GameManager.gameMode == GameMode.AppleShoot && hasResetCamera)
                return;
            isShoot = false;
            hasResetCamera = true;
            if (GameManager.Instance != null)
            {
                if ((GameManager.Instance.GameState != GameState.Playing) || (GameManager.Instance.playerController == null))
                {
                    return;
                }
            }
            else
                return;
            if (GameManager.gameMode == GameMode.AppleShoot || GameManager.gameMode == GameMode.BirdHunt)
            {
                if (isScore)
                {
                    GameManager.Instance.playerController.CreateRightPlayer();
                    //GameManager.Instance.playerController.RecreateApple();
                }

                if ((!isScore || hasTriggerPlayer) && GameManager.gameMode != GameMode.BirdHunt)
                {

                    GameManager.Instance.playerController.LostLife();

                }
            }
            if (!isFinishShot)
            {
                if (canShoot && GameManager.Instance != null)
                    GameManager.Instance.playerController.canShoot = true;
                if (Camera.main != null && GameManager.gameMode != GameMode.BirdHunt)
                {
                    if (GameManager.Instance != null)
                        Camera.main.orthographicSize = GameManager.Instance.minZoom;
                    Camera.main.transform.SetParent(null);
                    Camera.main.GetComponent<CameraController>().followInjuredPlayer = false;
                    Camera.main.GetComponent<CameraController>().fixPosition = true;
                }
            }
            else
            {
                if (!hasTriggerPlayer)
                    GameManager.Instance.playerController.MissFinish();
            }
        }

        //Get bound of all child object
        private void CalculateLocalBounds()
        {
            Quaternion currentRotation = this.transform.rotation;
            this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            Bounds bounds = new Bounds(this.transform.position, Vector3.one);

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            Vector3 localCenter = bounds.center - this.transform.position;
            bounds.center = localCenter;

            this.transform.rotation = currentRotation;
            weaponWidth = bounds.max.y / 2;
        }

        protected virtual void ProtectLocalTransform()
        {
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            if (GameManager.gameMode != GameMode.BirdHunt)
            {
                Camera.main.transform.localScale = Vector3.zero;
                Camera.main.transform.localRotation = Quaternion.identity;
                Vector3 position = transform.position;
                position.z = Camera.main.transform.position.z;
                Camera.main.transform.position = position;
            }
        }

        protected virtual void CheckHitPlane()
        {
            if (!isHitPlane)
            {
                RaycastHit2D hit = Physics2D.CircleCast(transform.position, weaponWidth, rigidbodyWeapon.velocity, 5, LayerMask.GetMask("Plane"));
                if (hit.collider != null && hit.transform.gameObject.CompareTag("Plane"))
                    isHitPlane = true;
            }
        }

        /// <summary>
        /// Regulates the movement of weapons.
        /// Default is make it look in velocity direction
        /// </summary>
        protected virtual void MoveAction()
        {
            if (weaponType == MoveWeaponType.Static)
            {
                float angle = Vector2.SignedAngle(Vector2.right, rigidbodyWeapon.velocity);
                transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }

        /// <summary>
        /// Place this method in update function to make weapon rotate follow velocity direction when move
        /// </summary>
        public virtual void MoveWeapon()
        {
            if (transform.parent != null)
                transform.SetParent(null);
            ProtectLocalTransform();
            MoveAction();
            CheckHitPlane();
        }


    }
}
