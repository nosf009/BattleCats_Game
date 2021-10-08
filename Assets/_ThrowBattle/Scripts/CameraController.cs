using UnityEngine;
using System.Collections;

namespace _ThrowBattle
{
    public class CameraController : MonoBehaviour
    {
        [HideInInspector]
        public GameObject recordCamera;
        [HideInInspector]
        public Transform playerTransform;
        private Vector3 velocity = Vector3.zero;
        private Vector3 originalDistance;
        [HideInInspector]
        public Transform injuredPlayerTransform;
        [HideInInspector]
        public bool followInjuredPlayer;

        [Header("Camera Follow Smooth-Time")]
        public float smoothTime = 0.1f;

        [Header("Shaking Effect")]
        // How long the camera shaking.
    public float shakeDuration = 0.1f;
        // Amplitude of the shake. A larger value shakes the camera harder.
        public float shakeAmount = 0.2f;
        public float decreaseFactor = 0.3f;
        [HideInInspector]
        public Vector3 originalPos;
        Vector3 originalPosition;

        [HideInInspector]
        public bool fixPosition=true;
        [HideInInspector]
        public bool isChangeTurn;
        [HideInInspector]
        public bool isCamMoving = false;

        private float currentShakeDuration;
        private float currentDistance;

        private Vector2 curPos;
        private Vector2 lastPos;


        //void OnEnable()
        //{
        //    CharacterScroller.ChangeCharacter += ChangeCharacter;
        //}

        //void OnDisable()
        //{
        //    CharacterScroller.ChangeCharacter -= ChangeCharacter;
        //}

        void Start()
        {
            StartCoroutine(WaitingPlayerController());
        }

        void Update()
        {
            //#if EASY_MOBILE_PRO
            //            recordCamera.transform.position = transform.position;
            //#endif
            curPos = transform.position;
            if (Vector2.Distance(curPos,lastPos) < 0.1f)
            {
               
                isCamMoving = false;
            }
            else
                isCamMoving = true;
            lastPos = curPos;
            if (followInjuredPlayer && injuredPlayerTransform != null)
            {
                FollowInjuredPlayer();
            }                           
            else
            if (GameManager.Instance.GameState == GameState.Playing && playerTransform != null && fixPosition)
            {
                FollowPlayerTransform();
            }
        }

        void FollowInjuredPlayer()
        {
            Vector3 pos = injuredPlayerTransform.position + originalDistance;
            transform.position = Vector3.SmoothDamp(transform.position, pos, ref velocity, smoothTime);
        }

        void FollowPlayerTransform()
        {
            Vector3 pos = playerTransform.position + originalDistance;
            if(GameManager.gameMode==GameMode.BirdHunt)
                pos.y += 5;
            transform.position = Vector3.SmoothDamp(transform.position, pos, ref velocity, smoothTime);
            if (GameManager.gameMode == GameMode.BirdHunt && Vector2.Distance(transform.position, pos) < 0.1f)
            {
                fixPosition = false;
                originalPos = transform.position;
            }
            if (isChangeTurn && Vector3.Distance(transform.position, pos) < 0.25f)
            {
                isChangeTurn = false;
                GameManager.Instance.playerController.ChangeTurn();
                GameManager.Instance.playerController.isPlay = true;
                GameManager.Instance.playerController.canShoot = true;
                if (GameManager.Instance.playerController.isThisPlayerTurn && GameManager.Instance.playerController.isPlay)
                    GameManager.Instance.playerController.NoticePlayerTurn();
            }
        }

        private void LateUpdate()
        {
            if (!fixPosition)
                transform.rotation = Quaternion.identity;
            if (!fixPosition && GameManager.gameMode == GameMode.BirdHunt && GameManager.Instance.GameState == GameState.Playing)
                transform.position = originalPos;
        }

        public void FixPosition()
        {
            transform.position = playerTransform.position + originalDistance;
        }

        public void ShakeCamera()
        {
            StartCoroutine(Shake());
        }

        IEnumerator Shake()
        {
            originalPos = transform.position;
            currentShakeDuration = shakeDuration;
            while (currentShakeDuration > 0)
            {
                transform.position = originalPos + Random.insideUnitSphere * shakeAmount;
                currentShakeDuration -= Time.deltaTime * decreaseFactor;
                yield return null;
            }
            transform.position = originalPos;
        }

        void ChangeCharacter(int cur)
        {
            StartCoroutine(WaitingPlayerController());
        }

        IEnumerator WaitingPlayerController()
        {
            yield return new WaitForSeconds(0.05f);
            playerTransform = GameManager.Instance.playerController.transform;
            originalDistance = transform.position - playerTransform.transform.position;
        }

        public void ResetCamera()
        {
            
        }
    }
}