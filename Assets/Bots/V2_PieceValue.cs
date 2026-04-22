/*using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MoveGenerator;

public class Bot
{
    public int[] pieceValues = new int[] { 100, 300, 300, 500, 900, 10000 };

    public Move BestMove(BoardState _boardState)//returns the best move
    {
        Debug.Log("BOT IS MOVING");

        Debug.Log("Piece value of this position: " + PieceValue(_boardState));

        List<Move> moves = MoveGenerator.GetAllLegalMoves(_boardState);
        List<int> weights = new List<int>();

        for (int i = 0; i < moves.Count; i++)
        {
            BoardState clone = _boardState.Clone();
            MakeMove(clone, moves[i], MakeMoveContext.Bot);
            weights.Add(0);

            //---EVALUATION---
            weights[i] += PieceValue(clone);//careful since list needs to be the same size.
            weights[i] += RandomAdjustment();
            //add on all functions here

            //for side
            weights[i] *= _boardState.whiteToMove ? 1 : -1;
        }

        return moves[MaxWeightIndex(weights)];
    }

    public int PieceValue(BoardState _boardState)
    {
        int pieceValue = 0;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece? piece = _boardState.board[x, y];
                if (piece == null)
                    continue;

                pieceValue += piece.Value.isWhite ? pieceValues[(int)piece.Value.type] : -pieceValues[(int)piece.Value.type];
            }
        }

        return pieceValue;
    }

    public int RandomAdjustment()
    {
        return Random.Range(-5, 6);
    }

    public int MaxWeightIndex(List<int> _weights)
    {
        int max = -99999999;
        int maxIndex = 0;//for no errors idk

        for (int i = 0; i < _weights.Count; i++)
        {
            if (_weights[i] > max)
            {
                max = _weights[i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }
}*/