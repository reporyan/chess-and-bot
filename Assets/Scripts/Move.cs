using UnityEngine;

public enum MoveType
{
    Fail,
    Move,
    EnPassant,
    QueenSideCastle,
    KingSideCastle,
    Promotion
}

public enum MoveResult
{
    None,
    WhiteCheckmate,
    BlackCheckmate,
    Stalemate,
    Repetition,
    FiftyMoveRule
}

public class MoveReturn
{
    public bool isCheck;
    public bool isCapture;

    public MoveType type;
    public MoveResult result;
    

    public MoveReturn()
    {
        isCheck = false;
        isCapture = false;

        type = MoveType.Move;
        result = MoveResult.None;       
    }

    public MoveReturn(MoveType _type, MoveResult _result)
    {
        isCheck = false;
        isCapture = false;

        type = _type;
        result = _result;
    }
}

public struct Move
{
    public Vector2Int from;
    public Vector2Int to;
    public PieceType? promotion;

    public Move(Vector2Int _from, Vector2Int _to)
    {
        from = _from;
        to = _to;
        promotion = null;
    }
    public void DebugMove()
    {
        Debug.Log("Move From: " + from);
        Debug.Log("Move To: " + to);
    }

    public int YDirection
    {
        get
        {
            if(to.y == from.y)
            {
                return 0;
            }
            else
            {
                return (to.y - from.y) / Mathf.Abs(to.y - from.y);
            }
        }
    }
}
