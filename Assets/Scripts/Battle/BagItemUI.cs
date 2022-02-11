using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagItemUI : MonoBehaviour
{
    [SerializeField] Text itemText;
    [SerializeField] Text countText;
    [SerializeField] Color highlightedColor;

    Item _item;

    public void SetData(Item item) {
        _item = item;
        itemText.text = item.Base.Name;
        countText.text = item.Amount.ToString();
    }

    public void SetSelected(bool selected) {
        if (selected) {
            itemText.color = highlightedColor;
            countText.color = highlightedColor;
        } else {
            itemText.color = Color.black;
            countText.color = Color.black;
        }
    }
}
