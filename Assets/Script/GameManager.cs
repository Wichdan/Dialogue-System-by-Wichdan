using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<Dialogue> dialogues;
    [SerializeField] int index;

    private void Start()
    {
        //StartDialogue();
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

    public void StartDialogue()
    {
        DialogueManager.singleton.SetDialogueRef(dialogues[index]);
        DialogueManager.singleton.StartDialogue();
    }

    public void ResetScene()
    {
        int curScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(curScene);
    }
}
