using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class piece : MonoBehaviour
{
    public string pieceType;
    public square location;
    public int color;
    bool moved = false;
    public void show()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/"+pieceType+color.ToString());
        location.piece = this;
    }
    public List<int> returnIndexOfTheoreticalMoves()
    {
        int file = location.file;
        int rank = location.rank;
        board board = location.parent;
        List<int> moves = new();
        if (pieceType == "P"){
            if (rank == 7 || rank == 0)
            {
                return new List<int>();
            }
            moves.Add((rank+(1*color))*8+file);
            if (board.isOccupied(moves[0]))
            {
                moves.RemoveAt(0);
            }
            else
            {
                if (!moved && ! board.isOccupied((rank+(2*color))*8+file))
                {
                    moves.Add((rank+(2*color))*8+file);
                }
            }
        }
        else if (pieceType == "R")
        {
            int[][] permutations = 
            {
                new[] {0,1},
                new[] {1,0},
                new[] {-1,0},
                new[] {0,-1}
            };
            moves = permutate(permutations);
        }
        else if (pieceType == "B")
        {
            int[][] permutations = 
            {
                new[] {1,1},
                new[] {1,-1},
                new[] {-1,1},
                new[] {-1,-1}
            };
            moves = permutate(permutations);
        }
        else if (pieceType == "Q")
        {
            int[][] permutations = 
            {
                new[] {1,1},
                new[] {1,-1},
                new[] {-1,1},
                new[] {-1,-1},
                new[] {0,1},
                new[] {1,0},
                new[] {-1,0},
                new[] {0,-1}
            };
            moves = permutate(permutations);
        }
        else if (pieceType == "N")
        {
            int[][] permutations =
            {
                new[] { 2, 1 },
                new[] { 2, -1 },
                new[] { 1, 2 },
                new[] { 1, -2 },
                new[] { -2, 1 },
                new[] { -2, -1 },
                new[] { -1, 2 },
                new[] { -1, -2 }
            };
            foreach (int[] perm in permutations)
            {
                Debug.Log($"File: {file}, Rank: {rank}");
                Debug.Log($"Permutation: {perm[0]}, {perm[1]}");
                Debug.Log($"Result: {file + perm[0]}, {rank + perm[1]}");
                if (file + perm[0] < 0 || file + perm[0] > 7 ||
                rank + perm[1] < 0 || rank + perm[1] > 7) continue;
                if (!board.isOccupied(file+perm[0], rank + perm[1]))
                {
                    moves.Add(file+perm[0] + (rank + perm[1])*8);
                }
            }
        }
        return moves;
    }
    public List<int> returnIndexOfTheoreticalCaptures()
    {
        int file = location.file;
        int rank = location.rank;
        board board = location.parent;
        List<int> moves = new();
        List<int> safe = new();
        if (pieceType == "P"){
            if (rank == 7 || rank == 0)
            {
                return new List<int>();
            }
            if (file != 7){
                moves.Add((rank+(1*color))*8+file+1);
            }
            if (file != 1){
            moves.Add((rank+(1*color))*8+file-1);
            }
            foreach (int move in moves)
            {
                if (board.isOccupied(move))
                {
                    if (board.getSquareFromIndex(move).piece.color == -color)
                    {
                        safe.Add(move);
                    }
                }
            }
        }
        else if (pieceType == "R")
        {
            int[][] permutations = 
            {
                new[] {0,1},
                new[] {1,0},
                new[] {-1,0},
                new[] {0,-1}
            };
            safe = permutateCapture(permutations);
        }
        else if (pieceType == "B")
        {
            int[][] permutations = 
            {
                new[] {1,1},
                new[] {1,-1},
                new[] {-1,1},
                new[] {-1,-1}
            };
            safe = permutateCapture(permutations);
        }
        else if (pieceType == "Q")
        {
            int[][] permutations = 
            {
                new[] {1,1},
                new[] {1,-1},
                new[] {-1,1},
                new[] {-1,-1},
                new[] {0,1},
                new[] {1,0},
                new[] {-1,0},
                new[] {0,-1}
            };
            safe = permutateCapture(permutations);
        }
        else if (pieceType == "N")
        {
            int[][] permutations =
            {
                new[] { 2, 1 },
                new[] { 2, -1 },
                new[] { 1, 2 },
                new[] { 1, -2 },
                new[] { -2, 1 },
                new[] { -2, -1 },
                new[] { -1, 2 },
                new[] { -1, -2 }
            };
            foreach (int[] perm in permutations)
            {
                if (file + perm[0] < 0 || file + perm[0] > 7 ||
                rank + perm[1] < 0 || rank + perm[1] > 7) continue;
                if (board.isOccupied(file+perm[0], rank + perm[1]))
                {
                    if (board.getSquareFromCoord(file+perm[0], rank + perm[1]).piece.color == -color){
                        safe.Add(file + perm[0] + (rank + perm[1]) * 8);;
                    }
                }
            }
        }
        return safe;
    }
    public void move(int file, int rank)
    {
        location.piece = null;
        location = location.parent.getSquareFromCoord(file, rank);
        location.piece = this;
        moved = true;
        transform.position = new Vector3(location.transform.position.x, location.transform.position.y, -1);
        transform.parent = location.transform;
        if (pieceType == "P" && (location.rank == 0 || location.rank ==7))
        {
            location.parent.beginPromotionProcess();
        }
        location.parent.lastMoved = this;
    }
    List<int> permutate(int[][] permutations)
    {
        board board = location.parent;
        List<int> moves = new();
        int file = location.file;
        int rank = location.rank;
        int runningFile = file;
        int runningRank = rank;
        foreach (int[] perm in permutations)
        {
            runningFile = file;
            runningRank = rank;
            while (0 <= runningFile && runningFile <= 7 && runningRank >= 0 && runningRank <= 7){
                runningFile += perm[0];
                runningRank += perm[1];
                if (!(0 <= runningFile && runningFile <= 7 && runningRank >= 0 && runningRank <= 7)) break;
                if (board.isOccupied(runningFile, runningRank))break;
                moves.Add(runningFile + (runningRank * 8));
            }
        }
        return moves;
    }
    List<int> permutateCapture(int[][] permutations)
    {
        board board = location.parent;
        List<int> moves = new();
        int file = location.file;
        int rank = location.rank;
        int runningFile = file;
        int runningRank = rank;
        foreach (int[] perm in permutations)
        {
            runningFile = file;
            runningRank = rank;
            while (0 <= runningFile && runningFile <= 7 && runningRank >= 0 && runningRank <= 7){
                runningFile += perm[0];
                runningRank += perm[1];
                if (!(0 <= runningFile && runningFile <= 7 && runningRank >= 0 && runningRank <= 7)) break;
                if (board.isOccupied(runningFile + (runningRank * 8)))
                {
                    if (board.getSquareFromCoord(runningFile, runningRank).piece.color == -color)
                    {
                        moves.Add(runningFile + (runningRank * 8));
                    }
                    break;
                }
            }
        }
        return moves;
    }
}
