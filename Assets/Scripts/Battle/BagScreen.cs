using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    BagItemUI[] itemSlots;
    List<Pokemon> pokemons;
    List<Item> items;

    public void Init() {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
        itemSlots = GetComponentsInChildren<BagItemUI>();
    }

    public void SetBagData(List<Pokemon> pokemons, List<Item> items) {
        this.pokemons = pokemons;
        this.items = items;

        for (int i = 0; i < itemSlots.Length; i++) {
            if (i < items.Count) {
                itemSlots[i].SetData(items[i]);
            } else {
                itemSlots[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < pokemons.Count) {
                memberSlots[i].SetData(pokemons[i]);
            } else {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateItemSelection(int selectedItem) {
        for (int i = 0; i < items.Count; i++) {
            if (i == selectedItem) {
                itemSlots[i].SetSelected(true);
                messageText.text = items[i].Base.Description;
            } else {
                itemSlots[i].SetSelected(false);
            }
        }
    }

    public void UpdateMemberSelection(int selectedMember) {
        for (int i = 0; i < pokemons.Count; i++) {
            if (i == selectedMember) {
                memberSlots[i].SetSelected(true);
            } else {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetBagMessage(string message) {
        messageText.text = message;
    }

    public void ResetSelection() {
        UpdateItemSelection(-1);
        UpdateMemberSelection(-1);
    }
}
