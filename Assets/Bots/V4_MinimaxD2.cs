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

        //which move is this bot taking. Every function returns in terms of white
        bool whiteToMove = _boardState.whiteToMove;

        List<Move> moves = MoveGenerator.GetAllLegalMoves(_boardState);
        List<int> evals = new List<int>();

        for (int i = 0; i < moves.Count; i++)
        {
            BoardState clone = _boardState.Clone();
            MakeMove(clone, moves[i], MakeMoveContext.Bot);

            //---EVALUATION---
            evals.Add(Minimax(clone, 2, !whiteToMove));//check this, ALSO IT IS WAY TOO LAGGY LOL

            //modify so it is relevent for bot side
            evals[i] *= whiteToMove ? 1 : -1;
        }

        Debug.Log("Bot Move Weight: " + evals[MaxWeightIndex(evals)]);//lol
        moves[MaxWeightIndex(evals)].DebugMove();
        return moves[MaxWeightIndex(evals)];
    }

    public int Minimax(BoardState _boardState, int _depth, bool _maxForWhite)//I think maxForWhite means whos turn is it in this context
    {
        //return if reached depth
        if (_depth == 0)
            return EvaluateBoardState(_boardState);

        //get all moves and keep going
        List<Move> moves = MoveGenerator.GetAllLegalMoves(_boardState);

        if (moves.Count == 0)
            return EvaluateBoardState(_boardState);

        if (_maxForWhite)
        {
            int maxEval = int.MinValue;

            foreach (Move move in moves)//find highest move
            {
                BoardState clone = _boardState.Clone();
                MakeMove(clone, move, MakeMoveContext.Bot);

                int eval = Minimax(clone, _depth - 1, false);//max for black

                maxEval = Mathf.Max(eval, maxEval);
            }

            return maxEval;//before this returns it needs to run recusively
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (Move move in moves)//find highest move
            {
                BoardState clone = _boardState.Clone();
                MakeMove(clone, move, MakeMoveContext.Bot);

                int eval = Minimax(clone, _depth - 1, true);//max for black

                minEval = Mathf.Min(eval, minEval);
            }

            return minEval;//before this returns it needs to run recusively
        }
    }

    public int EvaluateBoardState(BoardState _boardState)//pass clone
    {
        int score = 0;

        score += PieceValue(_boardState);//all scores should stay relative to this
        score += RandomAdjustment(5);
        score += MoveableSquares(_boardState, 10);

        //add on all functions here

        return score;
    }

    public int MoveableSquares(BoardState _boardState, int _factor)
    {
        int whiteMoveableSquares = MoveGenerator.GetAllPossibleMoves(_boardState.WhiteToMove(true)).Count;
        int blackMoveableSquares = MoveGenerator.GetAllPossibleMoves(_boardState.WhiteToMove(false)).Count;
        int moveableSquaresWeight = (whiteMoveableSquares - blackMoveableSquares) * _factor;
        return moveableSquaresWeight;
    }

    public int PieceValue(BoardState _boardState)//wants to have more pieces
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

    public int RandomAdjustment(int _factor)//add a bit of randomness to stop edge moves all the time
    {
        return Random.Range(-_factor, _factor + 1);
    }

    public int MaxWeightIndex(List<int> _weights)
    {
        int max = int.MinValue;
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
