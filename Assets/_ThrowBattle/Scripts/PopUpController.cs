using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace _ThrowBattle
{
    public interface OkBtnClickAction
    {
        void OnClickOkBtn();
    }
    public class PopUpController : MonoBehaviour
    {
        public static PopUpController Instance;
        public Text priceText;
        public Text messageText;
        public GameObject btnUnlock;
        public GameObject btnBlock;
        public GameObject btnOk;
        public GameObject priceImg;
        public GameObject objectPopUp;

        public void SetMessage(string message)
        {
            messageText.text = message;
        }
        public void SetPrice(int price)
        {
            priceText.text = price.ToString();
        }

        /// <summary>
        /// Show pop up with button
        /// </summary>
        /// <param name="objectCallPopup">object to implement interface OkAction</param>
        /// <param name="canUnlock">To show unlock or lock button</param>
        public void ShowPopUp(GameObject objectCallPopup,bool canUnlock,bool showPrice=true)
        {
            objectPopUp =objectCallPopup;
            if (canUnlock)
            {
                btnUnlock.SetActive(true);
                btnBlock.SetActive(false);
            }
            else
            {
                btnUnlock.SetActive(false);
                btnBlock.SetActive(true);
            }
            if (!showPrice)
            {
                btnUnlock.SetActive(false);
                btnOk.SetActive(true);
                priceText.gameObject.SetActive(false);
                priceImg.gameObject.SetActive(false);
            }
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Only show pop up with message
        /// </summary>
        public void ShowPopUp()
        {
            priceText.gameObject.SetActive(false);
            btnOk.SetActive(false);
            btnBlock.SetActive(false);
            btnUnlock.SetActive(false);
            priceImg.SetActive(false);
            gameObject.SetActive(true);
        }

        public void Unlock()
        {
            objectPopUp.GetComponent<OkBtnClickAction>().OnClickOkBtn();
        }

        public void HidePopUp()
        {
            gameObject.SetActive(false);
        }
    }
}
