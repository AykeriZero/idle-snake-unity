using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TileController handles the tile display

public class TileController : MonoBehaviour {
    public TileType tileType;

    [SerializeField]
    private Sprite tile_sprite;
    [SerializeField]
    private Sprite snake_end;
    [SerializeField]
    private Sprite snake_middle_straight;
    [SerializeField]
    private Sprite snake_middle_corner;

    private SpriteRenderer s;

    void Awake() {
        s = GetComponent<SpriteRenderer>();
    }

    public void SetSnakeSprite(Coordinate next, Coordinate cur, Coordinate prev) {
        if (next.x == -1 && prev.x == -1) {
            s.sprite = tile_sprite;
            return;
        } 

        // head of snake
        if (next.x == -1) {
            s.sprite = snake_end;
            if (prev.y == cur.y) {
                if( prev.x < cur.x) {
                    transform.eulerAngles = new Vector3(0, 0, 90);
                    return;
                }
                transform.eulerAngles = new Vector3(0, 0, 270);
                return;
            }

            if (prev.x == cur.x) {
                if( prev.y < cur.y) {
                    transform.eulerAngles = new Vector3(0, 0, 180);
                    return;
                }
                transform.eulerAngles = new Vector3(0, 0, 0);
                return;
            }
        }

        // tail of snake
        if (prev.x == -1) {
            s.sprite = snake_end;
            if (next.y == cur.y) {
                if( next.x < cur.x) {
                    transform.eulerAngles = new Vector3(0, 0, 90);
                    return;
                }
                transform.eulerAngles = new Vector3(0, 0, 270);
                return;
            }

            if (next.x == cur.x) {
                if(next.y < cur.y) {
                    transform.eulerAngles = new Vector3(0, 0, 180);
                    return;
                }
                transform.eulerAngles = new Vector3(0, 0, 0);
                return;
            }
        }

        if (next.y == prev.y) {
            Debug.Assert(cur.y == next.y);
            s.sprite = snake_middle_straight;
            transform.eulerAngles = new Vector3(0, 0, 90);
            return;
        }

        if (next.x == prev.x) {
            Debug.Assert(cur.x == next.x);
            s.sprite = snake_middle_straight;
            transform.eulerAngles = new Vector3(0, 0, 0);
            return;
        }

        if (next.x == cur.x) {
            s.sprite = snake_middle_corner;
            Debug.Assert(prev.x != cur.x);
            if (next.y > cur.y) {
                if (prev.x < cur.x) {
                    transform.eulerAngles = new Vector3(0, 0, 90);
                    return;
                }
                transform.eulerAngles = new Vector3(0, 0, 0);
                return;
            }
            if (prev.x < cur.x) {
                transform.eulerAngles = new Vector3(0, 0, 180);
                return;
            }
            transform.eulerAngles = new Vector3(0, 0, 270);
            return;
        }

        if (prev.x == cur.x) {
            s.sprite = snake_middle_corner;
            Debug.Assert(next.x != cur.x);
            if (prev.y > cur.y) {
                if (next.x < cur.x) {
                    transform.eulerAngles = new Vector3(0, 0, 90);
                    return;
                }
                transform.eulerAngles = new Vector3(0, 0, 0);
                return;
            }
            if (next.x < cur.x) {
                transform.eulerAngles = new Vector3(0, 0, 180);
                return;
            }
            transform.eulerAngles = new Vector3(0, 0, 270);
            return;
        }


    }

    public void RemoveSnakeSprite() {
        s.sprite = tile_sprite;
    }

    public void SetTileType(TileType tt) {
        tileType = tt;

        switch (tt) {
            case TileType.Clear:
                s.color = new Color(0.05f, 0.05f, 0.05f);
                s.sprite = tile_sprite;
                return;
            case TileType.Apple:
                s.color = Color.red;
                s.sprite = tile_sprite;
                return;
            case TileType.Snake:
                s.color = Color.green;
                return;
        }

    }
}