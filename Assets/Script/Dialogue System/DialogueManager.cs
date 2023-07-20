using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Reference")]
    [SerializeField] Dialogue dialogueRef;

    [Header("Text (sentence) Reference")]
    [SerializeField] TextMeshProUGUI textSentenceTMP;

    [Header("Sentence History")]
    [SerializeField] GameObject textHistoryPanel;

    [Header("Speaker")]
    [SerializeField] PortraitManager portraitManager;

    [Header("Object Reference")]
    [SerializeField] GameObject doneTalkingImg;
    [SerializeField] GameObject dialoguePanel, namePanel, sentenceHistoryPanel;
    [SerializeField] Image sentencePanel, backgroundPanel;
    [SerializeField] Button nextConversationBtn;

    [Header("Auto Next")]
    [SerializeField] bool isAuto;
    [SerializeField] float timeToNext = 3f;
    [SerializeField] TextMeshProUGUI textAutoTMP;

    [Header("Text Speed")]
    [SerializeField] float textSpeed = 0.05f;
    [SerializeField] int txtSpdChanger = -1;
    [SerializeField] TextMeshProUGUI textSpeedTMP;

    [Header("Choice")]
    [SerializeField] GameObject choicePanel;
    [SerializeField] List<GameObject> choiceBtn;

    [Header("Animation")]
    [SerializeField] Animator startTransition;
    [SerializeField] float startDelay = 1f;
    [SerializeField] Animator screenEffect;

    [Header("Sound")]
    [SerializeField] AudioSource voiceAudioSource;

    [Header("Other Setting")]
    [SerializeField] bool isPlayOnStart;
    [SerializeField] bool isUpdateSomething;

    bool isHide;
    int conversationIndex;
    bool isPrint, isCanNext;


    public static DialogueManager singleton;

    private void Reset()
    {
        textSpeed = 0.05f;
        txtSpdChanger = -1;
        startDelay = 1f;
    }

    private void Awake()
    {
        if (singleton != null && singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            singleton = this;
        }
    }

    private void Start()
    {
        voiceAudioSource = GetComponent<AudioSource>();
        ChangeTextSpeed();
        //StartDialogue();
        nextConversationBtn.onClick.AddListener(() =>
        {
            PrintAllSentence();
        });
    }

    private void Update()
    {
        if (dialogueRef == null) return;
        if (Input.GetKeyDown(KeyCode.Space))
            PrintAllSentence();

        if (isCanNext && !isPrint)
            NextConversation();
    }

    public void SetDialogueRef(Dialogue reference) => dialogueRef = reference;

    //dialog dimulai
    public void StartDialogue()
    {
        if (dialogueRef == null) return;
        ResetSentenceHistory();

        if (portraitManager != null)
            portraitManager.ResetSpeaker();

        conversationIndex = 0;
        isUpdateSomething = false;

        SetTransition(startTransition, true);

        StartCoroutine(StartDelay());
    }

    //Digunakan untuk memulai pembicaraan
    void StartConversation()
    {
        isPrint = false;
        isCanNext = false;
        StopAllCoroutines();

        if (conversationIndex >= dialogueRef.dialogueData.Capacity)
        {
            EndConversation();
            return;
        }

        //hidupin dialog panel saat mulai
        dialoguePanel.SetActive(true);

        //ngisi sentencenya dari referensi dialog
        textSentenceTMP.text = dialogueRef.dialogueData[conversationIndex].dialogueSentece;

        if (portraitManager != null)
            portraitManager.GetCharOrder(dialogueRef, conversationIndex);

        if (!dialogueRef.isHasChoice)
            choicePanel.SetActive(false);

        SetBackground();
        SetAndPlayTalkSfx();
        CheckUseSentenceBG();
        SetAndPlayVoiceActor();

        StartCoroutine(TextAnimation());
    }

    //mengecek jika data sudah sampe akhir maka dialog selesai
    void EndConversation()
    {
        //Debug.Log("End Dialogue!");
        SetTransition(startTransition, false);
        CheckUpdateSomething();
        if (dialogueRef.isHasChoice)
            SetAndShowChoiceBtn();
        else
            dialoguePanel.SetActive(false);

    }

    //Digunakan untuk melanjutkan dialog
    void NextConversation()
    {
        if (dialogueRef == null) return;
        //cek agar dialogueIndex tdk ketambah
        if (conversationIndex >= dialogueRef.dialogueData.Capacity) return;
        conversationIndex++;
        //mulai dialog
        StartConversation();
    }

    //skip dialog
    public void SkipDialogue()
    {
        if (portraitManager != null)
            portraitManager.CheckTalkingAnimation(dialogueRef, conversationIndex, false);

        conversationIndex = dialogueRef.dialogueData.Capacity - 1;
        StartConversation();
    }

    //tampil / sembunyikan dialog
    public void ShowHidePanel()
    {
        isHide = !isHide;
        sentencePanel.gameObject.SetActive(!isHide);

        isAuto = false;
        CheckAutoNextBtn();
    }

    //button autonext
    public void AutoNextBtn()
    {
        isAuto = !isAuto;
        CheckAutoNextBtn();
    }

    void CheckAutoNextBtn()
    {
        if (isAuto)
            textAutoTMP.text = "Stop";
        else
            textAutoTMP.text = "Auto";
    }

    //membuat auto next dialog
    IEnumerator AutoNextDialogue()
    {
        yield return new WaitForSeconds(timeToNext);
        NextConversation();
    }

    //mengganti textspeed
    public void ChangeTextSpeed()
    {
        if (txtSpdChanger >= 2) txtSpdChanger = -1;
        txtSpdChanger++;

        if (txtSpdChanger == 0)
            SetTextSpeed(0.075f, "Slow");
        else if (txtSpdChanger == 1)
            SetTextSpeed(0.03f, "Normal");
        else if (txtSpdChanger == 2)
            SetTextSpeed(0.01f, "Fast");
    }

    void SetTextSpeed(float _textSpeed, string message)
    {
        textSpeed = _textSpeed;
        textSpeedTMP.text = message;
    }

    //matikan semua button pilihan
    void ResetChoiceBtn()
    {
        for (int i = 0; i < choiceBtn.Capacity; i++)
            choiceBtn[i].SetActive(false);
    }

    //Be sure to disable all the choice Btn first!
    void SetAndShowChoiceBtn()
    {
        if (!dialogueRef.isHasChoice) return;
        choicePanel.SetActive(true);
        for (int i = 0; i < dialogueRef.choiceList.Capacity; i++)
        {
            choiceBtn[i].SetActive(true);
            choiceBtn[i].GetComponentInChildren<TextMeshProUGUI>().text = dialogueRef.choiceList[i].name;
        }
    }

    //pilih button dan setelah itu di reset dan mulai lagi dialog
    public void SelectChoiceBtn(int select)
    {
        dialogueRef = dialogueRef.choiceList[select];
        conversationIndex = 0;
        ResetChoiceBtn();
        choicePanel.SetActive(false);
        StartConversation();
    }

    void SetBackground()
    {
        bool condition = dialogueRef.background != null;
        backgroundPanel.enabled = condition;
        backgroundPanel.sprite = dialogueRef.background;
    }

    void SetTransition(Animator anim, bool condition)
    {
        if (!dialogueRef.isUseTransition) return;
        anim.SetBool("start", condition);
    }

    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(startDelay);
        StartConversation();
    }

    void SetAndPlayTalkSfx()
    {
        GameObject o_portraitData = dialogueRef.dialogueData[conversationIndex].portraitData;
        if (o_portraitData == null) return;
        PortraitData portraitData = o_portraitData.GetComponent<PortraitData>();
        AudioClip talkSfx = portraitData.talkSfx;
        voiceAudioSource.loop = true;
        voiceAudioSource.clip = talkSfx;
        voiceAudioSource.Play();
    }

    void SetAndPlayVoiceActor()
    {
        AudioClip voiceActor = dialogueRef.dialogueData[conversationIndex].voiceActorClip;
        if (voiceActor == null) return;
        voiceAudioSource.clip = voiceActor;
        voiceAudioSource.loop = false;
        voiceAudioSource.Play();
    }

    void PrintAllSentence()
    {
        if (sentenceHistoryPanel.activeSelf) return;
        if (isHide) return;
        isPrint = !isPrint;
    }

    void CheckUseSentenceBG()
    {
        bool checkAltSentenceBG = dialogueRef.altSentenceBG != null;
        if (checkAltSentenceBG)
        {
            sentencePanel.enabled = checkAltSentenceBG;
            namePanel.SetActive(checkAltSentenceBG);
        }

        bool checkNotUseSentenceBG = dialogueRef.isNotUseSentenceBG;
        if (checkNotUseSentenceBG)
        {
            sentencePanel.enabled = !checkNotUseSentenceBG;
            namePanel.SetActive(!checkNotUseSentenceBG);
        }
    }

    public void PrintSentenceHistoryBtn()
    {
        TextMeshProUGUI[] historyText = textHistoryPanel.GetComponentsInChildren<TextMeshProUGUI>();

        for (int i = 0; i <= conversationIndex; i++)
        {
            historyText[i].text =
            dialogueRef.dialogueData[i].charName + ": "
            + dialogueRef.dialogueData[i].dialogueSentece;
        }
    }

    void ResetSentenceHistory()
    {
        TextMeshProUGUI[] historyText = textHistoryPanel.GetComponentsInChildren<TextMeshProUGUI>();
        for (int i = 0; i < historyText.Length; i++)
            historyText[i].text = "";
    }

    public bool UpdateSomething => isUpdateSomething;
    void CheckUpdateSomething() => isUpdateSomething = dialogueRef.isUpdateSomething;

    IEnumerator TextAnimation()
    {
        textSentenceTMP.ForceMeshUpdate();

        int totalVisibleCharacters = textSentenceTMP.textInfo.characterCount;
        int counter = 0;
        int visibleCount = 0;
        bool isDone = false;
        doneTalkingImg.SetActive(isDone);

        if (portraitManager != null)
            portraitManager.CheckTalkingAnimation(dialogueRef, conversationIndex, !isDone);

        while (!isDone)
        {
            visibleCount = counter % (totalVisibleCharacters + 1);

            textSentenceTMP.maxVisibleCharacters = visibleCount;

            if (visibleCount >= totalVisibleCharacters && !isPrint)
            {
                isDone = true;
                isPrint = true;
                DoneTalking(isDone);
                yield return new WaitForSeconds(1.0f);
            }
            else if (isPrint && visibleCount <= totalVisibleCharacters)
            {
                isDone = true;
                visibleCount = totalVisibleCharacters;
                textSentenceTMP.maxVisibleCharacters = visibleCount;
                DoneTalking(isDone);
                yield return new WaitForSeconds(1.0f);
            }
            counter += 1;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void DoneTalking(bool isDone)
    {
        isCanNext = true;
        if (isAuto)
            StartCoroutine(AutoNextDialogue());

        doneTalkingImg.SetActive(isDone);

        if (portraitManager != null)
            portraitManager.CheckTalkingAnimation(dialogueRef, conversationIndex, !isDone);
    }
}
