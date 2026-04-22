using UnityEngine;

public class Game : MonoBehaviour
{
    public PhysicalBoard physicalBoard;

    public BoardState game = new BoardState();

    void Start()
    {
        SetupStartingPosition();
        //SetupEndgamePosition();
        //SetupTestPosition();
        physicalBoard.SpawnPieces(game);
    }

    void Update()
    {
        //hotkeys
        if (Input.GetKeyDown(KeyCode.D))
        {
            game.PrintBoard();
        }
    }

    public void SetupStartingPosition()
    {
        game = new BoardState();

        // Pawns
        for (int x = 0; x < 8; x++)
        {
            game.board[x, 1] = new Piece(PieceType.Pawn, true);
            game.board[x, 6] = new Piece(PieceType.Pawn, false);
        }

        // Rooks
        game.board[0, 0] = new Piece(PieceType.Rook, true);
        game.board[7, 0] = new Piece(PieceType.Rook, true);
        game.board[0, 7] = new Piece(PieceType.Rook, false);
        game.board[7, 7] = new Piece(PieceType.Rook, false);

        // Knights
        game.board[1, 0] = new Piece(PieceType.Knight, true);
        game.board[6, 0] = new Piece(PieceType.Knight, true);
        game.board[1, 7] = new Piece(PieceType.Knight, false);
        game.board[6, 7] = new Piece(PieceType.Knight, false);

        // Bishops
        game.board[2, 0] = new Piece(PieceType.Bishop, true);
        game.board[5, 0] = new Piece(PieceType.Bishop, true);
        game.board[2, 7] = new Piece(PieceType.Bishop, false);
        game.board[5, 7] = new Piece(PieceType.Bishop, false);

        // Queens
        game.board[3, 0] = new Piece(PieceType.Queen, true);
        game.board[3, 7] = new Piece(PieceType.Queen, false);

        // Kings
        game.board[4, 0] = new Piece(PieceType.King, true);
        game.whiteKingPos = new Vector2Int(4, 0);
        game.board[4, 7] = new Piece(PieceType.King, false);
        game.blackKingPos = new Vector2Int(4, 7);

        //castling
        game.hasWhiteKingMoved = false;
        game.hasBlackKingMoved = false;
        game.hasWhiteQueensideRookMoved = false;
        game.hasBlackQueensideRookMoved = false;
        game.hasWhiteKingsideRookMoved = false;
        game.hasBlackKingsideRookMoved = false;

        game.whiteToMove = true;
    }

    public void SetupEndgamePosition()
    {
        game = new BoardState();

        //rooks
        game.board[0, 0] = new Piece(PieceType.Rook, true);
        game.board[7, 0] = new Piece(PieceType.Rook, true);
        game.board[4, 1] = new Piece(PieceType.Rook, true);
        game.board[0, 7] = new Piece(PieceType.Rook, false);
        game.board[7, 7] = new Piece(PieceType.Rook, false);

        //kings
        game.board[4, 0] = new Piece(PieceType.King, true);
        game.whiteKingPos = new Vector2Int(4, 0);
        game.board[4, 7] = new Piece(PieceType.King, false);
        game.blackKingPos = new Vector2Int(4, 7);

        //castling
        game.hasWhiteKingMoved = false;
        game.hasBlackKingMoved = false;
        game.hasWhiteQueensideRookMoved = false;
        game.hasBlackQueensideRookMoved = false;
        game.hasWhiteKingsideRookMoved = false;
        game.hasBlackKingsideRookMoved = false;

        game.whiteToMove = true;
    }

    public void SetupTestPosition()
    {
        game = new BoardState();

        //a pawn
        game.board[1, 6] = new Piece(PieceType.Pawn, true);
        game.board[1, 5] = new Piece(PieceType.Bishop, true);
        game.board[1, 4] = new Piece(PieceType.Queen, true);

        //rooks
        game.board[0, 0] = new Piece(PieceType.Rook, true);
        game.board[7, 0] = new Piece(PieceType.Rook, true);

        game.board[0, 7] = new Piece(PieceType.Rook, false);
        game.board[7, 7] = new Piece(PieceType.Rook, false);

        //kings
        game.board[4, 0] = new Piece(PieceType.King, true);
        game.whiteKingPos = new Vector2Int(4, 0);
        game.board[4, 7] = new Piece(PieceType.King, false);
        game.blackKingPos = new Vector2Int(4, 7);

        //castling
        game.hasWhiteKingMoved = false;
        game.hasBlackKingMoved = false;
        game.hasWhiteQueensideRookMoved = false;
        game.hasBlackQueensideRookMoved = false;
        game.hasWhiteKingsideRookMoved = false;
        game.hasBlackKingsideRookMoved = false;

        game.whiteToMove = true;
    }


}
