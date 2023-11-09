
using System;

public class Events
{
    public static Action<bool,Dot> StateChange;
    public static Action DestroyMatches;
    public static Action FindAllMatches;
    public static Action<String> MatchPiecesOfShape;
}
public enum GameState
{
    Wait, Move
}