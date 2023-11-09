using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    public List<GameObject> currentMatches = new List<GameObject>();
    void Start()
    {
        Events.FindAllMatches += FindAllMatches;
        Events.MatchPiecesOfShape += MatchPiecesOfShape;
    }
    private void OnDestroy()
    {
        Events.FindAllMatches -= FindAllMatches;
        Events.MatchPiecesOfShape -= MatchPiecesOfShape;
    }
    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }
    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(.2f);
        for (int i = 0; i < Board.instance.width; i++)
        {
            for (int j = 0; j < Board.instance.height; j++)
            {
                GameObject currentDot = Board.instance.allDots[i, j];
                Dot currentDotDot = currentDot.GetComponent<Dot>();
                if (currentDot != null)
                {
                    if (i > 0 && i < Board.instance.width - 1)
                    {
                        GameObject leftDot = Board.instance.allDots[i - 1, j];
                        Dot leftDotDot = leftDot.GetComponent<Dot>();
                        GameObject rightDot = Board.instance.allDots[i + 1, j];
                        Dot rightDotDot = rightDot.GetComponent<Dot>();
                        if (leftDot != null && rightDot != null)
                        {
                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {

                                currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot));

                                currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot));

                                GetNearbyPieces(leftDot, currentDot, rightDot);

                            }
                        }
                    }

                    if (j > 0 && j < Board.instance.height - 1)
                    {
                        GameObject upDot = Board.instance.allDots[i, j + 1];
                        Dot upDotDot = upDot.GetComponent<Dot>();
                        GameObject downDot = Board.instance.allDots[i, j - 1];
                        Dot downDotDot = downDot.GetComponent<Dot>();
                        if (upDot != null && downDot != null)
                        {
                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {

                                currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot));

                                currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));

                                GetNearbyPieces(upDot, currentDot, downDot);

                            }
                        }
                    }

                }
            }
        }

    }
    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot1.Row));
        }

        if (dot2.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot2.Row));
        }

        if (dot3.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot3.Row));
        }
        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot1.Column));
        }

        if (dot2.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot2.Column));
        }

        if (dot3.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot3.Column));
        }
        return currentDots;
    }
    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }
    private void AddToListAndMatch(GameObject dot)
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }
   
    public void MatchPiecesOfShape(string shape)
    {
        for (int i = 0; i < Board.instance.width; i++)
        {
            for (int j = 0; j < Board.instance.height; j++)
            {
                if (Board.instance.allDots[i, j] != null)
                {
                    if (Board.instance.allDots[i, j].tag == shape)
                    {
                        Board.instance.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < Board.instance.height; i++)
        {
            if (Board.instance.allDots[column, i] != null)
            {
                dots.Add(Board.instance.allDots[column, i]);
                Board.instance.allDots[column, i].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < Board.instance.width; i++)
        {
            if (Board.instance.allDots[i, row] != null)
            {
                dots.Add(Board.instance.allDots[i, row]);
                Board.instance.allDots[i, row].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }

    public void CheckBombs()
    {
        if (Board.instance.CurrentDot != null)
        {
            if (Board.instance.CurrentDot.isMatched)
            {
                Board.instance.CurrentDot.isMatched = false;
                if ((Board.instance.CurrentDot.swipeAngle > -45 && Board.instance.CurrentDot.swipeAngle <= 45)
                   || (Board.instance.CurrentDot.swipeAngle < -135 || Board.instance.CurrentDot.swipeAngle >= 135))
                {
                    Board.instance.CurrentDot.MakeRowBomb();
                }
                else
                {
                    Board.instance.CurrentDot.MakeColumnBomb();
                }
            }

            else if (Board.instance.CurrentDot.otherDot != null)
            {
                Dot otherDot = Board.instance.CurrentDot.otherDot.GetComponent<Dot>();
                if (otherDot.isMatched)
                {
                    otherDot.isMatched = false;
                    if ((Board.instance.CurrentDot.swipeAngle > -45 && Board.instance.CurrentDot.swipeAngle <= 45)
                   || (Board.instance.CurrentDot.swipeAngle < -135 || Board.instance.CurrentDot.swipeAngle >= 135))
                    {
                        otherDot.MakeRowBomb();
                    }
                    else
                    {
                        otherDot.MakeColumnBomb();
                    }
                }
            }

        }
    }
}
