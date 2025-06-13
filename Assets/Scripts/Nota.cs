using System.Collections;
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;
public class Nota : MonoBehaviour
{
    private Vector3 Spawn;
    private Vector3 Objetivo;
    private float Duracion; 
    public float tiempoObjetivo; // El tiempo exacto en la canci�n en que esta nota debe ser golpeada


    [Header("Efectos de Nota")]
    [SerializeField] GameObject EfectoHitPreFab;
    [SerializeField] ShakeData EfectoMiss;
    [SerializeField] AudioClip missSound;
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

            transform.position = Vector3.Lerp(Spawn, Objetivo, t); 

            timeElapse += Time.deltaTime;
            yield return null;
        }
        transform.position = Objetivo;
    }
    public void OnHit()
    {
        Instantiate(EfectoHitPreFab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void OnMiss()
    {
        AudioSource.PlayClipAtPoint(missSound, transform.position); 
        CameraShakerHandler.Shake(EfectoMiss);
        Destroy(gameObject); 
    }
}
