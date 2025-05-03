using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    public BoardManager board;

    Vector2Int startCell;
    bool dragging;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startCell = ScreenToCell(Input.mousePosition);
            dragging = true;
            Debug.Log("Down @ " + startCell);
        }
        else if (Input.GetMouseButtonUp(0) && dragging)
        {
            Vector2Int endCell = ScreenToCell(Input.mousePosition);
            float dist = Mathf.Abs(startCell.x - endCell.x) + Mathf.Abs(startCell.y - endCell.y);
            if (dist == 1)
                board.TrySwap(startCell, endCell);

            dragging = false;
            Debug.Log("Up   @ " + endCell);
        }
    }

    Vector2Int ScreenToCell(Vector3 screenPos)
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(screenPos);
        world.z = 0f;

        float halfX = (board.boardSize.x - 1) * 0.5f;
        float halfY = (board.boardSize.y - 1) * 0.5f;

        int x = Mathf.FloorToInt(world.x + halfX + 0.5f);
        int y = Mathf.FloorToInt(halfY - world.y + 0.5f);

        return new Vector2Int(x, y);
    }

}

