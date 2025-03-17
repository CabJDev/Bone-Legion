// Author: Hope Oberez (H.O)
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

// Controller to handle unit movement
public class UnitController : MonoBehaviour
{

    [SerializeField]
    private Transform selectedAreaTranform;

    public List<Unit> allUnitsList;
    public List<Unit> selectedUnitsList;
    private InputAction pointer;

    private Vector3 startPos;
    private Vector3 currentPos;
    private Vector3 finalPos;

    public static UnitController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        selectedUnitsList = new List<Unit>();
        allUnitsList = new List<Unit>();
        selectedAreaTranform.gameObject.SetActive(false);

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        pointer = InputSystem.actions.FindAction("Pointer");

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            selectedAreaTranform.gameObject.SetActive(true);

            startPos = GetMousePosition();
        }

        if (Input.GetMouseButton(0))
        {
            selectedAreaTranform.gameObject.SetActive(true);

            currentPos = GetMousePosition();


            Vector3 lowerLeft = new Vector3(
                Mathf.Min(startPos.x, currentPos.x),
                Mathf.Min(startPos.y, currentPos.y));

            Vector3 upperRight = new Vector3(
                Mathf.Max(startPos.x, currentPos.x),
                Mathf.Max(startPos.y, currentPos.y));


            selectedAreaTranform.position = lowerLeft;
            selectedAreaTranform.localScale = upperRight - lowerLeft;
        }
        if (Input.GetMouseButtonUp(0))
        {
            selectedAreaTranform.gameObject.SetActive(false);

            finalPos = GetMousePosition();

            Collider2D[] collider2DArray = Physics2D.OverlapAreaAll(startPos, finalPos);


            // Deselect all units
            foreach (Unit unit in selectedUnitsList)
            {
                unit.SetSelectedVisible(false);
            }

            // Select units within area
            selectedUnitsList.Clear();
            GetSelectedUnits();
        }

        if (Input.GetMouseButtonDown(1) && !UIManager.Instance.IsMouseOverUI())
        {
            Vector3 moveToPosition = GetMousePosition();

            // MAX 155 unit selections for now
            List<Vector3> targetPositionList = GetPositionListAround(moveToPosition, new float[] {0.6f, 1f, 1.5f, 2f,2.5f, 3f}, new int[] {5, 10, 20, 30, 40, 50});
            int targetPositionListIndex = 0;
            UIManager.Instance.PlayGame();
            foreach (Unit unit in selectedUnitsList)
            {
                unit.MoveTo(targetPositionList[targetPositionListIndex]);
                targetPositionListIndex = (targetPositionListIndex + 1) % selectedUnitsList.Count;
            }
        }
    }

    private void GetSelectedUnits()
    {
        float leftPosX;
        float rightPosX;
        float upperPosY;
        float lowerPosY;

        if (startPos.x < finalPos.x) {
            leftPosX = startPos.x;
            rightPosX = finalPos.x;
        }
        else
        {
            leftPosX = finalPos.x;
            rightPosX = startPos.x;
        }

        if (startPos.y < finalPos.y)
        {
            lowerPosY = startPos.y;
            upperPosY = finalPos.y;
        }
        else
        {
            lowerPosY = finalPos.y;
            upperPosY = startPos.y;
        }

        foreach(Unit unit in allUnitsList)
        {
            Vector3 position = unit.GetCurrentPosition();
            Debug.Log("===" + position + " " + startPos + " " + finalPos);
            Debug.Log(leftPosX + " " + rightPosX);
            Debug.Log(upperPosY + " " + lowerPosY);
            Debug.Log(position.x > leftPosX);
            Debug.Log(position.x < rightPosX);
            Debug.Log(position.y > lowerPosY);
            Debug.Log(position.y < upperPosY);
            if (position.x > leftPosX && position.x < rightPosX &&
                position.y > lowerPosY && position.y < upperPosY)
            {
                selectedUnitsList.Add(unit);
                unit.SetSelectedVisible(true);

            }
        }
    }

    private Vector3 GetMousePosition()
    {
        Vector2 pointerPos = pointer.ReadValue<Vector2>();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, 0));

        return worldPos;
    }

    private List<Vector3> GetPositionListAround(Vector3 startPosition, float[] ringDistanceArray, int[] ringPositionCountArray)
    {
        List<Vector3> positionList = new List<Vector3>();
        positionList.Add(startPosition);

        for (int i = 0; i < ringPositionCountArray.Length; i++)
        {
            positionList.AddRange(GetPositionListAround(startPosition, ringDistanceArray[i], ringPositionCountArray[i]));
        }
        return positionList;
    }

    private List<Vector3> GetPositionListAround(Vector3 startPosition, float distance, int positionCount)
    {
        List<Vector3> positionList = new List<Vector3>();
        for (int i = 0; i < positionCount; i++)
        {
            float angle = i * (360f / positionCount);
            Vector3 dir = ApplyRotationToVector(new Vector3(1, 0), angle);
            Vector3 position = startPosition + dir * distance;
            positionList.Add(position);
        }
        return positionList;
    }

    private Vector3 ApplyRotationToVector(Vector3 vec, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vec;
    }

}
