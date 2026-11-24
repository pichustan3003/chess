using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

public class square : MonoBehaviour
{
    public int file, rank;
    GameObject piecePrefab = null;
    GameObject overlayPrefab = null;
    public GameObject overlay;
    public board parent;
    public piece piece = null;
    void Start()
    {
        overlayPrefab = Resources.Load<GameObject>("Prefabs/Overlay");
        overlay = Instantiate(overlayPrefab);
        overlay.transform.parent = transform;
        overlay.transform.position = new Vector3(transform.position.x, transform.position.y, -2);
        overlay.GetComponent<overlay>().parent = this;
    }
    public void color()
    {
        gameObject.GetComponent<SpriteRenderer>().color = (rank+file) % 2 == 0 ? new Color(0.451f, 0.584f, 0.322f) : new Color(0.922f, 0.926f, 0.816f);
    }
    public void RecolourAsSelected()
    {
        gameObject.GetComponent<SpriteRenderer>().color = (rank+file) % 2 == 0 ? new Color(0.7255f, 0.7922f, 0.2588f) : new Color(0.961f, 0.961f, 0.502f);
    }
    public void select()
    {
        foreach (int mv in piece.returnIndexOfTheoreticalMoves())
        {
            parent.displayMoveOnOverlayAtIndex(mv);
        }
        foreach (int mv in piece.returnIndexOfTheoreticalCaptures())
        {
            parent.displayTakeOnOverlayAtIndex(mv);
        }
    }
    public void deselect()
    {
        if (piece == null)
        {
            return;
        }
        foreach (int mv in piece.returnIndexOfTheoreticalMoves())
        {
            parent.ClearOverlayAtIndex(mv);
        }
        foreach (int mv in piece.returnIndexOfTheoreticalCaptures())
        {
            parent.ClearOverlayAtIndex(mv);
        }
    }
    public void createPieceOnSquare(string type, int color = 1)
    {
        piecePrefab = piecePrefab == null ? Resources.Load<GameObject>("Prefabs/piece") : piecePrefab;
        GameObject obj = Instantiate(piecePrefab);
        obj.transform.parent = transform;
        obj.transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        piece objAsP = obj.GetComponent<piece>();
        objAsP.location = this;
        objAsP.pieceType = type;
        objAsP.color = color;
        objAsP.show();
    }
}
