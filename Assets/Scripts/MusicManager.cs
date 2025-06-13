using Fungus;
using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    //juasjuas

    [Header("1. Musica Y Nivel Actual")]
    [SerializeField] AudioSource Audio;
    [SerializeField] ChartAsset nivelActual;  //queda como serializedField hasta que haga el game manager asi puedo testear
    [SerializeField] AudioSource winQuote;
    [SerializeField] AudioSource loseQuote;

    [Header("2. Notas")]
    [SerializeField] float TiempoDeViajeNota = 1.5f;
    [SerializeField] float HitWindow = 0.15f;

    [Header("3. Recorrido De Las Notas")]
    [SerializeField] GameObject NotaPreFab;
    [SerializeField] Transform Spawn;
    [SerializeField] Transform Hit;
    [SerializeField] Transform destroy;      //uso destroy y no Destroy por que con D mayuscula ya esta usado 

    [Header("4. Vida")]
    [SerializeField] int VidasIniciales = 3; // Cu�ntos fallos se permiten


    [Header("5. Referencias UI")]
    [SerializeField] GameObject corazonesFullSprite;
    [SerializeField] GameObject corazones2Sprite;
    [SerializeField] GameObject corazones1Sprite;
    [SerializeField] GameObject corazonesVacioSprite;

    [Header("6. Referencias Personajes")]
    [SerializeField] GameObject mirada;
    [SerializeField] GameObject miradaHit;
    [SerializeField] GameObject miradaMiss;

    //variables privadas
    private int vidasActuales;
    private bool juegoTerminado = false;
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
        if (juegoTerminado) return;
        if (Audio.isPlaying)
        {
            spawnearNotas();
            golpearNotas();
        }
        else
        {

            if (NotasSinAparecer.Count == 0 && NotasActivas.Count == 0 && Audio.time >= Audio.clip.length - 0.1f)
            {
                TriggerRhythmGameOver(true);
            }
        }
    }

    private void actualizarUI()
    {
        if (vidasActuales == 3)
        {
            corazonesFullSprite.SetActive(true);
        }
        else if (vidasActuales == 2)
        {
            corazonesFullSprite.SetActive(false);
            corazones2Sprite.SetActive(true);
        }
        else if (vidasActuales == 1)
        {
            corazones2Sprite.SetActive(false);
            corazones1Sprite.SetActive(true);
        }
        else // vidasActuales <= 0
        {
            corazones1Sprite.SetActive(false);
            corazonesVacioSprite.SetActive(true);
        }
    }
    private void InitializeGame() //incializo variables, ignorar
    {
        NotasSinAparecer.Clear();
        NotasActivas.Clear();
        juegoTerminado = false;
        vidasActuales = VidasIniciales;
        notasProcesadas = 0;
        notasTotal = nivelActual.notas.Count;
        actualizarUI();
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
                actualizarUI();
                CheckGameEndConditions();
                StartCoroutine(CambiarDibujo(false));
                Debug.Log("MISS!");
                return;
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
                    StartCoroutine(CambiarDibujo(true));
                    Debug.Log("HIT!");
                }
                else
                {
                    Debug.Log("Fallo, muy temprano");
                    vidasActuales--;
                    actualizarUI();
                    StartCoroutine(CambiarDibujo(false));
                    CheckGameEndConditions();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Fallo: Presi�n sin notas en pantalla.");
            }
        }
    }
    private void TriggerRhythmGameOver(bool playerWon)
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        Audio.Stop();
        StartCoroutine(EndGameSequence(playerWon));
    }
    private void CheckGameEndConditions()
    {
        if (juegoTerminado) return;


        if (vidasActuales <= 0)
        {
            TriggerRhythmGameOver(false); // Derrota
            return;
        }


        if (notasProcesadas >= notasTotal && Audio.time >= Audio.clip.length - 0.1f)
        {
            TriggerRhythmGameOver(true); // Victoria
            return;
        }
    }

    private void finishMinigame(bool Resultado)
    {
        string sceneToLoad = "";
        if (nivelActual.NumeroDeNivel == 1)
        {
            if (Resultado)
            {
                sceneToLoad = "Dia 1 Ganar"; // Nombre de tu escena Fungus para Día 1 Gane
            }
            else
            {
                sceneToLoad = "Dia 1 Perder"; // Nombre de tu escena Fungus para Día 1 Pierde
            }
        }
        else if (nivelActual.NumeroDeNivel == 2)
        {
            if (Resultado)
            {
                sceneToLoad = "Dia 2 Ganar"; // Nombre de tu escena Fungus para Día 2 Gane
            }
            else
            {
                sceneToLoad = "Dia 2 Perder"; // Nombre de tu escena Fungus para Día 2 Pierde
            }
        }
        else if (nivelActual.NumeroDeNivel == 3)
        {
            if (Resultado)
            {
                sceneToLoad = ""; // Nombre de tu escena Fungus para Día 3 Gane
            }
            else
            {
                sceneToLoad = ""; // Nombre de tu escena Fungus para Día 3 Pierde
            }
        }
        SceneManager.LoadScene(sceneToLoad);
    }
    private IEnumerator EndGameSequence(bool Resultado)
    {
        if (Resultado)
            winQuote.Play();
        else
            loseQuote.Play(); 
        yield return StartCoroutine(screenfader.Instance.FadeOut());
        
        finishMinigame(Resultado);
    }

    private IEnumerator CambiarDibujo(bool FueUnHit)
    {
        mirada.SetActive(false); 
        miradaHit.SetActive(false); 
        miradaMiss.SetActive(false); 
        if (FueUnHit)
        {
            miradaHit.SetActive(true); 
        }
        else 
        {
            miradaMiss.SetActive(true); 
        }
        
        yield return new WaitForSeconds(0.5f); 
       
        miradaHit.SetActive(false); 
        miradaMiss.SetActive(false);
        mirada.SetActive(true);    
    }
}


