using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Chart : MonoBehaviour
{
    [SerializeField] private ChartAsset notasASS;
    [SerializeField] bool HacerNuevaLista;
    [SerializeField] AudioSource cancion;
    [SerializeField] private float bpm;
    private float beat;

    void Awake()
    {
        beat = 60f / bpm;
    }
    void Update()
    {
        if (HacerNuevaLista)
        {
            CrearChart();
        }
    }
    private void CrearChart()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            float subdivision = 0.25f;
            float beatActual = cancion.time / beat;
            float beatActualR = Mathf.Round(beatActual / subdivision) * subdivision; // redondeamos el beat en subdivisiones
            notasASS.notas.Add(new NotasData(beatActualR)); // guardamos beat redondeado
            Debug.Log($"guardado en {beatActualR}");
        }
    }
    [ContextMenu("Guardar")]
    private void Guardar()
    {
        if (notasASS != null)
        {
            UnityEditor.EditorUtility.SetDirty(notasASS);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("Chart data saved to " + notasASS.name + " asset!");
        } // esto especifico me lo hizo la IA, pero funciona
    }
    [ContextMenu("Limpiar")]
    private void Limpiar()
    {
        if (notasASS != null)
        {
            notasASS.notas.Clear();
            UnityEditor.EditorUtility.SetDirty(notasASS);
            UnityEditor.AssetDatabase.SaveAssets();
        } // esto lo hice yo pero funciona
    }
}
