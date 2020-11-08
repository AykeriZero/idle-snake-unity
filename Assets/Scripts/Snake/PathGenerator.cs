using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour {

    private static PathGenerator instance;
    private Coordinate[, ] next_map;

    // ---------------------- PUBLIC ----------------------

    public static Coordinate GetNextTile(Coordinate cur) {
        return instance.next_map[cur.x, cur.y];
    }

    void Awake() {

        Debug.Assert(instance == null);
        instance = this;

        EventBus.Subscribe<BoardGeneratedEvent>(_OnBoardGenerated);

    }

    struct MstNode {
        public Coordinate representative; // used to check for cycles in mst
        // array of size 4
        /*
                | 0 | 
            -------------
              3 |   | 1
            -------------
                | 2 | 
        */
        public bool[] nearby_edges;

        public MstNode(int _x, int _y) {
            representative = new Coordinate(_x, _y); // indicates rep = self
            nearby_edges = new bool[] { false, false, false, false }; // no edges yet
        }
    }

    Coordinate GetRepresentative(MstNode[, ] nodes, Coordinate cur) {
        if (nodes[cur.x, cur.y].representative == cur) {
            return cur;
        }

        nodes[cur.x, cur.y].representative = GetRepresentative(nodes, nodes[cur.x, cur.y].representative);
        return nodes[cur.x, cur.y].representative;
    }

    // an edge is represented by a coordinate and the output side of the coordinate
    // (see MstNode) 
    // e.g.
    //      (0,0) --- (1, 0) is represented by Edge { (0,0), 1 } or { (1, 0), 4 } 
    struct Edge {
        public Coordinate vertex;
        public int output_side;

        public Edge(Coordinate _v, int _o) {
            vertex = _v;
            output_side = _o;
        }
    }

    void _OnBoardGenerated(BoardGeneratedEvent e) {
        int height = BoardData.GetHeight();
        int width = BoardData.GetWidth();

        // num nodes = (height / 2) * (width / 2)
        int node_height = height / 2;
        int node_width = width / 2;
        // Create MST with Kruskal's algorithm
        List<Edge> edges = new List<Edge>(); // not sorted in any order
        MstNode[, ] nodes = new MstNode[node_width, node_height];

        // populate nodes and edges
        for (int x = 0; x < node_width; ++x) {
            for (int y = 0; y < node_height; ++y) {
                nodes[x, y] = new MstNode(x, y);

                // add edge 0 if not at top of grid
                if (y < node_height - 1) {
                    edges.Add(new Edge(new Coordinate(x, y), 0));
                }
                // add edge 1 if not at right side of grid
                if (x < node_width - 1) {
                    edges.Add(new Edge(new Coordinate(x, y), 1));
                }
            }
        }
        Debug.Assert(edges.Count == (node_height * (node_width - 1) + node_width * (node_height - 1)));

        int added_edges = 0;
        while (added_edges < node_width * node_height - 1) {
            // select a random edge
            int i = Random.Range(0, edges.Count);
            Edge edge = edges[i];
            Coordinate vertex2;
            if (edge.output_side == 0) {
                vertex2 = new Coordinate(edge.vertex.x, edge.vertex.y + 1);
            } else {
                vertex2 = new Coordinate(edge.vertex.x + 1, edge.vertex.y);
            }

            // remove from edges 
            edges[i] = edges[edges.Count - 1];
            edges.RemoveAt(edges.Count - 1);

            // if the edge forms a cycle, discard, else, add

            // check representative
            Coordinate repA = GetRepresentative(nodes, edge.vertex);
            Coordinate repB = GetRepresentative(nodes, vertex2);

            // if reps are equal, there is a cycle (do nothing)
            // otherwise add the edges to mstTree, update representatives
            if (repA != repB) {
                added_edges++;

                nodes[edge.vertex.x, edge.vertex.y].nearby_edges[edge.output_side] = true;
                nodes[vertex2.x, vertex2.y].nearby_edges[edge.output_side + 2] = true;

                nodes[repA.x, repA.y].representative = repB;
            }
        }

        // Assign paths based on MST
        next_map = new Coordinate[width, height];

        // start at lower left edge
        Coordinate current_step = new Coordinate(0, 0);

        while (current_step != new Coordinate(1, 0)) {
            MstNode node = nodes[current_step.x / 2, current_step.y / 2];
            Coordinate next = new Coordinate(0, 0);

            // check an edge based on position in node quadrant

            // lower left
            if (current_step.x % 2 == 0 && current_step.y % 2 == 0) {
                if (node.nearby_edges[3]) {
                    next = new Coordinate(current_step.x - 1, current_step.y);
                } else {
                    next = new Coordinate(current_step.x, current_step.y + 1);
                }
            }
            // top left
            else if (current_step.x % 2 == 0 && current_step.y % 2 == 1) {
                if (node.nearby_edges[0]) {
                    next = new Coordinate(current_step.x, current_step.y + 1);
                } else {
                    next = new Coordinate(current_step.x + 1, current_step.y);
                }
            }
            // top right
            else if (current_step.x % 2 == 1 && current_step.y % 2 == 1) {
                if (node.nearby_edges[1]) {
                    next = new Coordinate(current_step.x + 1, current_step.y);
                } else {
                    next = new Coordinate(current_step.x, current_step.y - 1);
                }
            }
            // bottom right
            else if (current_step.x % 2 == 1 && current_step.y % 2 == 0) {
                if (node.nearby_edges[2]) {
                    next = new Coordinate(current_step.x, current_step.y - 1);
                } else {
                    next = new Coordinate(current_step.x - 1, current_step.y);
                }
            }

            next_map[current_step.x, current_step.y] = next;
            current_step = next;
        }
    }

}