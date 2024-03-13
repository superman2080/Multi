using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class ChattingManager : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public ScrollRect chattingHistory;
    public InputField input;
    

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (input.isFocused == false)
                input.Select();
            ChattingHistoryControl();
        }
    }

    private void ChattingHistoryControl()
    {
        if(chattingHistory.gameObject.activeSelf == false)
        {
            chattingHistory.gameObject.SetActive(true);
            input.gameObject.SetActive(true);
        }
        else
        {
            if (string.IsNullOrEmpty(input.text))
            {
                chattingHistory.gameObject.SetActive(false);
                input.gameObject.SetActive(false);
            }
            else
            {
                GameObject text = PhotonNetwork.Instantiate("ChattingItemPrefab", Vector2.zero, Quaternion.identity);
                pv.RPC(nameof(SetText), RpcTarget.All, PhotonNetwork.LocalPlayer.NickName + "*" + input.text, text.GetComponent<PhotonView>().ViewID);
                input.text = string.Empty;
            }
        }
    }

    [PunRPC]
    public void SetText(string msg, int viewID)
    {
        foreach (var text in FindObjectsOfType<PhotonView>())
        {
            if(text.ViewID == viewID)
            {
                text.transform.SetParent(chattingHistory.content.transform);
                string[] split = msg.Split("*");
                if(viewID / 1000 == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    split[0] = split[0].Insert(0, " <color=green>");   //맨 앞 문자에 컬러값 추가
                    split[0] += "</color> : ";    //맨 뒤 문자에 컬러값 닫아주기
                }
                else
                {
                    split[0] = split[0].Insert(0, " <color=red>");   //맨 앞 문자에 컬러값 추가
                    split[0] += "</color> : ";    //맨 뒤 문자에 컬러값 닫아주기
                }
                string result = "";
                foreach (var str in split)
                {
                    result += str;
                }
                text.GetComponent<Text>().text = result;
                if(chattingHistory.content.sizeDelta.y >= 300)
                {
                    chattingHistory.content.anchoredPosition = new Vector2(
                        0, chattingHistory.content.sizeDelta.y - 50);
                }
                return;
            }
        }
    }
}
