using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SchapeStorage : MonoBehaviour
{
    public List<ShapeData> shapeData;
    public List<Shape> shapeList;

    private void OnEnable() 
    {
        GameEvents.OnShapePlacedSuccessfully += ReplaceShape;
    }

    private void OnDisable() 
    {
        GameEvents.OnShapePlacedSuccessfully -= ReplaceShape;
    }

    private void ReplaceShape(int shapeIndex)
    {
        // shapeIndex에 해당하는 Shape을 새로운 것으로 교체하는 로직
        var newShapeData = GetRandomShapeData();
        shapeList[shapeIndex].RequestNewShape(newShapeData);
    }

    private ShapeData GetRandomShapeData()
    {
        // 랜덤 ShapeData 반환 로직
        int shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
        return shapeData[shapeIndex];
    }

    void Start()
    {
        for (int i = 0; i < shapeList.Count; i++) {
        var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
        shapeList[i].CreateShape(shapeData[shapeIndex]);
        shapeList[i].ShapeIndex = i; // 여기에 인덱스 할당 로직 추가
    }

        foreach(var shape in shapeList)
        {
            var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
            shape.CreateShape(shapeData[shapeIndex]);
        }   
    }

    public Shape GetCurrentSelectedShape()
    {
        foreach(var shape in shapeList)
        {
            if(shape.IsOnStartPosition() == false && shape.IsAnyOfShapeSquareActive())
                return shape;
        }
            Debug.LogError("There is no shape selected!");
            return null;
    }

    private void RequestNewShapes()
    {
        foreach(var shape in shapeList)
        {
            var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
            shape.RequestNewShape(shapeData[shapeIndex]);
        }
    }
}
