using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// GameAssets class to access the tile color scriptable object through script
/// </summary>
public class GameAssets : MonoBehaviour
{
    /// <summary>
    /// Singleton pattern
    /// </summary>
    private static GameAssets instance;
    public static GameAssets Instance {
        get {
            if (instance == null) {
                instance = Instantiate(Resources.Load<GameAssets>("GameAssets")).GetComponent<GameAssets>();
            }
            return instance;
        }
    }
    
    [SerializeField] public List<TileColorSO> tileColorSO;
    [SerializeField] public TextMeshProUGUI shuffleText;

    private void Awake() {
        instance = this;
    }
}
