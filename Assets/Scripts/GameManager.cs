/*using NUnit.Framework.Constraints;
using System;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    //public int BOARD_WIDTH = 8;
    //public int BOARD_HEIGHT = 8;

    [Header("References")]
    public GameObject pawnGO;
    public GameObject rookGO;
    public GameObject knightGO;
    public GameObject bishopGO;
    public GameObject queenGO;
    public GameObject kingGO;

    public GameObject selectionHoverGO;
    private Transform selectionHoverTrans;

    public GameObject selectedGO;
    private Transform selectedTrans;

    public GameObject canGoGO;
    private GameObject[,] canGos;

    public LayerMask boardLayer;

    public Camera cam;
    public Transform whiteCamTrans;
    public Transform blackCamTrans;

    [Header("Script")]

    public Piece[,] board;
    public Piece[,] boardSave1;
    public char turnSave1;

    public Vector2Int whiteKingSquare;
    public Vector2Int blackKingSquare;

    public char turn;

    //
    public Piece? selectedPiece;//does this even do anything... it might be messing things up
    public Vector2Int selectedPosition;

    void Start()
    {
        selectionHoverTrans = Instantiate(selectionHoverGO, new Vector3(0, 0.01f, 0), Quaternion.identity).transform;
        selectedTrans = Instantiate(selectedGO, new Vector3(0, 0.02f, 0), Quaternion.identity).transform;

        GenerateCanGos();

        Deselect();

        StandardSetup();
        PrintBoard();

        NewTurn();
    }

    void Update()
    {
        //make a valid squares option!
        //use "can get to" function that does not move piece

        Vector2Int hoveredSquare = SelectionHover();
        
        if(Input.GetMouseButtonDown(0) && IsValid(hoveredSquare))
        {
            if(selectedPiece == null)
            {
                TrySelectPiece(hoveredSquare);
            }
            else
            {
                TryMovePiece(selectedPosition, hoveredSquare);//need to be able to select a different piece
            }
        }
    }

    public void GenerateCanGos()
    {
        canGos = new GameObject[8, 8];

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                canGos[x, y] = Instantiate(canGoGO, new Vector3(x * 10, 0.03f, y * 10), Quaternion.identity);
            }
        }
    }

    public Piece VectorToPiece(Vector2Int _vector)
    {
        return board[_vector.x, _vector.y];
    }

    public bool IsValid(Vector2Int _vector) //is this square on the board?
    {
        if(_vector == new Vector2Int(-1, -1))
        {
            return false;
        }

        return true;
    }

    public void NextTurn()
    {
        //swithc side
        if(turn == 'w')
        {
            turn = 'b';
            cam.transform.position = blackCamTrans.position;
            cam.transform.localEulerAngles = blackCamTrans.localEulerAngles;
        }
        else
        {
            turn = 'w';
            cam.transform.position = whiteCamTrans.position;
            cam.transform.localEulerAngles = whiteCamTrans.localEulerAngles;
        }

        //setup for next turn
        NewTurn();
    }

    public void NewTurn()
    {
        //reset selected
        Deselect();

        //generate valid move locations
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (board[x, y] != null && board[x, y].side == turn) //if there is a piece here
                {
                    Piece piece = board[x, y];

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            piece.validMoves[i, j] = CanMove(new Vector2Int(x, y), new Vector2Int(i, j)); //store whether we can move there in the piece
                        }
                    }
                }
            }
        }

        //cam
        if (turn == 'w')
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

    public void TryMovePiece(Vector2Int _from, Vector2Int _to)
    {
        if(_from == _to) //if clicking piece selected, just unselect
        {
            Deselect();
            return;
        }

        if (CanMove(_from, _to))
        {
            Move(_from, _to);

            //move physical piece
            selectedPiece.SetPosition(_to);

            //new turn
            NextTurn();
        }
        else
        {
            Deselect(); //stops cangos
            TrySelectPiece(_to); //will be null if failed
        }
    }

    public void Move(Vector2Int _from, Vector2Int _to) //just moves it no matter what
    {
        Piece piece = board[_from.x, _from.y];

        //capture, _to piece is different colour
        if (board[_to.x, _to.y] != null && board[_to.x, _to.y].side != turn)
        {
            //capture
            board[_to.x, _to.y].Capture();
        }

        //king
        if (board[_from.x, _from.y].type == PieceType.King)
        {
            if (board[_from.x, _from.y].side == 'w')
            {
                whiteKingSquare = _to;
            }
            else
            {
                blackKingSquare = _to;
            }
        }

        //move piece
        board[_to.x, _to.y] = piece;
        board[_from.x, _from.y] = null;
    }




    public void TrySelectPiece(Vector2Int _square)
    {
        Debug.Log("Selecting Square: " + _square);

        Piece wantedPiece = board[_square.x, _square.y];

        if (wantedPiece != null && wantedPiece.side == turn)
        {
            selectedPosition = _square;
            selectedPiece = VectorToPiece(_square);

            //visual
            selectedTrans.gameObject.SetActive(true);
            selectedTrans.position = new Vector3(_square.x * 10, 0.02f, _square.y * 10);

            //highlight canGo locations
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (selectedPiece.validMoves[x, y] == true)
                    {
                        canGos[x, y].SetActive(true);
                    }
                }
            }
        }
        else
        {
            Deselect();
        }
    }

    public void Deselect()
    {
        selectedPosition = new Vector2Int(-1, -1);
        selectedPiece = null;

        //visual
        selectedTrans.gameObject.SetActive(false);

        //can gos
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                canGos[x, y].SetActive(false);
            }
        }
    }

    public int IntToSign(int _num)
    {
        if(_num == 0)
        {
            return 0;
        }

        return _num / Mathf.Abs(_num);
    }

    public void PrintBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                if(board[i, j] != null)
                    Debug.Log(board[i, j].type);
                else
                {
                    Debug.Log('x');
                } 
            }
            Debug.Log("...");
        }
    }

    public Vector2Int SelectionHover()
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
        return new Vector2Int(-1, -1);
    }

    public void SpawnPiece(GameObject _go, int _x, int _y, PieceType _type, char _side)
    {
        //physical
        GameObject go = Instantiate(_go, new Vector3(_x, 0, _y) * 10, Quaternion.identity);

        //logical
        board[_x, _y] = go.GetComponent<Piece>();
        board[_x, _y].SetUp(_type, _side);
    }

    public void StandardSetup()
    {
        //initial values
        board = new Piece[8, 8];
        turn = 'w';

        //white

        //pawns
        for(int i = 0; i < 8; i++)
        {
            SpawnPiece(pawnGO, i, 1, PieceType.Pawn, 'w');
        }

        //rest
        SpawnPiece(rookGO, 0, 0, PieceType.Rook, 'w');
        SpawnPiece(knightGO, 1, 0, PieceType.Knight, 'w');
        SpawnPiece(bishopGO, 2, 0, PieceType.Bishop, 'w');
        SpawnPiece(queenGO, 3, 0, PieceType.Queen, 'w');
        SpawnPiece(kingGO, 4, 0, PieceType.King, 'w');
        whiteKingSquare = new Vector2Int(4, 0);
        SpawnPiece(bishopGO, 5, 0, PieceType.Bishop, 'w');
        SpawnPiece(knightGO, 6, 0, PieceType.Knight, 'w');
        SpawnPiece(rookGO, 7, 0, PieceType.Rook, 'w');

        //SpawnPiece(rookGO, 3, 3, PieceType.Rook, 'w');

        //black

        //pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnPiece(pawnGO, i, 6, PieceType.Pawn, 'b');
        }

        //rest
        SpawnPiece(rookGO, 0, 7, PieceType.Rook, 'b');
        SpawnPiece(knightGO, 1, 7, PieceType.Knight, 'b');
        SpawnPiece(bishopGO, 2, 7, PieceType.Bishop, 'b');
        SpawnPiece(queenGO, 3, 7, PieceType.Queen, 'b');
        SpawnPiece(kingGO, 4, 7, PieceType.King, 'b');
        blackKingSquare = new Vector2Int(4, 7);
        SpawnPiece(bishopGO, 5, 7, PieceType.Bishop, 'b');
        SpawnPiece(knightGO, 6, 7, PieceType.Knight, 'b');
        SpawnPiece(rookGO, 7, 7, PieceType.Rook, 'b');
    }
}*/
