using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _ThrowBattle
{
    public class ArrowWithoutAction : Weapon
    {
        // Update is called once per frame
        void Update()
        {
            if(isShoot)
            MoveWeapon();
        }

        public override void StartShoot()
        {
            DisableCollider(true);
        }
    }
}
