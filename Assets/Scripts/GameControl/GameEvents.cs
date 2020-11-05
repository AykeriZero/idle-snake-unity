using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGeneratedEvent { }
public class SnakeAteAppleEvent { }
public class BoardExpansionRequestEvent { }
public class SnakeLengthReductionEvent {
    public int reduction;
    public SnakeLengthReductionEvent(int reduction) { this.reduction = reduction; }
}
public class GoldChangeEvent {
    public int gold;
    public GoldChangeEvent(int gold) { this.gold = gold; }
}
public class SpendGoldEvent {
    public int cost;
    public SpendGoldEvent(int cost) { this.cost = cost; }
}
public class BoardFullEvent { }