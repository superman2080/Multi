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
        
        //RPC�Լ��� ����ȭ �ֱ�(fps, 1�ʿ� 60��)
        PhotonNetwork.SendRate = 60;
        //����ȭ�� ������(OnPhotonSerializeView)�� ����ȭ �ֱ� (1�ʿ� 30��)
        PhotonNetwork.SerializationRate = 30;
        //���� �濡 ���� �ִ� ����� �ٸ� ���� �ε����� �� �� Ŭ���̾�Ʈ�� �Ȱ��� �� �ε�
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
                //�÷��̾ �̹� ������ ��� �Լ� ����
                if (player.pv.ViewID / 1000 == PhotonNetwork.LocalPlayer.ActorNumber)
                    return;
            }
            //�÷��̾ �������� �ʴ´ٸ� �÷��̾� ����
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
