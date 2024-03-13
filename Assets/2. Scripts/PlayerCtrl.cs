using Cinemachine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Stats")]
    public float maxHP;
    [HideInInspector]
    public float hp;
    public float damage;
    public float speed;
    public float jumpPow;
    public float attSpeed;
    public Transform jumpTr;
    public float dashTime = 0.2f;
    public float dashDist = 2f;
    private float dashCoolDown;
    private const float dashCoolTime = 5f;

    [Header("UI")]
    public Image hpBar;
    public Text nickname;
    public Image dashCoolTimeImg;

    public PhotonView pv;
    public Rigidbody2D rb2d;

    [Header("Appearance")]
    public SpriteRenderer sR;
    public Animator animator;

    [Header("Items")]
    public Transform itemTr;
    public List<ItemCtrl> itemList;
    public GunCtrl nowWeapon;

    private Vector3 curPos;
    private int jumpCnt;
    private bool flipX;
    private bool canFire = true;
    private Coroutine dashCor = null;

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            Hashtable cp = PhotonNetwork.CurrentRoom.CustomProperties;
            foreach (var item in cp)
            {
                Debug.LogError(item.Value);
            }
            GameObject.Find("Map").transform.Find((string)cp["Map"]).gameObject.SetActive(true);
        }
        pv = gameObject.GetComponent<PhotonView>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        nickname.color = pv.IsMine ? Color.green : Color.red;
        nickname.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        dashCoolTimeImg = GameObject.Find("DashCoolTime").GetComponent<Image>();
        hp = maxHP;
        if (pv.IsMine)
        {
            var cm = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            cm.LookAt = transform;
            cm.Follow = transform;
            cm.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = GameObject.Find("Constraint").GetComponent<Collider2D>();
            pv.RPC(nameof(SetWeapon), RpcTarget.All, "DefaultGun");
        }
        SetSpawnPos();
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            foreach (var item in itemList)
            {
                item?.pv.RPC(nameof(ItemCtrl.UpdateItemEffect), RpcTarget.All);
            }
            hpBar.fillAmount = hp / maxHP;
            PlayerMove();
            PlayerAttack();
            SpriteRenderFlip(flipX);
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100)
            transform.position = curPos;
        else
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            SpriteRenderFlip(flipX);
            hpBar.fillAmount = hp / maxHP;
        }
    }

    private void PlayerMove()
    {
        float velocity = Input.GetAxisRaw("Horizontal");

        animator.SetFloat("Velocity", Mathf.Abs(velocity));

        rb2d.velocity = new Vector2(velocity * speed, rb2d.velocity.y);

        if(Physics2D.OverlapCircle((Vector2)jumpTr.position, 0.1f, 1 << LayerMask.NameToLayer("GROUND")) != null && rb2d.velocity.y < 0)
        {
            jumpCnt = 2;
        }

        if(Input.GetButtonDown("Jump") && jumpCnt > 0)
        {
            jumpCnt--;
            rb2d.AddForce(Vector2.up * jumpPow, ForceMode2D.Impulse);
        }

        if(jumpCnt != 2)
        {
            animator.SetTrigger("Jump");
        }


        //´ë½¬
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCor == null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left * (flipX ? 1 : -1) * dashDist, dashDist, 1 << LayerMask.NameToLayer("GROUND"));
            //RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(1, 1.75f),
            //    transform.eulerAngles.z, Vector2.left * (flipX ? 1 : -1) * dashDist, dashDist,
            //    1 << LayerMask.NameToLayer("GROUND"));
            if (hit)
            {
                 StartCoroutine(DashCor(transform.position, hit.point, dashTime));
            }
            else
            {
                StartCoroutine(DashCor(transform.position, (Vector2)transform.position + (Vector2.left * (flipX ? 1 : -1) * dashDist), dashTime));
            }
            dashCor = StartCoroutine(DashCoolDownCor(0.2f));
        }
        //
    }

    private IEnumerator DashCor(Vector2 origin, Vector2 moveTo, float time)
    {
        float dT = 0;
        while(dT < time)
        {
            transform.position = Vector2.Lerp(origin, moveTo, dT / time);
            yield return null;
            dT += Time.deltaTime;
        }
        rb2d.velocity = Vector2.zero;
    }

    private IEnumerator DashCoolDownCor(float alphaTime)
    {
        dashCoolTimeImg.color = Color.white;
        dashCoolTimeImg.fillAmount = 1;
        dashCoolDown = dashCoolTime;
        while(dashCoolDown > 0)
        {
            dashCoolDown -= Time.deltaTime;
            dashCoolTimeImg.fillAmount = dashCoolDown / dashCoolTime;
            yield return null;
        }
        float a = 1;
        float dT = 0;
        while (true)
        {
            dashCoolTimeImg.fillAmount = 1;
            dashCoolTimeImg.color = new Color(1, 1, 1, a);
            a = Mathf.Lerp(1, 0, dT / alphaTime);
            dT += Time.deltaTime;
            yield return null;
            if (a <= 0)
                break;
        }
        dashCoolDown = 0;
        dashCoolTimeImg.color = new Color(1, 1, 1, 0);
        dashCor = null;
    }

    private void PlayerAttack()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x > transform.position.x)
        {
            flipX = false;
        }
        else
        {
            flipX = true;
        }
        if (Input.GetMouseButtonDown(0) && canFire)
        {
            animator.SetTrigger("Fire");
            StartCoroutine(FireCooldown(attSpeed));
            nowWeapon.pv.RPC(nameof(GunCtrl.WeaponBehavior), RpcTarget.All, GameMath.GetAngle(transform.position, mousePos));
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(hp);
            stream.SendNext(maxHP);
            stream.SendNext(flipX);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            hp = (float)stream.ReceiveNext();
            maxHP = (float)stream.ReceiveNext();
            flipX = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            pv.RPC(nameof(GameOver), RpcTarget.All);
        }
    }

    [PunRPC]
    public void GameOver()
    {
        PhotonNetwork.LeaveRoom();
        if(hp <= 0 && pv.IsMine)
        {
            GameObject.Find("Canvas").transform.Find("LosePanel").gameObject.SetActive(true);
            GameObject.Find("Canvas").transform.Find("LosePanel").transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("Network");
                return;
            });
        }
        else
        {
            GameObject.Find("Canvas").transform.Find("WinPanel").gameObject.SetActive(true);
            GameObject.Find("Canvas").transform.Find("WinPanel").transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("Network");
                return;
            });
        }
    }

    [PunRPC]
    public void SpriteRenderFlip(bool b) => sR.flipX = b;


    [PunRPC]
    public void SetWeapon(string weaponName)
    {
        if (pv.IsMine)
        {
            if (nowWeapon != null)
                PhotonNetwork.Destroy(nowWeapon.gameObject);

            nowWeapon = PhotonNetwork.Instantiate("Weapons/" + weaponName, transform.position, Quaternion.identity).GetComponent<GunCtrl>();
        }
        if(nowWeapon != null)
        {
            nowWeapon.pv.RPC(nameof(GunCtrl.SetOwner), RpcTarget.All, pv.ViewID);
            nowWeapon.pv.RPC(nameof(GunCtrl.InitialWeapon), RpcTarget.All);
        }
    }

    public void CreateBullet(float dmg, float ang)
    {
        GameObject bullet = PhotonNetwork.Instantiate("Bullet", transform.position, Quaternion.identity);
        bullet.GetComponent<BulletCtrl>().pv.RPC(nameof(BulletCtrl.SetDamage), RpcTarget.All, dmg);
        bullet.GetComponent<BulletCtrl>().pv.RPC(nameof(BulletCtrl.SetAngle), RpcTarget.All, ang);
    }

    private IEnumerator FireCooldown(float speed)
    {
        canFire = false;
        yield return new WaitForSeconds(1 / speed);
        canFire = true;
    }

    private void SetSpawnPos()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            transform.position = GameObject.Find("SpawnPos_1").transform.position;
        }
        else
        {
            transform.position = GameObject.Find("SpawnPos_2").transform.position;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("LADDER"))
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
            if (Input.GetKey(KeyCode.W))
            {
                rb2d.velocity = Vector2.up * speed;
            }
            if(Input.GetKey(KeyCode.S))
                rb2d.velocity = Vector2.down * speed;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(jumpTr.position, 0.1f);
    }
}
