using System.Collections.Generic;
using UnityEngine;


public class BoardController : MonoBehaviour
{
    [SerializeField] GameObject sueloPrefab;
    [SerializeField] GameObject pastoPrefab;
    [SerializeField] GameObject paredPrefab;
    [SerializeField] GameObject puertaCerradaPrefab;
    [SerializeField] GameObject puertaAbiertaPrefab;
    [SerializeField] GameObject fuegoPrefab;
    [SerializeField] GameObject humoPrefab;

    [Header("Depuracion (sin servidor Python)")]
    [SerializeField] TextAsset debugJsonInicial;
    [SerializeField] TextAsset debugJsonActualizado;

    readonly Dictionary<(int row, int col), GameObject> paredesV = new Dictionary<(int row, int col), GameObject>();
    readonly Dictionary<(int row, int col), GameObject> paredesH = new Dictionary<(int row, int col), GameObject>();
    readonly Dictionary<(int row, int col), GameObject> fuegoHumo = new Dictionary<(int row, int col), GameObject>();
    readonly List<GameObject> pisos = new List<GameObject>();

    int[] ultimoVertical;
    int[] ultimoHorizontal;
    int[] ultimoTablero;
    int filasCache;
    int columnasCache;

    public void BuildInitial(GameState state)
    {
        LimpiarTodo();

        foreach (var piso in BoardLayoutRequirement.ComputeFloorPositions(state))
        {
            var prefab = piso.EsExterior ? pastoPrefab : sueloPrefab;
            var go = Instantiate(prefab, piso.Position, Quaternion.identity, transform);
            pisos.Add(go);
        }

        foreach (var muro in BoardLayoutRequirement.ComputeWallPlacements(state))
        {
            var dict = muro.EsVertical ? paredesV : paredesH;
            dict[(muro.Row, muro.Col)] = InstanciarMuro(muro);
        }

        foreach (var celda in BoardLayoutRequirement.ComputeFireCellPlacements(state))
        {
            fuegoHumo[(celda.Row, celda.Col)] = InstanciarFuegoHumo(celda);
        }

        ultimoVertical = (int[])state.paredesVerticales.Clone();
        ultimoHorizontal = (int[])state.paredesHorizontales.Clone();
        ultimoTablero = (int[])state.tablero.Clone();
        filasCache = state.filas;
        columnasCache = state.columnas;
    }

    public void ApplyUpdate(GameState nuevo)
    {
        var cambios = BoardLayoutRequirement.ComputeWallDiff(
            ultimoVertical, nuevo.paredesVerticales,
            ultimoHorizontal, nuevo.paredesHorizontales,
            filasCache, columnasCache);

        foreach (var cambio in cambios)
        {
            AplicarCambio(cambio);
        }

        var cambiosFuego = BoardLayoutRequirement.ComputeFireDiff(
            ultimoTablero, nuevo.tablero, filasCache, columnasCache);

        foreach (var cambio in cambiosFuego)
        {
            AplicarCambioFuego(cambio);
        }

        ultimoVertical = (int[])nuevo.paredesVerticales.Clone();
        ultimoHorizontal = (int[])nuevo.paredesHorizontales.Clone();
        ultimoTablero = (int[])nuevo.tablero.Clone();
    }

    void AplicarCambioFuego(BoardLayoutRequirement.FireCellChange cambio)
    {
        var key = (cambio.Row, cambio.Col);
        if (fuegoHumo.TryGetValue(key, out var existente) && existente != null)
        {
            Destroy(existente);
        }
        fuegoHumo.Remove(key);

        if (cambio.EstadoNuevo != CellValue.Humo && cambio.EstadoNuevo != CellValue.Fuego) return;

        var celda = new BoardLayoutRequirement.FireCellPlacement
        {
            Row = cambio.Row,
            Col = cambio.Col,
            Estado = cambio.EstadoNuevo,
            Position = new Vector3(
                (cambio.Col + 1) * BoardLayoutRequirement.CELL_SIZE,
                0f,
                (cambio.Row + 1) * BoardLayoutRequirement.CELL_SIZE),
        };
        fuegoHumo[key] = InstanciarFuegoHumo(celda);
    }

    GameObject InstanciarFuegoHumo(BoardLayoutRequirement.FireCellPlacement celda)
    {
        var prefab = celda.Estado == CellValue.Fuego ? fuegoPrefab : humoPrefab;
        return Instantiate(prefab, celda.Position, Quaternion.identity, transform);
    }

    void AplicarCambio(BoardLayoutRequirement.WallChange cambio)
    {
        // Cada estado (Pared/PuertaCerrada/PuertaAbierta) es un prefab distinto, asi que
        // cualquier cambio real de estado se resuelve igual: destruir lo que habia (si
        // habia) e instanciar el prefab correcto para el nuevo estado.
        var dict = cambio.EsVertical ? paredesV : paredesH;
        var key = (cambio.Row, cambio.Col);
        if (dict.TryGetValue(key, out var existente) && existente != null)
        {
            Destroy(existente);
        }
        dict.Remove(key);

        if (cambio.EstadoNuevo == WallState.Abierto) return;

        dict[key] = InstanciarMuroPorIndice(cambio.Row, cambio.Col, cambio.EsVertical, cambio.EstadoNuevo);
    }

    GameObject InstanciarMuroPorIndice(int row, int col, bool esVertical, WallState estado)
    {
        var muro = BoardLayoutRequirement.ComputeSingleWallPlacement(row, col, esVertical, columnasCache, estado);
        return InstanciarMuro(muro);
    }

    GameObject InstanciarMuro(BoardLayoutRequirement.WallPlacement muro)
    {
        GameObject prefab = muro.Estado switch
        {
            WallState.PuertaCerrada => puertaCerradaPrefab,
            WallState.PuertaAbierta => puertaAbiertaPrefab,
            _ => paredPrefab,
        };

        var go = Instantiate(prefab, transform);
        go.transform.position = new Vector3(muro.Position.x, prefab.transform.position.y, muro.Position.z);
        go.transform.rotation = muro.Rotation;

        return go;
    }

    void LimpiarTodo()
    {
        foreach (var kv in paredesV) if (kv.Value != null) Destroy(kv.Value);
        foreach (var kv in paredesH) if (kv.Value != null) Destroy(kv.Value);
        foreach (var kv in fuegoHumo) if (kv.Value != null) Destroy(kv.Value);
        foreach (var piso in pisos) if (piso != null) Destroy(piso);

        paredesV.Clear();
        paredesH.Clear();
        fuegoHumo.Clear();
        pisos.Clear();
    }

    [ContextMenu("Debug: Build From JSON")]
    void DebugBuildFromJson()
    {
        if (debugJsonInicial == null)
        {
            Debug.LogWarning("Asigna debugJsonInicial en el Inspector.");
            return;
        }
        BuildInitial(JsonUtility.FromJson<GameState>(debugJsonInicial.text));
    }

    [ContextMenu("Debug: Apply Update From JSON")]
    void DebugApplyUpdateFromJson()
    {
        if (debugJsonActualizado == null)
        {
            Debug.LogWarning("Asigna debugJsonActualizado en el Inspector.");
            return;
        }
        ApplyUpdate(JsonUtility.FromJson<GameState>(debugJsonActualizado.text));
    }
}
