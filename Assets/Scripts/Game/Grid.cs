using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.VFX;

public class Grid : MonoBehaviour
{
    public SchapeStorage shapeStorage;
    public int columns = 0;
    public int rows = 0;
    public float squaresGap = 0.1f;
    public GameObject gridSquare;
    public Vector2 startPosition = new Vector2(0.0f, 0.0f);
    public float squareScale = 0.5f;
    public float everySquareOffset = 0.0f;

    private Vector2 _offset = new Vector2(0.0f, 0.0f);
    private List<GameObject> _gridSquares = new List<GameObject>();

    private void OnEnable() {
        GameEvents.CheckIfShapeCanBePlaced += CheckIfShapeCanBePlaced;
    }

    private void OnDisable() 
    {
        GameEvents.CheckIfShapeCanBePlaced -= CheckIfShapeCanBePlaced;
    }

    void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        SpawnGridSquares();
        SetGridSquaresPositions();
    }

    private void SpawnGridSquares()
    {
        //0, 1, 2, 3, 4,
        //5, 6, 7, 8, 9

        int square_index = 0;
        
        for(var row = 0; row < rows; ++row)
        {
            for(var column = 0; column < rows; ++column)
            {
                _gridSquares.Add(Instantiate(gridSquare) as GameObject);

                _gridSquares[_gridSquares.Count - 1].GetComponent<GridSquare>().SquareIndex = square_index; //맵 안쪽으로 알맞게 넣었을때 좌측상단에만 배치되는거 해결해줌
                _gridSquares[_gridSquares.Count - 1].transform.SetParent(this.transform);
                _gridSquares[_gridSquares.Count - 1].transform.localScale = new Vector3(squareScale, squareScale, squareScale);
                _gridSquares[_gridSquares.Count - 1].GetComponent<GridSquare>().SetImage(square_index % 2 == 0);
                square_index++;            
            }
        }
    }

    private void SetGridSquaresPositions()
    {
        int coulmn_number = 0;
        int row_number = 0;
        Vector2 square_gap_number = new Vector2(0.0f, 0.0f);
        bool row_moved = false;
        
        var square_rect = _gridSquares[0].GetComponent<RectTransform>();

        _offset.x = square_rect.rect.width * square_rect.transform.localScale.x + everySquareOffset;
        _offset.y = square_rect.rect.height * square_rect.transform.localScale.y + everySquareOffset;

        foreach(GameObject square in _gridSquares)
        {
            if(coulmn_number + 1 > columns)
            {
                square_gap_number.x = 0;
                //go to the next column
                coulmn_number = 0;
                row_number++;
                row_moved = true;
            }

            var pos_x_offset = _offset.x * coulmn_number + (square_gap_number.x * squaresGap);
            var pos_y_offset = _offset.y * row_number + (square_gap_number.y * squaresGap);

            if(coulmn_number > 0 && coulmn_number % 3 == 0)
            {
                square_gap_number.x++;
                pos_x_offset += squaresGap;
            }

            if(row_number > 0 && row_number % 3 == 0 && row_moved == false)
            {
                row_moved = true;
                square_gap_number.y++;
                pos_y_offset += squaresGap;
            }

            square.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPosition.x + pos_x_offset, startPosition.y - pos_y_offset);

            square.GetComponent<RectTransform>().localPosition = new Vector3(startPosition.x + pos_x_offset, startPosition.y - pos_y_offset, 0.0f);

            coulmn_number++;
        }
    }


    private void CheckIfShapeCanBePlaced()
    {
        var SquareIndexs = new List<int>();

        foreach(var square in _gridSquares)
        {
            var gridSquare = square.GetComponent<GridSquare>();

            if(gridSquare.Selected && !gridSquare.SquareOccupied)
            {
                SquareIndexs.Add(gridSquare.SquareIndex);
                gridSquare.Selected = false;
                //gridSquare.ActivateSquare();
            }
        }

        var GetCurrentSelectedShape = shapeStorage.GetCurrentSelectedShape();
        if(GetCurrentSelectedShape == null) return; //there is no selected shape.

        if(GetCurrentSelectedShape.TotalSquareNumber == SquareIndexs.Count)
        {

            foreach(var SquareIndex in SquareIndexs)
            {
                _gridSquares[SquareIndex].GetComponent<GridSquare>().PlaceShapeOnBoard();
            }
            
            var shapeLeft = 0;

            foreach(var shape in shapeStorage.shapeList)
            {
                if(shape.IsOnStartPosition() && shape.IsAnyOfShapeSquareActive())
                {
                    shapeLeft++;
                }
            }

            if(shapeLeft == 0)
            {
                GameEvents.RequestNewShapes(); //3개의 블록이 모두 소진되면 다시 3개를 불러와라
            }        
            else
            {
                GameEvents.SetShapeInactive();
            } 
        }
        else
        {
            GameEvents.MoveShapeToStartPosition();
        }
    }
}
