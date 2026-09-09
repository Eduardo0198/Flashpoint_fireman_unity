[System.Serializable]
public class TurnoPayload
{
    public int turno;

    // Se reproducen en este orden, uno por uno (ver Framework/Controllers/TurnoController).
    public EventoTurno[] eventos;

    // Snapshot completo al final del turno. Sirve de red de seguridad: despues de
    // reproducir los eventos, se aplica igual que un ApplyUpdate normal para
    // reconciliar cualquier cosa que los eventos todavia no cubran.
    public GameState estadoFinal;
}
