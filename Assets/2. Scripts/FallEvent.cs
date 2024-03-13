using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class FallEvent : MonoBehaviourPunCallbacks
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PLAYER"))
        {
            collision.GetComponent<PlayerCtrl>().pv.RPC(nameof(PlayerCtrl.TakeDamage), RpcTarget.All, 999f);
        }
    }
}
