using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DefaultGun : GunCtrl
{

    [PunRPC]
    public override void InitialWeapon()
    {
    }

    [PunRPC]
    public override void WeaponBehavior(float angle)
    {
        if (pv.IsMine)
        {
            owner.CreateBullet(owner.damage, angle);
        }
    }
}
