using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
public class GridSystem : MonoBehaviour
{
    [Header("References")]
    public static GridSystem currentGrid;
    private GameManager manager;
    [SerializeField] private SpriteMask spriteMask;

    [Space(10)]
    [Header("Grid Properties")]
    [SerializeField] public GridLayout gridLayout;
    [SerializeField] private Tilemap tilemap;
    public Bounds bounds { get; private set; }
    public Bounds fullBounds { get; private set; }

    [Space(10)]
    [Header("Class Properties")]
    private List<TileColorSO> tileColorSO;
    private Dictionary<Vector3Int, TileColor> tileColors = new Dictionary<Vector3Int, TileColor>();

    #region Initalize
    private void Awake()
    {
        currentGrid = this;
    }
    public void Initalize()
    {
        manager = GameManager.Instance;
        InputManager.Instance.onMouseClicked += HandleMouseClick;

        SetBounds();
        SetSpriteMask();
        SetColors();
        GenerateRandomTiles();
        SetNewSprites();
        while(CheckDeadlock())
        {
            Shuffle();
        }
    }

    /// <summary>
    /// Set the bounds of the grid
    /// </summary>
    private void SetBounds()
    {
        Vector3 initialpos = gridLayout.transform.position;
        if (manager.n % 2 != 0)
        {
            initialpos = new Vector3(initialpos.x + 0.5f, initialpos.y, initialpos.z);
            Camera.main.transform.position += new Vector3(0.5f, 0f, 0f);
        }
        if (manager.m % 2 != 0)
        {
            initialpos = new Vector3(initialpos.x, initialpos.y + 0.5f, initialpos.z);
        }
        bounds = new Bounds(initialpos, new Vector3(manager.n, manager.m, 1));
        fullBounds = new Bounds(initialpos + new Vector3(0f, 2f, 0f), new Vector3(manager.n, manager.m + 4, 1));
    }

    /// <summary>
    /// Set the sprite mask bounds
    /// </summary>
    private void SetSpriteMask()
    {
        spriteMask.transform.position = new Vector3(bounds.center.x, bounds.center.y - 0.08f, spriteMask.transform.position.z);
        spriteMask.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1);
    }

    /// <summary>
    /// Draw the grid bounds in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (gridLayout != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(fullBounds.center, fullBounds.size);
        }
    }

    /// <summary>
    /// Set the colors of the tiles in the grid according to k
    /// </summary>
    public void SetColors()
    {
        tileColorSO = new List<TileColorSO>();
        for (int i = 0; i < manager.k; i++)
        {
            tileColorSO.Add(GameAssets.Instance.tileColorSO[i]);
        }
    }

    #endregion

    #region Get Set

    /// <summary>
    /// Set the tile at a given position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="tile"></param>
    /// <param name="color"></param>
    public void SetTileAtPosition(Vector3Int position, Tile tile, TileColor color)
    {
        tilemap.SetTile(position, tile);
        tileColors[position] = color;
    }

    /// <summary>
    /// Get the tile at a given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Tile GetTile(Vector3Int position)
    {
        return tilemap.GetTile<Tile>(position);
    }

    /// <summary>
    /// Get the tile color at a given position
    /// </summary>
    /// <param name="tileColor">
    /// Custom enum for tile colors
    /// </param>
    /// <returns></returns>
    public TileColorSO GetSObyColor(TileColor tileColor)
    {
        foreach (TileColorSO tileColorSO in tileColorSO)
        {
            if (tileColorSO.color == tileColor)
            {
                return tileColorSO;
            }
        }
        return null;
    }

    /// <summary>
    /// Set the new sprites after a match, shuffle or initalization
    /// </summary>
    public void SetNewSprites()
    {
        List<List<Vector3Int>> allGroups = GetAllTileGroups();
        foreach (var group in allGroups)
        {
            TileColorSO tileColorSO = GetSObyColor(tileColors[group[0]]);
            for (int i = 0; i < group.Count; i++)
            {
                if (group.Count >= manager.c)
                {
                    SetTileAtPosition(group[i], tileColorSO.tileC, tileColors[group[0]]);
                }
                else if (group.Count >= manager.b)
                {
                    SetTileAtPosition(group[i], tileColorSO.tileB, tileColors[group[0]]);
                }
                else if (group.Count >= manager.a)
                {
                    SetTileAtPosition(group[i], tileColorSO.tileA, tileColors[group[0]]);
                }
                else
                {
                    SetTileAtPosition(group[i], tileColorSO.tileDefaut, tileColors[group[0]]);
                }
            }
        }
    }

    /// <summary>
    /// Get the min and max x position of the tiles and narrow down the tiles to be checked
    /// for optimization purposes
    /// </summary>
    /// <param name="positions"></param>
    /// <returns></returns>
    private Vector2Int GetMinMaxXTile(List<Vector3Int> positions)
    {
        Vector3Int tempMin = new Vector3Int(1000, 0, 0);
        Vector3Int tempMax = new Vector3Int(-1000, 0, 0);
        foreach (Vector3Int pos in positions)
        {
            if (pos.x < tempMin.x)
            {
                tempMin.x = pos.x;
            }
            if (pos.x > tempMax.x)
            {
                tempMax.x = pos.x;
            }
        }
        return new Vector2Int(tempMin.x, tempMax.x);
    }

    private List<List<Vector3Int>> GetAllTileGroups()
    {
        List<List<Vector3Int>> allGroups = new List<List<Vector3Int>>();
        Vector3Int min = gridLayout.WorldToCell(bounds.min);
        Vector3Int max = gridLayout.WorldToCell(bounds.max) - new Vector3Int(1, 1, 0);
        List<Vector3Int> visited = new List<Vector3Int>();

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (visited.Contains(cellPosition))
                {
                    continue;
                }
                List<Vector3Int> group = CheckNeighborTiles(cellPosition);
                allGroups.Add(group);
                visited.AddRange(group);
            }
        }
        // foreach (var group in allGroups)
        // {
        //     Debug.Log("group count: " + group.Count);
        // }
        return allGroups;
    }

    #endregion

    #region Core methods & click handling
    public void HandleMouseClick(Vector3 pos)
    {
        StartCoroutine(MouseRoutine(pos));
    }

    /// <summary>
    /// Main routine for mouse click
    /// </summary>
    /// <param name="pos">
    /// clicked position on the grid
    /// </param>
    /// <returns></returns>
    public IEnumerator MouseRoutine(Vector3 pos)
    {
        InputManager.Instance.SetClick(false);
        Vector3Int cellPosition = gridLayout.WorldToCell(pos);
        if (!bounds.Contains(cellPosition) || bounds.max.y == cellPosition.y)
        {
            InputManager.Instance.SetClick(true);
            yield break;
        }
        List<Vector3Int> visited = CheckNeighborTiles(cellPosition);
        if (visited.Count < 2)
        {
            InputManager.Instance.SetClick(true);
            yield break;
        }
        foreach (Vector3Int position in visited)
        {
            yield return StartCoroutine(DestroyTile(position));
        }

        Vector2Int minMaxX = GetMinMaxXTile(visited);

        Vector3Int min = gridLayout.WorldToCell(new Vector3(minMaxX.x, fullBounds.min.y, fullBounds.min.z));
        Vector3Int max = gridLayout.WorldToCell(new Vector3(minMaxX.y, fullBounds.max.y, fullBounds.min.z));

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (tilemap.GetTile<Tile>(tilePosition) != null)
                {
                    yield return StartCoroutine(FallDown(tilePosition));
                }
            }
        }
        GenerateRandomTiles(min, max);
        SetNewSprites();
        while(CheckDeadlock())
        {
            Shuffle();
        }
        InputManager.Instance.SetClick(true);
    }


    /// <summary>
    /// Destroy the tile at a given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private IEnumerator DestroyTile(Vector3Int position)
    {
        Tile tile = tilemap.GetTile<Tile>(position);
        if (tile != null)
        {
            tilemap.SetTile(position, null);
            tileColors.Remove(position);
        }
        yield return new WaitForSeconds(0.01f);
    }

    /// <summary>
    /// Make the tile fall down to the bottom
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private IEnumerator FallDown(Vector3Int position)
    {
        Vector3Int current = position;
        Vector3Int below = current + new Vector3Int(0, -1, 0);
        while (tilemap.GetTile<Tile>(below) == null && fullBounds.Contains(gridLayout.CellToWorld(below)))
        {
            Tile tile = tilemap.GetTile<Tile>(current);
            tilemap.SetTile(current, null);
            tilemap.SetTile(below, tile);
            tileColors[below] = tileColors[current];
            tileColors.Remove(current);
            current = below;
            below = current + new Vector3Int(0, -1, 0);
            yield return new WaitForSeconds(0.01f);
        }
    }


    /// <summary>
    /// Check the neighboring tiles of a given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public List<Vector3Int> CheckNeighborTiles(Vector3Int position)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        List<Vector3Int> visited = new List<Vector3Int>();
        queue.Enqueue(position);
        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            visited.Add(current);
            if (!tileColors.ContainsKey(current))
            {
                continue;
            }
            if(!bounds.Contains(current) || bounds.max.y == current.y)
            {
                Debug.Log("out of bounds" + current);
                continue;
            }
            TileColor currentColor = tileColors[current];
            Vector3Int[] neighbors = new Vector3Int[]
            {
                current + new Vector3Int(1, 0, 0),
                current + new Vector3Int(-1, 0, 0),
                current + new Vector3Int(0, 1, 0),
                current + new Vector3Int(0, -1, 0)
            };
            foreach (Vector3Int neighbor in neighbors)
            {
                if (visited.Contains(neighbor))
                {
                    continue;
                }
                if (!bounds.Contains(neighbor) || bounds.max.y == neighbor.y)
                {
                    continue;
                }
                if (tileColors.ContainsKey(neighbor) && tileColors[neighbor] == currentColor)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }
        return visited;
    }


    /// <summary>
    /// loop through the grid and generate random tiles if empty
    /// </summary>
    public void GenerateRandomTiles()
    {
        Vector3Int fullBoundsMin = gridLayout.WorldToCell(fullBounds.min);
        Vector3Int fullBoundsMax = gridLayout.WorldToCell(fullBounds.max) - new Vector3Int(1, 1, 0);

        for (int x = fullBoundsMin.x; x <= fullBoundsMax.x; x++)
        {
            for (int y = fullBoundsMin.y; y <= fullBoundsMax.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                Tile tile = tilemap.GetTile<Tile>(tilePosition);
                if (tile == null)
                {
                    int randomTileIndex = Random.Range(0, tileColorSO.Count);
                    TileColorSO randomTileColorSO = tileColorSO[randomTileIndex];
                    Tile randomTile = randomTileColorSO.tileDefaut;
                    tilemap.SetTile(tilePosition, randomTile);
                    tileColors[tilePosition] = randomTileColorSO.color;
                }
            }
        }
    }


    /// <summary>
    /// (overload method) Generate random tiles in a given range if empty
    /// for optimisation purposes
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void GenerateRandomTiles(Vector3Int min, Vector3Int max)
    {
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                Tile tile = tilemap.GetTile<Tile>(tilePosition);
                if (tile == null)
                {
                    int randomTileIndex = Random.Range(0, tileColorSO.Count);
                    TileColorSO randomTileColorSO = tileColorSO[randomTileIndex];
                    Tile randomTile = randomTileColorSO.tileDefaut;
                    tilemap.SetTile(tilePosition, randomTile);
                    tileColors[tilePosition] = randomTileColorSO.color;
                }
            }
        }
    }


    #endregion

    #region Shuffle

    /// <summary>
    /// Check if there is a deadlock in the grid
    /// </summary>
    /// <returns></returns>
    public bool CheckDeadlock()
    {
        List<List<Vector3Int>> allGroups = GetAllTileGroups();
        foreach (var group in allGroups)
        {
            if (group.Count >= 2)
            {
                return false;
            }
        }
        GameAssets.Instance.shuffleText.enabled = true;
        Invoke("ResetShuffleText", 2f);
        return true;
    }

    /// <summary>
    /// We store all our tile positions and color counts
    /// and clear the grid
    /// then we start to set tiles randomly starting from the first position
    /// for the next position in the list, if we can't get the prevliously setled color,
    /// we increase the rate for the next color to be prevliously setled color.
    /// that will help us to get rid of the deadlock without random shuffling
    /// </summary>
    public void Shuffle() {
        Debug.Log("Shuffling");
        List<Vector3Int> allPositions = new List<Vector3Int>();
        Dictionary<TileColor, int> gridColorsCount = new Dictionary<TileColor, int>();
        Vector3Int min = gridLayout.WorldToCell(bounds.min);
        Vector3Int max = gridLayout.WorldToCell(bounds.max) - new Vector3Int(1, 1, 0);
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                allPositions.Add(tilePosition);
                AddCountDictionary(gridColorsCount, tileColors[tilePosition]);
            }
        }
        ClearGrid();
        float randomRate = 1 / (float) gridColorsCount.Count;
        TileColor prevPickedColor = gridColorsCount.Keys.GetEnumerator().Current;
        for (int i = 0; i < allPositions.Count; i++)
        {
            List<TileColor> colors = new List<TileColor>(gridColorsCount.Keys);
            TileColor randomTileColor = GetRandomWithRate(colors, randomRate, prevPickedColor);
            if(randomTileColor != prevPickedColor)
            {
                randomRate += randomRate;
            }
            else{
                randomRate = 1 / (float) gridColorsCount.Count;
            }
            TileColorSO randomTileColorSO = GetSObyColor(randomTileColor);
            Tile randomTile = randomTileColorSO.tileDefaut;
            tilemap.SetTile(allPositions[i], randomTile);
            tileColors[allPositions[i]] = randomTileColorSO.color;
            RemoveCountDictionary(gridColorsCount, randomTileColor);
            prevPickedColor = randomTileColor;
        }
        GenerateRandomTiles();
    }

    /// <summary>
    /// if there is a dictionary for counting objects, add the value to the dictionary
    /// or increment the value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keyValuePairs"></param>
    /// <param name="value"></param>
    public void AddCountDictionary<T>(Dictionary<T, int> keyValuePairs, T value)
    {
        if (keyValuePairs.ContainsKey(value))
        {
            keyValuePairs[value]++;
        }
        else
        {
            keyValuePairs.Add(value, 1);
        }
    }

    /// <summary>
    /// if there is a dictionary for counting objects, remove the value from the dictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keyValuePairs"></param>
    /// <param name="value"></param>
    public void RemoveCountDictionary<T>(Dictionary<T, int> keyValuePairs, T value)
    {
        if (keyValuePairs.ContainsKey(value))
        {
            keyValuePairs[value]--;
            if(keyValuePairs[value] == 0)
            {
                keyValuePairs.Remove(value);
            }
        }
    }

    /// <summary>
    /// Assume that we want a value from a value list
    /// and we want to get it with a certain success rate
    /// if we can't get the wanted value, we get a random value that is not the wanted value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="rate"></param>
    /// <param name="wantedValue"></param>
    /// <returns></returns>
    public T GetRandomWithRate<T>(List<T> values, float rate, T wantedValue)
    {
        float random = Random.Range(0f, 1f);
        if (random <= rate && values.Contains(wantedValue))
        {
            return wantedValue;
        }
        else
        {
            while(true)
            {
                T randomValue = values[Random.Range(0, values.Count)];
                if (!randomValue.Equals(wantedValue))
                {
                    return randomValue;
                }
            }
        }
    }

    /// <summary>
    /// Clear the grid
    /// </summary>
    public void ClearGrid()
    {
        tilemap.ClearAllTiles();
        tileColors.Clear();
    }

    /// <summary>
    /// Reset the shuffle text
    /// </summary>
    public void ResetShuffleText() {
        GameAssets.Instance.shuffleText.enabled = false;
    }

    #endregion
}