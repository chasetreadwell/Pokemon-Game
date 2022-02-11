using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] Image previewImage;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    public void Init() {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Pokemon> pokemons) {
        this.pokemons = pokemons;

        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < pokemons.Count) {
                memberSlots[i].SetData(pokemons[i]);
            } else {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Choose a Pokemon.";
    }

    public void UpdateMemberSelection(int selectedMember) {
        for (int i = 0; i < pokemons.Count; i++) {
            if (i == selectedMember) {
                memberSlots[i].SetSelected(true);
                previewImage.sprite = pokemons[i].Base.FrontSprite;
                previewImage.rectTransform.sizeDelta = new Vector2(pokemons[i].Base.FrontSprite.bounds.size.x * 100, pokemons[i].Base.FrontSprite.bounds.size.y * 100);
            } else {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetPartyMessage(string message) {
        messageText.text = message;
    }
}
