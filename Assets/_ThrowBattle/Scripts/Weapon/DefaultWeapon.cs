using System.Collections;
using UnityEngine;
using System;
using System.Linq;

namespace _ThrowBattle
{
    public class DefaultWeapon : Weapon
    {
        [HideInInspector]
        public Vector2 targetPosition;

        /// <summary>
        /// Loop when weapon is moving
        /// </summary>
        protected virtual void Loop()
        {

        }

        private void Update()
        {
            if (isShoot)
            {
                Loop();
                MoveWeapon();
            }
        }
    }
}
