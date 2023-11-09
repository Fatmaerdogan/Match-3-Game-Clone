using System.Collections;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public bool isMatched = false;
    public int Column, Row, targetX, targetY, previousColumn, previousRow;


    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist=1f;
    public Sprite SpriteTouchDown, SpriteTouchUp;

    public GameObject otherDot;
    private Vector2 firstTouchPosition, finalTouchPosition, TempPosition;

    [Header("Powerup Stuff")]
    public bool isColumnBomb, isRowBomb;
    public GameObject ColumnArrow,RowArrow;

    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
    }
    
    void Update()
    {
        targetX = Column;
        targetY = Row;
        if (Mathf.Abs(targetX-transform.position.x) > .1)
        {
            TempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, TempPosition, .6f);
            if(Board.instance.allDots[Column, Row] != this.gameObject) Board.instance.allDots[Column, Row] = this.gameObject;
            Events.FindAllMatches?.Invoke();
        } else {
            TempPosition = new Vector2(targetX, transform.position.y);
            transform.position = TempPosition;
            
        }
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            TempPosition = new Vector2( transform.position.x,targetY);
            transform.position = Vector2.Lerp(transform.position, TempPosition, .6f);
            if (Board.instance.allDots[Column, Row] != this.gameObject) Board.instance.allDots[Column, Row] = this.gameObject;
            Events.FindAllMatches?.Invoke();
        }
        else
        {
            TempPosition = new Vector2(transform.position.x, targetY);
            transform.position = TempPosition;
        }
    }
    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(.5f);
        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().Row = Row;
                otherDot.GetComponent<Dot>().Column = Column;
                Row = previousRow;
                Column = previousColumn;
                yield return new WaitForSeconds(.5f);
                Events.StateChange?.Invoke(true,this);
            }
            else
            {
                Events.DestroyMatches?.Invoke();
            }
        }
    }
    private void OnMouseDown()
    {
        if(Board.instance.currentState==GameState.Move){
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GetComponent<SpriteRenderer>().sprite = SpriteTouchDown;
        }

    }
    private void OnMouseUp()
    {
        if (Board.instance.currentState == GameState.Move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GetComponent<SpriteRenderer>().sprite = SpriteTouchUp;
            CalculateAngle();
        }
    }
    void CalculateAngle()
    {
        if (finalTouchPosition.y - firstTouchPosition.y > swipeResist|| finalTouchPosition.x - firstTouchPosition.x > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            Events.StateChange?.Invoke(false, this);
        }
        else {
            Events.StateChange?.Invoke(true, this);
        }
    }
    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && Column < Board.instance.width - 1)
        {
            otherDot = Board.instance.allDots[Column + 1, Row];
            previousRow = Row;
            previousColumn = Column;
            otherDot.GetComponent<Dot>().Column -= 1;
            Column += 1;

        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && Row < Board.instance.height - 1)
        {
            otherDot = Board.instance.allDots[Column, Row + 1];
            previousRow = Row;
            previousColumn = Column;
            otherDot.GetComponent<Dot>().Row -= 1;
            Row += 1;

        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && Column > 0)
        {
            otherDot = Board.instance.allDots[Column - 1, Row];
            previousRow = Row;
            previousColumn = Column;
            otherDot.GetComponent<Dot>().Column += 1;
            Column -= 1;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && Row > 0)
        {
            otherDot = Board.instance.allDots[Column,Row - 1];
            previousRow = Row;
            previousColumn = Column;
            otherDot.GetComponent<Dot>().Row += 1;
            Row -= 1;
        }

        StartCoroutine(CheckMoveCo());
    }
    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(RowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }
    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(ColumnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }
}
