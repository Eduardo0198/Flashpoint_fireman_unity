using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

// Repository: administra la fuente de datos del estado del juego
public static class GameStateRepository
{
    const string BASE_URL = "http://localhost:5000";

    public static IEnumerator GetState(Action<GameState> alTerminar)
    {
        using (UnityWebRequest peticion = UnityWebRequest.Get(BASE_URL + "/game/state"))
        {
            yield return peticion.SendWebRequest();
            if (peticion.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fallo GET /game/state: " + peticion.error);
                alTerminar(null);
                yield break;
            }
            alTerminar(JsonUtility.FromJson<GameState>(peticion.downloadHandler.text));
        }
    }

    public static IEnumerator PostStep(Action<TurnoPayload> alTerminar)
    {
        using (UnityWebRequest peticion = UnityWebRequest.PostWwwForm(BASE_URL + "/game/step", ""))
        {
            yield return peticion.SendWebRequest();
            if (peticion.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fallo POST /game/step: " + peticion.error);
                alTerminar(null);
                yield break;
            }
            alTerminar(JsonUtility.FromJson<TurnoPayload>(peticion.downloadHandler.text));
        }
    }
}
