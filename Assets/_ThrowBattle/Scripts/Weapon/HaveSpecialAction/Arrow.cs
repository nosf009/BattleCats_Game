using UnityEngine;

namespace _ThrowBattle
{
    public class Arrow :  DefaulSpecialtWeapon{

        protected override void OnActiveSpecialAction()
        {
                hasCreateSpecialAction = true;
                rigidbodyWeapon.velocity = Vector2.zero;
                rigidbodyWeapon.AddForce(Vector2.down * 1000);
        }
        /// <summary>
        /// Check to see if activating special actions in the current position can hit the player
        /// </summary>
        public override void PredictImpactPosition()
        {
            if (!hasCreateSpecialAction && isHit && Mathf.Abs(targetPosition.x - transform.position.x) < weaponWidth)
            {
                ActiveSpecialAction();
            }
        }
    }
}
