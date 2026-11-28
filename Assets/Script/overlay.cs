using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class overlay : MonoBehaviour
{
    public square parent;
    public bool yellowd = false;
    public bool moveSq = false;
    public bool captureSq = false;
    public bool duckSquare = false;
    public bool samSquare = false;
    public bool placementSquare = false;
    void OnMouseDown()
    {
        if (parent.parent.forcePromotion) return;
        if (placementSquare)
        {
            parent.parent.catchPlacement(this);
        }
        else
        {
            parent.parent.clearAllPlacement();
        }
        if (duckSquare)
        {
            parent.createPieceOnSquare("Duck", 2);
            parent.parent.ClearDuckOverlays();
            parent.parent.lastMoved = parent.piece;
            parent.parent.switchTurn();
        }
        if (samSquare)
        {
            parent.createPieceOnSquare("Sam", 2);
            parent.parent.ClearSamOverlays();
            parent.parent.lastMoved = parent.piece;
            parent.parent.switchTurn();
        }
        if (parent.parent.blockAllMovement) return;
        if (moveSq)
        {
            square current = parent.parent.currentSelected;
            current.overlay.GetComponent<overlay>().deselect();
            current.piece.move(parent.file, parent.rank);
            return;
        }
        if (captureSq)
        {
            if (parent.piece != null)
            {
                if (parent.parent.settings["crazyhouse"])
                {
                    string capturedPieceType = parent.piece.pieceType;
                    int capturedPieceColor = parent.piece.color;
                    placeableOption pocketOption = null;
                    foreach (placeableOption obj in parent.parent.placementBack.GetComponentsInChildren<placeableOption>(true))
                    {
                        if (obj.gameObject.name == capturedPieceType)
                        {
                            pocketOption = obj;
                            break;
                        }
                    }
                    if (pocketOption == null)
                    {
                        Debug.LogWarning($"No Crazyhouse pocket option found for captured piece '{capturedPieceType}'.", parent.parent.placementBack);
                    }
                    else if (capturedPieceColor == -1)
                    {
                        pocketOption.countWhite += 1;
                    }
                    else if (capturedPieceColor == 1)
                    {
                        pocketOption.countBlack += 1;
                    }
                }
                Destroy(parent.piece.gameObject);
            }
            string destroyed = parent.piece != null ? parent.piece.pieceType : null;
            square current = parent.parent.currentSelected;
            current.overlay.GetComponent<overlay>().deselect();
            bool dSwitch = false;
            bool adSwitch = false;
            if (parent.parent.settings["atomic"])
            {
                adSwitch = true;
            }
            if (parent.parent.settings["chain"] && destroyed != "P" && destroyed != null)
            {
                dSwitch = true;
            }
            current.piece.move(parent.file, parent.rank, dSwitch || adSwitch);
            if (parent.parent.settings["atomic"])
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
                    new[] { 0, -1 },
                    new[] { 0 , 0 }
                };
                foreach (int[] perm in permutations)
                {
                    if (parent.file + perm[0] > 7 || parent.file + perm[0] < 0 || parent.rank + perm[1] > 7 || parent.rank + perm[1] < 0) continue;
                    square sq = parent.parent.getSquareFromCoord(parent.file + perm[0], parent.rank + perm[1]);
                    if (sq.piece != null)
                    {
                        if (sq.piece.pieceType != "P" || sq.piece.pieceType != "Duck" || sq.piece.pieceType != "Sam" || (perm[0] == 0 && perm[1] == 0))
                        {
                            piece explodedPiece = sq.piece;
                            sq.piece = null;
                            Destroy(explodedPiece.gameObject);
                        }
                    }
                }
                if (!dSwitch){
                    parent.parent.switchTurn();
                }
            }
            return;
        }
        if (parent.piece == null)
        {
            parent.parent.nullifySelected();
            return;
        }
        if (parent.piece.color != parent.parent.turn && parent.piece.color != 2)
        {
            return;
        }
        if ((parent.parent.settings["duck"] || parent.parent.settings["sam"]) && parent.parent.requireDuckMove && parent.piece.pieceType != "Duck" && parent.piece.pieceType != "Sam") return;
        if ((parent.parent.settings["duck"] || parent.parent.settings["sam"]) && !parent.parent.requireDuckMove && (parent.piece.pieceType == "Duck" || parent.piece.pieceType == "Sam")) return;
        if (!yellowd)
        {
            parent.parent.setSelected(parent);
            parent.RecolourAsSelected();
            parent.select();
            yellowd = true;
        }
        else
        {
            parent.parent.nullifySelected();
        }
    }
    public void deselect()
    {
        parent.color();
        yellowd = false;
        parent.deselect();
    }
}
