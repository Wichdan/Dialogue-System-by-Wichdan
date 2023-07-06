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
    [SerializeField] List<TextMeshProUGUI> textHistoryTMP;

    [Header("Speaker")]
    [SerializeField] List<Speaker> speakers;

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
    [SerializeField] int txtSpdChanger;
    [SerializeField] TextMeshProUGUI textSpeedTMP;

    [Header("Choice")]
    [SerializeField] GameObject choicePanel;
    [SerializeField] List<GameObject> choiceBtn;

    [Header("Animation")]
    [SerializeField] Animator startTransition;
    [SerializeField] float startDelay = 1f;
    [SerializeField] Animator screenEffect;

    [Header("Sound")]
    [SerializeField] AudioSource talkAudioSource;

    [Header("Other Setting")]
    [SerializeField] bool isPlayOnStart;
    [SerializeField] bool isUpdateSomething;

    [System.Serializable]
    public struct Speaker
    {
        public string elementName;
        [Header("Name Panel")]
        public GameObject charNamePanel;
        public TextMeshProUGUI charNameTxt;

        [Header("Portrait")]
        public Image portrait;
        public GameObject mask;

        [Header("Animation")]
        public Animator eyesAnim;
        public Animator mouthAnim, gestureAnim;
    }

    bool isHide;
    int conversationIndex;
    bool isPrint, isCanNext;


    public static DialogueManager singleton;

    private void Reset()
    {
        textSpeed = 0.05f;
        txtSpdChanger = 0;
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
        talkAudioSource = GetComponent<AudioSource>();
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
        ResetSpeaker();
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

        GetCharOrder();

        if (!dialogueRef.isHasChoice)
            choicePanel.SetActive(false);

        SetBackground();
        SetTalkSfx();
        CheckUseSentenceBG();

        StartCoroutine(TextAnimation());
    }

    //Mengambil charOrder index sekarang
    void GetCharOrder()
    {
        int charOrder = (int)dialogueRef.dialogueData[conversationIndex].speakerOrder;
        //isi dan hidupkan nama panel
        for (int i = 0; i < speakers.Capacity; i++)
        {
            //menghidupkan / mematikan CharOrder nama sesuai dengan ordernya
            if (charOrder != i)
                SetNameAndPortraitMask(i, false);
            else
            {
                SetNameAndPortraitMask(i, true);
                if (dialogueRef.dialogueData[conversationIndex].portraitData != null)
                {
                    SetPortraitSprite(i);
                    SetAndPlayEyesAnimation(i);
                    SetAndPlayMouthAnimation(i);
                    PlayGestureAnimation(i);
                }
            }
        }
    }

    void SetNameAndPortraitMask(int index, bool condition)
    {
        speakers[index].charNamePanel.SetActive(condition);
        speakers[index].charNameTxt.text = dialogueRef.dialogueData[conversationIndex].charName;
        speakers[index].mask.SetActive(!condition);
    }

    void SetPortraitSprite(int index)
    {
        speakers[index].portrait.sprite =
        dialogueRef.dialogueData[conversationIndex].portraitData.GetComponent<PortraitData>().portrait;
        if (speakers[index].portrait.sprite != null)
            speakers[index].portrait.gameObject.SetActive(true);
        else if (speakers[index].portrait.sprite == null)
            speakers[index].portrait.gameObject.SetActive(false);
    }

    void SetAndPlayEyesAnimation(int index)
    {
        speakers[index].eyesAnim.runtimeAnimatorController =
        dialogueRef.dialogueData[conversationIndex].portraitData.GetComponent<PortraitData>().eyesCharCtrller;

        if (speakers[index].eyesAnim.runtimeAnimatorController == null)
            speakers[index].eyesAnim.gameObject.SetActive(false);
        else
            speakers[index].eyesAnim.gameObject.SetActive(true);

        speakers[index].eyesAnim.SetFloat("eyesValue", dialogueRef.dialogueData[conversationIndex].eyesValue);
    }

    void SetAndPlayMouthAnimation(int index)
    {
        speakers[index].mouthAnim.runtimeAnimatorController =
        dialogueRef.dialogueData[conversationIndex].portraitData.GetComponent<PortraitData>().mouthCharCtrller;

        if (speakers[index].mouthAnim.runtimeAnimatorController == null)
            speakers[index].mouthAnim.gameObject.SetActive(false);
        else
            speakers[index].mouthAnim.gameObject.SetActive(true);

        speakers[index].mouthAnim.SetFloat("mouthValue", dialogueRef.dialogueData[conversationIndex].mouthValue);
    }

    void PlayTalkingAnimation(bool isTalk)
    {
        if (dialogueRef.dialogueData[conversationIndex].portraitData == null) return;
        int charOrder = (int)dialogueRef.dialogueData[conversationIndex].speakerOrder;
        for (int i = 0; i < speakers.Capacity; i++)
        {
            if (charOrder == i)
                speakers[i].mouthAnim.SetBool("isTalk", isTalk);
        }
    }

    void PlayGestureAnimation(int index)
    {
        speakers[index].gestureAnim.SetFloat("gestureValue", dialogueRef.dialogueData[conversationIndex].gestureValue);
    }

    //mengecek jika data sudah sampe akhir maka dialog selesai
    void EndConversation()
    {
        Debug.Log("End Dialogue!");
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
        PlayTalkingAnimation(false);
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
            SetTextSpeed(0.1f, "Slow");
        else if (txtSpdChanger == 1)
            SetTextSpeed(0.05f, "Normal");
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
        if (dialogueRef.isUseBackground)
        {
            backgroundPanel.enabled = true;
            backgroundPanel.sprite = dialogueRef.background;
        }
        else
            backgroundPanel.enabled = false;
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

    void SetTalkSfx()
    {
        if (dialogueRef.dialogueData[conversationIndex].portraitData == null) return;
        talkAudioSource.clip =
        dialogueRef.dialogueData[conversationIndex].portraitData.GetComponent<PortraitData>().talkSfx;
    }

    void ResetSpeaker()
    {
        for (int i = 0; i < speakers.Capacity; i++)
        {
            speakers[i].portrait.gameObject.SetActive(false);
            speakers[i].portrait.sprite = null;
            speakers[i].eyesAnim.runtimeAnimatorController = null;
            speakers[i].mouthAnim.runtimeAnimatorController = null;
        }
    }

    void PrintAllSentence()
    {
        if(sentenceHistoryPanel.activeSelf) return;
        if (isHide) return;
        isPrint = !isPrint;
    }

    void CheckUseSentenceBG()
    {
        sentencePanel.enabled = !dialogueRef.isNotUseSentenceBG;
        namePanel.SetActive(!dialogueRef.isNotUseSentenceBG);
    }

    public void PrintSentenceHistoryBtn()
    {
        for (int i = 0; i <= conversationIndex; i++)
        {
            textHistoryTMP[i].text =
            dialogueRef.dialogueData[i].charName + ": "
            + dialogueRef.dialogueData[i].dialogueSentece;
        }

        Button sentenceHistoryBtn = null;
        for (int i = 0; i < textHistoryTMP.Capacity; i++)
        {
            sentenceHistoryBtn = textHistoryTMP[i].GetComponentInParent<Button>();
            if (textHistoryTMP[i].text != "")
                sentenceHistoryBtn.interactable = true;
            else
                sentenceHistoryBtn.interactable = false;
        }
    }

    void ResetSentenceHistory()
    {
        for (int i = 0; i < textHistoryTMP.Capacity; i++)
            textHistoryTMP[i].text = "";
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
        PlayTalkingAnimation(!isDone);
        talkAudioSource.Play();

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

        talkAudioSource.Stop();
        doneTalkingImg.SetActive(isDone);
        PlayTalkingAnimation(!isDone);
    }
}
