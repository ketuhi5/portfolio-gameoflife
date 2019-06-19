using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOfLife : MonoBehaviour
{
    private const int GRIDHEIGHT = 100;
    private const int GRIDWIDTH = 100;

    public static event Action<int> GenerationChanged;

    private bool simulationStarted = false;
    private float timeElapsed;

    [SerializeField]
    private Button _SimBtn;

    [SerializeField]
    private Camera _GameCam;

    [SerializeField]
    private Transform _GameBoard;

    [SerializeField]
    private GameObject _GameCell;

    private Vector2 CellCenterOffset = new Vector2(0.5f, 0.5f);
    private GameObject[,] gameGrid;
    private int currentGeneration;
    private float simInterval = 1.0f;
    private Text simBtnText;

    private void Awake()
    {
        gameGrid = new GameObject[GRIDWIDTH, GRIDWIDTH];
        simBtnText = _SimBtn.GetComponentInChildren<Text>();
    }

    private void Start()
    {
        OnGenerationChanged(currentGeneration = 0);
    }
    
    void Update()
    {
        if (simulationStarted)
        {
            timeElapsed += Time.deltaTime;
            int simTimePassed = Mathf.FloorToInt(timeElapsed / simInterval);
            timeElapsed -= simTimePassed * simInterval;
            for (int i = 0; i < simTimePassed; i++)
            {
                ChangeGeneration();
            }
        }
        else
        {
            bool mouseClicked = Input.GetMouseButtonDown(0);
            if (mouseClicked)
            {
                Vector2 pos = _GameCam.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int gameBoardPos = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
                ChangeCell(gameBoardPos);
            }
        }
    }

    private void ChangeCell(Vector2Int gameBoardPos)
    {
        try
        {
            GameObject cell = gameGrid[gameBoardPos.x, gameBoardPos.y];
            if (cell == null)
                CreateCell(gameBoardPos);
            else
                DestroyCell(gameBoardPos);
        }
        catch 
        {
            return;
        }
    }

    private void OnGenerationChanged(int gen)
    {
        if (GenerationChanged != null)
            GenerationChanged(gen);
    }    

    private void ChangeGeneration()
    {
        OnGenerationChanged(++currentGeneration);
        List<Vector2Int> toSpawn = new List<Vector2Int>();
        List<Vector2Int> toBeKilled = new List<Vector2Int>();
        for (int i = 0; i < GRIDWIDTH; i++)
        {
            for (int j = 0; j < GRIDHEIGHT; j++)
            {
                bool cellHasLife = gameGrid[i, j] != null;
                int numNeighbours = GetNeighboursCount(i, j);
                if (cellHasLife)
                {
                    if (numNeighbours < 2 || numNeighbours > 3)
                    {
                        toBeKilled.Add(new Vector2Int(i, j));
                    }
                }
                else
                {
                    if (numNeighbours == 3)
                    {
                        toSpawn.Add(new Vector2Int(i, j));
                    }
                }
            }
        }
        foreach (Vector2Int cell in toSpawn)
        {
            CreateCell(cell);
        }
        foreach (Vector2Int cell in toBeKilled)
        {
            DestroyCell(cell);
        }
    }

    private int GetNeighboursCount(int x, int y)
    {
        int numNeighbours = 0;

        int minXRange = x > 0 ? -1 : 0;
        int maxXRange = x < GRIDWIDTH - 1 ? 1 : 0;
        int minYRange = y > 0 ? -1 : 0;
        int maxYRange = y < GRIDHEIGHT - 1 ? 1 : 0;

        for (int i = minXRange; i <= maxXRange; i++)
        {
            for (int j = minYRange; j <= maxYRange; j++)
            {
                if (i == 0 && j == 0)
                { // Player need not to be counted
                    continue;
                }
                bool neighbourIsAlive = gameGrid[x + i, y + j] != null;
                numNeighbours += neighbourIsAlive ? 1 : 0;
            }
        }
        return numNeighbours;
    }

    private void CreateCell(Vector2Int cellPos)
    {        
        GameObject newCell = Instantiate(_GameCell);
        newCell.transform.SetParent(_GameBoard);
        Vector2 pos = cellPos + CellCenterOffset;
        newCell.transform.position = new Vector3(pos.x, pos.y, -1.0f);
        gameGrid[cellPos.x, cellPos.y] = newCell;
    }

    private void DestroyCell(Vector2Int cellPos)
    {
        GameObject deadCell = gameGrid[cellPos.x, cellPos.y];
        if (deadCell != null)
        {
            Destroy(deadCell);
        }
        gameGrid[cellPos.x, cellPos.y] = null;
    }

    public void SimulateGOL()
    {
        simulationStarted = !simulationStarted;

        simBtnText.text = simulationStarted ? "Pause Game of Life" : "Simulate Game of Life";
    }
}
