using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameManager class to store the values of the sliders
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int a;
    public int b;
    public int c;
    public int m;
    public int n;
    public int k;


    private void Awake() {
        Instance = this;
    }
}
