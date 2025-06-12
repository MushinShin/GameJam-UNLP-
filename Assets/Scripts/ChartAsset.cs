using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newChart", menuName = "Scriptable Objects/Chart")]
public class ChartAsset : ScriptableObject
{
    [SerializeField] public List<NotasData> notas = new List<NotasData>();

    [SerializeField] public float songBPM;
    [SerializeField] public int NumeroDeNivel;
}
