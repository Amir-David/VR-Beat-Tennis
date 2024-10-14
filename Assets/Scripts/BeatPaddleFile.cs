using System.Collections.Generic;

[System.Serializable]
public class BeatPaddleFile
{
    public string _version;
    public List<BeatPaddleBallSpawner.BeatPaddleBallData> _notes;
}