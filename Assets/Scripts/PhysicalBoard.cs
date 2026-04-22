using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PhysicalBoard : MonoBehaviour
{
    [Header("Settings")]
    public bool playingBot = false;
    public bool playingAsWhite = true;

    [Header("References")]
    public GameObject pawnGO;
    public GameObject rookGO;
    public GameObject knightGO;
    public GameObject bishopGO;
    public GameObject queenGO;
    public GameObject kingGO;

    //banner
    public ResultBanner banner;

    //when moving mouse around
    public GameObject selectionHoverGO;
    private Transform selectionHoverTrans;

    //once clicking on a piece
    public GameObject selectedGO;
    private Transform selectedTrans;

    //possible moves
    public GameObject possibleMoveGO;
    public List<GameObject> possibleMoveGOIs = new List<GameObject>();

    public LayerMask boardLayer;

    public Vector2Int? selectedPosition;

    //game
    public Game game;
    public BoardState boardState;

    //materials
    public Material whiteMat;
    public Material blackMat;

    //camera
    public Camera cam;
    public Transform whiteCamTrans;
    public Transform blackCamTrans;

    //sound
    public AudioSource source;
    public AudioClip moveSound;
    public AudioClip takeSound;
    public AudioClip checkSound;
    public AudioClip castleSound;
    public AudioClip promoteSound;

    //ui
    public TMP_Text resultText;

    //test
    public bool waitingForBot;

    //bot
    Bot bot;

    //dictionary to relate physical to logical
    PhysicalPiece[,] physicalPieces = new PhysicalPiece[8, 8];

    void Start()
    {
        boardState = game.game;

        //get objects
        selectionHoverTrans = Instantiate(selectionHoverGO, new Vector3(0, 0.01f, 0), Quaternion.identity).transform;
        selectedTrans = Instantiate(selectedGO, new Vector3(0, 0.02f, 0), Quaternion.identity).transform;

        Deselect();

        //bot
        bot = new Bot();
    }

    void Update()
    {
        Vector2Int? hoveredSquare = SelectionHover();

        if (Input.GetMouseButtonDown(0) && hoveredSquare != null)//clicking
        {
            if (selectedPosition == null || game.game.board[selectedPosition.Value.x, selectedPosition.Value.y] == null)
            {
                TrySelectPiece(game.game, hoveredSquare.Value);//this can't be null so pass value
            }
            else
            {
                TryMovePiece(game.game, new Move(selectedPosition.Value, hoveredSquare.Value));//need to be able to select a different piece
            }
        }

        if (playingBot && (!game.game.whiteToMove == playingAsWhite) && !waitingForBot && game.game.isInProgress)
        {
            StartCoroutine(WaitForBot(0.5f));
        }
    }

    IEnumerator WaitForBot(float delay)
    {
        waitingForBot = true;

        yield return new WaitForSeconds(delay);

        Move botMove = bot.BestMove(game.game);

        yield return new WaitForSeconds(0.1f);

        TryMovePiece(game.game, botMove);

        waitingForBot = false;
    }

    public void TryMovePiece(BoardState _boardState, Move _move)//SOUNDS ARE MESSED UP FIX THOSE
    {
        //if clicking piece selected, just unselect. maybe put this in a click function
        if (_move.from == _move.to) 
        {
            Deselect();
            return;
        }

        //see if we can do the move. If we can, do it
        MoveReturn moveReturn = MoveGenerator.TryMove(_boardState, _move, MoveGenerator.MakeMoveContext.Real);

        //store sound
        AudioClip sound = moveSound;

        if(moveReturn.type == MoveType.Fail)
        {
            Deselect();
            TrySelectPiece(_boardState, _move.to);
            return;
        }

        //capture. enpassant doesn't matter
        if (moveReturn.isCapture)
        {
            //destroy physical piece
            RemovePhysicalPiece(_move.to);
        }

        //move the piece, this always happens regardless of move type
        MovePhysicalPiece(_move);

        //move pieces accosiated with special moves, like rooks in castling. before we do this, the logical board has changed
        SpecialMove(moveReturn.type, _move, _boardState);

        //condition end
        ShowEndScreen(moveReturn.result);

        //sound
        MoveSound(moveReturn);

        //new turn
        NextTurn(_boardState);
    }

    public void MovePhysicalPiece(Move _move)//warning: move does not correlate with actual move, as this needs to be done twice with castle etc
    {
        physicalPieces[_move.from.x, _move.from.y].SetPosition(_move.to);
        physicalPieces[_move.to.x, _move.to.y] = physicalPieces[_move.from.x, _move.from.y];
        physicalPieces[_move.from.x, _move.from.y] = null;
    }

    public void MoveSound(MoveReturn _moveReturn)
    {
        if (_moveReturn.isCheck)
            source.PlayOneShot(checkSound);
        else if (_moveReturn.isCapture)
            source.PlayOneShot(takeSound);
        else if (_moveReturn.type == MoveType.KingSideCastle || _moveReturn.type == MoveType.QueenSideCastle)
            source.PlayOneShot(castleSound);
        else if (_moveReturn.type == MoveType.Promotion)
            source.PlayOneShot(promoteSound);
        else
            source.PlayOneShot(moveSound);
    }

    public void ShowEndScreen(MoveResult _moveResult)
    {
        if(_moveResult == MoveResult.None)//just cleaner like this idk
            return;

        banner.DisplayBanner();

        switch (_moveResult)
        {
            case MoveResult.WhiteCheckmate:
                resultText.text = "White Wins: Checkmate";
                break;

            case MoveResult.BlackCheckmate:
                resultText.text = "Black Wins: Checkmate";
                break;

            case MoveResult.Stalemate:
                resultText.text = "Draw: Stalemate";
                break;

            case MoveResult.Repetition:
                resultText.text = "Draw: Repetition";
                break;

            case MoveResult.FiftyMoveRule:
                resultText.text = "Draw: 50 Move Rule";
                break;
        }
    }

    public void SpecialMove(MoveType _type, Move _move, BoardState _boardState)//only uses boardstate for promotion... hmm...
    {
        switch (_type)
        {
            case MoveType.EnPassant:

                //destroy physical piece
                RemovePhysicalPiece(_move.to - Vector2Int.up * _move.YDirection);

                break;

            case MoveType.QueenSideCastle:

                //move rook piece
                MovePhysicalPiece(new Move(new Vector2Int(0, _move.from.y), new Vector2Int(3, _move.from.y)));

                break;

            case MoveType.KingSideCastle:

                //move rook piece
                MovePhysicalPiece(new Move(new Vector2Int(7, _move.from.y), new Vector2Int(5, _move.from.y)));

                break;

            case MoveType.Promotion:

                //remove physical pawn
                RemovePhysicalPiece(_move.to);

                //spawn promoted piece and move it to final square (not perfect)
                SpawnPiece(_boardState.board[_move.to.x, _move.to.y].Value, _move.to);
                break;
        }
    }

    public void RemovePhysicalPiece(Vector2Int _square)
    {
        if (physicalPieces[_square.x, _square.y] == null)
            return;

        GameObject go = physicalPieces[_square.x, _square.y].gameObject;
        Destroy(go);
        physicalPieces[_square.x, _square.y] = null;
    }

    public Vector2Int? SelectionHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 200f, boardLayer))
        {
            selectionHoverTrans.position = new Vector3(Mathf.Round(hit.point.x / 10) * 10, 0.01f, Mathf.Round(hit.point.z / 10) * 10);

            //return
            return new Vector2Int((int)Mathf.Round(hit.point.x / 10), (int)Mathf.Round(hit.point.z / 10));
        }

        //didn't hit
        return null;
    }

    //when clicking on board
    public void TrySelectPiece(BoardState _boardState, Vector2Int _square)
    {
        //
        Piece? wantedPiece = _boardState.board[_square.x, _square.y];

        if (wantedPiece == null || wantedPiece.Value.isWhite != _boardState.whiteToMove)
        {
            Debug.Log("Invalid Square to Select");

            Deselect();
            return;
        }

        //a piece is now selected
        Debug.Log("Selecting Square: " + _square);

        selectedPosition = _square;

        //visual
        selectedTrans.gameObject.SetActive(true);
        selectedTrans.position = new Vector3(_square.x * 10, 0.02f, _square.y * 10);

        //show possible moves
        ShowPossibleMoves(_boardState, _square);
    }

    //shows moves
    public void ShowPossibleMoves(BoardState _boardState, Vector2Int from)
    {
        // Remove any previous highlights
        ClearPossibleMoves();

        List<Move> legalMoves = MoveGenerator.GetLegalMoves(_boardState, from);
        
        foreach (Move move in legalMoves)
        {
            GameObject possibleMove = Instantiate(possibleMoveGO, new Vector3(move.to.x, 0.03f, move.to.y) * 10, Quaternion.identity);
            possibleMoveGOIs.Add(possibleMove);
        }
    }

    public void ClearPossibleMoves()
    {
        foreach (GameObject go in possibleMoveGOIs)
        {
            Destroy(go);
        }

        possibleMoveGOIs.Clear();
    }

    //sets up to match logial board
    public void SpawnPieces(BoardState _boardState)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece? piece = _boardState.board[x, y];
                if (piece == null)
                    continue;

                SpawnPiece(piece.Value, new Vector2Int(x, y));
            }
        }
    }

    //spawns individual piece
    public void SpawnPiece(Piece _piece, Vector2Int _square)
    {
        //spawn physical piece based on piece type
        //GameObject go = Instantiate(GetPrefab(_piece.type), new Vector3(_square.x, 0, _square.y) * 10, Quaternion.identity);
        GameObject go = Instantiate(GetPrefab(_piece.type), new Vector3(35f, 1, 35f) + 0 * Random.onUnitSphere, Quaternion.identity);//cool animation
        
        //set correct fields
        PhysicalPiece physicalPiece = go.GetComponent<PhysicalPiece>();
        physicalPiece.square = _square;
        physicalPiece.isWhite = _piece.isWhite;
        physicalPiece.SetPosition(_square);

        //material
        Material mat;
        if(_piece.isWhite)
        {
            mat = whiteMat;
        }
        else
        {
            mat = blackMat;
        }

        //apply material
        foreach (MeshRenderer rend in go.GetComponentsInChildren<MeshRenderer>())
        {
            rend.material = mat;
        }

        //dictionary
        physicalPieces[_square.x, _square.y] = physicalPiece;
    }

    //get go from enum
    public GameObject GetPrefab(PieceType _type)
    {
        switch (_type)
        {
            case PieceType.Pawn: return pawnGO;
            case PieceType.Rook: return rookGO;
            case PieceType.Knight: return knightGO;
            case PieceType.Bishop: return bishopGO;
            case PieceType.Queen: return queenGO;
            case PieceType.King: return kingGO;
        }

        return null;
    }

    //changes physical things for next turn
    public void NextTurn(BoardState _boardState)
    {
        Deselect();

        //switch camera side
        if (playingBot)
            return;

        if (_boardState.whiteToMove)
        {
            cam.transform.position = whiteCamTrans.position;
            cam.transform.localEulerAngles = whiteCamTrans.localEulerAngles;
        }
        else
        {
            cam.transform.position = blackCamTrans.position;
            cam.transform.localEulerAngles = blackCamTrans.localEulerAngles;        
        }
    }

    //removes selection
    public void Deselect()
    {
        selectedPosition = null;

        //visual
        selectedTrans.gameObject.SetActive(false);

        //possible moves
        ClearPossibleMoves();
    }
}
