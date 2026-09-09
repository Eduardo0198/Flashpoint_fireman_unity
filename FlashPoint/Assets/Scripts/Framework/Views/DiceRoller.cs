using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Panel de UI con 2 dados: cicla caras al azar por un rato y termina mostrando
// exactamente el valor que llego en el JSON. Los dos dados tienen su propio set
// de caras porque en Flash Point uno es d6 (fila) y el otro d8 (columna).
public class DiceRoller : MonoBehaviour
{
    [SerializeField] GameObject panel; // se activa/desactiva mientras se tira
    [SerializeField] Image imagenDado1;
    [SerializeField] Image imagenDado2;
    [SerializeField] Sprite[] carasDado1;
    [SerializeField] Sprite[] carasDado2;

    [SerializeField] float duracionCicleo = 1f;
    [SerializeField] float intervaloCicleo = 0.08f;
    [SerializeField] float esperaAlTerminar = 0.5f;

    public IEnumerator Tirar(int valorFinal1, int valorFinal2)
    {
        if (panel != null) panel.SetActive(true);

        float transcurrido = 0f;
        while (transcurrido < duracionCicleo)
        {
            MostrarCaraAlAzar(imagenDado1, carasDado1);
            MostrarCaraAlAzar(imagenDado2, carasDado2);
            yield return new WaitForSeconds(intervaloCicleo);
            transcurrido += intervaloCicleo;
        }

        MostrarCara(imagenDado1, carasDado1, valorFinal1);
        MostrarCara(imagenDado2, carasDado2, valorFinal2);

        yield return new WaitForSeconds(esperaAlTerminar);

        if (panel != null) panel.SetActive(false);
    }

    void MostrarCaraAlAzar(Image imagen, Sprite[] caras)
    {
        if (imagen == null || caras == null || caras.Length == 0) return;
        imagen.sprite = caras[Random.Range(0, caras.Length)];
    }

    
    void MostrarCara(Image imagen, Sprite[] caras, int valor)
    {
        if (imagen == null || caras == null || caras.Length == 0) return;
        int indice = Mathf.Clamp(valor - 1, 0, caras.Length - 1);
        imagen.sprite = caras[indice];
    }
}
