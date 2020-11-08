using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// BoardData handles the management of the board including instantiation, growth,
// and maintaining dimensions

public class BoardData : MonoBehaviour {
    // dimensions of the map
    public int map_height = 2;
    public float map_width_ratio = 1.5f;

    public GameObject TilePrefab;

    private TileController[, ] tile_map;
    private List<TileController> free_tiles; // unordered list

    private int map_width;
    private static BoardData instance;

    // -------------- PUBLIC INTERFACE ----------------
    public static int GetHeight() {
        return instance.map_height;
    }

    public static int GetWidth() {
        return instance.map_width;
    }

    // Gets a random free tile and marks it as used
    // returns false on failure, true on success
    public static bool RandomApple() {
        if (instance.free_tiles.Count == 0) return false;

        // Get random index 
        int i = Random.Range(0, instance.free_tiles.Count);
        TileController t = instance.free_tiles[i];

        // Remove from free tiles
        instance.free_tiles[i] = instance.free_tiles[instance.free_tiles.Count - 1];
        instance.free_tiles.RemoveAt(instance.free_tiles.Count - 1);

        Debug.Assert(t.tileType == TileType.Clear);
        t.SetTileType(TileType.Apple);
        return true;
    }

    // Turns all the tiles in the tile_map to Clear
    public static void ClearBoard() {
        instance.free_tiles.Clear();

        for (int y = 0; y < instance.map_height; ++y) {
            for (int x = 0; x < instance.map_width; ++x) {
                instance.tile_map[x, y].SetTileType(TileType.Clear);
                instance.free_tiles.Add(instance.tile_map[x, y]);
            }
        }

        instance.tile_map[0, 0].SetTileType(TileType.Snake);
        MarkUsed(instance.tile_map[0, 0]);
    }

    // Clears the tile and marks it as free
    public static void MarkFree(TileController t) {
        t.SetTileType(TileType.Clear);
        instance.free_tiles.Add(t);
    }

    // Removes the tile from the free list (note: setting tiletype happens elsewhere)
    public static void MarkUsed(TileController t) {
        // get index to remove 
        int i = instance.free_tiles.IndexOf(t);
        Debug.Assert(i != -1);

        instance.free_tiles[i] = instance.free_tiles[instance.free_tiles.Count - 1];
        instance.free_tiles.RemoveAt(instance.free_tiles.Count - 1);
    }

    public static TileController GetTile(Coordinate c) {
        return instance.tile_map[c.x, c.y];
    }

    public static TileController GetTile(int x, int y) {
        return instance.tile_map[x, y];
    }

    // ------------ PRIVATE METHODS -------------------

    private void GrowBoard() {

        // Calculate new height and width
        int new_map_height = map_height + 2;
        int new_map_width = (int) Mathf.Floor(new_map_height * map_width_ratio / 2) * 2; // must always be a multiple of 2

        // create new tilemap
        TileController[, ] new_tile_map = new TileController[new_map_width, new_map_height];

        // Reassign old values
        for (int y = 0; y < map_height; ++y) {
            for (int x = 0; x < map_width; ++x) {
                new_tile_map[x, y] = tile_map[x, y];
            }
        }

        // instantiate new tiles
        for (int y = map_height; y < new_map_height; ++y) {
            for (int x = 0; x < new_map_width; ++x) {
                GameObject go = Instantiate(TilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                go.transform.parent = gameObject.transform;
                new_tile_map[x, y] = go.GetComponent<TileController>();
            }
        }

        for (int y = 0; y < map_height; ++y) {
            for (int x = map_width; x < new_map_width; ++x) {
                GameObject go = Instantiate(TilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                go.transform.parent = gameObject.transform;
                new_tile_map[x, y] = go.GetComponent<TileController>();
            }
        }

        // update values
        map_width = new_map_width;
        map_height = new_map_height;
        tile_map = new_tile_map;

        ClearBoard();
        EventBus.Publish<BoardGeneratedEvent>(new BoardGeneratedEvent());

    }

    // ----------------- EVENTS -----------------
    void Awake() {

        EventBus.Subscribe<BoardExpansionRequestEvent>(_OnBoardExpansionRequest);

        Debug.Assert(instance == null);
        instance = this;

        map_width = (int) Mathf.Floor(map_height * map_width_ratio / 2) * 2;

        tile_map = new TileController[map_width, map_height];
        free_tiles = new List<TileController>();

        // instantiate the gameobjects for the board
        for (int y = 0; y < map_height; ++y) {
            for (int x = 0; x < map_width; ++x) {
                GameObject go = Instantiate(TilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                go.transform.parent = gameObject.transform;
                tile_map[x, y] = go.GetComponent<TileController>();
            }
        }

        ClearBoard();
    }

    void Start() {
        EventBus.Publish<BoardGeneratedEvent>(new BoardGeneratedEvent());
    }

    private void _OnBoardExpansionRequest(BoardExpansionRequestEvent e) {
        GrowBoard();
    }
}

public struct Coordinate {
    public int x;
    public int y;

    public Coordinate(int x_, int y_) {
        x = x_;
        y = y_;
    }

    public static bool operator ==(Coordinate c1, Coordinate c2) {
        return c1.Equals(c2);
    }

    public static bool operator !=(Coordinate c1, Coordinate c2) {
        return !c1.Equals(c2);
    }

    public bool Equals(Coordinate other) {
        return (other.x == x && other.y == y);
    }
}

public enum TileType {
    Clear,
    Apple,
    Snake
}