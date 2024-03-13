using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    public InputField nicknameInput;
    public Text stateText;

    private void Awake()
    {
        //if(instance == null)
        //{
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    if (instance != this)
        //        Destroy(gameObject);
        //}
        
        //RPC함수의 동기화 주기(fps, 1초에 60번)
        PhotonNetwork.SendRate = 60;
        //직렬화된 데이터(OnPhotonSerializeView)의 동기화 주기 (1초에 30번)
        PhotonNetwork.SerializationRate = 30;
        //같은 방에 들어와 있는 대상이 다른 씬을 로드했을 때 내 클라이언트도 똑같은 씬 로드
        PhotonNetwork.AutomaticallySyncScene = true;
        //
        Screen.SetResolution(960, 540, false);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += GameSceneLoadedEvent;
    }

    private void GameSceneLoadedEvent(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "InGame")
        {
            foreach (var player in FindObjectsOfType<PlayerCtrl>())
            {
                //Debug.LogError(PhotonNetwork.LocalPlayer.ActorNumber);
                //플레이어가 이미 생성된 경우 함수 종료
                if (player.pv.ViewID / 1000 == PhotonNetwork.LocalPlayer.ActorNumber)
                    return;
            }
            //플레이어가 존재하지 않는다면 플레이어 생성
            PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        }
    }

    private void Update()
    {
        stateText.text = PhotonNetwork.NetworkClientState.ToString();
    }

    public void Connect()
    {
        if (string.IsNullOrEmpty(nicknameInput.text) == false)
            PhotonNetwork.ConnectUsingSettings();
    }

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;
        roomOptions.CustomRoomProperties = new Hashtable() { { "Map", "MAP" + Random.Range(1, 3).ToString()} };

        PhotonNetwork.CreateRoom("Room" + RandomRoomCode(), roomOptions, null);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("InGame");
        }
    }

    private string RandomRoomCode()
    {
        string result = "";
        for (int i = 0; i < 5; i++)
        {
            result += Random.Range(0, 10).ToString();
        }
        return result;
    }
}
