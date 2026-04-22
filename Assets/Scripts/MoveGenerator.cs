using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public static class MoveGenerator//check the use of the word "Possible"
{
    //public static BoardState boardState = new BoardState();

    //only does the move if it can
    public static MoveReturn TryMove(BoardState _boardState, Move _move, MakeMoveContext _context)
    {
        //only does the move if it can
        if (!CanMove(_boardState, _move))
            return new MoveReturn(MoveType.Fail, MoveResult.None);

        return MakeMove(_boardState, _move, _context);
    }

    //perform the move
    public static MoveReturn MakeMove(BoardState _boardState, Move _move, MakeMoveContext _context) //this doesn't feel right coz we have to reconstruct info from CanSee //complex so can't just do multiple in one move //does the move no matter what, is used by canMove
    {
        MoveReturn moveReturn = new MoveReturn();//defaults to move

        Piece? piece = _boardState.board[_move.from.x, _move.from.y];
        Piece?[,] board = _boardState.board;

        //capture, _to piece is different colour is given by can see
        if (board[_move.to.x, _move.to.y] != null)
        {
            //capture
            moveReturn.isCapture = true;
            //moveReturn.type = MoveType.Capture;
        }

        //pawn moves
        bool passantable = false;
        if(board[_move.from.x, _move.from.y].Value.type == PieceType.Pawn)
        {
            //capturing enPassant
            if (new Vector2Int(_move.to.x, _move.to.y) == _boardState.enPassant)
            {
                board[_move.to.x, _move.to.y - _move.YDirection] = null;
                moveReturn.type = MoveType.EnPassant;
            }

            //setting enPassant
            if (board[_move.from.x, _move.from.y].Value.type == PieceType.Pawn && ((_move.from.y == 1 && _move.to.y == 3) || (_move.from.y == 6 && _move.to.y == 4)))
            {
                _boardState.enPassant = _move.from + new Vector2Int(0, _move.YDirection);
                passantable = true;
            }

            //promotion
            if(piece.Value.isWhite ? _move.to.y == 7 : _move.to.y == 0)
            {
                board[_move.to.x, _move.to.y] = new Piece(PieceType.Queen, piece.Value.isWhite);
                moveReturn.type = MoveType.Promotion;
            }
        }
        if (!passantable)//reset passant square if normal move
        {
            _boardState.enPassant = null;
        }

        //king
        if (piece.Value.type == PieceType.King)
        {
            if (piece.Value.isWhite)//white
            {
                _boardState.whiteKingPos = _move.to;
                _boardState.hasWhiteKingMoved = true;

                //castling
                if (_move.to.x - _move.from.x == -2)//queenside
                {
                    _boardState.hasWhiteQueensideRookMoved = true;
                    moveReturn.type = MoveType.QueenSideCastle;

                    board[3, 0] = board[0, 0];
                    board[0, 0] = null;
                }
                else if(_move.to.x - _move.from.x == 2)//kingside
                {
                    _boardState.hasWhiteKingsideRookMoved = true;
                    moveReturn.type = MoveType.KingSideCastle;

                    board[5, 0] = board[7, 0];
                    board[7, 0] = null;
                }
            }
            else//black
            {
                _boardState.blackKingPos = _move.to;
                _boardState.hasBlackKingMoved = true;

                //castling
                if (_move.to.x - _move.from.x == -2)//queenside
                {
                    _boardState.hasBlackQueensideRookMoved = true;
                    moveReturn.type = MoveType.QueenSideCastle;
                    
                    board[3, 7] = board[0, 7];
                    board[0, 7] = null;
                }
                else if (_move.to.x - _move.from.x == 2)//kingside
                {
                    _boardState.hasBlackKingsideRookMoved = true;
                    moveReturn.type = MoveType.KingSideCastle;

                    board[5, 7] = board[7, 7];
                    board[7, 7] = null;
                }
            }
        }

        //rook
        if (piece.Value.type == PieceType.Rook)
        {
            if (piece.Value.isWhite)//white
            {
                if (_move.to.x == 0)//queenside
                {
                    _boardState.hasWhiteQueensideRookMoved = true;
                }
                else if (_move.to.x == 7)//kingside
                {
                    _boardState.hasWhiteKingsideRookMoved = true;
                }
            }
            else//black
            {
                if (_move.to.x == 0)//queenside
                {
                    _boardState.hasBlackQueensideRookMoved = true;
                }
                else if (_move.to.x == 7)//kingside
                {
                    _boardState.hasBlackKingsideRookMoved = true;
                }
            }
        }

        if(_context >= MakeMoveContext.Bot)
        {
            //check for repetition
            for (int i = 0; i < _boardState.pastBoardStates.Count; i++)
            {
                BoardState boardState = _boardState.pastBoardStates[i];
                if (_boardState.EqualState(boardState))
                {
                    _boardState.pastBoardStatesCount[i]++;

                    if (_boardState.pastBoardStatesCount[i] >= 3)
                    {
                        Debug.Log("REPETITION: DRAW");
                        moveReturn.result = MoveResult.Repetition;
                        _boardState.isInProgress = false;
                    }
                }
            }
            _boardState.pastBoardStates.Add(_boardState.Clone());
            _boardState.pastBoardStatesCount.Add(1);

            //50 move rule
            if (piece.Value.type != PieceType.Pawn && !moveReturn.isCapture)
            {
                if (_boardState.IncreaseFiftyMoveRule())
                {
                    Debug.Log("50 MOVE RULE: DRAW");
                    moveReturn.result = MoveResult.FiftyMoveRule;
                    _boardState.isInProgress = false;
                }
            }
            else
            {
                _boardState.ResetFiftyMoveRule();
            }
        }

        //
        //move piece
        if (moveReturn.type != MoveType.Promotion)
            board[_move.to.x, _move.to.y] = piece;
        
        board[_move.from.x, _move.from.y] = null;

        //turn
        _boardState.whiteToMove = !_boardState.whiteToMove;

        //actually did this move
        if (_context == MakeMoveContext.Real)
        {
            //check for check
            moveReturn.isCheck = InCheck(_boardState, !_boardState.whiteToMove);//switch which side, as it is like we are having another turn. it is reversed

            if (_boardState.isInProgress == true)
            {
                //check for checkmate
                List<Move> moves = GetAllLegalMoves(_boardState);
                if (moves.Count == 0)
                {
                    if (moveReturn.isCheck)
                    {
                        if (!_boardState.whiteToMove)
                        {
                            Debug.Log("CHECKMATE: WHITE WINS");
                            moveReturn.result = MoveResult.WhiteCheckmate;
                            _boardState.isInProgress = false;
                        }
                        else
                        {
                            Debug.Log("CHECKMATE: BLACK WINS");
                            moveReturn.result = MoveResult.BlackCheckmate;
                            _boardState.isInProgress = false;
                        }
                    }
                    else
                    {
                        Debug.Log("STALEMATE: DRAW");
                        moveReturn.result = MoveResult.Stalemate;
                        _boardState.isInProgress = false;
                    }
                }
            }
            
            Debug.Log("Move Type: " + moveReturn.type);
            Debug.Log("Move Result: " + moveReturn.result);
        }

        return moveReturn;
    }

    //can I legally do that move? (checks considered)
    public static bool CanMove(BoardState _boardState, Move _move)
    {
        //set up references
        Piece? piece = _boardState.board[_move.from.x, _move.from.y];
        Piece?[,] board = _boardState.board;

        //can't move if end of game or can't see
        if (!_boardState.isInProgress || !CanSee(_boardState, _move) || !CanCastle(_boardState, _move)) //canMove logic continues after cansee
        {
            //Debug.Log("cannot see square: " + _move.to);
            return false;
        }

        //do the move temporarily to check king attack
        BoardState clone = _boardState.Clone();
        MakeMove(clone, _move, MakeMoveContext.Check);
        if(InCheck(clone, clone.whiteToMove))
        {
            //Debug.Log("square in check: " + _move.to);
            return false;
        }

        return true;
    }

    //this returns true if move is NOT a castle, or if it is a legal castle. THIS DOES NOT CHECK CAN SEE, LIKE BLOCKING PIECES, OR IF HAVE ALREADY CASTLED
    public static bool CanCastle(BoardState _boardState, Move _move)
    {
        //set up references
        Piece? piece = _boardState.board[_move.from.x, _move.from.y];
        Piece?[,] board = _boardState.board;

        //we only care if they are castling
        if(piece.Value.type == PieceType.King && Mathf.Abs(_move.to.x - _move.from.x) == 2 && _move.to.y - _move.from.y == 0)
        {
            bool isKingSide = _move.from.x < _move.to.x;

            //intermediate check
            int step = isKingSide ? 1 : -1;
            for (int x = _move.from.x + step; x != _move.to.x; x += step)
            {
                //checking intermediate square
                if (AnyCanSee(_boardState, new Vector2Int(x, _move.from.y), !piece.Value.isWhite))//using piece colour instead of board colour
                    return false;
            }

            //in check
            if (AnyCanSee(_boardState, _move.from, !piece.Value.isWhite))//using piece colour instead of board colour
                return false;

            //end check is handled after move
        }

        return true;
    }

    //can the from piece get to the to piece?
    public static bool CanSee(BoardState _boardState, Move _move)//convert to use these local vars. idk if this can be called with null piece. switch to everything returning false, or redo canMove var logic (non negative or something)
    {
        //set up references
        Piece? piece = _boardState.board[_move.from.x, _move.from.y];
        Piece?[,] board = _boardState.board;

        if(piece == null)
        {
            return false;
        }

        bool canSee = true; //canMove logic continues after switch

        switch (piece.Value.type)
        {
            case PieceType.Rook:

                //can't move to same square
                if (_move.from == _move.to)
                {
                    canSee = false;
                    break;
                }

                //piece move squares
                if (_move.to.x != _move.from.x && _move.to.y != _move.from.y)
                {
                    canSee = false;
                    break;
                }

                //blocking pieces
                Vector2Int rookCheckingSquare = _move.from; //dumb
                Vector2Int rookCheckDifference = new Vector2Int(IntToSign(_move.to.x - _move.from.x), IntToSign(_move.to.y - _move.from.y));
                while (rookCheckingSquare + rookCheckDifference != _move.to)
                {
                    rookCheckingSquare += rookCheckDifference;
                    if (board[rookCheckingSquare.x, rookCheckingSquare.y] != null)
                    {
                        canSee = false;
                        break;
                    }
                }

                break;

            case PieceType.Pawn: //need to add en passant

                int direction = piece.Value.isWhite ? 1 : -1;
                int startRow = piece.Value.isWhite ? 1 : 6;
                int promotionRow = piece.Value.isWhite ? 7 : 0;

                // Can't move backwards
                if ((piece.Value.isWhite && _move.to.y <= _move.from.y) || (!piece.Value.isWhite && _move.to.y >= _move.from.y))//refactor this
                {
                    canSee = false;
                    break;
                }

                // Move forward
                if (_move.to.x == _move.from.x)
                {
                    // Single move
                    if (_move.to.y == _move.from.y + direction)
                    {
                        if (_boardState.board[_move.to.x, _move.to.y] != null)
                        {
                            canSee = false;
                            break;
                        }
                    }
                    // Double move from starting row
                    else if (_move.to.y == _move.from.y + 2 * direction && _move.from.y == startRow)
                    {
                        if (_boardState.board[_move.to.x, _move.from.y + direction] != null || _boardState.board[_move.to.x, _move.to.y] != null)//can't go past or land on something
                        {
                            canSee = false;
                            break;
                        }
                    }
                    else
                    {
                        canSee = false;
                        break;
                    }
                }
                // Capture move
                else if (Mathf.Abs(_move.to.x - _move.from.x) == 1 && _move.to.y == _move.from.y + direction)//one to the side and one forwards
                {
                    // Normal capture
                    if (_boardState.board[_move.to.x, _move.to.y] == null)//no piece on the side square
                    {
                        // En passant
                        if (_boardState.enPassant == null || _boardState.enPassant.Value != _move.to)//there is no en passant on this square
                        {
                            canSee = false;
                            break;
                        }
                    }
                    else if (_boardState.board[_move.to.x, _move.to.y].Value.isWhite == piece.Value.isWhite)//same side as pawn
                    {
                        canSee = false;
                        break;
                    }
                }
                else
                {
                    canSee = false;
                    break;
                }

                break;

            case PieceType.Bishop:

                //can't move to same square
                //first to stop div 0 error (or not)
                if (_move.from == _move.to)
                {
                    canSee = false;
                    break;
                }

                //piece move squares
                if (_move.to.y - _move.from.y == 0 || (float)Mathf.Abs(_move.to.x - _move.from.x) / (float)Mathf.Abs(_move.to.y - _move.from.y) != 1) //need float here!
                {
                    canSee = false;
                    break;
                }

                //blocking pieces
                Vector2Int bishopCheckingSquare = _move.from; //dumb
                Vector2Int bishopCheckDifference = new Vector2Int(IntToSign(_move.to.x - _move.from.x), IntToSign(_move.to.y - _move.from.y));
                while (bishopCheckingSquare + bishopCheckDifference != _move.to)
                {
                    bishopCheckingSquare += bishopCheckDifference;
                    if (board[bishopCheckingSquare.x, bishopCheckingSquare.y] != null)
                    {
                        canSee = false;
                        break;
                    }
                }

                break;

            case PieceType.Queen:

                //can't move to same square
                if (_move.from == _move.to)
                {
                    canSee = false;
                    break;
                }

                //piece move squares
                if (_move.to.x != _move.from.x && _move.to.y != _move.from.y && (_move.to.y - _move.from.y == 0 || (float)Mathf.Abs(_move.to.x - _move.from.x) / (float)Mathf.Abs(_move.to.y - _move.from.y) != 1))
                {
                    canSee = false;
                    break;
                }

                //blocking pieces
                Vector2Int queenCheckingSquare = _move.from; //dumb
                Vector2Int queenCheckDifference = new Vector2Int(IntToSign(_move.to.x - _move.from.x), IntToSign(_move.to.y - _move.from.y));
                while (queenCheckingSquare + queenCheckDifference != _move.to)
                {
                    queenCheckingSquare += queenCheckDifference;
                    if (board[queenCheckingSquare.x, queenCheckingSquare.y] != null)
                    {
                        canSee = false;
                        break;
                    }
                }

                break;

            case PieceType.Knight:

                //piece move squares
                if (!(Mathf.Abs(_move.to.x - _move.from.x) == 2 && Mathf.Abs(_move.to.y - _move.from.y) == 1) && !(Mathf.Abs(_move.to.x - _move.from.x) == 1 && Mathf.Abs(_move.to.y - _move.from.y) == 2))
                {
                    canSee = false;
                    break;
                }

                break;

            case PieceType.King:

                //can't move to same square
                if (_move.from == _move.to)
                {
                    canSee = false;
                    break;
                }

                //piece move squares
                if (!(Mathf.Abs(_move.to.x - _move.from.x) <= 1 && Mathf.Abs(_move.to.y - _move.from.y) <= 1))//if not in normal 1 square range
                {
                    //castling
                    if ((Mathf.Abs(_move.to.x - _move.from.x) == 2 && _move.to.y - _move.from.y == 0))//if going 2 squares to the side
                    {
                        //moved before
                        if (piece.Value.isWhite ? _boardState.hasWhiteKingMoved : _boardState.hasBlackKingMoved)
                            return false;

                        bool isKingSide = _move.from.x < _move.to.x;
                        if (!isKingSide)//queenside
                        {
                            if (piece.Value.isWhite ? _boardState.hasWhiteQueensideRookMoved : _boardState.hasBlackQueensideRookMoved)
                                return false;
                        }
                        else//kingside
                        {
                            if (piece.Value.isWhite ? _boardState.hasWhiteKingsideRookMoved : _boardState.hasBlackKingsideRookMoved)
                                return false;
                        }

                        //piece in way
                        int step = isKingSide ? 1 : -1;
                        for (int x = _move.from.x + step; x != _move.to.x; x += step)
                        {
                            if (board[x, _move.from.y] != null)
                                return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }    

                break;
        }

        //logic continues, piece non specific. cannot capture own piece
        if (board[_move.to.x, _move.to.y] != null && board[_move.to.x, _move.to.y].Value.isWhite == piece.Value.isWhite)//using piece colour instead of board colour
        {
            canSee = false;
        }

        return canSee;
    } //convert this to use Piece as piece.value maybe

    //any enemy piece can see
    public static bool AnyCanSee(BoardState _boardState, Vector2Int _targetSquare)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece? piece = _boardState.board[x, y];

                if (piece != null && piece.Value.isWhite == _boardState.whiteToMove && CanSee(_boardState, new Move(new Vector2Int(x, y), _targetSquare)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    //any piece can see (side stated by flipSide)
    public static bool AnyCanSee(BoardState _boardState, Vector2Int _targetSquare, bool _whiteToMove)//uses different input for checking (sound and checkmate)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece? piece = _boardState.board[x, y];

                if (piece != null && piece.Value.isWhite == _whiteToMove && CanSee(_boardState, new Move(new Vector2Int(x, y), _targetSquare)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool InCheck(BoardState _boardState, bool _whiteToMove)
    {
        return AnyCanSee(_boardState, _whiteToMove ? _boardState.blackKingPos : _boardState.whiteKingPos, _whiteToMove);
    }

    public static List<Move> GetLegalMoves(BoardState _boardState, Vector2Int _from)
    {
        List<Move> moves = new List<Move>();
        Piece piece = _boardState.board[_from.x, _from.y].Value;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector2Int to = new Vector2Int(x, y);
                if (CanMove(_boardState, new Move(_from, to)))
                {
                    moves.Add(new Move(_from, to));
                }
            }
        }

        return moves;
    }

    public static List<Move> GetPossibleMoves(BoardState _boardState, Vector2Int _from)
    {
        List<Move> moves = new List<Move>();
        Piece piece = _boardState.board[_from.x, _from.y].Value;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector2Int to = new Vector2Int(x, y);
                if (CanSee(_boardState, new Move(_from, to)))
                {
                    moves.Add(new Move(_from, to));
                }
            }
        }

        return moves;
    }

    public static List<Move> GetAllLegalMoves(BoardState _boardState)
    {
        List<Move> moves = new List<Move>();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece? piece = _boardState.board[x, y];

                if (piece == null || piece.Value.isWhite != _boardState.whiteToMove)
                {
                    continue;
                }

                Vector2Int from = new Vector2Int(x, y);
                moves.AddRange(GetLegalMoves(_boardState, from));
            }
        }

        return moves;
    }

    public static List<Move> GetAllPossibleMoves(BoardState _boardState)
    {
        List<Move> moves = new List<Move>();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece? piece = _boardState.board[x, y];

                if (piece == null || piece.Value.isWhite != _boardState.whiteToMove)
                {
                    continue;
                }

                Vector2Int from = new Vector2Int(x, y);
                moves.AddRange(GetPossibleMoves(_boardState, from));
            }
        }

        return moves;
    }

    //math function to help
    public static int IntToSign(int _num)
    {
        if (_num == 0)
        {
            return 0;
        }

        return _num / Mathf.Abs(_num);
    }

    public enum MakeMoveContext
    {
        Check,//move is made to see if it is check, don't need to calculate repetition
        Bot,//move is made to scan ahead by bot, need to check repetition and other things but don't need to debug stuff or return
        Real//move is actually made, need to do everything
    }
}
