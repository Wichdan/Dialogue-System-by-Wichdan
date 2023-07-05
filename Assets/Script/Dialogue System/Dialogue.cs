using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new dialogue", menuName = "Dialogue System by Wichdan/Create Dialogue", order = 0)]
public class Dialogue : ScriptableObject
{
    [Header("Conversation")]
    public List<Conversation> dialogueData;
    public List<AutoFillPortraitData> autoFillPortraitData;

    [System.Serializable]
    public struct AutoFillPortraitData
    {
        public string charName;
        public GameObject portraitData;
    }

    [Header("Choice")]
    public bool isHasChoice;
    public List<Dialogue> choiceList;

    [Header("Other")]
    public Sprite background;
    public bool isUseTransition;
    public bool updateSomething;

    private void OnEnable()
    {
        if(dialogueData == null) return;
        for (int i = 0; i < dialogueData.Capacity; i++)
        {
            for (int j = 0; j < autoFillPortraitData.Capacity; j++)
            {
                if(autoFillPortraitData[j].charName == dialogueData[i].charName){
                    dialogueData[i].portraitData = autoFillPortraitData[j].portraitData;
                }
            }
        }
    }
}

[System.Serializable]
public class Conversation
{
    [Header("Conversation")]
    public string elementName;
    public string charName;
    [TextArea(1, 3)]
    public string dialogueSentece;

    [Header("Portrait Order")]
    public CharacterOrder charOrder;
    public GameObject portraitData;

    [Header("Face Animation")]
    [Range(0, 1)]
    public float eyesValue;
    [Range(0,1)]
    public float mouthValue;

    [Header("Gesture Animation")]
    [Range(0,1)]
    public float gestureValue;

    public enum CharacterOrder
    {
        Right, Mid, Left, None
    }
}
