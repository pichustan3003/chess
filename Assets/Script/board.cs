using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class board : MonoBehaviour
{
    GameObject rank;
    GameObject square;
    public List<square> squares = new();
    public square currentSelected = null;
    public bool forcePromotion = false;
    public piece lastMoved = null;
    public GameObject promogrid;
    public int turn = 1;

    public void switchTurn()
    {
        turn *= -1;
    }
    void Start()
    {
        rank = Resources.Load<GameObject>("Prefabs/Rank");
        createBoard();
        fillBoardByDefault();
    }
    public bool isCheck(int color)
    {
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
        Debug.Log("Captures at " + plausibleCaptures);
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
        for (int i = 0; i < 8; i++)
        {
            getSquareFromCoord(i,1).createPieceOnSquare("P", 1);
            getSquareFromCoord(i,6).createPieceOnSquare("P", -1);
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
}
