using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoGrow : MonoBehaviour {
    public int cost = 500;

    private Subscription<GoldChangeEvent> s;
    private bool autogrow;

    public GameObject go;
    public Button b;
    public Text c;
    public Text n;

    public void BuyAutoGrow() {
        autogrow = true;
        EventBus.Unsubscribe<GoldChangeEvent>(s);
        b.interactable = false;
    }

    void Awake() {
        s = EventBus.Subscribe<GoldChangeEvent>(_OnGoldChange);
        EventBus.Subscribe<BoardFullEvent>(_OnBoardFull);
        autogrow = false;

        c.text = cost.ToString();
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

    void _OnBoardFull(BoardFullEvent e) {
        if (!autogrow) {
            return;
        }
        EventBus.Publish<BoardExpansionRequestEvent>(new BoardExpansionRequestEvent());
    }

}