using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour {

    private int gold = 1;

    public Text GoldUI;

    // --------------- PRIVATE METHODS ----------------
    void UpdateShop() {
        GoldUI.text = gold.ToString();
        EventBus.Publish<GoldChangeEvent>(new GoldChangeEvent(gold));
    }

    // ---------------- EVENTS -----------------------
    void Awake() {
        EventBus.Subscribe<SnakeAteAppleEvent>(_OnSnakeAteAppleEvent);
        EventBus.Subscribe<BoardGeneratedEvent>(_OnBoardGeneratedEvent);
        EventBus.Subscribe<SpendGoldEvent>(_OnSpendGoldEvent);

    }

    void _OnSnakeAteAppleEvent(SnakeAteAppleEvent e) {
        ++gold;
        UpdateShop();
    }

    void _OnBoardGeneratedEvent(BoardGeneratedEvent e) {
        gold = 1;
        UpdateShop();
    }

    void _OnSpendGoldEvent(SpendGoldEvent e) {
        Debug.Assert(gold - e.cost > 0);
        gold -= e.cost;

        EventBus.Publish<SnakeLengthReductionEvent>(new SnakeLengthReductionEvent(e.cost));
        UpdateShop();
    }

}