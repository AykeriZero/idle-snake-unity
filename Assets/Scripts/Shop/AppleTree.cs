using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppleTree : MonoBehaviour {
    public int cost = 5;
    public float tree_time = 1f;
    public float growth_ratio = 1.5f;

    private int num = 0;

    public GameObject go;
    public Button b;
    public Text c;
    public Text n;

    public void BuyAppleTree() {
        ++num;
        n.text = num.ToString();

        // update cost (must happen before spending gold)
        int old_cost = cost;
        cost = (int) Mathf.Floor(cost * growth_ratio);
        c.text = cost.ToString();

        EventBus.Publish<SpendGoldEvent>(new SpendGoldEvent(old_cost));

        // Trigger Apple Tree
        StartCoroutine(GrowApples());
    }

    void Awake() {
        EventBus.Subscribe<GoldChangeEvent>(_OnGoldChange);

        go.SetActive(false);
        n.text = num.ToString();
        c.text = cost.ToString();
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

    IEnumerator GrowApples() {
        while (true) {
            yield return new WaitForSeconds(tree_time);
            BoardData.RandomApple();
        }
    }

}