using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public abstract class GunCtrl : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public string gunName;
    protected PlayerCtrl owner;

    [PunRPC]
    // Start is called before the first frame update
    public abstract void InitialWeapon();

    [PunRPC]
    public abstract void WeaponBehavior(float angle);

    [PunRPC]
    public void SetOwner(int actorNum)
    {
        foreach (var player in FindObjectsOfType<PlayerCtrl>())
        {
            if(player.pv.ViewID / 1000 == actorNum / 1000)
            {
                owner = player;
                owner.nowWeapon = this;
                transform.SetParent(player.transform);
                return;
            }
        }
    }
}
