

[System.Serializable]
public class GameState
{
    public int turno;
    public int width;
    public int height;
    public int[] fuego;
    public int[] pois;
    public AgentState[] agentes;
}