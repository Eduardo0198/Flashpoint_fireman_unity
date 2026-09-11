using UnityEngine;
using UnityEngine.UI;

// Una tarjeta del roster de agentes.
public class TarjetaAgenteView : MonoBehaviour
{
    [SerializeField] int agenteId;
    [SerializeField] Image[] pipsActionPoint; // 8: fila de arriba izq->der, luego fila de abajo izq->der
    [SerializeField] Sprite spriteConAP;
    [SerializeField] Sprite spriteSinAP;

    public int AgenteId => agenteId;

    public void SetActionPoints(int cantidad)
    {
        for (int i = 0; i < pipsActionPoint.Length; i++)
        {
            pipsActionPoint[i].sprite = i < cantidad ? spriteConAP : spriteSinAP;
        }
    }
}
