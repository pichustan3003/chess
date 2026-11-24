using UnityEngine;

public class rank : MonoBehaviour
{
    GameObject square;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        square = Resources.Load<GameObject>("Prefabs/BoardPiece");
        for (int i=0; i < 8; i++)
        {
            GameObject obj = Instantiate(square);
            obj.transform.position = new Vector2(i,0);
            obj.transform.parent = transform;
        }
    }
}
