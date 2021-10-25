using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public enum MatchResult
{
    Win,
    Lose,
    Draw,
    Player1Win,
    Player2Win,
    Suicide,
    AppleShoot,
    BirdHunt
}

namespace _ThrowBattle
{
    public class Character : MonoBehaviour
    {
        public int characterSequenceNumber;
        public string characterName;
        public string characterDescription;
        public int price;
        public bool isFree = false;
        public int health;
        public int originalHealth;
        public GameObject weapon;
        public AILevel BotLevel;
        public bool isChangeTurn;
        public int playerIndex;

        public bool isCheckShoot;
        public bool isThisPlayerCharacter;
        public bool hasReceivedShootData;
        public List<byte> shootData = new List<byte>();
        public Rigidbody2D rigidbody2;

        public float waitTime = 5f;

        public float timeStart = 0;
        public int limitRequestData = 3;
        public int requestDataCount = 0;

        public bool isScore;

        private bool isDead;
        private bool isTouch = true;
        private bool waitFinishShot;
        private bool hasStopCoroutine = false;
        MatchResult result;
        private PlayerManager animatorController;

        private void OnEnable()
        {
            if (animatorController == null)
                animatorController = GetComponent<PlayerManager>();
        }

        public bool IsUnlocked
        {
            get
            {
                return (isFree || PlayerPrefs.GetInt(characterName, 0) == 1);
            }
        }

        void Awake()
        {
            characterName = characterName.ToUpper();
        }

        private void Start()
        {
            originalHealth = health;
            weapon.SetActive(false);
            rigidbody2 = animatorController.listPhysicsBodyParts[0].bodyPart.GetComponent<Rigidbody2D>();
        }

        public void TakeDamage(int damage, Vector2 force, Vector2 position,bool isMakeDamage=true)
        {
            //if (waitFinishShot)
            //    GetComponent<PlayerManager>().BreakAllJoint();
            Camera.main.transform.SetParent(null);
            Camera.main.GetComponent<CameraController>().injuredPlayerTransform = animatorController.body;
            Camera.main.GetComponent<CameraController>().followInjuredPlayer = true;
            if(isMakeDamage)
                health -= damage;
            rigidbody2.AddForceAtPosition(force, position);

            if (GameManager.Instance.playerController.isShotFromThisPlayer)
            {
                CheckHealth();
                GameManager.Instance.playerController.UpdatePlayerHealth();
                if (GameManager.Instance.IsMultiplayerMode())
                {
                    if (playerIndex == 1)
                        SendHealthData(1);
                    else
                        SendHealthData(2);
                }
            }
        }

        void SendHealthData(int playerByteIndex)
        {
            byte[] healthData = { 0, (byte)playerByteIndex };
            byte[] healthByteArray = BitConverter.GetBytes(health);
            healthData = healthData.Concat(healthByteArray).ToArray();
            //GameManager.Instance.playerController.SendDataToOtherPlayer(healthData);
        }

        public void ReceiveHealthData(byte[] data)
        {
            health = BitConverter.ToInt32(data, 1);
            GameManager.Instance.playerController.UpdatePlayerHealth();
            CheckHealth();
        }

        void CheckHealth()
        {
            if (health <= 0)
            {              
                health = 0;
                if (!waitFinishShot && !isDead)
                {
                    isDead = true;
                    CheckPlayerWinOrLose();
                    waitFinishShot = true;
                    Invoke("ResetPlayerTransform", 2);
                    GameManager.Instance.playerController.resultMode = result;
                    GameManager.Instance.playerController.UpdatePlayerHealth();
                    GameManager.Instance.playerController.RaiseFinishPlayer();
                }else if(waitFinishShot)
                {
                    Invoke("Die", 5f);
                }
            }
        }

        void ResetPlayerTransform()
        {
            animatorController.AnimatorControll(true);
            animatorController.ResetAnimator();
        }

        void StartWaitFinish()
        {
            waitFinishShot = true;
        }

        void Die()
        {
            GameManager.Instance.playerController.Die();
        }

        void CheckPlayerWinOrLose()
        {
            if (GameManager.Instance.playerController.IsLastShot == true)
            {
                result = MatchResult.Draw;
                return;
            }
            if (isThisPlayerCharacter)
            {
                if (GameManager.Instance.IsMultiplayerMode())
                {                                  
                    result = MatchResult.Win;
                }
                else
                {
                    if (GameManager.gameMode == GameMode.PlayWithFriend)
                    {
                        if (playerIndex == 1)
                            result = MatchResult.Player2Win;
                        else
                                            result = MatchResult.Player1Win;
                    }
                    else
                    {
                        if (playerIndex == 1)
                        {
                            result = MatchResult.Lose;
                            if (GameManager.Instance.playerController.isThisPlayerShoot)
                                result = MatchResult.Suicide;
                        }
                        else
                        {
                            result = MatchResult.Win;
                        }
                    }
                }
            }
            else
            {
                if (GameManager.Instance.IsMultiplayerMode())
                {
                    result = MatchResult.Lose;
                }
                else
                {
                    if (GameManager.gameMode == GameMode.PlayWithFriend)
                    {
                        if (playerIndex == 1)
                            result = MatchResult.Player2Win;
                        else
                                            result = MatchResult.Player1Win;
                    }
                    else
                    {
                        if (playerIndex == 1)
                        {
                            result = MatchResult.Lose;
                            if (GameManager.Instance.playerController.isThisPlayerShoot)
                                result = MatchResult.Suicide;
                        }
                        else
                        {
                            result = MatchResult.Win;
                        }
                    }
                }
            }
        }

        public void StartCheckHandleShootData()
        {
            StartCoroutine(WaitCheckShootData());
        }

        IEnumerator WaitCheckShootData()
        {
            yield return new WaitForSeconds(2);
            if (!hasStopCoroutine)
                ReceiveDataRequest();
        }

        public void SetShootData(byte[] data)
        {
            shootData.Clear();
            shootData = data.ToList<byte>();
        }

        private void Update()
        {
            if (isChangeTurn)
            {
                if (rigidbody2.velocity == Vector2.zero)
                {
                    isChangeTurn = false;
                    GameManager.Instance.playerController.ChangeTurn();
                    GameManager.Instance.playerController.isPlay = true;
                    GameManager.Instance.playerController.canShoot = true;
                }
            }

            if (isCheckShoot && !isDead )
            {
                if (Vector2.Distance(rigidbody2.velocity, Vector2.zero) < 0.001f || Time.time > (timeStart + waitTime))
                {
                    if (GameManager.Instance.IsMultiplayerMode() && !GameManager.Instance.playerController.isShotFromThisPlayer)
                    {
                        if (isTouch)
                        {
                            isTouch = false;
                            hasStopCoroutine = false;
                            StartCoroutine(WaitCheckReceivedShootData());
                        }
                    }
                    if (hasReceivedShootData || GameManager.Instance.playerController.isShotFromThisPlayer)
                    {
                        ResetCamera();
                        GameManager.Instance.playerController.canShoot = true;
                        isCheckShoot = false;
                        transform.rotation = Quaternion.identity;
                        if (GameManager.Instance.IsMultiplayerMode())
                        {
                            if (GameManager.Instance.playerController.isShotFromThisPlayer)
                            {
                                Invoke("WaitSendData", 0.1f);
                            }
                            else
                            {
                                isTouch = true;
                                hasStopCoroutine = true;
                                StopAllCoroutines();
                                HandleShootReceiveData();
                            }
                        }
                        animatorController.AnimatorControll(true);
                        hasReceivedShootData = false;
                    }
                }
            }
        }

        IEnumerator WaitCheckReceivedShootData()
        {
            yield return new WaitForSeconds(2.0f);
            if (!hasStopCoroutine && requestDataCount < limitRequestData)
            {
                requestDataCount++;
                SendRequestData();
            }
            //else if (!hasStopCoroutine)
                //GameManager.Instance.multiplayerManager.Disconnect();
        }

        void SendRequestData()
        {
            byte[] data = { 13 };
            //GameManager.Instance.playerController.SendDataToOtherPlayer(data);
        }

        public void ReceiveDataRequest(byte[] data = null)
        {
            ResetCamera();
            GameManager.Instance.playerController.canShoot = true;
            isCheckShoot = false;
            transform.rotation = Quaternion.identity;
            isTouch = true;
            hasStopCoroutine = true;
            StopAllCoroutines();
            if (data != null)
            {
                shootData.Clear();
                shootData = data.ToList<byte>();
            }
            HandleShootReceiveData();
            animatorController.AnimatorControll(true);
            hasReceivedShootData = false;
        }

        void ResetCamera()
        {
            Camera.main.orthographicSize = GameManager.Instance.minZoom;
            requestDataCount = 0;
            GameManager.Instance.playerController.canShoot = true;
            Camera.main.GetComponent<CameraController>().followInjuredPlayer = false;
            Camera.main.transform.SetParent(null);
            Camera.main.GetComponent<CameraController>().fixPosition = true;
            Camera.main.transform.rotation = Quaternion.identity;
            if (GameManager.gameMode == GameMode.AppleShoot || GameManager.gameMode == GameMode.BirdHunt)
            {
               GameManager.Instance.playerController.LostLife();
            }
        }

        void WaitSendData()
        {
            byte[] byteArrayHealth = BitConverter.GetBytes(health);
            byte[] byteArrayPoisionX = BitConverter.GetBytes(transform.position.x);
            byte[] byteArrayPoisionY = BitConverter.GetBytes(transform.position.y);
            if (playerIndex == 1)
            {
                byte[] shootDataSend = { 7, 1 };
                shootDataSend = shootDataSend.Concat(TotalArray(byteArrayHealth, byteArrayPoisionX, byteArrayPoisionY)).ToArray();
                //GameManager.Instance.playerController.SendDataToOtherPlayer(shootDataSend);
            }
            else
            {
                byte[] shootDataSend = { 7, 2 };
                shootDataSend = shootDataSend.Concat(TotalArray(byteArrayHealth, byteArrayPoisionX, byteArrayPoisionY)).ToArray();
               // GameManager.Instance.playerController.SendDataToOtherPlayer(shootDataSend);
            }
        }
        Vector3 positionAfterShot;
        void HandleShootReceiveData()
        {
            if (shootData.Count > 0)
            {
                byte[] dataArray = shootData.ToArray();
                health = BitConverter.ToInt32(dataArray, 1);
                GameManager.Instance.playerController.UpdatePlayerHealth();
                positionAfterShot = Vector3.zero;
                positionAfterShot.x = BitConverter.ToSingle(dataArray, 5);
                positionAfterShot.y = BitConverter.ToSingle(dataArray, 9);
                positionAfterShot.z = 0;
                Invoke("FixPosition", 0.01f);
            }
        }

        void FixPosition()
        {
            transform.position = positionAfterShot;
        }

        byte[] TotalArray(byte[] array1, byte[] array2, byte[] array3)
        {
            byte[] totalArray;
            totalArray = array1.Concat(array2).Concat(array3).ToArray();
            return totalArray;
        }

        public void WaitCheckShoot(System.Action callback)
        {
            StartCoroutine(WaitEndFrame(callback));
        }

        IEnumerator WaitEndFrame(System.Action callback)
        {
            
            yield return new WaitForSeconds(0.1f);            
            isCheckShoot = true;
            if (callback != null)
                callback();

        }

        public bool Unlock()
        {
            if (IsUnlocked)
                return true;

            if (_ThrowBattle.CoinManager.Instance.Coins >= price)
            {
                PlayerPrefs.SetInt(characterName, 1);
                PlayerPrefs.Save();
                _ThrowBattle.CoinManager.Instance.RemoveCoins(price);

                return true;
            }

            return false;
        }
    }
}