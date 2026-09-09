// Un paso individual de una onda expansiva (explosion/flashover): o cambia una
// arista (pared/puerta) o cambia una celda de tablero (fuego/humo), nunca ambas.
[System.Serializable]
public class PasoOnda
{
    public bool esPared;    // true = arista (paredesVerticales/Horizontales), false = celda de tablero
    public int row;
    public int col;
    public bool esVertical; // solo aplica si esPared
    public int estadoNuevo; // WallState o CellValue segun esPared
}
