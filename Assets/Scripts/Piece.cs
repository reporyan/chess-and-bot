using UnityEditor;
using UnityEngine;

public enum PieceType
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

public struct Piece
{
    //logical
    public PieceType type;
    public bool isWhite;

    public Piece(PieceType _type, bool _isWhite)
    {
        type = _type;
        isWhite = _isWhite;
    }
}
