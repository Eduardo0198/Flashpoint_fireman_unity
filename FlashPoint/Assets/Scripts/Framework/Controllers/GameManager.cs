using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] BoardController board;
    [SerializeField] TurnoController turnoController;

    [Header("Avance automatico de turnos")]
    [SerializeField] bool autoAvanzar = true;
    [SerializeField] float delayEntreTurnos = 1.5f; 

    public int VictimasRescatadas { get; private set; }
    public int VictimasPerdidas { get; private set; }

    bool inicializado;
    bool partidaTerminada;

    void Start()
    {
        StartCoroutine(GameStateRepository.GetState(RenderState));
    }

    void OnEnable()
    {
        if (turnoController != null) turnoController.OnTurnoTerminado += OnTurnoTerminado;
    }

    void OnDisable()
    {
        if (turnoController != null) turnoController.OnTurnoTerminado -= OnTurnoTerminado;
    }

    public void OnNextTurnButton()
    {
        SolicitarSiguienteTurno();
    }

    void OnTurnoTerminado()
    {
        if (!autoAvanzar || partidaTerminada) return;
        StartCoroutine(SolicitarSiguienteTurnoConDelay());
    }

    IEnumerator SolicitarSiguienteTurnoConDelay()
    {
        yield return new WaitForSeconds(delayEntreTurnos);
        SolicitarSiguienteTurno();
    }

    void SolicitarSiguienteTurno()
    {
        if (partidaTerminada) return;
        StartCoroutine(GameStateRepository.PostStep(OnTurnoRecibido));
    }

    void OnTurnoRecibido(TurnoPayload payload)
    {
        if (payload == null) return;

        if (payload.estadoFinal != null)
        {
            VictimasRescatadas = payload.estadoFinal.victimasRescatadas;
            VictimasPerdidas = payload.estadoFinal.victimasPerdidas;

            if (VictimasRescatadas >= 7 || VictimasPerdidas >= 4 || payload.estadoFinal.danoEstructura >= 25)
            {
                partidaTerminada = true;
            }
        }

        turnoController.IniciarTurno(payload);
    }

    void RenderState(GameState state)
    {
        if (state == null) return;

        if (!inicializado)
        {
            board.BuildInitial(state);
            inicializado = true;

            if (autoAvanzar) SolicitarSiguienteTurno();
        }
        else
        {
            board.ApplyUpdate(state);
        }
    }
}
