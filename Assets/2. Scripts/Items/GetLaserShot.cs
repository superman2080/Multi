using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GetLaserShot : ItemCtrl
{
    [PunRPC]
    protected override void InitItemEffect()
    {
        owner.pv.RPC(nameof(PlayerCtrl.SetWeapon), RpcTarget.AllBuffered, "LaserShot");
    }

    [PunRPC]
    public override void UpdateItemEffect()
    {
    }
}
