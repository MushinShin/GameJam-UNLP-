using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{

    [Header("1. Musica Y Nivel Actual")]
    [SerializeField] AudioSource Audio; 
    [SerializeField] ChartAsset nivelActual;  //queda como serializedField hasta que haga el game manager asi puedo testear

    [Header("2. Notas")]
    [SerializeField] float TiempoDeViajeNota = 1.5f; 
    [SerializeField] float HitWindow = 0.15f;     

    [Header("3. Recorrido De Las Notas")]
    [SerializeField] GameObject NotaPreFab;       
    [SerializeField] Transform Spawn;        
    [SerializeField] Transform Hit;          
    [SerializeField] Transform destroy;      //uso destroy y no Destroy por que con D mayuscula ya esta usado 

    [Header("4. Vida")]
    [SerializeField] int VidasIniciales = 3; // Cuántos fallos se permiten


    private int vidasActuales;
    private bool juegoTerminado = false; // Bandera para asegurar que solo termine una vez
    private float beat; 
    private Queue<NotasData> NotasSinAparecer = new Queue<NotasData>(); 
    private Queue<Nota> NotasActivas = new Queue<Nota>();
    private int notasTotal = 0;
    private int notasProcesadas = 0;



    void Awake()
    {
        //despues agrego la logica con el gameManager, cuando lo haga
        beat = 60f / nivelActual.songBPM;
    }

    void Start()
    {
        InitializeGame(); 
    }

    void Update()
    {
        if (juegoTerminado) return; //medio feo este if pero supongo que esta bien
        if (Audio.isPlaying)
        {
            spawnearNotas();  
            golpearNotas();  
        }
        else 
        {
            // si hago algo como audio.length < audio.time o algo asi? tipo si ya paso mas tiempo de lo que la cancion dura, si agregamos un boton de pausa abria que pausar el contador no mas
            if (NotasSinAparecer.Count == 0 && NotasActivas.Count == 0 && Audio.time >= Audio.clip.length - 0.1f) // Pequeño margen para el final de audio
            {
                TriggerRhythmGameOver(true); 
            }
        }
    }

    private void InitializeGame()
    {
        NotasSinAparecer.Clear(); // esto es necesario?
        NotasActivas.Clear();
        juegoTerminado = false;
        vidasActuales = VidasIniciales;
        notasProcesadas = 0; // no se puede hacer sin estas dos variables? 
        notasTotal = nivelActual.notas.Count;
        foreach (NotasData noteData in nivelActual.notas)           
        {               
            NotasSinAparecer.Enqueue(noteData);            
        }            
        Audio.Play(); 
    }


    private void spawnearNotas()
    {
        if (NotasSinAparecer.Count > 0)
        {
            NotasData NotaSiguiente = NotasSinAparecer.Peek(); //tomas la proxima nota

            float tiempoObjetivo = NotaSiguiente.getBeat() * beat; // convierte la medida de tiempo beat a segundos 
            float SpawnTime = tiempoObjetivo - TiempoDeViajeNota; // le restas el tiempo de viaje de la nota

            if (Audio.time >= SpawnTime)
            {
                NotasSinAparecer.Dequeue(); // sacamos la nota que estabamos mirando

                GameObject nuevaNota = Instantiate(NotaPreFab, Spawn.position, Quaternion.identity); // creamos la nota desde el spawn
                Nota controladorNotas = nuevaNota.GetComponent<Nota>();

                controladorNotas.Initialize(Hit.position, destroy.position, TiempoDeViajeNota, tiempoObjetivo);
                NotasActivas.Enqueue(controladorNotas);
            }
        }
    }


    private void golpearNotas()
    {
        if (NotasActivas.Count > 0)
        {
            Nota notaActual = NotasActivas.Peek(); 

            float missWindow = notaActual.tiempoObjetivo + (HitWindow / 2f); 
            //MISS
            if (Audio.time >= missWindow)
            {
                NotasActivas.Dequeue(); 
                notaActual.OnMiss();
                notasProcesadas++;
                vidasActuales--;
                CheckGameEndConditions();
                Debug.Log("MISS!");
                return;
                //agregar perder vida si tengo tiempo xddd
            }

            //HIT
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                float timeDifference = Mathf.Abs(Audio.time - notaActual.tiempoObjetivo);

                
                if (timeDifference <= HitWindow / 2f) 
                {
                    NotasActivas.Dequeue(); 
                    notaActual.OnHit();
                    notasProcesadas++;
                    CheckGameEndConditions();
                    Debug.Log("¡HIT!");
                }
                else
                {
                    Debug.Log("Fallo, muy temprano");
                    vidasActuales--;
                    CheckGameEndConditions();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                Debug.Log("Fallo: Presión sin notas en pantalla.");
            }
        }
    }
    // Método final que se llama para terminar el minijuego y reportar al GameManager
    private void TriggerRhythmGameOver(bool playerWon)
    {
        if (juegoTerminado) return; // Asegura que solo se ejecute una vez
        juegoTerminado = true;

        Audio.Stop(); // Detener la música
        Debug.Log($"Juego de ritmo terminado. El jugador {(playerWon ? "GANÓ" : "PERDIÓ")}.");

        // Opcional: Mostrar efectos visuales de victoria/derrota (durante un breve tiempo)
        // Puedes iniciar una corrutina aquí para un pequeño delay antes de cargar la escena
        StartCoroutine(EndGameSequence(playerWon));
    }
    private void CheckGameEndConditions() // este es el mas raro, siento que aca se pueden acortar varias cosas 
    {
        if (juegoTerminado) return; // Ya terminó, no revisar de nuevo

        // Condición de derrota por vidas
        if (vidasActuales <= 0)
        {
            Debug.Log("GAME OVER: Vidas agotadas.");
            TriggerRhythmGameOver(false); // Derrota
            return;
        }

        // Condición de victoria (todas las notas procesadas y la canción casi al final)
        // También verifica que el Audio.time esté cerca del final, en caso de charts muy cortos
        if (notasProcesadas >= notasTotal && Audio.time >= Audio.clip.length - 0.1f)
        {
            Debug.Log("VICTORIA: Todas las notas procesadas y canción finalizada.");
            TriggerRhythmGameOver(true); // Victoria
            return;
        }
    }
    private IEnumerator EndGameSequence(bool playerWon)
    {

        Debug.Log("termino");
        yield return new WaitForSeconds(2f); // Esperar 2 segundos para que se vean los efectos/sonidos

        // Ahora sí, notificar al GameManager. Él cargará la siguiente escena.
        // todavia no existe
        // if (GameManager.Instance != null)
        //{
        //    GameManager.Instance.RhythmGameEnded(playerWon);
        //}
        //else
        //{
        //Debug.LogError("GameManager no encontrado. No se pudo notificar el resultado del ritmo.");
        // Si esto ocurre, la escena no cambiará automáticamente. Podrías forzar una carga de escena de fallback.
    }
}


