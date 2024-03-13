using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MaxHPUp : ItemCtrl
{
    [PunRPC]
    public override void UpdateItemEffect()
    {
        if(owner.hp < owner.maxHP)
        {
            owner.hp += 5f * Time.deltaTime;
        }
        else
        {
            owner.hp = owner.maxHP;
        }
    }

    [PunRPC]
    protected override void InitItemEffect()
    {
        if (owner.pv.IsMine)
            owner.maxHP = 150;
        owner.hp = owner.maxHP;
        //owner.hpBar.fillAmount = 1;
        owner.hpBar.color = Color.magenta;
    }
}
