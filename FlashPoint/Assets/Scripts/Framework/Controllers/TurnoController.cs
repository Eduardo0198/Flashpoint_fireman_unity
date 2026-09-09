using System.Collections;
using UnityEngine;

// Reproduce un TurnoPayload como una secuencia de eventos con su propio ritmo,
// en vez de aplicar todos los cambios de golpe.
public class TurnoController : MonoBehaviour
{
    [SerializeField] BoardController board;
    [SerializeField] DiceRoller diceRoller;
    [SerializeField] AgentRosterController roster;

    [Header("Ritmo")]
    public float delayEntreEventos = 1.5f; // pausa entre cada evento del turno, sin importar el tipo
    [SerializeField] float delayPasoOnda = 0.4f;
    [SerializeField] float delayPoiMuereAntesDeQuitar = 1.5f;
    [SerializeField] float delaySinEfecto = 0.3f; 

    [Header("Efectos (opcionales, sin arte todavia)")]
    [SerializeField] GameObject efectoHumoPrefab;
    [SerializeField] GameObject efectoExplosionPrefab;
    [SerializeField] GameObject efectoPoiCaePrefab;

    [Header("Depuracion (sin servidor Python)")]
    [SerializeField] TextAsset debugTurnoJson;

    Coroutine reproduccionActual;
    TurnoPayload payloadActual;
    bool pausado;

    // Arranca la reproduccion de un turno. Si ya habia una corriendo, la corta
    // (no se acumulan turnos encimados).
    public void IniciarTurno(TurnoPayload payload)
    {
        if (reproduccionActual != null)
        {
            StopCoroutine(reproduccionActual);
        }

        payloadActual = payload;
        pausado = false;

        if (roster != null && payload.estadoFinal != null) roster.Actualizar(payload.estadoFinal.agentes);

        reproduccionActual = StartCoroutine(ReproducirTurnoCoroutine(payload));
    }

    // Boton Pausa: congela la reproduccion donde va; un segundo clic la reanuda.
    public void TogglePausa()
    {
        if (reproduccionActual == null) return;
        pausado = !pausado;
    }

    // Boton Adelantar: corta la animacion en curso y aplica de una vez el
    // resultado final del turno.
    public void Adelantar()
    {
        if (reproduccionActual != null)
        {
            StopCoroutine(reproduccionActual);
            reproduccionActual = null;
        }

        pausado = false;

        if (payloadActual?.estadoFinal != null)
        {
            board.ApplyUpdate(payloadActual.estadoFinal);
        }
    }

    IEnumerator ReproducirTurnoCoroutine(TurnoPayload payload)
    {
        if (payload.eventos != null)
        {
            foreach (var evento in payload.eventos)
            {
                yield return ReproducirEvento(evento);
                yield return EsperarConPausa(delayEntreEventos);
            }
        }

        if (payload.estadoFinal != null)
        {
            board.ApplyUpdate(payload.estadoFinal);
        }

        reproduccionActual = null;
    }

    IEnumerator EsperarConPausa(float segundos)
    {
        float restante = segundos;
        while (restante > 0f)
        {
            if (!pausado) restante -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ReproducirEvento(EventoTurno evento)
    {
        switch (evento.tipo)
        {
            case "tirar_dados":
                if (diceRoller != null) yield return diceRoller.Tirar(evento.dado1, evento.dado2);
                break;

            case "humo_cae":
                board.AplicarCambioFuego(new BoardLayoutRequirement.FireCellChange
                {
                    Row = evento.row,
                    Col = evento.col,
                    EstadoNuevo = CellValue.Humo,
                });
                yield return ReproducirEfectoOpcional(efectoHumoPrefab, evento.row, evento.col);
                break;

            case "poi_muere":
                yield return board.RevelarYQuitarPoi(evento.row, evento.col, (PoiTipo)evento.tipoPoi, delayPoiMuereAntesDeQuitar);
                break;

            case "explosion":
            case "flashover":
                yield return ReproducirOnda(evento.pasos);
                break;

            case "poi_cae":
                board.AplicarCambioPoi(new BoardLayoutRequirement.PoiChange
                {
                    Row = evento.row,
                    Col = evento.col,
                    TipoNuevo = (PoiTipo)evento.tipoPoi,
                    RevNuevo = evento.revelado,
                });
                yield return ReproducirEfectoOpcional(efectoPoiCaePrefab, evento.row, evento.col);
                if (evento.apagaFuego)
                {
                    board.AplicarCambioFuego(new BoardLayoutRequirement.FireCellChange
                    {
                        Row = evento.row,
                        Col = evento.col,
                        EstadoNuevo = CellValue.Vacio,
                    });
                }
                break;

            case "agente_mueve":
            case "agente_apaga_fuego":
            case "agente_abre_puerta":
            case "agente_cierra_puerta":
            case "bombero_muere":
            
                Debug.Log($"TurnoController: evento pendiente de implementar '{evento.tipo}'.");
                break;

            default:
                Debug.LogWarning($"TurnoController: tipo de evento desconocido '{evento.tipo}'.");
                break;
        }
    }

    IEnumerator ReproducirOnda(PasoOnda[] pasos)
    {
        if (pasos == null) yield break;

        foreach (var paso in pasos)
        {
            if (paso.esPared)
            {
                board.AplicarCambio(new BoardLayoutRequirement.WallChange
                {
                    Row = paso.row,
                    Col = paso.col,
                    EsVertical = paso.esVertical,
                    EstadoNuevo = (WallState)paso.estadoNuevo,
                });
            }
            else
            {
                board.AplicarCambioFuego(new BoardLayoutRequirement.FireCellChange
                {
                    Row = paso.row,
                    Col = paso.col,
                    EstadoNuevo = (CellValue)paso.estadoNuevo,
                });
            }

            yield return ReproducirEfectoOpcional(efectoExplosionPrefab, paso.row, paso.col);
            yield return EsperarConPausa(delayPasoOnda);
        }
    }

    IEnumerator ReproducirEfectoOpcional(GameObject prefab, int row, int col)
    {
        if (prefab == null)
        {
            yield return new WaitForSeconds(delaySinEfecto);
            yield break;
        }

        var posicion = BoardLayoutRequirement.CellCenterWorldPosition(row, col);
        var efecto = Instantiate(prefab, posicion, Quaternion.identity);
        Destroy(efecto, 2f);
    }

    [ContextMenu("Debug: Reproducir Turno")]
    void DebugReproducirTurno()
    {
        if (debugTurnoJson == null)
        {
            Debug.LogWarning("Asigna debugTurnoJson en el Inspector.");
            return;
        }
        IniciarTurno(JsonUtility.FromJson<TurnoPayload>(debugTurnoJson.text));
    }
}
