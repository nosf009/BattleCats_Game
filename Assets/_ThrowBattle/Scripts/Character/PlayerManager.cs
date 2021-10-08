using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    [System.Serializable]
    public struct PhysicsBodyPart
    {
        public GameObject bodyPart;
        public Vector2 originalLocalPosition;
        Rigidbody2D rigid;
        public PhysicsBodyPart(GameObject bodyPart, Vector2 OriginalLocalPosition)
        {
            this.bodyPart = bodyPart;
            this.originalLocalPosition = OriginalLocalPosition;
            rigid = bodyPart.GetComponent<Rigidbody2D>();
        }

        public void ResetTransform()
        {
            if(rigid==null)
                rigid = bodyPart.GetComponent<Rigidbody2D>();
            if (rigid.bodyType != RigidbodyType2D.Static)
                rigid.velocity = Vector2.zero;
            rigid.bodyType = RigidbodyType2D.Static;
            bodyPart.transform.localRotation = Quaternion.identity;
            bodyPart.transform.localPosition = originalLocalPosition;
        }

        public void TurnOnPhysics()
        {
            rigid.bodyType = RigidbodyType2D.Dynamic;
        }

        public void TurnOnTrigger()
        {
            bodyPart.GetComponent<Collider2D>().isTrigger = true;
        }

        public void TurnOffTrigger()
        {
            bodyPart.GetComponent<Collider2D>().isTrigger = false;
        }

        public void TurnOffPhysics()
        {
            rigid.bodyType = RigidbodyType2D.Kinematic;
        }

        public void BreakJoint()
        {
            Joint2D[] joints = bodyPart.GetComponents<Joint2D>();
            if (joints.Length > 0)
            {
                foreach (Joint2D joint in joints)
                {
                    joint.breakForce = 10;
                    joint.breakTorque = 10;
                }
            }
        }
    }

    public class PlayerManager : MonoBehaviour
    {
        public List<PhysicsBodyPart> listPhysicsBodyParts;
        public GameObject[] lowerBodyPart;
        public GameObject[] footConnectGroundPositions;
        public bool turnOn;
        public bool turnOff;
        //bool isAimAnimator;
        [HideInInspector]
        public Transform body;
        [HideInInspector]
        public float characterHeight;
        [HideInInspector]
        public float characterWidth;

        private List<GameObject> bodyParts = new List<GameObject>();

        public HingeJoint2D firstHingeJoin;
        public HingeJoint2D secondHingeJoin;

        private Animator animator;

        private bool hasTurnOffAnimator = false;
        private bool firstEnable = true;
        public bool isEnemy=false;
        GameObject apple=null;
        GameObject head;

        private void OnEnable()
        {
            if (firstEnable)
            {
                firstEnable = false;
                CalculateLocalBounds();
                FindAllPhysicBodyPart();
            }
        }

        public void ResetAnimator()
        {
            
            foreach (PhysicsBodyPart part in listPhysicsBodyParts)
                part.ResetTransform();
            animator.Play("Idle", -1, 0);
            foreach (PhysicsBodyPart part in listPhysicsBodyParts)
                part.TurnOffPhysics(); ;
        }

        public void ResetTransform()
        {
            foreach (PhysicsBodyPart part in listPhysicsBodyParts)
            {
                part.bodyPart.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                part.bodyPart.transform.rotation = Quaternion.identity;
                part.bodyPart.transform.localPosition = part.originalLocalPosition;
                part.ResetTransform();
            }
        }

        // Use this for initialization
        void Start()
        {
            FindTwoFootHingeJoin();
            animator = transform.GetChild(0).GetComponent<Animator>();

            foreach (PhysicsBodyPart part in listPhysicsBodyParts)
            {
                if (part.bodyPart.name == "Body")
                    body = part.bodyPart.transform;
                part.bodyPart.tag = "Player";
            }

            GameObject child = transform.GetChild(0).gameObject;
            foreach (Transform grandchild in child.transform)
            {
                bodyParts.Add(grandchild.gameObject);
            }

            AnimatorControll(true);
        }

        public void TurnOnOfCollider(bool isOn)
        {
            if (isOn)
                foreach (PhysicsBodyPart part in listPhysicsBodyParts)
                    part.bodyPart.GetComponent<Collider2D>().enabled = true;
            else
                foreach (PhysicsBodyPart part in listPhysicsBodyParts)
                    part.bodyPart.GetComponent<Collider2D>().enabled = false;
        }

        public void BreakAllJoint()
        {
            foreach (PhysicsBodyPart part in listPhysicsBodyParts)
            {
                part.BreakJoint();
            }
        }

        void FindAllPhysicBodyPart()
        {
            listPhysicsBodyParts = new List<PhysicsBodyPart>();
            foreach (Transform GrandChild in GetComponentsInChildren<Transform>())
            {
                if (GrandChild.GetComponent<Collider2D>() != null && GrandChild.gameObject.activeSelf && GrandChild.GetComponent<Weapon>() == null && GrandChild.tag != "Weapon")
                {
                    PhysicsBodyPart physicsBodyPart = new PhysicsBodyPart(GrandChild.gameObject, GrandChild.localPosition);
                    listPhysicsBodyParts.Add(physicsBodyPart);
                    if (GrandChild.gameObject.name == "Head")
                    {
                        head = GrandChild.gameObject;
                        if(apple!=null)
                        apple.transform.SetParent(head.transform);
                    }
                }
            }
        }

        public void SetAimAnimation(bool turnOn)
        {
            if (turnOn)
            {
                //isAimAnimator = true;
                TurnOnOfCollider(false);
                SettingAnchorFootHingeJoin();
                foreach (GameObject part in lowerBodyPart)
                    part.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                animator.SetBool("IsAim", turnOn);
            }
            else
            {
                animator.SetBool("IsAim", turnOn);
                //isAimAnimator = false;
                TurnOnOfCollider(true);
                firstHingeJoin.enabled = false;
                secondHingeJoin.enabled = false;
                ResetTransform();
            }
        }

        void SettingAnchorFootHingeJoin()
        {
            firstHingeJoin.enabled = true;
            secondHingeJoin.enabled = true;

            firstHingeJoin.connectedBody = GameManager.Instance.playerController.ground.GetComponent<Rigidbody2D>();
            firstHingeJoin.autoConfigureConnectedAnchor = false;

            secondHingeJoin.connectedBody = firstHingeJoin.connectedBody;
            secondHingeJoin.autoConfigureConnectedAnchor = false;

            UpdateFootJoinAnchor();
        }

        public void UpdateFootJoinAnchor()
        {
            firstHingeJoin.anchor = footConnectGroundPositions[0].transform.localPosition;
            firstHingeJoin.connectedAnchor = footConnectGroundPositions[0].transform.position;

            secondHingeJoin.anchor = footConnectGroundPositions[1].transform.localPosition;
            secondHingeJoin.connectedAnchor = footConnectGroundPositions[1].transform.position;
        }

        void FindTwoFootHingeJoin()
        {
            HingeJoint2D[] firstFootHingerJoins = footConnectGroundPositions[0].transform.parent.GetComponents<HingeJoint2D>();
            foreach (HingeJoint2D hinge in firstFootHingerJoins)
            {
                if (hinge.connectedBody == null)
                {
                    firstHingeJoin = hinge;
                    break;
                }
            }
            HingeJoint2D[] secondFootHingerJoins = footConnectGroundPositions[1].transform.parent.GetComponents<HingeJoint2D>();
            foreach (HingeJoint2D hinge in secondFootHingerJoins)
            {
                if (hinge.connectedBody == null)
                {
                    secondHingeJoin = hinge;
                    break;
                }
            }
        }

        public void AnimatorControll(bool isTurnOn)
        {
            if (isTurnOn)
            {
                TurnOnAnimator();
            }
            else
            {
                TurnOffAnimator();
            }
        }

        void TurnOnAnimator()
        {
            if (hasTurnOffAnimator)
            {
                hasTurnOffAnimator = false;
                transform.position = body.position;
            }
            foreach (PhysicsBodyPart part in listPhysicsBodyParts)
            {
                part.ResetTransform();
            }
            transform.GetChild(0).GetComponent<Animator>().enabled = true;
            SetPositionOnPlane();
            ResetTransform();
            if(GameManager.gameMode == GameMode.AppleShoot)
            {
                if (isEnemy)
                {
                    if (apple != null)
                        Destroy(apple);
                    GenerateApple();
                }
            }
        }

        public void SetPositionOnPlane()
        {
            gameObject.SetActive(false);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down,LayerMask.GetMask("Plane"));
            gameObject.SetActive(true);
            if (hit.collider != null)
            {
                Vector2 position = hit.point;
                position.y += characterHeight / 2;
                transform.position = position;
            }
            if (isEnemy)
            {
                if (apple == null)
                {
                    GenerateApple();
                }
            }
        }


        public void GenerateApple()
        {
            apple = Instantiate(GameManager.Instance.playerController.apple);
            Vector2 position = transform.position;
            position.y += 20;
            RaycastHit2D applehit = Physics2D.Raycast(position, Vector2.down);
            apple.transform.position = applehit.point;
            if (head != null)
                apple.transform.SetParent(head.transform);          
        }

        void TurnOffAnimator()
        {
            hasTurnOffAnimator = true;
            transform.GetChild(0).GetComponent<Animator>().enabled = false;
            foreach (PhysicsBodyPart part in listPhysicsBodyParts)
            {
                part.bodyPart.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            }
        }

        public void TurnOnPhysics()
        {
            foreach (PhysicsBodyPart part in listPhysicsBodyParts)
            {
                part.TurnOnPhysics();
            }
        }

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
            characterHeight = bounds.max.y;
            characterWidth = bounds.max.x;
        }

        private void Update()
        {
            //if (isAimAnimator)
            //    UpdateFootJoinAnchor();
        }
    }
}
