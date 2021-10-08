using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    public enum Reward
    {
        None,
        Coin,
        Time,
        CoinAndTime
    }

    public class TargetObject : MonoBehaviour {
        public Reward objectReward;
        public int coins;
        public float time;
        public bool isScoreObject;
        [HideInInspector]
        public float height;
        Vector3 currentDestination;
        float segmentFlyDistance = 2;
        float amplitudeFly = 1;
        public float speed;
        public float fallSpeed = 5;
        public float fallAccel = 5;
        [HideInInspector]
        public bool isLeftPosition;
        [HideInInspector]
        public bool isFly = false;
        bool hasGetReward=false;
        bool hasVisible;

        private bool isDie = false;
        // Use this for initialization
        void Start() {

        }

        private void OnEnable()
        {
            Invoke("Setting", 0.1f);
        }

        void Setting()
        {
            height = transform.position.y;
            currentDestination = transform.position;
            if(isLeftPosition)
                currentDestination.x += segmentFlyDistance;
            else
                currentDestination.x -= segmentFlyDistance;

            currentDestination.y = height + Random.Range(-amplitudeFly, amplitudeFly);
            isFly = true;
        }

        private void OnBecameInvisible()
        {
            if(hasVisible)
            Destroy(gameObject);
        }

        private void OnBecameVisible()
        {
            hasVisible = true;
        }

        public void GetReward()
        {
            if (!hasGetReward)
            {
                hasGetReward = true;
                if (objectReward == Reward.Coin)
                {
                    CoinManager.Instance.AddCoins(coins);
                    GameManager.Instance.playerController.totalCoin += coins;
                }
                else if (objectReward == Reward.Time)
                {
                    GameManager.Instance.playerController.timeCountDown += time;
                    GameManager.Instance.playerController.UpdateTimeText();
                }
                else if (objectReward == Reward.CoinAndTime)
                {
                    CoinManager.Instance.AddCoins(coins);
                    GameManager.Instance.playerController.totalCoin += coins;
                    GameManager.Instance.playerController.timeCountDown += time;
                    GameManager.Instance.playerController.UpdateTimeText();
                }
                if (isScoreObject)
                    ScoreManager.Instance.AddScore(1);
            }
        }

        private void OnDestroy()
        {
            if(GameManager.Instance!=null)
            GameManager.Instance.playerController.birdCount--;
        }

        public void BirdDie()
        {
            isFly = false;
            isDie = true;
            Animator anim = gameObject.GetComponent<Animator>();
            gameObject.GetComponent<Collider2D>().enabled = false;
            if (anim)
                anim.Play("Die");
            StartCoroutine(WaitToDie(1));
        }

        public IEnumerator WaitToDie(float t)
        {
            yield return new WaitForSeconds(t);
            Destroy(gameObject);
        }

        void Update()
        {
            if (isFly)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentDestination, speed * Time.deltaTime);
                if (Vector2.Distance(transform.position, currentDestination) < 0.01f)
                {

                    if (isLeftPosition)
                        currentDestination.x += segmentFlyDistance;
                    else
                        currentDestination.x -= segmentFlyDistance;

                    currentDestination.y = height + Random.Range(-amplitudeFly, amplitudeFly);
                }
            }
            else if (isDie)
            {
                transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
                fallSpeed += Time.deltaTime* fallAccel;
            }
        }
    }
}
