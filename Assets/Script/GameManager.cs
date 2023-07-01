using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<Dialogue> dialogues;
    [SerializeField] int index;

    private void Start()
    {
        StartDialogue();
    }

    private void Update()
    {
        if (DialogueManager.singleton.GetUpdateSomething() && index < dialogues.Capacity - 1)
        {
            Debug.Log("A");
            index++;
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        DialogueManager.singleton.SetDialogueRef(dialogues[index]);
        DialogueManager.singleton.StartDialogue();
    }
}
