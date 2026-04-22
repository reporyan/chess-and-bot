using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public class BoardState
{
    //initialised by other classes like game, probably change this later

    //pieces on board
    public Piece?[,] board = new Piece?[8,8];

    //turn
    public bool whiteToMove;

    //en passant
    public Vector2Int? enPassant;

    //tracking checks
    public Vector2Int whiteKingPos;
    public Vector2Int blackKingPos;

    //castling
    public bool hasWhiteKingMoved;
    public bool hasBlackKingMoved;
    public bool hasWhiteQueensideRookMoved;
    public bool hasBlackQueensideRookMoved;
    public bool hasWhiteKingsideRookMoved;
    public bool hasBlackKingsideRookMoved;

    //repetition
    public List<BoardState> pastBoardStates = new List<BoardState>();
    public List<int> pastBoardStatesCount = new List<int>();

    //50 move rule
    public int fiftyMoveRule = 0;

    //
    public bool isInProgress = true;

    //debug
    public static string[] whitePieceLetter = new string[]
    {
        "P",
        "N",
        "B",
        "R",
        "Q",
        "K"
    };

    public static string[] blackPieceLetter = new string[]
    {
        " p",
        " n",
        " b",
        " r",
        " q",
        " k"
    };

    public BoardState Clone()//need to make new function that deep copies past board states for bot, use clone function inside it
    {
        BoardState clone = new BoardState();

        for(int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                clone.board[x, y] = board[x, y];
            }
        }

        clone.whiteToMove = whiteToMove;
        clone.enPassant = enPassant;

        clone.whiteKingPos = whiteKingPos;
        clone.blackKingPos = blackKingPos;

        clone.hasWhiteKingMoved = hasWhiteKingMoved;
        clone.hasBlackKingMoved = hasBlackKingMoved;
        clone.hasWhiteQueensideRookMoved = hasWhiteQueensideRookMoved;
        clone.hasBlackQueensideRookMoved = hasBlackQueensideRookMoved;
        clone.hasWhiteKingsideRookMoved = hasWhiteKingsideRookMoved;
        clone.hasBlackKingsideRookMoved = hasBlackKingsideRookMoved;

        //this does not copy past

        return clone;
    }

    public bool EqualState(BoardState _boardState)
    {
        //moves and en passant
        if (_boardState.whiteToMove != whiteToMove)
            return false;
        if (_boardState.enPassant != enPassant)
            return false;

        //king pos
        if (_boardState.whiteKingPos != whiteKingPos)
            return false;
        if (_boardState.blackKingPos != blackKingPos)
            return false;

        //castling
        if (_boardState.hasWhiteKingMoved != hasWhiteKingMoved)
            return false;
        if (_boardState.hasBlackKingMoved != hasBlackKingMoved)
            return false;
        if (_boardState.hasWhiteQueensideRookMoved != hasWhiteQueensideRookMoved)
            return false;
        if (_boardState.hasBlackQueensideRookMoved != hasBlackQueensideRookMoved)
            return false;
        if (_boardState.hasWhiteKingsideRookMoved != hasWhiteKingsideRookMoved)
            return false;
        if (_boardState.hasBlackKingsideRookMoved != hasBlackKingsideRookMoved)
            return false;

        //non equal piece
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece? pastPiece = _boardState.board[x, y];
                Piece? piece = board[x, y];

                if(pastPiece.HasValue != piece.HasValue)
                {
                    return false;
                }

                if (pastPiece == null || piece == null)//technically don't have to condition both lol
                    continue;

                if (pastPiece.Value.type != piece.Value.type || pastPiece.Value.isWhite != piece.Value.isWhite)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void ResetFiftyMoveRule()
    {
        fiftyMoveRule = 0;
    }

    public bool IncreaseFiftyMoveRule()
    {
        fiftyMoveRule++;

        if(fiftyMoveRule >= 50)
        {
            return true;
        }

        return false;
    }

    public BoardState Flipped//makes more sense as property
    {
        get
        {
            BoardState clone = Clone();
            clone.whiteToMove = !clone.whiteToMove;
            return clone;
        }
    }

    public BoardState WhiteToMove(bool _whiteToMove)
    {
        BoardState clone = Clone();
        clone.whiteToMove = _whiteToMove;
        return clone;
    }

    public void PrintBoard()
    {
        string toPrint = "";

        toPrint += ("--- Debug Logical Board State ---");
        toPrint += ("\n|");

        for (int y = 7; y > -1; y--)
        {
            for (int x = 0; x < 8; x++)
            {
                if (board[x, y] != null)
                {
                    if(board[x, y].Value.isWhite)
                    {
                        toPrint += whitePieceLetter[(int)board[x, y].Value.type] + " | ";
                    }
                    else
                    {
                        toPrint += blackPieceLetter[(int)board[x, y].Value.type] + " | ";
                    }
                }
                else
                {
                    toPrint += "__| ";
                }
            }
            toPrint += "\n|";
        }

        toPrint += ("White King Square: " + whiteKingPos);
        toPrint += ("\n");
        toPrint += ("Black King Square: " + blackKingPos);
        toPrint += ("\n");
        toPrint += ("50 Move Rule Moves: " + fiftyMoveRule);
        toPrint += ("\n");

        Debug.Log(toPrint);
    }
}
