using UnityEngine;

public class board : MonoBehaviour
{
    GameObject rank;
    void Start()
    {
        rank = Resources.Load<GameObject>("Prefabs/Rank");
        createBoard();
    }
    void createBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject rnk = createRank();
            rnk.transform.position = new Vector2(0,i);
        }
    }
    GameObject createRank()
    {
        GameObject rnk = Instantiate(rank);
        rnk.transform.parent = transform;
        return rnk;
    }
}
