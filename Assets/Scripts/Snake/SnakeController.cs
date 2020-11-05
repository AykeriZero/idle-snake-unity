using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SnakeController handles the movement of the head and tail. 

public class SnakeController : MonoBehaviour {
    public float snake_move_time = 1f;

    private Coordinate head;
    private Coordinate tail;

    // remember old coordinates for setting sprites
    private Coordinate prev_head;

    // map from coordinate to next link in snake
    private Dictionary<Coordinate, Coordinate> snake_next_memory;

    void Awake() {
        // instantiate data
        head = new Coordinate(0, 0);
        tail = new Coordinate(0, 0);
        prev_head = new Coordinate(-1, -1);
        snake_next_memory = new Dictionary<Coordinate, Coordinate>();

        EventBus.Subscribe<BoardGeneratedEvent>(_OnBoardGenerated);
        EventBus.Subscribe<SnakeLengthReductionEvent>(_OnSnakeLengthReduction);
    }

    void Start() {
        // start up snake movement
        StartCoroutine(MoveSnake());
    }

    IEnumerator MoveSnake() {
        while (true) {
            yield return new WaitForSeconds(snake_move_time);

            // Calculate next head coordinates
            Coordinate head_next_coordinate = CalculateNextTile(head);
            TileController head_next = BoardData.GetTile(head_next_coordinate);
            snake_next_memory.Add(head, head_next_coordinate);

            // if the next is an apple, don't move the tail
            if (head_next.tileType == TileType.Apple) {
                EventBus.Publish<SnakeAteAppleEvent>(new SnakeAteAppleEvent());
            } else {
                // Move the tail
                MoveTail();

                // Only Mark the next head as used if not an apple
                BoardData.MarkUsed(head_next);
            }

            // Move the head

            // set sprites
            if (head_next_coordinate.x == tail.x && head_next_coordinate.y == tail.y) {
                head_next.RemoveSnakeSprite();
            } else {
                BoardData.GetTile(head).SetSnakeSprite(head_next_coordinate, head, prev_head);
                head_next.SetSnakeSprite(new Coordinate(-1, -1), head_next_coordinate, head);
                BoardData.GetTile(tail).SetSnakeSprite(snake_next_memory[tail], tail, new Coordinate(-1, -1));
            }

            head_next.SetTileType(TileType.Snake);

            prev_head = head;
            head = head_next_coordinate;
        }
    }

    void MoveTail() {
        // Clean up
        TileController tail_controller = BoardData.GetTile(tail);
        Debug.Assert(tail_controller.tileType == TileType.Snake);

        BoardData.MarkFree(tail_controller);
        tail_controller.RemoveSnakeSprite();

        // Move
        Coordinate prev_tail = tail;
        tail = snake_next_memory[tail];

        snake_next_memory.Remove(prev_tail);
    }

    Coordinate CalculateNextTile(Coordinate cur) {
        // left edge, move snake up
        // if not at the top
        if (cur.x == 0 && cur.y != BoardData.GetHeight() - 1) {
            return new Coordinate(0, cur.y + 1);
        }

        // if odd row, move to the right
        if (cur.y % 2 == 1) {
            // if at the end, move down
            if (cur.x == BoardData.GetWidth() - 1) {
                return new Coordinate(cur.x, cur.y - 1);
            }
            return new Coordinate(cur.x + 1, cur.y);
        } else {
            // if even row, move to the left

            // if end, move down
            if (cur.x == 1 && cur.y != 0) {
                return new Coordinate(1, cur.y - 1);
            }
            return new Coordinate(cur.x - 1, cur.y);
        }

    }

    void _OnBoardGenerated(BoardGeneratedEvent e) {
        head = new Coordinate(0, 0);
        tail = new Coordinate(0, 0);
        snake_next_memory.Clear();
    }

    void _OnSnakeLengthReduction(SnakeLengthReductionEvent e) {
        for (int i = 0; i < e.reduction; ++i) {
            MoveTail();
        }
    }

}