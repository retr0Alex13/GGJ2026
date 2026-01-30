using System.Collections.Generic;
using UnityEngine;

public static class PlaybackData
{
    public static int activePlayerIndex = 0;
    public static Dictionary<CharaterType, List<CharacterFrame>> Records = new();

    public static void SaveRecord(CharaterType type, List<CharacterFrame> frames)
    {
        Records[type] = new List<CharacterFrame>(frames);
    }
}

[System.Serializable]
public struct CharacterFrame
{
    public Vector3 position;
    public Quaternion rotation;
    public float time;

    public CharacterFrame(Vector3 pos, Quaternion rot, float t)
    {
        position = pos;
        rotation = rot;
        time = t;
    }
}