using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
namespace _ThrowBattle
{
    public class CharacterSelection : MonoBehaviour
    {
        public GameObject characterPriceUI;
        public GameObject unlockBtn;
        public GameObject selectBtn;
        public GameObject lockBtn;
        public Text priceText;
        public Text playerSelectText;
        public GameObject priceImg;
        public GameObject characterUI;
        public GameObject weaponUI;
        public GameObject leftArrow;
        public GameObject rightArrow;
        public Text characterName;
        public Text characterDescription;
        public Text weaponName;
        public Image weaponIcon;
        public GameObject title;
        public Dictionary<int, GameObject> characterInstantiateds = new Dictionary<int, GameObject>();
        public GameObject backGround;
        public Transform playerPosition;
        Character characterComponent;
        GameObject _currentCharacter = null;
        GameObject currentCharacter
        {
            get { return _currentCharacter; }
            set
            {
                if (_currentCharacter != value)
                {
                    _currentCharacter = value;
                    if (value != null)
                    {
                        characterComponent = _currentCharacter.GetComponent<Character>();
                        currentCharacter = value;
                        //ChangeCharacterDescription();
                    }
                }
            }
        }
        Vector2 firstTouchPosition;
        Vector2 currentTouchPosition;
        bool startDrag;
        public int currentCharacterIndex;
        public int selectedCharacterIndex;
        private bool isSelectAble = false;

        bool player1Selected = false;

        private void OnEnable()
        {
            player1Selected = false;
            Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
            GameManager.EnterCharSelectionEvent += EnterCharacterSelection;
            GameManager.ExitCharSelectionEvent += ExitCharacterSelection;
        }

        private void OnDisable()
        {
            GameManager.EnterCharSelectionEvent -= EnterCharacterSelection;
            GameManager.ExitCharSelectionEvent -= ExitCharacterSelection;
        }

        // Use this for initialization
        void Start()
        {
            playerSelectText.gameObject.SetActive(false);
            currentCharacterIndex = CharacterManager.Instance.CurrentCharacterIndex;
            selectedCharacterIndex = currentCharacterIndex;
            if (GameManager.Instance.GameState == GameState.Prepare)
                CreatePlayer(CharacterManager.Instance.CurrentCharacterIndex);
            characterUI.SetActive(false);
            weaponUI.SetActive(false);
        }

        private void EnterCharacterSelection()
        {
            if (player1Selected != true && GameManager.gameMode == GameMode.PlayWithFriend)
            {
                playerSelectText.text = "Player 1";
                playerSelectText.gameObject.SetActive(true);
            }
            else
                playerSelectText.gameObject.SetActive(false);
            CharacterManager.Instance.player2Index = -1;
            player1Selected = false;
            isSelectAble = true;
            ChangeCharacterDescription();
        }

        private void ExitCharacterSelection()
        {
            playerSelectText.gameObject.SetActive(false);
            player1Selected = false;
            isSelectAble = false;
            ChangeCharacterDescription();
        }

        public void UnlockCharacter()
        {
            bool unlockSucceeded = characterComponent.Unlock();
            if (unlockSucceeded)
            {
                //currentCharacter.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                unlockBtn.gameObject.SetActive(false);
                selectBtn.gameObject.SetActive(true);

                SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
            }
        }

        void ShowCharacterBtn()
        {
            if (!characterComponent.isFree && !characterComponent.IsUnlocked)
            {
                priceText.gameObject.SetActive(true);
                priceText.text = characterComponent.price.ToString();
                priceImg.gameObject.SetActive(true);
            }
            else
            {
                priceText.gameObject.SetActive(false);
                priceImg.gameObject.SetActive(false);
            }

            if (characterComponent.IsUnlocked)
            {
                unlockBtn.gameObject.SetActive(false);
                lockBtn.gameObject.SetActive(false);
                selectBtn.gameObject.SetActive(true);
            }
            else
            {
                selectBtn.gameObject.SetActive(false);
                if (CoinManager.Instance.Coins >= characterComponent.price)
                {
                    unlockBtn.gameObject.SetActive(true);
                    lockBtn.gameObject.SetActive(false);
                }
                else
                {
                    unlockBtn.gameObject.SetActive(false);
                    lockBtn.gameObject.SetActive(true);
                }
            }

            if (isSelectAble)
            {
                title.SetActive(false);
                characterUI.SetActive(true);
                weaponUI.SetActive(true);
                characterPriceUI.SetActive(true);
            }
            else
            {
                title.SetActive(true);
                characterUI.SetActive(false);
                weaponUI.SetActive(false);
                characterPriceUI.SetActive(false);

                selectBtn.SetActive(false);
                unlockBtn.gameObject.SetActive(false);
                lockBtn.gameObject.SetActive(false);

            }


        }

        public void SelectCharacter()
        {
            if (GameManager.gameMode == GameMode.PlayWithFriend)
            {
                if (player1Selected == true)
                {
                    CharacterManager.Instance.player2Index = currentCharacterIndex;
                    playerSelectText.gameObject.SetActive(false);
                }
                else
                {
                    player1Selected = true;
                    playerSelectText.text = "Player 2";
                    //characterPriceUI.SetActive(true);
                    CharacterManager.Instance.CurrentCharacterIndex = currentCharacterIndex;
                    selectedCharacterIndex = currentCharacterIndex;

#if EASY_MOBILE_PRO
                    if (GameManager.Instance.IsMultiplayerMode() && GameManager.Instance.multiplayerManager.IsPlayWithOtherPlayer())
                    {
                        byte[] characterSelectData = { 11, (byte)currentCharacterIndex };
                        GameManager.Instance.playerController.SendDataToOtherPlayer(characterSelectData);
                    }
#endif
                }
            }
            else
            {
                player1Selected = true;
                characterPriceUI.SetActive(false);
                CharacterManager.Instance.CurrentCharacterIndex = currentCharacterIndex;
                selectedCharacterIndex = currentCharacterIndex;

#if EASY_MOBILE_PRO
                if (GameManager.Instance.IsMultiplayerMode() && GameManager.Instance.multiplayerManager.IsPlayWithOtherPlayer())
                {
                    byte[] characterSelectData = { 11, (byte)currentCharacterIndex };
                    GameManager.Instance.playerController.SendDataToOtherPlayer(characterSelectData);
                }
#endif
            }

        }

        void ChangeCharacterDescription()
        {
            leftArrow.SetActive(true);
            rightArrow.SetActive(true);
            if (currentCharacterIndex == 0)
            {
                rightArrow.SetActive(false);
            }
            if (currentCharacterIndex == CharacterManager.Instance.characters.Length - 1)
            {
                leftArrow.SetActive(false);
            }
            if (title.activeSelf)
            {
                characterUI.SetActive(true);
                weaponUI.SetActive(true);
                title.SetActive(false);
            }
            characterName.text = characterComponent.characterName;
            characterDescription.text = characterComponent.characterDescription;
            weaponName.text = characterComponent.weapon.GetComponent<Weapon>().weaponName;
            weaponIcon.sprite = characterComponent.weapon.GetComponent<Weapon>().iconSprite;
            characterPriceUI.SetActive(true);
            if (characterDescription.text == "" || characterDescription.text == null)
                characterDescription.text = "Character Description";
            if (weaponName.text == "" || weaponName.text == null)
                weaponName.text = "Weapon Name";
            ShowCharacterBtn();
        }

        void CreatePlayer(int characterIndex)
        {
            if (characterInstantiateds.ContainsKey(characterIndex))
            {
                if (currentCharacter != null)
                    currentCharacter.SetActive(false);
                currentCharacter = characterInstantiateds[characterIndex];
                currentCharacter.SetActive(true);
            }
            else
            {
                if (currentCharacter != null)
                    currentCharacter.SetActive(false);
                currentCharacter = Instantiate(CharacterManager.Instance.characters[characterIndex], transform);
                currentCharacter.transform.position = playerPosition.position;
                characterInstantiateds.Add(characterIndex, currentCharacter);
            }
        }

        //Check if click in any UI element
        private bool isPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        void StartTouch()
        {
            startDrag = true;
            firstTouchPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }

        void MouseMoveHandle()
        {
            currentTouchPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            if (Vector2.Distance(currentTouchPosition, firstTouchPosition) > 0.2f)
            {
                if (currentTouchPosition.x > firstTouchPosition.x)
                    RightDrag();
                else
                    LeftDrag();
            }
        }

        public void RightDrag()
        {
            if (currentCharacterIndex < CharacterManager.Instance.characters.Length - 1)
            {
                currentCharacterIndex += 1;
                CreatePlayer(currentCharacterIndex);
                ChangeCharacterDescription();
            }
            startDrag = false;
        }

        public void LeftDrag()
        {
            if (currentCharacterIndex > 0)
            {
                currentCharacterIndex -= 1;
                CreatePlayer(currentCharacterIndex);
                ChangeCharacterDescription();
            }
            startDrag = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (isSelectAble)
            {
                if (Input.GetMouseButtonDown(0) && !isPointerOverUIObject() && !startDrag)
                    StartTouch();
                if (Input.GetMouseButton(0) && startDrag)
                    MouseMoveHandle();
                if (Input.GetMouseButtonUp(0))
                    startDrag = false;
            }
        }
    }
}