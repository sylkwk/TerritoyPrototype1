using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shape : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public GameObject squareShapeImage;
    public Vector3 shapeSelectedScale;
    public Vector2 offset = new Vector2(0f, 700f);

    [HideInInspector]
    public ShapeData CurrentShapeData;

    public int TotalSquareNumber { get; set; }

    private List<GameObject> _currentShape = new List<GameObject>();
    private Vector3 _shapeStartScale;
    private RectTransform _transform;
    private bool _shapeDraggable = true;
    private Canvas _canvas;
    private Vector3 _startPosition;
    private bool _shapeActive = true;
    private bool isDragging = false;
    public int ShapeIndex { get; set; }

    public void Awake()
    {
        _shapeStartScale = this.GetComponent<RectTransform>().localScale;
        _transform = this.GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _shapeDraggable = true;
        _startPosition = _transform.localPosition;
        _shapeActive = true;
    }

    private void OnEnable() {
        GameEvents.MoveShapeToStartPosition += MoveShapeToStartPosition;
        GameEvents.SetShapeInactive += SetShapeInactive;
    }

    private void OnDisable() {
         GameEvents.MoveShapeToStartPosition -= MoveShapeToStartPosition;
         GameEvents.SetShapeInactive -= SetShapeInactive;       
    }

    public bool IsOnStartPosition()
    {
        return _transform.localPosition == _startPosition;
    }

    public bool IsAnyOfShapeSquareActive()
    {
        foreach(var square in _currentShape)
        {
            if(square.gameObject.activeSelf)
                return true;
        }

        return false;
    }

    public void DeactivateShape()
    {
        if(_shapeActive)
        {
            foreach(var square in _currentShape)
            {
                square?.GetComponent<ShapeSquare>().DeactivateShape();
            }
        }

        _shapeActive = false;
    }

    public void SetShapeInactive() //다시 생성되었을때 생성된 블록을 재배치할 수 있게 해줌
    {
        if(IsOnStartPosition() == false && IsAnyOfShapeSquareActive())
        {
            foreach(var square in _currentShape)
            {
                square.gameObject.SetActive(false);  
            }
        }
    }

    public void ActivateShape()
    {
        if(!_shapeActive)
        {
            foreach(var square in _currentShape)
            {
                square?.GetComponent<ShapeSquare>().ActivateShape();
            }

            _shapeActive = true;
        }
    }

    public void RequestNewShape(ShapeData shapeData)
    {
        _transform.localPosition = _startPosition;
        CreateShape(shapeData);
    }

    public void CreateShape(ShapeData shapeData)
    {
        CurrentShapeData = shapeData;
        TotalSquareNumber = GetNumberOfSquares(shapeData);
        
        while(_currentShape.Count <= TotalSquareNumber)
        {
            _currentShape.Add(Instantiate(squareShapeImage, transform) as GameObject);
        }

        foreach(var square in _currentShape)
        {
            square.gameObject.transform.position = Vector3.zero;
            square.gameObject.SetActive(false);
        }

        var squareRect = squareShapeImage.GetComponent<RectTransform>();
        var moveDistance = new Vector2(squareRect.rect.width * squareRect.localScale.x,
            squareRect.rect.height * squareRect.localScale.y);

        int currentINdexInList = 0;

        for(var row = 0; row < shapeData.rows; row++)
        {
            for(var column = 0;column < shapeData.columns; column++)
            {
                if(shapeData.board[row].column[column])
                {
                    _currentShape[currentINdexInList].SetActive(true);
                    _currentShape[currentINdexInList].GetComponent<RectTransform>().localPosition = new Vector2(GetXPositionForShapeSquare(shapeData, column, moveDistance), GetYPositionForShapeSquare(shapeData, row, moveDistance));

                    currentINdexInList++;
                }
            }
        }
    }

    private float GetYPositionForShapeSquare(ShapeData shapeData, int row, Vector2 moveDistance)
    {
        float shiftOnY = 0f;

        if(shapeData.rows > 1)
        {
            if(shapeData.rows % 2 != 0)
            {
                var middleSquareIndex = (shapeData.rows - 1) / 2;
                var multiplier = (shapeData.rows - 1) / 2;

                if(row < middleSquareIndex)
                {
                    shiftOnY = moveDistance.y * 1;
                    shiftOnY *= multiplier;
                }
                else if(row > middleSquareIndex)
                {
                    shiftOnY = moveDistance.y * -1;
                    shiftOnY *= multiplier;
                }
            }
            else
            {
                var middleSquareIndex2 = (shapeData.rows == 2) ? 1 : (shapeData.rows / 2);
                var middleSquareIndex1 = (shapeData.rows == 2) ? 0 : shapeData.rows - 1;
                var multiplier = shapeData.rows / 2;

                if(row == middleSquareIndex1 || row == middleSquareIndex2)
                {
                    if(row == middleSquareIndex2)
                        shiftOnY = (moveDistance.y / 2) * -1;
                    if(row == middleSquareIndex1)
                        shiftOnY = (moveDistance.y / 2);
                }

                if(row < middleSquareIndex1 && row < middleSquareIndex2)
                {
                    shiftOnY = moveDistance.y * 1;
                    shiftOnY *= multiplier;
                }
                else if(row > middleSquareIndex1 && row > middleSquareIndex2)
                {
                    shiftOnY = moveDistance.y * -1;
                    shiftOnY *= multiplier;
                }
            }
        }

        return shiftOnY;
    }

    private float GetXPositionForShapeSquare(ShapeData shapeData, int column, Vector2 moveDistance)
    {
        float shiftOnX = 0f;

        if(shapeData.columns > 1)
        {
            if(shapeData.columns % 2 != 0)
            {
                var middleSquareIndex = (shapeData.columns - 1) / 2;
                var multiplier = (shapeData.columns - 1) / 2;
                if(column < middleSquareIndex)
                {
                    shiftOnX = moveDistance.x * -1;
                    shiftOnX *= multiplier;
                }
                else if(column > middleSquareIndex)
                {
                    shiftOnX = moveDistance.x * 1;
                    shiftOnX *= multiplier;
                }
            }
            else
            {
                var middleSquareIndex2 = (shapeData.columns == 2) ? 1 : (shapeData.columns / 2);
                var middleSquareIndex1 = (shapeData.columns == 2) ? 0 : shapeData.columns - 1;
                var multiplier = shapeData.columns / 2;

                if(column == middleSquareIndex1 || column == middleSquareIndex2)
                {
                    if(column == middleSquareIndex2)
                        shiftOnX = moveDistance.x / 2;
                    if(column == middleSquareIndex1)
                        shiftOnX = (moveDistance.x / 2) * -1;
                }

                if(column < middleSquareIndex1 && column < middleSquareIndex2)
                {
                    shiftOnX = moveDistance.x * -1;
                    shiftOnX *= multiplier;
                }
                else if(column > middleSquareIndex1 && column > middleSquareIndex2)
                {
                    shiftOnX = moveDistance.x * 1;
                    shiftOnX *= multiplier;
                }
            }
        }

        return shiftOnX;
    }

    private int GetNumberOfSquares(ShapeData shapeData)
    {
        int number = 0;

        foreach(var rowData in shapeData.board)
        {
            foreach(var active in rowData.column)
            {
                if(active)
                    number++;
            }
        }

        return number;
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData) {
        this.GetComponent<RectTransform>().localScale = shapeSelectedScale;
        GameEvents.CurrentDraggedShapeIndex = this.ShapeIndex; // 현재 드래그 중인 Shape 인덱스 저장
    }

    public void OnDrag(PointerEventData eventData)
    {
        _transform.anchorMin = new Vector2(0, 0);
        _transform.anchorMax = new Vector2(0, 0);
        _transform.pivot = new Vector2(0, 0);

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, eventData.position, Camera.main, out pos);
        _transform.localPosition = pos + offset;
    }

    public void OnEndDrag(PointerEventData eventData) {
        this.GetComponent<RectTransform>().localScale = _shapeStartScale;
        GameEvents.CheckIfShapeCanBePlaced?.Invoke();
        // 배치 성공 여부 확인 후 성공 시 해당 인덱스 사용하여 새로운 Shape 생성 요청
        bool isPlaced = true; // 실제 게임 로직에 따라 변경 필요
        if (isPlaced) {
            GameEvents.OnShapePlacedSuccessfully?.Invoke(GameEvents.CurrentDraggedShapeIndex);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // PointerUp 이벤트가 발생했을 때 실행될 로직
        isDragging = false; // 예시: 드래깅 상태를 해제
        // 필요한 추가적인 로직을 여기에 구현
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    private void MoveShapeToStartPosition()
    {
        _transform.transform.localPosition = _startPosition;
    }

void Update()
{
    if (isDragging)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        mousePosition.z = transform.position.z; // 오브젝트의 원래 z 위치를 유지하거나 고정된 값을 사용
        transform.position = mousePosition;
    }

    if (isDragging && Input.GetKeyDown(KeyCode.A))
    {
        RotateShape();
    }
}
    public void RotateShape()
    {
        // Z 축 기준으로 -90도 회전하여 시계 방향으로 회전
        transform.Rotate(0f, 0f, -90f);
    }
}


    