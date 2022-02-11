using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen, PartyScreenFaint, Bag, BagPokemon }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] BagScreen bagScreen;
    [SerializeField] Color highlightedColor;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;
    int currentItem;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    Bag playerBag;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, Bag playerBag) {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        this.playerBag = playerBag;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle() {
        currentAction = 0;
        currentMove = 0;
        currentMember = 0;
        currentItem = 0;
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        partyScreen.Init();
        bagScreen.Init();
        enemyUnit.PlayEnterAnimation();
        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");
        yield return new WaitForSeconds(1);
        playerUnit.PlayEnterAnimation();
        yield return dialogBox.TypeDialog($"Go, {playerUnit.Pokemon.Base.Name}!");

        PlayerAction();
    }

    void PlayerAction() {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Choose an action.");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen(bool fainted) {

        if(fainted) {
            state = BattleState.PartyScreenFaint;
        } else {
            state = BattleState.PartyScreen;
        }
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void OpenBagScreen() {
        state = BattleState.Bag;
        bagScreen.SetBagData(playerParty.Pokemons, playerBag.Items);
        bagScreen.ResetSelection();
        bagScreen.gameObject.SetActive(true);
    }

    void PlayerBagPokemon() {
        state = BattleState.BagPokemon;
    }

    IEnumerator SwitchPokemon(Pokemon nextPokemon, bool fainted) {
        state = BattleState.Busy;

        if (!fainted) {
            playerUnit.PlayWithdrawAnimation();
            yield return dialogBox.TypeDialog($"Come back, {playerUnit.Pokemon.Base.Name}!");
        }

        playerUnit.Setup(nextPokemon);
        playerHud.SetData(nextPokemon);

        playerUnit.PlayEnterAnimation();
        yield return dialogBox.TypeDialog($"Go, {playerUnit.Pokemon.Base.Name}!");

        currentAction = 0;
        currentMove = 0;
        currentMember = 0;
        currentItem = 0;
        yield return new WaitForSeconds(1f);
        if (!fainted) {
            StartCoroutine(EnemyMove());
        } else {
            PlayerAction();
        }
    }

    IEnumerator UseItemOnPokemon(Item item, Pokemon pokemon) {
        state = BattleState.Busy;

        item.UseItems(1);
        yield return dialogBox.TypeDialog($"You used a {item.Base.name}.");

        var oldHP = pokemon.HP;
        if (item.Base.IsRevive) {
            pokemon.HP = (int) Math.Ceiling((float) pokemon.MaxHp / 2f);
            pokemon.HP = Math.Min(pokemon.HP + item.Base.HealAmount, pokemon.MaxHp);

            yield return dialogBox.TypeDialog($"{pokemon.Base.Name} recovered from fainting!");
        } else {
            pokemon.HP = Math.Min(pokemon.HP + item.Base.HealAmount, pokemon.MaxHp);

            yield return playerHud.UpdateHP();
            yield return dialogBox.TypeDialog($"{pokemon.Base.Name} was healed by {pokemon.HP - oldHP} points.");
        }

        playerBag.cleanBag();
        StartCoroutine(EnemyMove());
    }

    IEnumerator AttemptRun() {
        state = BattleState.Busy;

        dialogBox.EnableActionSelector(false);
        var success = playerUnit.Pokemon.AttemptRun(enemyUnit.Pokemon);

        if (success) {
            yield return dialogBox.TypeDialog("You got away safely!");
            yield return new WaitForSeconds(1f);
            OnBattleOver(true);
        } else {
            yield return dialogBox.TypeDialog("You couldn't get away!");
            StartCoroutine(EnemyMove());
        }
    }

    void PlayerMove() {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
    }

    IEnumerator PerformPlayerMove() {
        state = BattleState.Busy;
        var move = playerUnit.Pokemon.Moves[currentMove];
        move.PP--;

        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        enemyUnit.PlayHitAnimation();

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails, true);

        if (damageDetails.Fainted) {
            yield return dialogBox.TypeDialog($"The opposing {enemyUnit.Pokemon.Base.Name} fainted!");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        } else {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove() {
        state = BattleState.EnemyMove;
        var move = enemyUnit.Pokemon.GetRandomMove();
        move.PP--;

        yield return dialogBox.TypeDialog($"The opposing {enemyUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails, false);

        if (damageDetails.Fainted) {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} fainted!");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            var nextPokemon = playerParty.GetHealthyPokemon();
            if(nextPokemon != null) {
                OpenPartyScreen(true);
            } else {
                OnBattleOver(false);
            }
        } else {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails, bool isEnemy) {
        if (damageDetails.Critical > 1f) {
            yield return dialogBox.TypeDialog("A critical hit!", highlightedColor);
        }

        if (damageDetails.TypeEffectiveness > 1) {
            yield return dialogBox.TypeDialog("It's super effective!");
        } else if (damageDetails.TypeEffectiveness == 0) {
            yield return (isEnemy) ? dialogBox.TypeDialog($"It doesn't affect the opposing {enemyUnit.Pokemon.Base.Name}...") : dialogBox.TypeDialog($"It doesn't affect {playerUnit.Pokemon.Base.Name}...");
        } else if (damageDetails.TypeEffectiveness < 1) {
            yield return dialogBox.TypeDialog("It's not very effective...");
        }
    }

    public void HandleUpdate() {
        if (state == BattleState.PlayerAction) {
            HandleActionSelection();
        } else if (state == BattleState.PlayerMove) {
            HandleMoveSelection();
        } else if (state == BattleState.PartyScreen) {
            HandlePartySelection(false);
        } else if (state == BattleState.PartyScreenFaint) {
            HandlePartySelection(true);
        } else if (state == BattleState.Bag) {
            HandleBagSelection();
        } else if (state == BattleState.BagPokemon) {
            HandlePartyBagSelection();
        }
    }
    
    void HandleActionSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)  && (currentAction == 0 || currentAction == 2)) {
            ++currentAction;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow) && (currentAction == 1 || currentAction == 3)) {
            --currentAction;
        } else if (Input.GetKeyDown(KeyCode.DownArrow) && (currentAction == 0 || currentAction == 1)) {
            currentAction += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)  && (currentAction == 2 || currentAction == 3)) {
            currentAction -= 2;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z)) {
            if (currentAction == 0) {
                PlayerMove();
            } else if (currentAction == 1) {
                OpenBagScreen();
            } else if (currentAction == 2) {
                OpenPartyScreen(false);
            } else if (currentAction == 3) {
                StartCoroutine(AttemptRun());
            }
        }
    }

    void HandleMoveSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)  && (currentMove == 0 || currentMove == 2)) {
            ++currentMove;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)  && (currentMove == 1 || currentMove == 3)) {
            --currentMove;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)  && (currentMove == 0 || currentMove == 1)) {
            currentMove += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)  && (currentMove == 2 || currentMove == 3)) {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z)) {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
        }
    }

    void HandlePartySelection(bool fainted) {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            ++currentMember;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            --currentMember;
        }

        if (currentMember >= playerParty.Pokemons.Count) {
            currentMember = 0;
        } else if (currentMember < 0) {
            currentMember = playerParty.Pokemons.Count - 1;
        }

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z)) {
            if (playerParty.Pokemons[currentMember] != playerUnit.Pokemon) {
                if (playerParty.Pokemons[currentMember].HP > 0) {
                    partyScreen.gameObject.SetActive(false);
                    dialogBox.EnableActionSelector(false);
                    StartCoroutine(SwitchPokemon(playerParty.Pokemons[currentMember], fainted));
                } else {
                    partyScreen.SetPartyMessage("That Pokemon is unable to battle!");
                }
            } else {
                partyScreen.SetPartyMessage("That Pokemon is already in battle!");
            }
        }

        if (Input.GetKeyDown(KeyCode.X) && !fainted) {
            partyScreen.gameObject.SetActive(false);
            PlayerAction();
        }
    }

    void HandleBagSelection() {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            ++currentItem;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            --currentItem;
        }

        if (currentItem >= playerBag.Items.Count) {
            currentItem = 0;
        } else if (currentItem < 0) {
            currentItem = playerBag.Items.Count - 1;
        }

        bagScreen.UpdateItemSelection(currentItem);

        if (Input.GetKeyDown(KeyCode.Z)) {
            PlayerBagPokemon();
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            bagScreen.gameObject.SetActive(false);
            PlayerAction();
        }
    }

    void HandlePartyBagSelection() {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            ++currentMember;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            --currentMember;
        }

        if (currentMember >= playerParty.Pokemons.Count) {
            currentMember = 0;
        } else if (currentMember < 0) {
            currentMember = playerParty.Pokemons.Count - 1;
        }

        bagScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z)) {
            if (playerBag.Items[currentItem].HasEffect(playerParty.Pokemons[currentMember])) {
                bagScreen.gameObject.SetActive(false);
                dialogBox.EnableActionSelector(false);
                StartCoroutine(UseItemOnPokemon(playerBag.Items[currentItem], playerParty.Pokemons[currentMember]));
            } else {
                bagScreen.SetBagMessage("It won't have any effect.");
            }
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            bagScreen.ResetSelection();
            state = BattleState.Bag;
        }
    }
}
