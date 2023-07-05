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
    [SerializeField] TextMeshProUGUI sentenceTxt;
    [SerializeField] private TMP_Text m_textMeshPro;

    [Header("Speaker")]
    [SerializeField] List<Speaker> speakers;

    [Header("Other Reference")]
    [SerializeField] GameObject doneImg;
    [SerializeField] GameObject dialoguePanel, sentencePanel;
    [SerializeField] Image backgroundPanel;
    [SerializeField] Button nextButton;

    [Header("Auto Next")]
    [SerializeField] bool isAuto;
    [SerializeField] float timeToNext = 3f;
    [SerializeField] TextMeshProUGUI txtAuto;

    [Header("Text Speed")]
    [SerializeField] float textSpeed = 0.05f;
    [SerializeField] int txtSpdChanger;
    [SerializeField] TextMeshProUGUI txtSpeed;

    [Header("Choice")]
    [SerializeField] GameObject choicePanel;
    [SerializeField] List<GameObject> choiceBtn;

    [Header("Animation")]
    [SerializeField] Animator startTransition;
    [SerializeField] float startDelay = 1f;
    [SerializeField] Animator screenEffect;

    [Header("Sound")]
    public AudioSource audioSource;

    [Header("Other Setting")]
    [SerializeField] bool isPlayOnStart;
    [SerializeField] bool updateSomething;

    [System.Serializable]
    public struct Speaker
    {
        public string speakerOrder;
        public GameObject charNamePanel;
        public TextMeshProUGUI charNameTxt;
        public Image portrait;
        public GameObject mask;
        public Animator eyesAnim, mouthAnim;
    }

    bool isHide;
    int dialogueIndex;
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
        audioSource = GetComponent<AudioSource>();
        ChangeTextSpeed();
        //StartDialogue();
        nextButton.onClick.AddListener(() =>
        {
            SwapPrint();
        });
    }

    private void Update()
    {
        if (dialogueRef == null) return;
        if (Input.GetKeyDown(KeyCode.Space))
            SwapPrint();

        if (isCanNext && !isPrint)
            NextConversation();

    }

    public void SetDialogueRef(Dialogue reference) => dialogueRef = reference;

    //dialog dimulai
    public void StartDialogue()
    {
        if (dialogueRef == null) return;
        ResetSpeaker();
        dialogueIndex = 0;
        updateSomething = false;
        SetTransition(startTransition, true);
        StartCoroutine(StartDelay());
    }

    //Digunakan untuk memulai pembicaraan
    void StartConversation()
    {
        isPrint = false;
        isCanNext = false;
        StopAllCoroutines();

        if (dialogueIndex >= dialogueRef.dialogueData.Capacity)
        {
            EndConversation();
            return;
        }

        //hidupin dialog panel saat mulai
        dialoguePanel.SetActive(true);

        //ngisi sentencenya dari referensi dialog
        sentenceTxt.text = dialogueRef.dialogueData[dialogueIndex].dialogueSentece;

        GetCharOrder();

        if (!dialogueRef.isHasChoice)
            choicePanel.SetActive(false);

        SetBackground();
        SetTalkSfx();

        StartCoroutine(TextAnimation());
    }

    //Mengambil charOrder index sekarang
    void GetCharOrder()
    {
        int charOrder = (int)dialogueRef.dialogueData[dialogueIndex].charOrder;
        //isi dan hidupkan nama panel
        for (int i = 0; i < speakers.Capacity; i++)
        {
            //menghidupkan / mematikan CharOrder nama sesuai dengan ordernya
            if (charOrder != i)
            {
                SetName(i, false);
            }
            else
            {
                SetName(i, true);
                if (dialogueRef.dialogueData[dialogueIndex].portraitData != null)
                {
                    SetPortrait(i);
                    SetEyesAnimation(i);
                    SetMouthAnimation(i);
                }
            }
        }
    }

    void SetName(int index, bool condition)
    {
        speakers[index].charNamePanel.SetActive(condition);
        speakers[index].charNameTxt.text = dialogueRef.dialogueData[dialogueIndex].charName;
        speakers[index].mask.SetActive(!condition);
    }

    void SetPortrait(int index)
    {
        speakers[index].portrait.sprite =
        dialogueRef.dialogueData[dialogueIndex].portraitData.GetComponent<PortraitData>().portrait;
        if (speakers[index].portrait.sprite != null)
            speakers[index].portrait.gameObject.SetActive(true);
        else if (speakers[index].portrait.sprite == null)
            speakers[index].portrait.gameObject.SetActive(false);
    }

    void SetEyesAnimation(int index)
    {
        speakers[index].eyesAnim.runtimeAnimatorController =
        dialogueRef.dialogueData[dialogueIndex].portraitData.GetComponent<PortraitData>().eyesCharCtrller;

        if (speakers[index].eyesAnim.runtimeAnimatorController == null)
            speakers[index].eyesAnim.gameObject.SetActive(false);
        else
            speakers[index].eyesAnim.gameObject.SetActive(true);

        speakers[index].eyesAnim.SetFloat("eyesValue", dialogueRef.dialogueData[dialogueIndex].eyesValue);
    }

    void SetMouthAnimation(int index)
    {
        speakers[index].mouthAnim.runtimeAnimatorController =
        dialogueRef.dialogueData[dialogueIndex].portraitData.GetComponent<PortraitData>().mouthCharCtrller;

        if (speakers[index].mouthAnim.runtimeAnimatorController == null)
            speakers[index].mouthAnim.gameObject.SetActive(false);
        else
            speakers[index].mouthAnim.gameObject.SetActive(true);

        speakers[index].mouthAnim.SetFloat("mouthValue", dialogueRef.dialogueData[dialogueIndex].mouthValue);
    }

    void SetTalkingAnimation(bool isTalk)
    {
        if (dialogueRef.dialogueData[dialogueIndex].portraitData == null) return;
        int charOrder = (int)dialogueRef.dialogueData[dialogueIndex].charOrder;
        for (int i = 0; i < speakers.Capacity; i++)
        {
            if (charOrder == i)
                speakers[i].mouthAnim.SetBool("isTalk", isTalk);
        }
    }

    //mengecek jika data sudah sampe akhir maka dialog selesai
    void EndConversation()
    {
        Debug.Log("End Dialogue!");
        SetTransition(startTransition, false);
        CheckUpdateSomething();
        if (dialogueRef.isHasChoice)
            SetChoice();
        else
            dialoguePanel.SetActive(false);

    }

    //Digunakan untuk melanjutkan dialog
    void NextConversation()
    {
        if (dialogueRef == null) return;
        //cek agar dialogueIndex tdk ketambah
        if (dialogueIndex >= dialogueRef.dialogueData.Capacity) return;
        dialogueIndex++;
        //mulai dialog
        StartConversation();
    }

    //skip dialog
    public void SkipDialogue()
    {
        SetTalkingAnimation(false);
        dialogueIndex = dialogueRef.dialogueData.Capacity - 1;
        StartConversation();
    }

    //tampil / sembunyikan dialog
    public void ShowHidePanel()
    {
        isHide = !isHide;
        sentencePanel.SetActive(!isHide);

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
            txtAuto.text = "Stop";
        else
            txtAuto.text = "Auto";
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
        txtSpeed.text = message;
    }

    //matikan semua button pilihan
    void ResetChoiceBtn()
    {
        for (int i = 0; i < choiceBtn.Capacity; i++)
            choiceBtn[i].SetActive(false);
    }

    //Be sure to disable all the choice Btn first!
    void SetChoice()
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
        dialogueIndex = 0;
        ResetChoiceBtn();
        choicePanel.SetActive(false);
        StartConversation();
    }

    void SetBackground()
    {
        if (dialogueRef.background == null) return;
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

    void SetTalkSfx()
    {
        if (dialogueRef.dialogueData[dialogueIndex].portraitData == null) return;
        audioSource.clip =
        dialogueRef.dialogueData[dialogueIndex].portraitData.GetComponent<PortraitData>().talkSfx;
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

    void SwapPrint()
    {
        if(isHide) return;
        isPrint = !isPrint;
    }
    public bool GetUpdateSomething() => updateSomething;
    void CheckUpdateSomething() => updateSomething = dialogueRef.updateSomething;

    IEnumerator TextAnimation()
    {
        m_textMeshPro.ForceMeshUpdate();

        int totalVisibleCharacters = m_textMeshPro.textInfo.characterCount;
        int counter = 0;
        int visibleCount = 0;
        bool isDone = false;
        doneImg.SetActive(isDone);
        SetTalkingAnimation(!isDone);
        audioSource.Play();

        while (!isDone)
        {
            visibleCount = counter % (totalVisibleCharacters + 1);

            m_textMeshPro.maxVisibleCharacters = visibleCount;

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
                m_textMeshPro.maxVisibleCharacters = visibleCount;
                DoneTalking(isDone);
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

        audioSource.Stop();
        doneImg.SetActive(isDone);
        SetTalkingAnimation(!isDone);
    }
}
