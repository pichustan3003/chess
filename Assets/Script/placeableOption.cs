using System.ComponentModel.Design;
using TMPro;
using UnityEditor;
using UnityEngine;

public class placeableOption : MonoBehaviour
{
    public int pieceValue;
    public string pieceName;
    public board helper;
    public int countBlack = 0;
    public int countWhite = 0;
    void Start()
    {
        pieceName = gameObject.name;
        render();
    }
    void OnMouseDown()
    {
        helper.summon(pieceName, pieceValue);
    }
    public void render()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/"+pieceName+helper.turn);
        bool canPlace = false;
        if (helper.settings["setup"] && helper.blockAllMovement){
            if (helper.turn == 1)
            {
                canPlace = helper.whiteMaterialRemaining >= pieceValue && !(helper.wKingPlaced && pieceName == "K");
            }
            else if (helper.turn == -1)
            {
                canPlace = helper.blackMaterialRemaining >= pieceValue && !(helper.bKingPlaced && pieceName == "K");
            }
        }
        else if (helper.settings["crazyhouse"])
        {
            TMP_Text txt = gameObject.GetComponentInChildren<TMP_Text>();
            if (helper.turn == 1)
            {
                canPlace = countWhite > 0;
                txt.text = countWhite.ToString();
            }
            else if (helper.turn == -1)
            {
                canPlace = countBlack > 0;
                txt.text = countBlack.ToString();
            }
        }
        gameObject.SetActive(canPlace);
        if (canPlace)
        {
            helper.requestShow();
        }
    }
}
