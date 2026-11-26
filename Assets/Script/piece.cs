using System;
using System.Collections.Generic;
using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
                if (!moved && rank == (color == 1 ? 1 : 6) &&
                    !board.isOccupied((rank+(2*color))*8+file))
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
                if (file + perm[0] < 0 || file + perm[0] > 7 ||
                rank + perm[1] < 0 || rank + perm[1] > 7) continue;
                if (!board.isOccupied(file+perm[0], rank + perm[1]))
                {
                    moves.Add(file+perm[0] + (rank + perm[1])*8);
                }
            }
        }
        else if (pieceType == "K")
        {
            int[][] permutations =
            {
                new[] { 1, 1 },
                new[] { 1, -1 },
                new[] { -1, 1 },
                new[] { -1, -1 },
                new[] { 0, 1 },
                new[] { 1, 0 },
                new[] { -1, 0 },
                new[] { 0, -1 }
            };
            foreach (int[] perm in permutations)
            {
                if (file + perm[0] < 0 || file + perm[0] > 7 ||
                rank + perm[1] < 0 || rank + perm[1] > 7) continue;
                if (!board.isOccupied(file+perm[0], rank + perm[1]))
                {
                    moves.Add(file+perm[0] + (rank + perm[1])*8);
                }
            }
            if (color == 1 || color == -1)
            {
                int homeRank = color == 1 ? 0 : 7;
                if (rank == homeRank)
                {
                    if (canCastle(true) && !moves.Contains(homeRank * 8 + 6))
                    {
                        moves.Add(homeRank * 8 + 6);
                    }
                    if (canCastle(false) && !moves.Contains(homeRank * 8 + 2))
                    {
                        moves.Add(homeRank * 8 + 2);
                    }
                }
            }

        }
        List<int> safe = new List<int>();
        foreach (int mv in moves)
        {
            if (!simulateMove(mv))
            {
                safe.Add(mv);
            }
        }
        return safe;
    }
    public List<int> returnIndexOfTheoreticalCaptures(bool ignoreCheck = false)
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
            if (file < 7){
                moves.Add((rank+(1*color))*8+file+1);
            }
            if (file > 0){
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
            int enPassantRank = rank + color;
            if (board.enPassantTarget >= 0 &&
                enPassantRank >= 0 && enPassantRank <= 7 &&
                board.enPassantTarget / 8 == enPassantRank &&
                Mathf.Abs(board.enPassantTarget % 8 - file) == 1 &&
                !board.isOccupied(board.enPassantTarget))
            {
                square adjacent = board.getSquareFromCoord(board.enPassantTarget % 8, rank);
                if (adjacent.piece != null &&
                    adjacent.piece.pieceType == "P" &&
                    adjacent.piece.color == -color &&
                    board.lastMoved == adjacent.piece)
                {
                    safe.Add(board.enPassantTarget);
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
        else if (pieceType == "K")
        {
            int[][] permutations =
            {
                new[] { 1, 1 },
                new[] { 1, -1 },
                new[] { -1, 1 },
                new[] { -1, -1 },
                new[] { 0, 1 },
                new[] { 1, 0 },
                new[] { -1, 0 },
                new[] { 0, -1 }
            };
            foreach (int[] perm in permutations)
            {
                if (file + perm[0] < 0 || file + perm[0] > 7 ||
                rank + perm[1] < 0 || rank + perm[1] > 7) continue;
                if (board.isOccupied(file+perm[0], rank + perm[1]))
                {
                    if (board.getSquareFromCoord(file+perm[0], rank + perm[1]).piece.color == -color){
                        safe.Add(file+perm[0] + (rank + perm[1])*8);
                    }
                }
            }
        }
        
        if (ignoreCheck) return safe;
        moves = safe;
        safe = new List<int>();
        foreach (int mv in moves)
        {
            if (!simulateMove(mv))
            {
                safe.Add(mv);
            }
        }
        return safe;
    }
    public void move(int file, int rank, bool dontSwitch = false)
    {
        board parentBoard = location.parent;
        square previousLocation = location;
        square destination = parentBoard.getSquareFromCoord(file, rank);
        bool kingSideCastle = pieceType == "K" &&
            rank == (color == 1 ? 0 : 7) && file == 6 && canCastle(true);
        bool queenSideCastle = pieceType == "K" &&
            rank == (color == 1 ? 0 : 7) && file == 2 && canCastle(false);
        piece castlingRook = kingSideCastle
            ? findCastlingRook(1)
            : queenSideCastle ? findCastlingRook(-1) : null;
        piece enPassantCaptured = null;
        square enPassantCapturedSquare = null;
        if (pieceType == "P" && destination.piece == null &&
            file != previousLocation.file &&
            parentBoard.enPassantTarget == rank * 8 + file)
        {
            enPassantCapturedSquare = parentBoard.getSquareFromCoord(file, previousLocation.rank);
            enPassantCaptured = enPassantCapturedSquare.piece;
            if (enPassantCaptured != null &&
                enPassantCaptured.pieceType == "P" &&
                enPassantCaptured.color == -color &&
                parentBoard.lastMoved == enPassantCaptured)
            {
                enPassantCapturedSquare.piece = null;
                Destroy(enPassantCaptured.gameObject);
            }
            else
            {
                enPassantCaptured = null;
                enPassantCapturedSquare = null;
            }
        }
        location.piece = null;
        if (castlingRook != null)
        {
            castlingRook.location.piece = null;
        }
        location.makeNormal();
        location = destination;
        location.piece = this;
        moved = true;
        transform.position = new Vector3(location.transform.position.x, location.transform.position.y, -1);
        transform.parent = location.transform;
        if (castlingRook != null)
        {
            int rookDestinationFile = kingSideCastle ? 5 : 3;
            castlingRook.location = parentBoard.getSquareFromCoord(rookDestinationFile, rank);
            castlingRook.location.piece = castlingRook;
            castlingRook.moved = true;
            castlingRook.transform.position = new Vector3(
                castlingRook.location.transform.position.x,
                castlingRook.location.transform.position.y,
                -1);
            castlingRook.transform.parent = castlingRook.location.transform;
        }
        if (pieceType == "P" && (location.rank == 0 || location.rank ==7))
        {
            location.parent.beginPromotionProcess();
        }
        parentBoard.enPassantTarget = -1;
        if (pieceType == "P" &&
            previousLocation.rank == (color == 1 ? 1 : 6) &&
            Mathf.Abs(rank - previousLocation.rank) == 2)
        {
            parentBoard.enPassantTarget =
                ((rank + previousLocation.rank) / 2) * 8 + file;
        }
        location.parent.lastMoved = this;
        if (dontSwitch) return;
        if (location.parent.settings["returnless"] && pieceType != "P" && previousLocation.piece == null)
        {
            previousLocation.createPieceOnSquare("BLOCK", 9);
        }
        location.parent.switchTurn();
    }
    bool canCastle(bool kingSide)
    {
        int homeRank = color == 1 ? 0 : 7;
        if (pieceType != "K" || moved || location.rank != homeRank) return false;

        piece rook = findCastlingRook(kingSide ? 1 : -1);
        if (rook == null) return false;

        int kingDestinationFile = kingSide ? 6 : 2;
        int rookDestinationFile = kingSide ? 5 : 3;
        if (!isPathClear(location.file, kingDestinationFile, rook)) return false;
        if (!isPathClear(rook.location.file, rookDestinationFile, rook)) return false;

        int step = Math.Sign(kingDestinationFile - location.file);
        int file = location.file;
        while (true)
        {
            if (simulateMove(homeRank * 8 + file)) return false;
            if (file == kingDestinationFile) break;
            file += step;
        }
        return true;
    }
    piece findCastlingRook(int direction)
    {
        int homeRank = color == 1 ? 0 : 7;
        if (location.rank != homeRank) return null;

        for (int file = location.file + direction; file >= 0 && file <= 7; file += direction)
        {
            piece candidate = location.parent.getSquareFromCoord(file, homeRank).piece;
            if (candidate != null &&
                candidate.pieceType == "R" &&
                candidate.color == color &&
                !candidate.moved)
            {
                return candidate;
            }
        }
        return null;
    }
    bool isPathClear(int startFile, int endFile, piece rook)
    {
        int homeRank = color == 1 ? 0 : 7;
        int step = Math.Sign(endFile - startFile);
        int file = startFile;
        while (true)
        {
            piece occupant = location.parent.getSquareFromCoord(file, homeRank).piece;
            if (occupant != null && occupant != this && occupant != rook) return false;
            if (file == endFile) break;
            file += step;
        }
        return true;
    }
    bool simulateMove(int index)
    {
        square stored = location;
        square destination = location.parent.getSquareFromIndex(index);
        square enPassantCapturedSquare = null;
        piece enPassantCaptured = null;

        if (pieceType == "P" &&
            index == location.parent.enPassantTarget &&
            destination.piece == null &&
            index % 8 != location.file)
        {
            enPassantCapturedSquare = location.parent.getSquareFromCoord(
                index % 8, location.rank);

            enPassantCaptured = enPassantCapturedSquare.piece;
            enPassantCapturedSquare.piece = null;
        }

        location.piece = null;
        location = destination;

        piece replaced = location.piece;
        location.piece = this;

        bool legal = location.parent.isCheck(color);

        location.piece = replaced;
        location = stored;
        location.piece = this;

        if (enPassantCapturedSquare != null)
        {
            enPassantCapturedSquare.piece = enPassantCaptured;
        }

        return legal;
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
