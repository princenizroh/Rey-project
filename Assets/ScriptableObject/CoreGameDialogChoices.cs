using UnityEngine;
[System.Serializable]
public class CoreGameDialogChoices
{
    public string playerChoice;
    [TextArea(2, 5)]
    public string npcResponse;
    public AudioClip audioDialogResponse;
}
