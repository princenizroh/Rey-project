using UnityEngine;

[System.Serializable]
public class DialogBlock
{
    // public string npcName;
    // public Sprite npcImage;

    [TextArea(2, 5)]
    public string npcDialog;
    public enum DialogType { ThreeD, TwoD }
    public DialogType dialogType;
    public DialogChoice[] choices;
}
