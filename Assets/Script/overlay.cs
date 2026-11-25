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
            Destroy(parent.piece.gameObject);
            square current = parent.parent.currentSelected;
            current.overlay.GetComponent<overlay>().deselect();
            current.piece.move(parent.file, parent.rank);
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
