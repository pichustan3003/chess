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
    public void render()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/"+gameObject.name[gameObject.name.Length-1]+board.turn);
    }
}
