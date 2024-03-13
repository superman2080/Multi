using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

[RequireComponent(typeof(PhotonView))]
public abstract class ItemCtrl : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    protected PlayerCtrl owner;

    [PunRPC]
    public void ItemDrop()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, float.PositiveInfinity, 1 << LayerMask.NameToLayer("GROUND"));
        if (hit)
        {
            Vector2 bottomPos = new Vector2(hit.point.x, hit.point.y + transform.localScale.y / 2);
            StartCoroutine(ItemDropCor(bottomPos));
        }
    }

    private IEnumerator ItemDropCor(Vector2 bottomPos)
    {
        while (transform.position.y >= bottomPos.y)
        {
            transform.Translate(Vector2.down * Time.deltaTime);
            yield return null;
        }
        transform.position = bottomPos;
    }

    [PunRPC]
    protected abstract void InitItemEffect();

    [PunRPC]
    public abstract void UpdateItemEffect();
    //protected abstract void ReleaseItemEffect();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PLAYER"))
        {
            int actorNum = collision.GetComponent<PhotonView>().ViewID;
            foreach (var player in FindObjectsOfType<PlayerCtrl>())
            {
                if(player.pv.ViewID == actorNum)
                {
                    owner = player;
                    transform.SetParent(owner.itemTr);
                    owner.itemList.Add(this);
                    if (this.TryGetComponent(out SpriteRenderer sr))
                    {
                        sr.enabled = false;
                    }
                    if (this.TryGetComponent(out Collider2D col))
                    {
                        col.enabled = false;
                    }
                    if (owner.pv.IsMine)
                        pv.RPC(nameof(ItemCtrl.InitItemEffect), RpcTarget.All);
                    break;
                }
            }
        }
    }
}
