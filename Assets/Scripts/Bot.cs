using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

public class Bot
{
    public int[] pieceValues = new int[] { 100, 300, 300, 500, 900, 10000 };

    public Move BestMove(BoardState _boardState)//returns the best move
    {
        //which move is this bot taking. Every function returns in terms of white
        bool whiteToMove = _boardState.whiteToMove;

        List<Move> moves = MoveGenerator.GetAllLegalMoves(_boardState);
        List<int> evals = new List<int>();

        for (int i = 0; i < moves.Count; i++)
        {
            BoardState clone = _boardState.Clone();
            MakeMove(clone, moves[i], MakeMoveContext.Bot);

            //---EVALUATION---
            evals.Add(Minimax(clone, 2, int.MinValue, int.MaxValue, !whiteToMove));//check this, ALSO IT IS WAY TOO LAGGY LOL

            //modify so it is relevent for bot side
            evals[i] *= whiteToMove ? 1 : -1;
        }

        moves[MaxWeightIndex(evals)].DebugMove();
        return moves[MaxWeightIndex(evals)];
    }

    public int Minimax(BoardState _boardState, int _depth, int _alpha, int _beta, bool _maxing)//I think maxForWhite means whos turn is it in this context
    {
        //return if reached depth
        if(_depth == 0)
            return EvaluateBoardState(_boardState);

        //get all moves and keep going
        List<Move> moves = MoveGenerator.GetAllLegalMoves(_boardState);

        if(moves.Count == 0)
            return EvaluateBoardState(_boardState);

        if (_maxing)
        {
            int maxEval = int.MinValue;

            foreach(Move move in moves)//find highest move
            {
                BoardState clone = _boardState.Clone();
                MakeMove(clone, move, MakeMoveContext.Bot);

                int eval = Minimax(clone, _depth - 1, _alpha, _beta, false);//max for black

                maxEval = Mathf.Max(eval, maxEval);

                //alpha beta pruning
                _alpha = Mathf.Max(_alpha, eval);
                if (_beta <= _alpha)
                    break; // beta cutoff
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

                int eval = Minimax(clone, _depth - 1, _alpha, _beta, true);//max for black

                minEval = Mathf.Min(eval, minEval);

                //alpha beta pruning
                _beta = Mathf.Min(_beta, eval);
                if (_beta <= _alpha)
                        break; // alpha cutoff
            }

        return minEval;//before this returns it needs to run recusively
        }
    }

    public int EvaluateBoardState(BoardState _boardState)//pass clone
    {
        int score = 0;

        score += PieceValue(_boardState);//all scores should stay relative to this
        score += MoveableSquares(_boardState, 10);
        //score += Checkmate(_boardState);

        //add on all functions here

        return score;
    }

    /*public int Checkmate(BoardState _boardState)
    {
        
    }*/

    public int MoveableSquares(BoardState _boardState, int _factor)//make another one that checks legal moves if move is a check, or just use legal moves only
    {
        int whiteMoveableSquares = MoveGenerator.GetAllLegalMoves(_boardState.WhiteToMove(true)).Count;
        int blackMoveableSquares = MoveGenerator.GetAllLegalMoves(_boardState.WhiteToMove(false)).Count;
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
}
