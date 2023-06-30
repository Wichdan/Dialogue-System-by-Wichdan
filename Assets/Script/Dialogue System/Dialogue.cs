using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new dialogue", menuName = "Dialogue System by Wichdan/Create Dialogue", order = 0)]
public class Dialogue : ScriptableObject
{
    public List<Conversation> dialogueData;
    public List<AutoFillPortraitData> autoFillPortraitData;

    [System.Serializable]
    public struct AutoFillPortraitData
    {
        public string charName;
        public GameObject portraitData;
    }

    public bool isHasChoice;
    public List<Dialogue> choiceList;

    private void OnEnable()
    {
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
    [Range(0, 1)]
    public float eyesValue, mouthValue;
    public enum CharacterOrder
    {
        Right, Mid, Left, None
    }
}
