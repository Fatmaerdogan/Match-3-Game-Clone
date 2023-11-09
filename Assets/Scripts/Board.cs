using System.Collections;
using UnityEngine;

public class Board : MonoBehaviour
{
    public GameState currentState=GameState.Move;
    public int width, height,offSet;
    public GameObject[,] allDots;
    public GameObject tilePrefab,DestroyEffect;
    public GameObject[] Dots;
    public Dot CurrentDot;
    private FindMatches findMatches;

    public static Board instance;
    private void Awake()=>  instance = this;
    void Start()
    {
        Events.StateChange += StateChanged;
        Events.DestroyMatches += DestroyMatches;
        findMatches = GameObject.FindObjectOfType<FindMatches>();
        allDots = new GameObject[width, height];
        SetUp();
    }
    private void OnDestroy()
    {
        Events.StateChange -= StateChanged;
        Events.DestroyMatches -= DestroyMatches;
    }
    public void StateChanged(bool temp,Dot dot)
    {
        if (temp)
        {
            CurrentDot = null;
            currentState = GameState.Move;
        }
        else
        {
            currentState = GameState.Wait;
            CurrentDot = dot;
        }
    }
    void SetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPos = new Vector2(i, j+offSet);
                GameObject Tile = Instantiate(tilePrefab, tempPos, Quaternion.identity) as GameObject;
                Tile.transform.parent = this.transform;
           
                int dotToUse = Random.Range(0, Dots.Length);
                int maxIterations = 0;
                while (MatchesAt(i, j, Dots[dotToUse]) && maxIterations < 100)
                {
                    dotToUse = Random.Range(0, Dots.Length);
                    maxIterations++;
                }
                maxIterations = 0;
                GameObject dot = Instantiate(Dots[dotToUse], Tile.transform) as GameObject;
                Tile.GetComponent<Dot>().Row = j;
                Tile.GetComponent<Dot>().Column = i;
                Tile.gameObject.tag = dot.gameObject.tag;
                dot.name = Tile.name = "(" + i + "," + j + ")";
                allDots[i, j] = Tile;
            }
        }
    }
    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                return true;
            if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                return true;
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            if(findMatches.currentMatches.Count==4|| findMatches.currentMatches.Count == 7)
            {
                findMatches.CheckBombs();
            }
            Destroy(Instantiate(DestroyEffect, allDots[column, row].transform.position, Quaternion.identity), .5f);
            Destroy(allDots[column, row],.5f);
            allDots[column, row] = null;
        }
    }
    public void DestroyMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        findMatches.currentMatches.Clear();
        StartCoroutine(DecreaseRowCo());
    }
    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(.5f);
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().Row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }
    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j+offSet);
                    int dotToUse = Random.Range(0, Dots.Length);
                    GameObject piece = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                    GameObject dot = Instantiate(Dots[dotToUse], piece.transform) as GameObject;
                    piece.gameObject.tag = dot.gameObject.tag;
                    allDots[i, j] = dot;
                    piece.GetComponent<Dot>().Row = j;
                    piece.GetComponent<Dot>().Column = i;
                }
            }
        }
    }
    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                        return true;
                }
            }
        }
        return false;
    }
    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);
        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        findMatches.currentMatches.Clear();
        yield return new WaitForSeconds(.5f);
        currentState = GameState.Move;
    }
}
