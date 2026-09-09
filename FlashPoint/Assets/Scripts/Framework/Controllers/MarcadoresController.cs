using TMPro;
using UnityEngine;

// Barra superior de marcadores: victimas muertas / salvadas / dano a la
// estructura, cada uno como "actual/limite".
public class MarcadoresController : MonoBehaviour
{
    [SerializeField] TMP_Text textoMuertas;
    [SerializeField] TMP_Text textoSalvadas;
    [SerializeField] TMP_Text textoDano;

    const int LIMITE_MUERTAS = 4;
    const int LIMITE_SALVADAS = 7;
    const int LIMITE_DANO = 24;

    public void Actualizar(GameState state)
    {
        if (state == null) return;

        if (textoMuertas != null) textoMuertas.text = $"{state.victimasPerdidas}/{LIMITE_MUERTAS}";
        if (textoSalvadas != null) textoSalvadas.text = $"{state.victimasRescatadas}/{LIMITE_SALVADAS}";
        if (textoDano != null) textoDano.text = $"{state.danoEstructura}/{LIMITE_DANO}";
    }
}
