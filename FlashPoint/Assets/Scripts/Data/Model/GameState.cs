[System.Serializable]
public class GameState
{
    public int turno;

    public int filas;      // filas jugables (6)
    public int columnas;   // columnas jugables (8)


    public int[] tablero;

    public int[] paredesVerticales;

    public int[] paredesHorizontales;

    public AgentState[] agentes;
}
