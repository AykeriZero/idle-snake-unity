using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowBoard : MonoBehaviour {
    private int cost;

    public GameObject go;
    public Button b;
    public Text c;
    public Text n;

    public void BuyGrowBoard() {
        EventBus.Publish<BoardExpansionRequestEvent>(new BoardExpansionRequestEvent());
    }

    void Awake() {
        EventBus.Subscribe<GoldChangeEvent>(_OnGoldChange);
        EventBus.Subscribe<BoardGeneratedEvent>(_OnBoardGeneratedEvent);

        c.text = cost.ToString();
        b.interactable = false;
        go.SetActive(false);
    }

    void _OnGoldChange(GoldChangeEvent e) {

        cost = BoardData.GetWidth() * BoardData.GetHeight();

        if (e.gold > cost / 2) {
            go.SetActive(true);
        }

        // check interatability
        if (e.gold - cost >= 0) {
            b.interactable = true;
        } else {
            b.interactable = false;
        }
    }

    void _OnBoardGeneratedEvent(BoardGeneratedEvent e) {
        cost = BoardData.GetWidth() * BoardData.GetHeight();
        c.text = cost.ToString();
    }

}