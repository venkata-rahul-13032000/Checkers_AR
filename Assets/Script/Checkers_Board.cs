using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Checkers_Board : MonoBehaviour
{
    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    public TextMeshProUGUI victory;

    private int black;
    private int white;

    private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    private Piece selectedPiece;
    
    private List<Piece> forcedPieces;

    public bool isWhite;
    private bool isWhiteTurn;
    private bool hasKilled;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    public class LoadSceneOnClick : MonoBehaviour
    {
        public void LoadByIndex(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
    private void Start()
    {
        black = 12;
        white = 12;
        isWhite = false;
        forcedPieces = new List<Piece>();
        GenerateBoard();
    }
    private void Update()
    {
        UpdateMouseOver();
        //Debug.Log(mouseOver);

        
        if(isWhite?isWhiteTurn:!isWhiteTurn)
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
            {
                UpdatePieceDrag(selectedPiece);
            }

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse Button Down");
                SelectPiece(x, y);
            }
             
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Mouse Button UP");
                TryMove((int)startDrag.x, (int)startDrag.y,x,y);
                   
            }
        }
    }
    private void UpdateMouseOver()
    {
        
        if (!Camera.main)
        {
            Debug.Log("unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    private void UpdatePieceDrag(Piece p)
    {
        
        if (!Camera.main)
        {
            Debug.Log("unable to find main camera");
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }

    }
    private void SelectPiece(int x, int y)
    {
        
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return;

        Piece p = pieces[x, y];
        if (p != null && p.isWhite == isWhite)
        {
            if(forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = mouseOver;
                //Debug.Log(selectedPiece.name);
            }
            else
            {
                
                if (forcedPieces.Find(fp => fp == p) == null)
                {
                    return;
                }
                selectedPiece = p;
                startDrag = mouseOver;
            }


        }
    }
    private void EndTurn()
    {
        int x = (int)(endDrag.x);
        int y = (int)(endDrag.y);

        
        if(selectedPiece != null)
        {
            if(selectedPiece.isWhite && !selectedPiece.isKing && y == 7)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            else if (!selectedPiece.isWhite && !selectedPiece.isKing && y == 0)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);            
            }
        }

        selectedPiece = null;
        startDrag = Vector2.zero;

        if(ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasKilled)
        {
            return;
        }

        isWhiteTurn = !isWhiteTurn;
        isWhite = !isWhite;
        hasKilled = false;
        CheckVictory();
    }
    private void CheckVictory()
    {
        if(black == 0)
        {
            Victory(true);
        }
        if(white == 0)
        {
            Victory(false);
        }


        Debug.Log("inside check victory");
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false;
        bool hasBlack = false; 
        for(int i = 0; i < ps.Length; i++)
        {
            Debug.Log("inside FOR LOOP");
            if (ps[i].isWhite)
            {
                hasWhite = true;
            }
            else
            {
                hasBlack = true;
            }
        }



        if (!hasWhite)
            Victory(false);
        if (!hasBlack)
            Victory(true);
    }
    private void Victory(bool isWhite)
    {
        if (isWhite)
        {
            victory.text = "White Wins";
            
        }
        else
        {
            victory.text = "Black Wins";
            
        }            
    }
    private List<Piece> ScanForPossibleMove(Piece p, int x, int y)
    {
        forcedPieces = new List<Piece>();
        
        if(pieces[x,y].IsForceToMove(pieces, x, y))
        {
            forcedPieces.Add(pieces[x, y]);
        } 
        return forcedPieces;
    }
    private List<Piece> ScanForPossibleMove()
    {
        forcedPieces = new List<Piece>();
        
        for(int i =0; i<8; i++)
        {
            for(int j =0; j<8; j++)
            {
                if(pieces[i,j] != null && pieces[i,j].isWhite == isWhiteTurn)
                {
                    if(pieces[i,j].IsForceToMove(pieces, i, j))
                    {
                        forcedPieces.Add(pieces[i, j]);
                    }
                }
            }
        }
        return forcedPieces;
    }
    private void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = ScanForPossibleMove();


        
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1]; 

        

        MovePiece(selectedPiece, x2, y2);


        
        if (x2 < 0 || x2> 8 ||  y2 < 0 || y2>= 8)
        {
           
            if (selectedPiece != null)
            {
                MovePiece(selectedPiece, x1,y1);
            }
            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }

        if (selectedPiece != null)
        {
            
            if (endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                
                return;
            }

            
            if(selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                
                if(Mathf.Abs(x2 - x1) == 2)
                {
                    Piece p = pieces[((x1 + x2) / 2), (y1 + y2) / 2];
                    if (p != null)
                    {
                        pieces[((x1 + x2) / 2), (y1 + y2) / 2] = null;
                        if (p.isWhite)
                            white--;
                        else
                            black--;
                        Destroy(p.gameObject);
                        hasKilled = true; 
                    }
                }

                
                if (forcedPieces.Count != 0 && !hasKilled)
                {
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);

                EndTurn();
            }
            else
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }
 

    }
    private void GenerateBoard()
    {
        
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                
                if (oddRow)
                    GeneratePiece(x, y);
                else
                    GeneratePiece(x + 1, y);
            }
        }
        
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                
                if (oddRow)
                    GeneratePiece(x, y);
                else
                    GeneratePiece(x + 1, y);
            }
        }
    } 
    private void GeneratePiece(int x, int y)
    {
        GameObject go;
        
        if (y > 3)
        {
            
            go = Instantiate(blackPiecePrefab) as GameObject;
        }
        else
        {
            
            go = Instantiate(whitePiecePrefab) as GameObject;
        }

        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        pieces[x, y] = p;
        MovePiece(p, x, y);
    }
    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }
}
