using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new dialogue", menuName = "Dialogue System by Wichdan/Create Dialogue", order = 0)]
public class Dialogue : ScriptableObject
{
    public List<Conversation> dialogueData;
    public bool isHasChoice;
    public List<Dialogue> choiceList;
}

[System.Serializable]
public class Conversation
{
    [Header("Conversation")]
    public string elementName;
    public string charName;
    [TextArea(1,3)]
    public string dialogueSentece;

    [Header("Portrait Order")]
    public CharacterOrder charOrder;
    public Sprite portrait;

    [Header("Animation")]
    public RuntimeAnimatorController eyesCharCtrller;
    public RuntimeAnimatorController mouthCharCtrller;
    [Range(0,1)]
    public float eyesValue, mouthValue;
    public enum CharacterOrder
    {
        Right, Mid, Left, None
    }
}
