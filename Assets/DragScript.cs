﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragScript : MonoBehaviour
{
    float distance = 10;
    float posY;
    float posYnow;

    private void Awake()
    {
        posY = this.transform.position.y;
    }
    private void OnMouseDrag()
    {      
        var barrel = this.transform;        
        Vector3 mousePos = new Vector3(Input.mousePosition.x, barrel.transform.position.y, distance);        
        Vector3 objPos = Camera.main.ScreenToWorldPoint(mousePos);
        // Stopping the barrel from snapping on screen
        var yDiff = objPos.y - posY;
        objPos = objPos - new Vector3(0, yDiff, 0);
        Debug.Log(objPos); // Testing
        barrel.position = objPos;      
    }
}
