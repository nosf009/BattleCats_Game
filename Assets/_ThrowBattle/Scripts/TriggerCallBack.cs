using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    public class TriggerCallBack : MonoBehaviour {

        public bool isClone;
        public GameObject objectCallBack;
        public bool destroyOntrigger = false;
        bool hasCallBack = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((collision.gameObject.CompareTag("Plane") || collision.gameObject.CompareTag("Player")) && destroyOntrigger)
            {
                if (collision.gameObject.CompareTag("Player"))
                {
                    if (!hasCallBack)
                    {
                        hasCallBack = true;
                        objectCallBack.GetComponent<Weapon>().OnTriggerCallBack(collision,isClone);
                        Destroy(GetComponent<Collider2D>());
                    }
                }
            }
            if (!hasCallBack && destroyOntrigger && !collision.CompareTag("Weapon") && !collision.CompareTag("Finish"))
            {
                hasCallBack = true;
                objectCallBack.GetComponent<Weapon>().OnTriggerCallBack(collision,isClone);
            }
            else if(!hasCallBack && !collision.CompareTag("Weapon") && !destroyOntrigger)
                objectCallBack.GetComponent<Weapon>().OnTriggerCallBack(collision,isClone);
            if(destroyOntrigger && !collision.CompareTag("Weapon"))
                Destroy(gameObject);
        }

        private void OnBecameInvisible()
        {
            if (destroyOntrigger)
            {
                StopAllCoroutines();
                Destroy(gameObject,0.5f);
            }
        }

        private void OnDestroy()
        {
            if (destroyOntrigger)
            {
                if(objectCallBack!=null && objectCallBack.GetComponent<CreateCloneWeapon>()!=null)
                    objectCallBack.GetComponent<CreateCloneWeapon>().DestroyCloneWeapon();
            }
        }

        public void DestroyObjectByTime(float time)
        {
            if (gameObject.activeSelf)
                StartCoroutine(WaitDestroy(time));
        }

        IEnumerator WaitDestroy(float time)
        {
            yield return new WaitForSeconds(time);
            if (gameObject.activeSelf)
                Destroy(gameObject);
        }
    }
}
