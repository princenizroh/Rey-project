using UnityEngine;

[System.Serializable]
public class DialogChoice
{
    public string playerChoice;
    [TextArea(2, 5)]
    public string npcResponse;
}
