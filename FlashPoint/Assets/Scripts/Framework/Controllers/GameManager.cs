using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] BoardController board;
    [SerializeField] TurnoController turnoController;

    bool inicializado;

    void Start()
    {
        StartCoroutine(GameStateRepository.GetState(RenderState));
    }

    public void OnNextTurnButton()
    {
        StartCoroutine(GameStateRepository.PostStep(OnTurnoRecibido));
    }

    void OnTurnoRecibido(TurnoPayload payload)
    {
        if (payload == null) return;
        turnoController.IniciarTurno(payload);
    }

    void RenderState(GameState state)
    {
        if (state == null) return;

        if (!inicializado)
        {
            board.BuildInitial(state);
            inicializado = true;
        }
        else
        {
            board.ApplyUpdate(state);
        }
    }
}
