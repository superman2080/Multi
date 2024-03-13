using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemManager : MonoBehaviourPunCallbacks
{
    public float itemCreateInterval;
    private List<Dictionary<string, object>> itemTable;
    // Start is called before the first frame update
    void Start()
    {
        itemTable = CSVReader.Read("ItemTable");
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(CreateRandomItem());
    }

    private IEnumerator CreateRandomItem()
    {
        while (true)
        {
            yield return new WaitForSeconds(itemCreateInterval);
            ItemCtrl item = PhotonNetwork.Instantiate("Items/" + itemTable[Random.Range(0, 3)]["ItemName"].ToString(),
                new Vector2(Random.Range(-18f, 18f), 10), Quaternion.identity).GetComponent<ItemCtrl>();

            item.pv.RPC(nameof(ItemCtrl.ItemDrop), RpcTarget.All);
        }
    }


    
}
