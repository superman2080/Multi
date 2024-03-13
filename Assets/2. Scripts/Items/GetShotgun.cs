using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GetShotgun : ItemCtrl
{
    [PunRPC]
    protected override void InitItemEffect()
    {
        owner.pv.RPC(nameof(PlayerCtrl.SetWeapon), RpcTarget.AllBuffered, "Shotgun");
    }

    [PunRPC]
    public override void UpdateItemEffect()
    {
    }

}
