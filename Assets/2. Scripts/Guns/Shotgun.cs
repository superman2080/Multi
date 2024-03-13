using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shotgun : GunCtrl
{
    [Range(1, 7)]
    public int bulletNum;
    public int interval;

    [PunRPC]
    public override void InitialWeapon()
    {
        if (!pv.IsMine)
            return;
        
        owner.attSpeed = 1f;
    }

    [PunRPC]
    public override void WeaponBehavior(float angle)
    {
        if (pv.IsMine)
        {
            for (int i = 0; i < bulletNum; i++)
            {
                owner.CreateBullet(owner.damage, angle + (-interval * (bulletNum - 1) / 2 + i * interval));
            }
        }
    }
}
