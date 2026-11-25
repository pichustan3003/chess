using TMPro;
using UnityEngine;

public class overlay : MonoBehaviour
{
    public square parent;
    public bool yellowd = false;
    public bool moveSq = false;
    public bool captureSq = false;
    void OnMouseDown()
    {
        if (parent.parent.forcePromotion) return;
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
                Destroy(parent.piece.gameObject);
            }
            square current = parent.parent.currentSelected;
            current.overlay.GetComponent<overlay>().deselect();
            current.piece.move(parent.file, parent.rank, parent.parent.settings["atomic"]);
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
                        if (sq.piece.pieceType != "P" || (perm[0] == 0 && perm[1] == 0))
                        {
                            piece explodedPiece = sq.piece;
                            sq.piece = null;
                            Destroy(explodedPiece.gameObject);
                        }
                    }
                }
                parent.parent.switchTurn();
            }
            return;
        }
        if (parent.piece == null)
        {
            parent.parent.nullifySelected();
            return;
        }
        if (parent.piece.color != parent.parent.turn)
        {
            return;
        }
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
