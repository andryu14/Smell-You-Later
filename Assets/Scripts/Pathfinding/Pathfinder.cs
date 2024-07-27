using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Pathfinder {

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static Pathfinder Instance { get; private set; }

    private GridTemplate<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    public Pathfinder(int width, int height) {
        Instance = this;
        grid = new GridTemplate<PathNode>(width, height, 1f, new Vector3(0, 0), (GridTemplate<PathNode> g, int x, int y) => new PathNode(g, x, y));    
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path = FindPath(startX, startY, endX, endY);
        if (path == null) {
            return null;
        } else {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode in path) {
                vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + new Vector3(1, 1, 0) * grid.GetCellSize() * 0.5f);
            }
            return vectorPath;
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY) {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        for (int i = 5; i < 14; i++) {
            for(int j = 2; j < 6; j++) {
                if (j == 2 || j == 5) {
                    grid.GetGridObject(i, j).SetIsWalkable(false);
                }
            }
        }
        

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                PathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0) {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode) {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighborNode in GetNeighborList(currentNode)) {
                if (closedList.Contains(neighborNode)) {
                    continue;
                }
                if (!neighborNode.isWalkable) {
                    closedList.Add(neighborNode);
                    continue;
                }
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);

                if (tentativeGCost < neighborNode.gCost) {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateDistanceCost(neighborNode, endNode);
                    neighborNode.CalculateFCost();

                    if (!openList.Contains(neighborNode)) {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        return null;
    }

    private List<PathNode> GetNeighborList(PathNode currentNode) {
        List<PathNode> neighborList = new List<PathNode>();

        if (currentNode.x - 1 >= 0) {
            // Left
            neighborList.Add(GetNode(currentNode.x - 1, currentNode.y));
            // Bottom Left
            //if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            // Top Left
            //if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        if (currentNode.x + 1 < grid.GetWidth()) {
            // Right
            neighborList.Add(GetNode(currentNode.x + 1, currentNode.y));
            // Bottom Right
            //if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            // Top Right
            //if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }
        // Bottom
        if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x, currentNode.y - 1));
        // Top
        if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return neighborList;
    }

    private PathNode GetNode(int x, int y) {
        return grid.GetGridObject(x, y);
    }

    public GridTemplate<PathNode> GetGrid() {
        return grid;
    }

    private List<PathNode> CalculatePath(PathNode endNode) {
        List<PathNode> path = new() { endNode };
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null) {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b) {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList) {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++) {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
