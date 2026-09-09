using UnityEngine;

// Actualiza las tarjetas del roster (Framework/Views/TarjetaAgenteView) segun
// el AgentState[] que llega en el GameState/TurnoPayload.
public class AgentRosterController : MonoBehaviour
{
    [SerializeField] TarjetaAgenteView[] tarjetas; // las 6, asignadas en el Inspector

    public void Actualizar(AgentState[] agentes)
    {
        if (agentes == null) return;

        foreach (var agente in agentes)
        {
            foreach (var tarjeta in tarjetas)
            {
                if (tarjeta.AgenteId == agente.id)
                {
                    tarjeta.SetActionPoints(agente.actionPoints);
                    break;
                }
            }
        }
    }
}
