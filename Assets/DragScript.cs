using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragScript : MonoBehaviour
{
    float distance = 10; 
    
    private void Awake()
    {
        
    }
    private void OnMouseDrag()
    {
        var barrel = this.transform;       
        Vector3 mousePos = new Vector3(Input.mousePosition.x, barrel.position.y, distance); 
        Vector3 objPos = Camera.main.ScreenToWorldPoint(mousePos);
        barrel.position = objPos;       
    }
}
