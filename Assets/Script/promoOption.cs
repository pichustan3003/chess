using UnityEngine;

public class promoOption : MonoBehaviour
{
    public GameObject parent;
    public string piece;
    public board board;

    void OnMouseDown()
    {
        board.handlePromotion(piece);
    }
}
