using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using TMPro;
using Unity.Android.Gradle;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SettingsManagement;
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
    public piece checkersFocal = null;
    public bool requireDuckMove = false;
    public bool blockAllMovement = false;
    public int whiteMaterialRemaining = 39;
    public int blackMaterialRemaining = 39;
    public int costOfPlacingPiece;
    public string typeOfPlacedPiece = null;
    public GameObject placementBack;
    public bool wKingPlaced = false;
    public bool bKingPlaced = false;
    void Start()
    {
        readCheckboxes();
    }
    public void switchTurn()
    {
        if (lastMoved){
            if (settings["duck"] && lastMoved.pieceType != "Duck")
            {
                requireDuckMove = true;
                bool duckFound = false;
                foreach (square sq in squares)
                {
                    if (sq.piece != null)
                    {
                        if (sq.piece.pieceType == "Duck") {duckFound = true; break;}
                    }
                }
                if (!duckFound)
                {
                    requireDuckMove = false;
                    foreach (square sq in squares)
                    {
                        if (sq.piece == null)
                        {
                            displayDuckOnOverlayAtIndex(sq.rank*8 + sq.file);
                        }
                    }
                }
                return;
            }
            else
            {
                requireDuckMove = false;
            }
            if (settings["sam"] && lastMoved.pieceType != "Sam")
            {
                requireDuckMove = true;
                bool duckFound = false;
                foreach (square sq in squares)
                {
                    if (sq.piece != null)
                    {
                        if (sq.piece.pieceType == "Sam") {duckFound = true; break;}
                    }
                }
                if (!duckFound)
                {
                    requireDuckMove = false;
                    foreach (square sq in squares)
                    {
                        if (sq.piece == null)
                        {
                            displaySamOnOverlayAtIndex(sq.rank*8 + sq.file);
                        }
                    }
                }
                return;
            }
            else
            {
                requireDuckMove = false;
            }
        }
        possibleCapture = false;
        turn *= -1;
        foreach (placeableOption place in placementBack.GetComponentsInChildren<placeableOption>(true))
        {
            place.render();
        }
        if (blockAllMovement)
        {
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
            }
            if (turn == 1)
            {
                if (wKingPlaced && whiteMaterialRemaining == 0)
                {
                    switchTurn();
                    foreach (placeableOption place in placementBack.GetComponentsInChildren<placeableOption>(true))
                    {
                        place.render();
                    }
                }
            }
            else if (turn == -1)
            {
                if (bKingPlaced && blackMaterialRemaining == 0)
                {
                    switchTurn();
                    foreach (placeableOption place in placementBack.GetComponentsInChildren<placeableOption>(true))
                    {
                        place.render();
                    }
                }
            }
            return;
        }
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
            if (insufficentMaterial() && !settings["giveaway"] && !settings["horde"])
            {
                winner.text = "Insufficent Material";
                winProtocol();
            }
        }
        if (settings["giveaway"] || settings["checkers"])
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
        if (settings["atomic"] || settings["chain"] || settings["horde"] || settings["checkers"] || settings["duck"] || settings["sam"]) atomicWin();  
    }
    void atomicWin()
    {
        bool hordeBypass = settings["horde"];
        bool checkersBypass = settings["checkers"];
        if (settings["giveaway"]) return;
        piece Wking = null;
        foreach (square sq in squares)
        {
            if (sq.piece != null)
            {
                if ((sq.piece.pieceType == "K" && sq.piece.color == 1) || (hordeBypass && sq.piece.pieceType == "P" && sq.piece.color == 1) || (checkersBypass & sq.piece.pieceType == "C" && sq.piece.color == 1) || (checkersBypass & sq.piece.pieceType == "KC" && sq.piece.color == -1))
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
                if ((sq.piece.pieceType == "K" && sq.piece.color == -1) || (checkersBypass & sq.piece.pieceType == "C" && sq.piece.color == -1) || (checkersBypass & sq.piece.pieceType == "KC" && sq.piece.color == -1))
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
    bool insufficentMaterial()
    {
        List<piece> allP = new();
        List<string> mateble = new List<string>(){"P","Q","R","C"};
        foreach (square sq in squares)
        {
            if (sq.piece != null)
            {
                allP.Add(sq.piece);
                if (mateble.Contains(sq.piece.pieceType))
                {
                    return false;
                }
            }
        }
        if (settings["3check"] && allP.Count > 2)
        {
            return false;
        }
        if (allP.Count <= 3)
        {
            return true;
        }
        int bCol = 7;
        foreach (piece p in allP)
        {
            if (p.pieceType == "B")
            {
                if (bCol != 7 && allP.Count < 5 && bCol==(p.location.rank + p.location.file) % 2)
                {
                    return true;
                }
                bCol = (p.location.rank + p.location.file) % 2;
            }
        }
        return true;
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
        if (settings["checkers"]){turn = 1;}
        switchTurn();
    }
    public bool IsCheckmate(int color)
    {
        if (!isCheck(color)) return false;
        if (settings["crazyhouse"])
        {
            bool canCrazy = false;
            bool onlyPawn = true;
            foreach (placeableOption place in placementBack.GetComponentsInChildren<placeableOption>())
            {
                if (color == 1)
                {
                    if (place.countWhite > 0)
                    {
                        canCrazy = true;
                        if (place.pieceName != "P") onlyPawn = false;
                    }
                }
                else if (color == -1)
                {
                    if (place.countBlack > 0)
                    {
                        canCrazy = true;
                        if (place.pieceName != "P") onlyPawn = false;
                    }
                }
            }

            if (canCrazy)
            {
                for (int r = 0; r < 8; r ++)
                {
                    for (int f = 0; f < 8; f++)
                    {
                        if (!isOccupied(f, r))
                        {
                            if (onlyPawn && (r == 0 || r == 7)) continue;
                            square test = getSquareFromCoord(f,r);
                            test.createPieceOnSquare("blank", turn);
                            if (!isCheck(turn))
                            {
                                DestroyImmediate(test.piece.gameObject);
                                return false;
                            }
                            DestroyImmediate(test.piece.gameObject);
                        }
                    }
                }
            }
        }
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
    public void startSetupChess()
    {
        blockAllMovement = true;
        placementBack.SetActive(true);
    }

    public void requestShow()
    {
        placementBack.SetActive(true);
    }
    public void summon(string pieceName, int cost)
    {
        if (settings["setup"] && blockAllMovement)
        {
            placementOfSetup(pieceName, cost);
            return;
        }
        if (settings["crazyhouse"])
        {
            placementOfCrazy(pieceName, cost);
        }
    }

    public void clearAllPlacement()
    {
        for (int r = 0; r <8; r ++)
        {
            for (int f = 0; f < 8; f++)
            {

                clearPlacementOverlayAtIndex(r*8 + f);

            }
        }
    }
    public void placementOfSetup(string pieceName, int cost)
    {
        clearAllPlacement();
        for (int r = turn == 1 ? 0 : 7; r > 4 || r < 3; r += turn)
        {
            for (int f = 0; f < 8; f++)
            {
                if (!isOccupied(f, r))
                {
                    square test = getSquareFromCoord(f,r);
                    test.createPieceOnSquare("blank", turn);
                    if (!isCheck(turn))
                    {
                        displayPlacementOverlayAtIndex(r*8 + f);
                    }
                    DestroyImmediate(test.piece.gameObject);
                }
            }
        }
        typeOfPlacedPiece = pieceName;
        costOfPlacingPiece = cost;
    }

    public void placementOfCrazy(string pieceName, int cost)
    {
        clearAllPlacement();
        for (int r = 0; r < 8; r ++)
        {
            for (int f = 0; f < 8; f++)
            {
                if (!isOccupied(f, r))
                {
                    if (pieceName == "P" && (r == 0 || r == 7)) continue;
                    square test = getSquareFromCoord(f,r);
                    test.createPieceOnSquare("blank", turn);
                    if (!isCheck(turn))
                    {
                        displayPlacementOverlayAtIndex(r*8 + f);
                    }
                    DestroyImmediate(test.piece.gameObject);
                }
            }
        }
        typeOfPlacedPiece = pieceName;
        costOfPlacingPiece = cost;
    }
    public bool IsStalemate(int color)
    {
        if (isCheck(color)) return false;
        if (settings["crazyhouse"])
        {
            foreach (placeableOption place in placementBack.GetComponentsInChildren<placeableOption>())
            {
                if (color == 1)
                {
                    if (place.countWhite > 0)
                    {
                        return false;
                    }
                }
                else if (color == -1)
                {
                    if (place.countBlack > 0)
                    {
                        return false;
                    }
                }
            }
        }
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
        if (settings["giveaway"] || settings["duck"] || settings["sam"]) return false;
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
            }
            promogrid.SetActive(false);
        }
    }
    public void beginPromotionProcess()
    {
        forcePromotion = true;
        foreach (promoOption promo in promogrid.GetComponentsInChildren<promoOption>())
        {
            promo.render();
        }
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
        if (settings["setup"])
        {
            startSetupChess();
            return;
        }
        if (settings["checkers"])
        {
            for (int r = 0; r < 3 ; r++)
            {
                for (int f = 0; f < 8; f++)
                {
                    if ((r + f) % 2 == 0)
                    {
                        getSquareFromCoord(f,r).createPieceOnSquare("C", 1);
                    }
                }
            }
            for (int r = 7; r > 4 ; r--)
            {
                for (int f = 0; f < 8; f++)
                {
                    if ((r + f) % 2 == 0)
                    {
                        getSquareFromCoord(f,r).createPieceOnSquare("C", -1);
                    }
                }
            }
            return;
        }
        if (settings["random"])
        {
            completeRandomGenerator();
            return;
        }
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
        if ((settings["giveaway"] || settings["checkers"]) && possibleCapture)
        {
            return;
        }
        if (!settings["blind"]){
            squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/move");
        }
        squares[index].overlay.GetComponent<overlay>().moveSq = true;
    }

    public void displayTakeOnOverlayAtIndex(int index)
    {
        if (!settings["blind"]){
            squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/capture");
        }
        squares[index].overlay.GetComponent<overlay>().captureSq = true;
    }
    public void completeRandomGenerator()
    {
        List<string> bag = new List<string>()
        {
            "K","Q","R","R","B","B","N","N","P","P","P","P","P","P","P","P"
        };
        List<string> final = new();
        while (bag.Count > 0)
        {
            int rand = pickRandomIndexFromList(bag);
            final.Add(bag[rand]);
            bag.RemoveAt(rand);
            if (!bag.Contains("K"))
            {
                if (final.IndexOf("K") > 7)
                {
                    bag = new List<string>()
                    {
                        "K","Q","R","R","B","B","N","N","P","P","P","P","P","P","P","P"
                    };
                    final = new();
                }
            }
        }
        int count = 0;
        foreach (string p in final)
        {
            getSquareFromIndex(count).createPieceOnSquare(p,1);
            count++;
        }
        bag = new List<string>()
        {
            "K","Q","R","R","B","B","N","N","P","P","P","P","P","P","P","P"
        };
        final = new();
        while (bag.Count > 0)
        {
            int rand = pickRandomIndexFromList(bag);
            final.Add(bag[rand]);
            bag.RemoveAt(rand);
            if (!bag.Contains("K"))
            {
                if (final.IndexOf("K") > 7)
                {
                    bag = new List<string>()
                    {
                        "K","Q","R","R","B","B","N","N","P","P","P","P","P","P","P","P"
                    };
                    final = new();
                }
            }
        }
        count = 63;
        foreach (string p in final)
        {
            getSquareFromIndex(count).createPieceOnSquare(p,-1);
            count--;
        }
        return;
    }
    public void displayDuckOnOverlayAtIndex(int index)
    {
        if (!settings["blind"]){
            squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/duckOverlay");
        }
        squares[index].overlay.GetComponent<overlay>().duckSquare = true;
    }

    public void catchPlacement(overlay point)
    {
        if (settings["setup"] && blockAllMovement && typeOfPlacedPiece != null)
        {
            if (typeOfPlacedPiece == "K" && (turn == 1 ? wKingPlaced : bKingPlaced))
            {
                return;
            }
            else if (typeOfPlacedPiece == "K")
            {
                if (turn == 1)
                {
                    wKingPlaced = true;
                }
                else if (turn == -1)
                {
                    bKingPlaced = true;
                }
            }
            point.parent.createPieceOnSquare(typeOfPlacedPiece, turn);
            clearAllPlacement();
            if (turn == 1)
            {
                whiteMaterialRemaining -= costOfPlacingPiece;
            }
            else if (turn == -1)
            {
                blackMaterialRemaining -= costOfPlacingPiece;
            }
            if (whiteMaterialRemaining == 0 && blackMaterialRemaining == 0 && bKingPlaced && wKingPlaced)
            {
                blockAllMovement = false;
                placementBack.SetActive(false);
            }
            switchTurn();
        }

        else if (settings["crazyhouse"] && typeOfPlacedPiece != null)
        {
            placeableOption counter = null;
            foreach (placeableOption place in placementBack.GetComponentsInChildren<placeableOption>())
            {
                if (typeOfPlacedPiece == place.pieceName)
                {
                    counter = place;
                    break;
                }
            }
            if (counter == null)
            {
                return;
            }
            point.parent.createPieceOnSquare(typeOfPlacedPiece, turn);
            clearAllPlacement();
            if (turn == 1)
            {
                counter.countWhite -= 1;
            }
            else if (turn == -1)
            {
                counter.countBlack -= 1;
            }
            placementBack.SetActive(false);
            switchTurn();
        }
    }

    public void displaySamOnOverlayAtIndex(int index)
    {
        if (!settings["blind"]){
            squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/move");
        }
        squares[index].overlay.GetComponent<overlay>().samSquare = true;
    }

    public void displayPlacementOverlayAtIndex(int index)
    {
        squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/move");
        squares[index].overlay.GetComponent<overlay>().placementSquare = true;
    }

    public void clearPlacementOverlayAtIndex(int index)
    {
        squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/blank");
        squares[index].overlay.GetComponent<overlay>().placementSquare = false;
    }

    public void ClearOverlayAtIndex(int index)
    {
        squares[index].overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/blank");
        squares[index].overlay.GetComponent<overlay>().moveSq = false;
        squares[index].overlay.GetComponent<overlay>().captureSq = false;
        squares[index].overlay.GetComponent<overlay>().duckSquare = false;
        squares[index].overlay.GetComponent<overlay>().samSquare = false;
    }
    public void ClearDuckOverlays()
    {
        Sprite duckOvers = Resources.Load<Sprite>("Pieces/duckOverlay");
        foreach (square sq in squares)
        {
            if (sq.overlay.GetComponent<SpriteRenderer>().sprite == duckOvers)
            {
                ClearOverlayAtIndex(sq.rank*8 + sq.file);
            }
        }
    }

    public void ClearSamOverlays()
    {
        Sprite duckOvers = Resources.Load<Sprite>("Pieces/move");
        foreach (square sq in squares)
        {
            if (sq.overlay.GetComponent<SpriteRenderer>().sprite == duckOvers && sq.overlay.GetComponent<overlay>().samSquare)
            {
                ClearOverlayAtIndex(sq.rank*8 + sq.file);
            }
        }
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
