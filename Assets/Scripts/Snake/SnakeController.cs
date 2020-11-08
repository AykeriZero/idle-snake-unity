using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SnakeController handles the movement of the head and tail. 

public class SnakeController : MonoBehaviour {
    public float snake_move_time = 1f;

    private Coordinate head;
    private Coordinate tail;
    private int snake_length;

    // remember old coordinates for setting sprites
    private Coordinate prev_head;

    // map from coordinate to next link in snake
    private Dictionary<Coordinate, Coordinate> snake_next_memory;

    void Awake() {
        // instantiate data
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

            // set sprites
            if (head_next_coordinate.x == tail.x && head_next_coordinate.y == tail.y) {
                // if snake_length is 1, don't do fancy sprites
                head_next.RemoveSnakeSprite();
            } else {
                BoardData.GetTile(head).SetSnakeSprite(head_next_coordinate, head, prev_head);
                head_next.SetSnakeSprite(new Coordinate(-1, -1), head_next_coordinate, head);
                BoardData.GetTile(tail).SetSnakeSprite(snake_next_memory[tail], tail, new Coordinate(-1, -1));
            }

            head_next.SetTileType(TileType.Snake);

            // Move the head
            snake_length++;
            prev_head = head;
            head = head_next_coordinate;

            if (snake_length == BoardData.GetHeight() * BoardData.GetWidth()) {
                EventBus.Publish<BoardFullEvent>(new BoardFullEvent());
            }
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

        snake_length--;
        snake_next_memory.Remove(prev_tail);
    }

    Coordinate CalculateNextTile(Coordinate cur) {

        Coordinate next_move = PathGenerator.GetNextTile(cur);

        // if the snake is quite large, just return next_move
        if (snake_length > BoardData.GetHeight() * BoardData.GetWidth() * 0.6) {
            return next_move;
        }

        // calculate potential other paths
        List<Coordinate> possible_moves = new List<Coordinate>();
        Coordinate p = new Coordinate(cur.x - 1, cur.y);
        if (p.x >= 0 && p.x < BoardData.GetWidth() && p.y >= 0 && p.y < BoardData.GetHeight() &&
            BoardData.GetTile(p).tileType != TileType.Snake && p != next_move) {
            possible_moves.Add(p);
        }
        p = new Coordinate(cur.x + 1, cur.y);
        if (p.x >= 0 && p.x < BoardData.GetWidth() && p.y >= 0 && p.y < BoardData.GetHeight() &&
            BoardData.GetTile(p).tileType != TileType.Snake && p != next_move) {
            possible_moves.Add(p);
        }
        p = new Coordinate(cur.x, cur.y - 1);
        if (p.x >= 0 && p.x < BoardData.GetWidth() && p.y >= 0 && p.y < BoardData.GetHeight() &&
            BoardData.GetTile(p).tileType != TileType.Snake && p != next_move) {
            possible_moves.Add(p);
        }
        p = new Coordinate(cur.x, cur.y + 1);
        if (p.x >= 0 && p.x < BoardData.GetWidth() && p.y >= 0 && p.y < BoardData.GetHeight() &&
            BoardData.GetTile(p).tileType != TileType.Snake && p != next_move) {
            possible_moves.Add(p);
        }

        // if surrounded by snake, just return the next step
        if (possible_moves.Count == 0) {
            return next_move;
        }

        // if we find an apple, just continue down path
        while (true) {
            // if we find one of the possible moves, go that way instead
            foreach (Coordinate possible_move in possible_moves) {
                if (next_move == possible_move) {
                    return next_move;
                }
            }

            if (BoardData.GetTile(next_move).tileType != TileType.Clear) {
                return PathGenerator.GetNextTile(cur);
            }

            next_move = PathGenerator.GetNextTile(next_move);
        }

        Debug.Assert(false);
        return new Coordinate(-1, -1);
    }

    void _OnBoardGenerated(BoardGeneratedEvent e) {
        head = new Coordinate(0, 0);
        tail = new Coordinate(0, 0);
        prev_head = new Coordinate(-1, -1);
        snake_length = 1;
        snake_next_memory.Clear();
    }

    void _OnSnakeLengthReduction(SnakeLengthReductionEvent e) {
        for (int i = 0; i < e.reduction; ++i) {
            MoveTail();
        }
    }

}