using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Item
{
    [SerializeField] ItemBase _base;
    [SerializeField] int amount;

    public Item(ItemBase pBase, int amount) {
        _base = pBase;
        this.amount = amount;
    }

    public ItemBase Base {
        get { return _base; }
    }

    public int Amount {
        get { return amount; }
    }

    public void UseItems(int used) {
        amount -= used;
    }

    public void GainItems(int gained) {
        amount += gained;
    }

    public bool HasEffect(Pokemon pokemon) {
        if(Base.IsRevive) {
            if(pokemon.HP == 0) {
                return true;
            }
        } else {
            if(pokemon.HP < pokemon.MaxHp) {
                return true;
            }
        }

        return false;
    }
}
