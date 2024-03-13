using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LaserShot : GunCtrl, IPunObservable
{
    public float keepLaserTime;
    public float laserWidth;
    public LineRenderer lineRenderer;
    public float magnitude;

    private Coroutine nowCor;
    private float nowWidth;
    
    [PunRPC]
    public override void InitialWeapon()
    {
        owner.attSpeed = 3f;
        lineRenderer.enabled = false;
    }


    [PunRPC]
    public override void WeaponBehavior(float angle)
    {
        if (pv.IsMine && nowCor == null)
            nowCor = StartCoroutine(LaserShotCor());
    }

    private IEnumerator LaserShotCor()
    {
        float dT = 0;
        while (true)
        {
            if (Input.GetMouseButtonUp(0))
            {
                break;
            }
            dT += Time.deltaTime;
            yield return null;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float angle = GameMath.GetAngle(transform.position, mousePos);

        Vector2 endPos = new Vector2((-Mathf.Cos(angle * Mathf.Deg2Rad) * dT * magnitude) + owner.transform.position.x,
            (-Mathf.Sin(angle * Mathf.Deg2Rad) * dT * magnitude) + owner.transform.position.y);

        Vector2 centerPos = new Vector2((owner.transform.position.x + endPos.x) / 2, (owner.transform.position.y + endPos.y) / 2);
        Vector2 size = new Vector2(Mathf.Abs(owner.transform.position.x - endPos.x), laserWidth);

        Collider2D[] players = Physics2D.OverlapBoxAll(centerPos, size, angle, 1 << LayerMask.NameToLayer("PLAYER"));
        foreach (var player in players)
        {
            if(player.GetComponent<PlayerCtrl>() != owner)
            {
                player.GetComponent<PlayerCtrl>().pv.RPC(nameof(PlayerCtrl.TakeDamage), RpcTarget.All, owner.damage);
            }
        }

        dT = 0;

        pv.RPC(nameof(DrawLaser), RpcTarget.All, endPos, laserWidth);

        while(dT < keepLaserTime)
        {
            dT += Time.deltaTime;
            yield return null;
            nowWidth = Mathf.Lerp(keepLaserTime, 0, dT / keepLaserTime);
            pv.RPC(nameof(DrawLaser), RpcTarget.All, endPos, nowWidth);
        }

        pv.RPC(nameof(DrawLaser), RpcTarget.All, Vector2.zero, 0f);
        nowCor = null;
    }

    [PunRPC]
    public void DrawLaser(Vector2 pos, float width)
    {
        if(pos == Vector2.zero)
        {
            //lineRenderer.SetPosition(0, Vector2.zero);
            //lineRenderer.SetPosition(1, Vector2.zero);
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
        else
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, owner.transform.position);
            lineRenderer.SetPosition(1, pos);
            lineRenderer.startWidth = lineRenderer.endWidth = width;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
