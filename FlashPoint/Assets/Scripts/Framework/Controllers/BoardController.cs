using System.Collections.Generic;
using UnityEngine;


public class BoardController : MonoBehaviour
{
    [SerializeField] GameObject sueloPrefab;
    [SerializeField] GameObject pastoPrefab;
    [SerializeField] GameObject paredPrefab;
    [SerializeField] GameObject puertaPrefab;

    [Header("Depuracion (sin servidor Python)")]
    [SerializeField] TextAsset debugJsonInicial;
    [SerializeField] TextAsset debugJsonActualizado;

    readonly Dictionary<(int row, int col), GameObject> paredesV = new Dictionary<(int row, int col), GameObject>();
    readonly Dictionary<(int row, int col), GameObject> paredesH = new Dictionary<(int row, int col), GameObject>();
    readonly List<GameObject> pisos = new List<GameObject>();

    int[] ultimoVertical;
    int[] ultimoHorizontal;
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

        ultimoVertical = (int[])state.paredesVerticales.Clone();
        ultimoHorizontal = (int[])state.paredesHorizontales.Clone();
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

        ultimoVertical = (int[])nuevo.paredesVerticales.Clone();
        ultimoHorizontal = (int[])nuevo.paredesHorizontales.Clone();
    }

    void AplicarCambio(BoardLayoutRequirement.WallChange cambio)
    {
        var dict = cambio.EsVertical ? paredesV : paredesH;
        var key = (cambio.Row, cambio.Col);
        dict.TryGetValue(key, out var existente);

        bool eraPuerta = cambio.EstadoAnterior == WallState.PuertaCerrada || cambio.EstadoAnterior == WallState.PuertaAbierta;

        switch (cambio.EstadoNuevo)
        {
            case WallState.Abierto:
                // pared destruida o puerta removida
                if (existente != null) Destroy(existente);
                dict.Remove(key);
                break;

            case WallState.Pared:
                if (eraPuerta || existente == null)
                {
                    if (existente != null) Destroy(existente);
                    dict[key] = InstanciarMuroPorIndice(cambio.Row, cambio.Col, cambio.EsVertical, cambio.EstadoNuevo);
                }
                break;

            case WallState.PuertaCerrada:
                if (!eraPuerta || existente == null)
                {
                    if (existente != null) Destroy(existente);
                    dict[key] = InstanciarMuroPorIndice(cambio.Row, cambio.Col, cambio.EsVertical, cambio.EstadoNuevo);
                }
                else
                {
                    existente.GetComponent<DoorController>()?.SetOpen(false);
                }
                break;

            case WallState.PuertaAbierta:
                if (eraPuerta && existente != null)
                {
                    existente.GetComponent<DoorController>()?.SetOpen(true);
                }
                else
                {
                    if (existente != null) Destroy(existente);
                    var creado = InstanciarMuroPorIndice(cambio.Row, cambio.Col, cambio.EsVertical, cambio.EstadoNuevo);
                    creado.GetComponent<DoorController>()?.SetOpen(true);
                    dict[key] = creado;
                }
                break;
        }
    }

    GameObject InstanciarMuroPorIndice(int row, int col, bool esVertical, WallState estado)
    {
        var muro = BoardLayoutRequirement.ComputeSingleWallPlacement(row, col, esVertical, columnasCache, estado);
        return InstanciarMuro(muro);
    }

    GameObject InstanciarMuro(BoardLayoutRequirement.WallPlacement muro)
    {
        bool esPuerta = muro.Estado == WallState.PuertaCerrada || muro.Estado == WallState.PuertaAbierta;
        var prefab = esPuerta ? puertaPrefab : paredPrefab;

        var go = Instantiate(prefab, transform);
        go.transform.position = new Vector3(muro.Position.x, prefab.transform.position.y, muro.Position.z);
        go.transform.rotation = muro.Rotation;

        if (esPuerta)
        {
            go.GetComponent<DoorController>()?.SetOpen(muro.Estado == WallState.PuertaAbierta);
        }

        return go;
    }

    void LimpiarTodo()
    {
        foreach (var kv in paredesV) if (kv.Value != null) Destroy(kv.Value);
        foreach (var kv in paredesH) if (kv.Value != null) Destroy(kv.Value);
        foreach (var piso in pisos) if (piso != null) Destroy(piso);

        paredesV.Clear();
        paredesH.Clear();
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
