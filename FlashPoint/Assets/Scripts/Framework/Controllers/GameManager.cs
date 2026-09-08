using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] BoardController board;

    bool inicializado;

    void Start()
    {
        StartCoroutine(GameStateRepository.GetState(RenderState));
    }

    public void OnNextTurnButton()
    {
        StartCoroutine(GameStateRepository.PostStep(RenderState));
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

        foreach (var a in state.agentes)
        {
     
        }
    }
}
