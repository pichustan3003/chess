using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class board : MonoBehaviour
{
    GameObject rank;
    GameObject square;
    public GameObject menu;

    public List<square> squares = new();
    public square currentSelected = null;
    public bool forcePromotion = false;
    public piece lastMoved = null;
    public int enPassantTarget = -1;
    public GameObject promogrid;
    public int turn = -1;
    public Canvas canvas;
    public TMP_Text winner;
    [SerializeField]
    public Dictionary<string, bool> settings = new();
    public TMP_Text titleTxt;
    public bool possibleCapture = false;

    public int checkCountWhite = 0;
    public int checkCountBlack = 0;

    void Start()
    {
        readCheckboxes();
    }
    public void switchTurn()
    {
        possibleCapture = false;
        turn *= -1;
        if (isCheck(turn))
        {
            checkCountBlack += turn == 1 ? 1 : 0;
            checkCountWhite += turn == -1 ? 1 : 0;
            piece king = null;
            foreach (square sq in squares)
            {
                if (sq.piece != null)
                {
                    if (sq.piece.pieceType == "K" && sq.piece.color == turn)
                    {
                        king = sq.piece;
                        break;
                    }
                }
            }
            if (king != null) king.location.makeRed();
            if (IsCheckmate(turn))
            {
                Debug.Log("Checkmate against "+turn);
                winner.text = turn == -1 ? "WHITE WINS" : "BLACK WINS";
                winProtocol();
                winner.gameObject.SetActive(true);
            }
        }
        else
        {
            piece king = null;
            foreach (square sq in squares)
            {
                if (sq.piece != null)
                {
                    if (sq.piece.pieceType == "K" && sq.piece.color == -turn)
                    {
                        king = sq.piece;
                        break;
                    }
                }
            }

            if (king != null) king.location.makeNormal();
            if (IsStalemate(turn))
            {
                winner.text = "Stalemate";
                winProtocol();
                if (settings["giveaway"])
                {
                    winner.text = turn == -1 ? "WHITE WINS" : "BLACK WINS";
                    winProtocol();
                }
                winner.gameObject.SetActive(true);
            }
        }
        if (settings["giveaway"])
        {
            foreach (square sq in squares)
            {
                if (sq.piece != null)
                {
                    if (sq.piece.color == turn)
                    {
                        if (sq.piece.returnIndexOfTheoreticalCaptures().Count != 0)
                        {
                            possibleCapture = true;
                        }
                    }
                }
            }
        }
        if (settings["3check"]){
            if (checkCountWhite >= 3)
            {
                winner.text = "WHITE WINS";
                winner.gameObject.SetActive(true);
                winProtocol();
            }
            else if (checkCountBlack >= 3)
            {
                winner.text = "BLACK WINS";
                winner.gameObject.SetActive(true);
                winProtocol();
            }
        }
        if (settings["atomic"] || settings["chain"] || settings["horde"]) atomicWin();  
    }
    void atomicWin()
    {
        bool hordeBypass = settings["horde"];
        if (settings["giveaway"]) return;
        piece Wking = null;
        foreach (square sq in squares)
        {
            if (sq.piece != null)
            {
                if ((sq.piece.pieceType == "K" && sq.piece.color == 1) || (hordeBypass && sq.piece.pieceType == "P" && sq.piece.color == 1))
                {
                    Wking = sq.piece;
                    break;
                }
            }
        }
        piece Bking = null;
        foreach (square sq in squares)
        {
            if (sq.piece != null)
            {
                if (sq.piece.pieceType == "K" && sq.piece.color == -1)
                {
                    Bking = sq.piece;
                    break;
                }
            }
        }
        if (Wking == null && Bking == null)
        {
            winner.text = "DRAW";
        }
        else if (Bking == null)
        {
            winner.text = "WHITE WINS";
        }
        else if (Wking == null)
        {
            winner.text = "BLACK WINS";
        }
        else
        {
            return;
        }
        winner.gameObject.SetActive(true);
        winProtocol();
    }
    void winProtocol()
    {
        turn = 0;
    }
    public void readCheckboxes()
    {
        List<string> condtradicts = new ();
        foreach (Toggle check in canvas.GetComponentsInChildren<Toggle>())
        {
            settings[check.gameObject.name] = check.isOn;
            if (check.isOn)
            {
                condtradicts.AddRange(check.gameObject.GetComponent<modeCheck>().contradicts);
            }
        }
        string runningTitle = "";
        foreach (String key in settings.Keys)
        {
            if (settings[key])
            {
                runningTitle += key + " ";
            }
        }
        runningTitle += "Chess";
        titleTxt.text = runningTitle;
        foreach (Toggle check in canvas.GetComponentsInChildren<Toggle>())
        {
            if (condtradicts.Contains(check.gameObject.name))
            {
                check.enabled = false;
                ColorBlock newColors = check.colors;
                newColors.normalColor = new Color(0.5f, 0.5f, 0.5f);
                check.colors = newColors;
            }
            else
            {
                check.enabled = true;
                ColorBlock newColors = check.colors;
                newColors.normalColor = new Color(1,1,1);
                check.colors = newColors;
            }
        }
    }
    public void beginGame()
    {
        rank = Resources.Load<GameObject>("Prefabs/Rank");
        menu.SetActive(false);
        createBoard();
        fillBoardByDefault();
        switchTurn();
    }
    public bool IsCheckmate(int color)
    {
        if (!isCheck(color)) return false;
        foreach (square sq in squares)
        {
            if (sq.piece != null)
            {
                if (sq.piece.color == -color) continue;
                List<int> theoryMoves = new();
                theoryMoves.AddRange(sq.piece.returnIndexOfTheoreticalCaptures());
                theoryMoves.AddRange(sq.piece.returnIndexOfTheoreticalMoves());
                if (theoryMoves.Count != 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public bool IsStalemate(int color)
    {
        if (isCheck(color)) return false;
        foreach (square sq in squares)
        {
            if (sq.piece != null)
            {
                if (sq.piece.color == -color) continue;
                List<int> theoryMoves = new();
                theoryMoves.AddRange(sq.piece.returnIndexOfTheoreticalCaptures());
                theoryMoves.AddRange(sq.piece.returnIndexOfTheoreticalMoves());
                if (theoryMoves.Count != 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public bool isCheck(int color)
    {
        if (settings["giveaway"]) return false;
        List<int> plausibleCaptures = new();
        color = -color;
        foreach (square sq in squares)
        {
            if (sq.piece != null)
            {
                if (sq.piece.color == color)
                {
                    plausibleCaptures.AddRange(sq.piece.returnIndexOfTheoreticalCaptures(true));
                }
            }
        }
        foreach (int c in plausibleCaptures)
        {
            if (squares[c].piece == null) continue;
            if (squares[c].piece.pieceType == "K" && squares[c].piece.color == -color)
            {
                return true;
            }
        }
        return false;
    }
    public void handlePromotion(string piece)
    {
        if (forcePromotion && (lastMoved.location.rank == 0 || lastMoved.location.rank == 7) && lastMoved.pieceType == "P")
        {
            lastMoved.pieceType = piece;
            lastMoved.show();
            forcePromotion = false;
            promogrid.SetActive(false);
        }
    }

    public void beginPromotionProcess()
    {
        forcePromotion = true;
        promogrid.SetActive(true);
    }
    void createBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject rnk = createRank(i);
            rnk.transform.position = new Vector2(0,i*1.2475f);
        }
        transform.position = new Vector2(-3,-7);
    }
    public void nullifySelected()
    {
        if (currentSelected != null)
        {
            currentSelected.overlay.GetComponent<overlay>().deselect();
        }
        currentSelected = null;
    }
    public void setSelected(square sel)
    {
        if (currentSelected != null)
        {
            currentSelected.overlay.GetComponent<overlay>().deselect();
        }
        currentSelected = sel;
    }
    void fillBoardByDefault()
    {
        if (settings["960"])
        {
            fischerSetup();
            for (int i = 0; i < 8; i++)
            {
                getSquareFromCoord(i,1).createPieceOnSquare("P", 1);
                getSquareFromCoord(i,6).createPieceOnSquare("P", -1);
            }
            return;
        }
        for (int i = 0; i < 8; i++)
        {
            getSquareFromCoord(i,1).createPieceOnSquare("P", 1);
            getSquareFromCoord(i,6).createPieceOnSquare("P", -1);
        }
        if (settings["horde"])
        {
            for (int i = 0; i < 8; i++)
            {
                getSquareFromCoord(i,0).createPieceOnSquare("P", 1);
                getSquareFromCoord(i,2).createPieceOnSquare("P", 1);
                getSquareFromCoord(i,3).createPieceOnSquare("P", 1);
            }
            getSquareFromCoord(1,4).createPieceOnSquare("P", 1);
            getSquareFromCoord(2,4).createPieceOnSquare("P", 1);
            getSquareFromCoord(5,4).createPieceOnSquare("P", 1);
            getSquareFromCoord(6,4).createPieceOnSquare("P", 1);
            getSquareFromCoord(0,7).createPieceOnSquare("R",-1);
            getSquareFromCoord(7,7).createPieceOnSquare("R",-1);
            getSquareFromCoord(1,7).createPieceOnSquare("N",-1);
            getSquareFromCoord(6,7).createPieceOnSquare("N",-1);
            getSquareFromCoord(2,7).createPieceOnSquare("B",-1);
            getSquareFromCoord(5,7).createPieceOnSquare("B",-1);
            getSquareFromCoord(3,7).createPieceOnSquare("Q",-1);
            getSquareFromCoord(4,7).createPieceOnSquare("K",-1);
            return;
        }
        // rooks

        getSquareFromCoord(0,0).createPieceOnSquare("R",1);
        getSquareFromCoord(7,0).createPieceOnSquare("R",1);
        getSquareFromCoord(0,7).createPieceOnSquare("R",-1);
        getSquareFromCoord(7,7).createPieceOnSquare("R",-1);

        // knights

        getSquareFromCoord(1,0).createPieceOnSquare("N",1);
        getSquareFromCoord(6,0).createPieceOnSquare("N",1);
        getSquareFromCoord(1,7).createPieceOnSquare("N",-1);
        getSquareFromCoord(6,7).createPieceOnSquare("N",-1);

        // bishops

        getSquareFromCoord(2,0).createPieceOnSquare("B",1);
        getSquareFromCoord(5,0).createPieceOnSquare("B",1);
        getSquareFromCoord(2,7).createPieceOnSquare("B",-1);
        getSquareFromCoord(5,7).createPieceOnSquare("B",-1);

        // royals

        getSquareFromCoord(3,0).createPieceOnSquare("Q",1);
        getSquareFromCoord(4,0).createPieceOnSquare("K",1);
        getSquareFromCoord(3,7).createPieceOnSquare("Q",-1);
        getSquareFromCoord(4,7).createPieceOnSquare("K",-1);
    }
    GameObject createRank(int rankNum)
    {
        GameObject rnk = Instantiate(rank);
        rnk.transform.parent = transform;
        square = Resources.Load<GameObject>("Prefabs/BoardPiece");
        for (int i=0; i < 8; i++)
        {
            GameObject obj = Instantiate(square);
            obj.transform.position = new Vector2(i*1.2475f,0);
            obj.transform.parent = rnk.transform;
            obj.GetComponent<square>().file = i;
            obj.GetComponent<square>().rank = rankNum;
            squares.Add(obj.GetComponent<square>());
            obj.GetComponent<square>().color();
            obj.GetComponent<square>().parent = this;
        }
        return rnk;
    }

    public square getSquareFromCoord(int file, int rank)
    {
        return squares[rank*8+file];
    }
    public square getSquareFromIndex(int index)
    {
        return squares[index];
    }
    public void displayMoveOnOverlayAtIndex(int index)
    {
        if (settings["giveaway"] && possibleCapture)
        {
            return;
        }
        squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/move");
        squares[index].overlay.GetComponent<overlay>().moveSq = true;
    }

    public void displayTakeOnOverlayAtIndex(int index)
    {
        squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/capture");
        squares[index].overlay.GetComponent<overlay>().captureSq = true;
    }

    public void ClearOverlayAtIndex(int index)
    {
        squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/blank");
        squares[index].overlay.GetComponent<overlay>().moveSq = false;
        squares[index].overlay.GetComponent<overlay>().captureSq = false;
    }
    public bool isOccupied(int file, int rank)
    {
        return squares[rank*8+file].piece != null;
    }
    public bool isOccupied(int index)
    {
        return squares[index].piece != null;
    }
    void fischerSetup()
    {
        List<string> bag = new List<string>()
        {
            "K","Q","R","R","B","B","N","N"
        };
        List<string> order = new List<string>();
        while (bag.Count != 0)
        {
            int chosenInt = pickRandomIndexFromList(bag);
            string chosen = bag[chosenInt];
            if (chosen == "K" && countOccurances(bag, "R") == 2) continue;
            if (chosen == "R" && countOccurances(bag, "R") == 1 && countOccurances(bag, "K") == 1) continue;
            if (chosen == "B" && countOccurances(bag, "B") == 1)
            {
                if (order.IndexOf("B") % 2 == bag.Count % 2) continue;
            }
            order.Add(chosen);
            bag.RemoveAt(chosenInt);
        }
        int indx = 0;
        foreach (string piece in order)
        {
            getSquareFromCoord(indx, 0).createPieceOnSquare(piece, 1);
            getSquareFromCoord(indx, 7).createPieceOnSquare(piece, -1);
            indx++;
        }
    }
    int pickRandomIndexFromList(List<string> list)
    {
        return UnityEngine.Random.Range(0,list.Count);
    }
    int countOccurances(List<string> list, string match)
    {
        int count = 0;
        foreach (string str in list)
        {
            if (str == match)
            {
                count ++;
            }
        }
        return count;
    }
}
