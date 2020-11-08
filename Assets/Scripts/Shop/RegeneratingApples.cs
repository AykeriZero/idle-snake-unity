using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegeneratingApples : MonoBehaviour {
    public int cost = 50;
    public float cost_increase_speed = 1.2f;
    public int cost_linear_increase = 2;

    private int original_cost;
    private int num = 1;
    private int num_inactive = 0;

    public GameObject go;
    public Button b;
    public Text c;
    public Text n;

    public void BuyRegeneratingApples() {
        ++num;
        n.text = num.ToString();

        if (!BoardData.RandomApple()) {
            num_inactive++;
        }

        // update cost (must happen before spending gold)
        int old_cost = cost;
        cost = (int) Mathf.Floor(cost * cost_increase_speed) + cost_linear_increase;
        c.text = cost.ToString();

        EventBus.Publish<SpendGoldEvent>(new SpendGoldEvent(old_cost));
    }

    void Awake() {
        EventBus.Subscribe<GoldChangeEvent>(_OnGoldChange);
        EventBus.Subscribe<SnakeAteAppleEvent>(_OnSnakeAteApple);
        EventBus.Subscribe<BoardGeneratedEvent>(_OnBoardGeneratedEvent);
        EventBus.Subscribe<SnakeLengthReductionEvent>(_OnSnakeLengthReduction);

        original_cost = cost;
        UpdateText();
        b.interactable = false;
        go.SetActive(false);
    }

    void _OnGoldChange(GoldChangeEvent e) {

        if (e.gold > cost / 2) {
            go.SetActive(true);
        }

        // check interatability
        if (e.gold - cost > 0) {
            b.interactable = true;
        } else {
            b.interactable = false;
        }

    }

    void _OnSnakeAteApple(SnakeAteAppleEvent e) {
        if (!BoardData.RandomApple()) {
            num_inactive++;
        }
    }

    void _OnSnakeLengthReduction(SnakeLengthReductionEvent e) {
        // add inactive apples to the board
        while (num_inactive > 0) {
            if (BoardData.RandomApple()) {
                num_inactive--;
            } else {
                break;
            }
        }
    }

    void _OnBoardGeneratedEvent(BoardGeneratedEvent e) {

        //num = 1;
        num_inactive = 0;
        //cost = original_cost;

        UpdateText();
        b.interactable = false;

        for (int i = 0; i < num; ++i) {
            BoardData.RandomApple();
        }
    }

    void UpdateText() {
        n.text = num.ToString();
        c.text = cost.ToString();
    }

}