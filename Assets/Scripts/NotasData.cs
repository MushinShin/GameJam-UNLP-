using UnityEngine;
[System.Serializable]
public class NotasData
{
    float beat;
    public NotasData (float beat)
    {
        this.beat = beat;
    }
    public float getBeat ()
    {
        return beat;
    }
}
