using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawnea los agentes en su posicion, los desliza al moverse, y les cuelga un
// icono (el mismo prefab de POI Victima)
public class AgentesController : MonoBehaviour
{
    [SerializeField] GameObject agentePrefab;
    [SerializeField] GameObject iconoVictimaPrefab;
    [SerializeField] float alturaIcono = 3f;
    [SerializeField] float alturaAgente = 0.75f;
    [SerializeField] float duracionMovimiento = 0.6f;

    class AgenteInstancia
    {
        public GameObject raiz;
        public GameObject icono;
    }

    readonly Dictionary<int, AgenteInstancia> agentes = new Dictionary<int, AgenteInstancia>();

    public void BuildInitial(AgentState[] estado)
    {
        LimpiarTodo();
        if (estado == null) return;

        foreach (var a in estado)
        {
            CrearAgente(a);
        }
    }

    // Snap instantaneo (sin animar): crea el agente si falta, reposiciona si ya existe.
    public void Actualizar(AgentState[] estado)
    {
        if (estado == null) return;

        foreach (var a in estado)
        {
            if (agentes.TryGetValue(a.id, out var instancia) && instancia.raiz != null)
            {
                instancia.raiz.transform.position = PosicionAgente(a.y, a.x);
            }
            else
            {
                CrearAgente(a);
            }
        }
    }

    public IEnumerator MoverAgente(int agenteId, int row, int col)
    {
        if (!TryGetRaiz(agenteId, out var raiz)) yield break;

        Vector3 desde = raiz.position;
        Vector3 hasta = PosicionAgente(row, col);
        float t = 0f;
        while (t < duracionMovimiento)
        {
            t += Time.deltaTime;
            raiz.position = Vector3.Lerp(desde, hasta, t / duracionMovimiento);
            yield return null;
        }
        raiz.position = hasta;
    }

    bool TryGetRaiz(int agenteId, out Transform raiz)
    {
        if (agentes.TryGetValue(agenteId, out var instancia) && instancia.raiz != null)
        {
            raiz = instancia.raiz.transform;
            return true;
        }
        raiz = null;
        return false;
    }

    public void SetCargandoVictima(int agenteId, bool cargando)
    {
        if (agentes.TryGetValue(agenteId, out var instancia) && instancia.icono != null)
        {
            instancia.icono.SetActive(cargando);
        }
    }

    public void RemoverAgente(int agenteId)
    {
        if (agentes.TryGetValue(agenteId, out var instancia) && instancia.raiz != null)
        {
            Destroy(instancia.raiz);
        }
        agentes.Remove(agenteId);
    }

    void CrearAgente(AgentState a)
    {
        if (agentePrefab == null)
        {
            Debug.LogWarning("AgentesController: falta asignar Agente Prefab en el Inspector.");
            return;
        }

        var posicion = PosicionAgente(a.y, a.x);
        var raiz = Instantiate(agentePrefab, posicion, Quaternion.identity, transform);

        GameObject icono = null;
        if (iconoVictimaPrefab != null)
        {
            icono = Instantiate(iconoVictimaPrefab, raiz.transform);
            icono.transform.localPosition = new Vector3(0f, alturaIcono, 0f);
            icono.SetActive(false);
        }

        agentes[a.id] = new AgenteInstancia { raiz = raiz, icono = icono };
    }

    Vector3 PosicionAgente(int row, int col)
    {
        return BoardLayoutRequirement.CellCenterWorldPosition(row, col) + new Vector3(0f, alturaAgente, 0f);
    }

    void LimpiarTodo()
    {
        foreach (var kv in agentes)
        {
            if (kv.Value.raiz != null) Destroy(kv.Value.raiz);
        }
        agentes.Clear();
    }
}
