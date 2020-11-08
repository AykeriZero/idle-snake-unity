using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FasterSnake : MonoBehaviour {
    public int cost = 200;
    public float cost_increase_speed = 1.2f;
    public int cost_linear_increase = 2;

    public float snake_speed_ratio;
    public SnakeController snake;

    private int num = 0;

    public GameObject go;
    public Button b;
    public Text c;
    public Text n;

    private float original_snake_speed;
    private int original_cost;

    public void BuyFasterSnake() {
        ++num;
        n.text = num.ToString();

        // update cost (must happen before spending gold)
        int old_cost = cost;
        cost = (int) Mathf.Floor(cost * cost_increase_speed) + cost_linear_increase;
        c.text = cost.ToString();

        EventBus.Publish<SpendGoldEvent>(new SpendGoldEvent(old_cost));

        // increase snake speed
        snake.snake_move_time *= snake_speed_ratio;
    }

    void Awake() {
        EventBus.Subscribe<GoldChangeEvent>(_OnGoldChange);
        EventBus.Subscribe<BoardGeneratedEvent>(_OnBoardGeneratedEvent);

        original_snake_speed = snake.snake_move_time;
        original_cost = cost;
        n.text = num.ToString();
        c.text = cost.ToString();
        b.interactable = false;
        go.SetActive(false);
    }

    void _OnBoardGeneratedEvent(BoardGeneratedEvent e) {
        return;

        num = 1;
        cost = original_cost;
        snake.snake_move_time = original_snake_speed;

        UpdateText();
        b.interactable = false;
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

    void UpdateText() {
        n.text = num.ToString();
        c.text = cost.ToString();
    }
}