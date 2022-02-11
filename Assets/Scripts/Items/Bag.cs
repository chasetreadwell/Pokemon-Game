using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bag : MonoBehaviour
{
    [SerializeField] List<Item> items;

    public List<Item> Items {
        get { return items; }
    }

    public void cleanBag() {
        for(int i = 0; i < items.Count; i++) {
            if(items[i].Amount <= 0) {
                items.RemoveAt(i);
            }
        }
    }
}
