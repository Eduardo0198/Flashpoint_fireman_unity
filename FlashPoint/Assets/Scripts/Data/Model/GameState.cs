[System.Serializable]
public class GameState
{
    public int turno;

    public int filas;      // filas jugables (6)
    public int columnas;   // columnas jugables (8)


    public int[] tablero;

    public int[] pois;          // 6x8 flat, 0=vacio, 1=falsa_alarma, 2=victima
    public bool[] poisRevelado; // 6x8 flat, true = ya revelado (V/F), false = oculto (?)

    public int[] paredesVerticales;

    public int[] paredesHorizontales;

    public AgentState[] agentes;

    public int victimasRescatadas;
    public int victimasPerdidas;
    public int danoEstructura; // 0-24, colapsa el edificio en 24 (derrota)
}
