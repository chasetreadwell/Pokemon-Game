using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum GameState { FreeRoam, Battle, Transition }

public class GameController : MonoBehaviour
{

    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] Image worldFadeScreen;
    [SerializeField] Image battleFadeScreen;
    GameState state;

    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    void StartBattle() {
        StartCoroutine(HandleStartBattle());
    }

    IEnumerator HandleStartBattle() {
        state = GameState.Transition;
        FadeToColor(Color.white, worldFadeScreen);
        yield return new WaitForSeconds(1f);

        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
        var playerBag = playerController.GetComponent<Bag>();

        battleSystem.StartBattle(playerParty, wildPokemon, playerBag);
        FadeFromColor(Color.white, battleFadeScreen);
    }

    void EndBattle(bool won) {
        StartCoroutine(HandleEndBattle(won));
    }

    IEnumerator HandleEndBattle(bool won) {
        Color fadeColor = (won) ? Color.white : Color.black;
        state = GameState.Transition;
        FadeToColor(fadeColor,battleFadeScreen);
        yield return new WaitForSeconds(1f);

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        FadeFromColor(fadeColor, worldFadeScreen);
    }

    void Update()
    {
        if (state == GameState.FreeRoam) {
            playerController.HandleUpdate();
        } else if (state == GameState.Battle) {
            battleSystem.HandleUpdate();
        }
    }

    void FadeToColor(Color color, Image screen) {
        screen.color = new Color(color.r, color.g, color.b, 0);
        screen.DOColor(color, 1f);
    }

    void FadeFromColor(Color color, Image screen) {
        screen.color = new Color(color.r, color.g, color.b, 1);
        screen.DOFade(0, 1f);
    }
}
