using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public Action<Vector3> onMouseClicked;
    public bool canClick = true;

    private void Awake() {
        Instance = this;
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && canClick) {
            onMouseClicked?.Invoke(GetMousePosition());
        }
    }

    /// <summary>
    /// Get the mouse position in the world
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMousePosition() {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    
    /// <summary>
    /// Set the click value
    /// </summary>
    /// <param name="value"></param>
    public void SetClick(bool value) {
        canClick = value;
    }  
}
