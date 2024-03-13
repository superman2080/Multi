using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class BulletCtrl : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public float angle;
    public float speed;
    public float deadTime;
    public float damage;
    private float dT;
    
    // Start is called before the first frame update
    void Start()
    {
        pv = gameObject.GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
        dT += Time.deltaTime;
        if (dT > deadTime)
            pv.RPC(nameof(DestroyRPC), RpcTarget.All);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("GROUND"))
        {
            pv.RPC(nameof(DestroyRPC), RpcTarget.All);
        }
        if(!pv.IsMine && collision.CompareTag("PLAYER") && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<PlayerCtrl>().pv.RPC(nameof(PlayerCtrl.TakeDamage), RpcTarget.All, damage);
            pv.RPC(nameof(DestroyRPC), RpcTarget.All);
        }
    }

    [PunRPC]
    public void SetAngle(float angle) => transform.eulerAngles = new Vector3(0, 0, angle);
    [PunRPC]
    public void SetDamage(float dmg) => damage = dmg;

    [PunRPC]
    public void DestroyRPC() => Destroy(gameObject);
}
