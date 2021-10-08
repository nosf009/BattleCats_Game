using UnityEngine;
using System.Collections;
namespace _ThrowBattle
{
    public class CameraWeapon : DefaulSpecialtWeapon
    {
        public LineRenderer lineRender;
        public float timeTurnOnSpecial=0.25f;
        public float timeTurnOffSpecial=0.75f;
        public float timeFirstWaitSpecial=0.25f;
        public float specialSpeed=8;
        Vector3 direction;
        int functionIndex = 1;
        bool isActive = false;
        int limit = 5;
        bool hasHitPlayer = false;
        int count = 0;
        bool hasMakeDamageToPlayer = false;
        //protected override void Init()
        protected override void Init()
        {
            lineRender.enabled=false;
        }

        protected override void ImpacWithRollingPart()
        {
            
        }

        protected override void OnActiveSpecialAction()
        {
            transform.rotation = Quaternion.identity;
            force = 100;
            isDestroyImediate = false;
            isDisableWeapon = false;
            isActive = true;
            GetComponent<Collider2D>().enabled = false;
            rigidbodyWeapon.bodyType = RigidbodyType2D.Kinematic;
            if (rigidbodyWeapon.velocity.x > 0)
                direction = Vector2.right;
            else
                direction = Vector2.left;
            rigidbodyWeapon.velocity = Vector2.zero;
            StartCoroutine(DelayLineRender(timeFirstWaitSpecial));
        }

        protected override void Loop()
        {
            if (isActive)
            {
                transform.position += direction * Time.deltaTime * specialSpeed;
            }
            if (lineRender.enabled)
                UpdateLineRender();
        }

        protected override void MoveAction()
        {
            if (!isActive)
                base.MoveAction();
            //else
            //  new move
        }

        protected override void ImpacWithBladePart(Vector2 oldVelocity)
        {
            gameObject.SetActive(false);
        }

        void UpdateLineRender()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down);
            if (hit.collider != null)
            {
                if (hit.transform.gameObject.CompareTag("Player") && !hasHitPlayer)
                {
                    hasMakeDamageToPlayer = true;
                    hasHitPlayer = true;
                    circle.transform.position = hit.point;
                    OnTriggerCallBack(hit.collider);
                }
                if (GameManager.gameMode == GameMode.BirdHunt && hit.transform.gameObject.GetComponent<TargetObject>() != null)
                {
                    hit.transform.gameObject.GetComponent<TargetObject>().GetReward();
                    hit.transform.gameObject.GetComponent<TargetObject>().BirdDie();
                }
                    lineRender.SetPosition(0, transform.position);
                lineRender.SetPosition(1, hit.point);
            }
        }

        void TurnOnLineRender()
        {
            RaycastHit2D hit= Physics2D.Raycast(transform.position,Vector2.down);
            lineRender.enabled = true;
            if (hit.collider!=null)
            {
                //if (hit.transform.gameObject.CompareTag("Player") && !hasHitPlayer)
                //{
                //    hasHitPlayer = true;
                //    circle.transform.position = hit.point;
                //    OnTriggerCallBack(hit.collider);
                //}
                lineRender.SetPosition(0, transform.position);
                lineRender.SetPosition(1, hit.point);
            }
        }

        IEnumerator DelayLineRender(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            hasHitPlayer = false;
            TurnLineRenderByIndex(functionIndex);
            functionIndex *= -1;
            if (count < limit)
            {
                count++;
                if(functionIndex>0)
                    StartCoroutine(DelayLineRender(timeTurnOffSpecial));
                else
                    StartCoroutine(DelayLineRender(timeTurnOnSpecial));
            }
            else
            {
                isActive = false;
                canShoot = true;
                isShoot = false;
                if (!hasChangeTurn)
                {
                    hasChangeTurn = true;
                    GameManager.Instance.playerController.ChangeTurn();
                }
                if (!hasMakeDamageToPlayer)
                    ResetCamera();
                gameObject.SetActive(false);
            }
        }
        
        void TurnOffLineRender()
        {
            lineRender.enabled = false;
        }

        protected override void SettingWeaponToShoot()
        {
            isActive = false;
            count = 0;
            hasMakeDamageToPlayer = false;
            isDestroyImediate = true;
            isDisableWeapon = true;
            GetComponent<Collider2D>().enabled = true;
            lineRender.enabled = false;
            circle.transform.localPosition = Vector2.zero;
            if (rigidbodyWeapon == null)
                rigidbodyWeapon = GetComponent<Rigidbody2D>();
            rigidbodyWeapon.bodyType = RigidbodyType2D.Dynamic;
        }

        void TurnLineRenderByIndex(int index)
        {
            switch(index)
            {
                case 1:
                    TurnOnLineRender();
                    break;
                case -1:
                    TurnOffLineRender();
                    break;
            }
        }
    }
}
