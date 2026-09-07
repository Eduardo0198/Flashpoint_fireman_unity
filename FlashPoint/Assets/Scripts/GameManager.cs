using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(APIHelper.GetState(RenderState));
    }

    public void OnNextTurnButton()
    {
        StartCoroutine(APIHelper.PostStep(RenderState));
    }

    void RenderState(GameState state)
    {
        if (state == null) return;

        for (int y = 0; y < state.height; y++)
        {
            for (int x = 0; x < state.width; x++)
            {
                int valor = state.fuego[y * state.width + x];
                // aquí pintas la celda: 0 vacio, 1 humo, 2 fuego
            }
        }

        foreach (var a in state.agentes)
        {
            // aquí mueves el sprite/prefab del agente a la celda (a.x, a.y)
        }
    }
}