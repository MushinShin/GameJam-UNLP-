using System.Collections;
using UnityEngine;

public class Nota : MonoBehaviour
{
    private Vector3 Spawn;
    private Vector3 Objetivo;
    [SerializeField] AnimationCurve Curva; 
    private float Duracion; 
    public float tiempoObjetivo; // El tiempo exacto en la canción en que esta nota debe ser golpeada

    public void Initialize(Vector3 hitPos, Vector3 destroyPos, float travelDuration, float noteTargetSongTime)
    {
        this.Spawn = transform.position;
        this.Objetivo = hitPos;
        this.Duracion = travelDuration;
        this.tiempoObjetivo = noteTargetSongTime; 

        StartCoroutine(moverNota());
    }

    IEnumerator moverNota()
    {
        float timeElapse = 0f;
        while (timeElapse < Duracion)
        {
            float t = timeElapse / Duracion;

            transform.position = Vector3.Lerp(Spawn, Objetivo, Curva.Evaluate(t));

            timeElapse += Time.deltaTime;
            yield return null;
        }
        transform.position = Objetivo;
    }

    public void OnHit()
    {
        Destroy(gameObject); 
    }

    public void OnMiss()
    {
        Destroy(gameObject); 
    }
}
