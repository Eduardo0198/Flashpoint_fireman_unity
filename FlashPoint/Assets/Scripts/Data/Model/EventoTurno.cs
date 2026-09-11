//
// tipo               | campos que usa                              | estado
// -------------------|----------------------------------------------|------------------
// tirar_dados        | dado1, dado2                                 | implementado
// humo_cae           | row, col                                     | implementado
// poi_muere          | row, col, tipoPoi                             | implementado
// explosion          | pasos[]                                       | implementado
// flashover          | pasos[]                                       | implementado
// poi_cae            | row, col, tipoPoi, revelado, apagaFuego       | implementado
// agente_mueve       | agenteId, row, col                            | pendiente (no hay agente)
// agente_apaga_fuego | agenteId, row, col                            | pendiente
// agente_abre_puerta / agente_cierra_puerta | agenteId, row, col, esVertical | pendiente
// bombero_muere      | agenteId                                      | pendiente
[System.Serializable]
public class EventoTurno
{
    public string tipo;

    public int agenteId;
    public int row;
    public int col;
    public bool esVertical;   // para eventos de pared/puerta
    public int estadoNuevo;   // WallState o CellValue como entero, segun el tipo
    public int dado1;
    public int dado2;
    public int tipoPoi;       // PoiTipo como entero
    public bool revelado;
    public bool apagaFuego;   // solo para "poi_cae"
    public PasoOnda[] pasos;  // solo para "explosion" / "flashover"
}
