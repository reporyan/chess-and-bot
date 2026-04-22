using UnityEngine;

public class PhysicalPiece : MonoBehaviour
{
    public Vector2Int square;

    public PieceType type;
    public bool isWhite;

    public Transform trans;

    //lerp
    public Vector3 targetPos;
    public float moveSpeed;
    public float stoppingDistance;

    void Update()
    {
        if(trans.position != targetPos)
        {
            trans.position = Vector3.Lerp(trans.position, targetPos, moveSpeed * Time.deltaTime);

            if(Vector3.Distance(targetPos, trans.position) <= stoppingDistance)
            {
                trans.position = targetPos;
            }
        }
    }

    public void SetPosition(Vector2Int _square)
    {
        square = _square;
        targetPos = new Vector3(_square.x, 0, _square.y) * 10;
    }   
}
