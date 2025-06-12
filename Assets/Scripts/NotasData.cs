using UnityEngine;
[System.Serializable]
public class NotasData
{
    public float beat;
    public NotasData (float beat)
    {
        this.beat = beat;
    }
    public float getBeat ()
    {
        return beat;
    }
}
