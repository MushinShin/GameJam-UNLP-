using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }    //singleton

    [Header("1. Musica Y Nivel Actual")]
    [SerializeField] AudioSource Audio; 
    [SerializeField] ChartAsset nivelActual; 

    [Header("2. Notas")]
    [SerializeField] float TiempoDeViajeNota = 1.5f; 
    [SerializeField] float HitWindow = 0.15f;     

    [Header("3. Recorrido De Las Notas")]
    [SerializeField] GameObject NotaPreFab;       
    [SerializeField] Transform Spawn;        
    [SerializeField] Transform Hit;          
    [SerializeField] Transform destroy;      //uso destroy y no Destroy por que con D mayuscula ya esta usado 

    private float beat; 
    private Queue<NotasData> NotasSinAparecer = new Queue<NotasData>(); 
    private Queue<Nota> NotasActivas = new Queue<Nota>();           

    

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        beat = 60f / nivelActual.songBPM; 
    }

    void Start()
    {
        InitializeGame(); 
    }

    void Update()
    {
        if (Audio.isPlaying)
        {
            spawnearNotas();  
            golpearNotas();  
        }
    }

    private void InitializeGame()
    {
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
                    Debug.Log("¡HIT!");
                }
                else
                {
                    Debug.Log("Fallo, muy temprano");
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
}
