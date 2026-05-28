using UnityEngine;
using UnityEngine.Rendering;

public class GridRenderer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GridManager gridManager;

    void Start()
    {
        gridManager = GridManager.instance;
        if(gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }

        gridManager.OnCrossChanged += HandleCrossChanged;
        gridManager.OnTileChanged += HandleTileChanged;

        renderBoard();

        
    }

    private void OnDestroy()
    {
        if(gridManager == null) { return; }
        gridManager.OnCrossChanged -= HandleCrossChanged;
        gridManager.OnTileChanged -= HandleTileChanged;
    }

    //no implementation yet, but now the grid renderer can track when changes are made to the server authoritative grid

    private void HandleCrossChanged(int x, int y, CrossData data)
    {

    }

    private void HandleTileChanged(int x, int y, TileData data)
    {

    }

    private void renderBoard()
    {
        //Implement board rendering
        //First thing would be to load mass units from the prefab into the scene. 
    }

    void Update()
    {
        
    }
}
