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

    [Header("Name Panel")]
    [SerializeField] TextMeshProUGUI[] charNameTxt;
    [SerializeField] GameObject[] charNamePanel;

    [Header("Portrait")]
    [SerializeField] GameObject[] portrait;
    [SerializeField] Animator[] eyesAnim, mouthAnim;

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

    [Header("Other Setting")]
    [SerializeField] bool isPlayOnStart;
    [SerializeField] bool updateSomething;
    bool isHide;

    int dialogueIndex;

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
        ChangeTextSpeed();
        //StartDialogue();
        nextButton.onClick.AddListener(()=>{
            NextConversation();
        });
    }

    private void OnEnable()
    {
        ResetChoiceBtn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextConversation();
        }
    }

    public void SetDialogueRef(Dialogue reference) => dialogueRef = reference;

    //dialog dimulai
    public void StartDialogue()
    {
        if(dialogueRef == null) return;
        dialogueIndex = 0;
        updateSomething = false;
        SetTransition(startTransition, true);
        StartCoroutine(StartDelay());
    }

    //Digunakan untuk memulai pembicaraan
    void StartConversation()
    {
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

        StartCoroutine(TextAnimation());
    }

    //Mengambil charOrder index sekarang
    void GetCharOrder()
    {
        int charOrder = (int)dialogueRef.dialogueData[dialogueIndex].charOrder;
        //isi dan hidupkan nama panel
        for (int i = 0; i < charNamePanel.Length; i++)
        {
            charNameTxt[i] = charNamePanel[i].GetComponentInChildren<TextMeshProUGUI>();
            //menghidupkan / mematikan CharOrder nama sesuai dengan ordernya
            if (charOrder != i)
            {
                charNamePanel[i].SetActive(false);
                portrait[i].SetActive(false);
            }
            else
            {
                charNamePanel[i].SetActive(true);
                charNameTxt[i].text = dialogueRef.dialogueData[dialogueIndex].charName;
                portrait[i].SetActive(true);
                SetPortrait(i);
                SetEyesAnimation(i);
                SetMouthAnimation(i);
            }
        }
    }

    void SetPortrait(int index)
    {
        Image portraitImg = portrait[index].GetComponent<Image>();

        if (dialogueRef.dialogueData[dialogueIndex].portraitData != null)
        {
            portraitImg.sprite =
            dialogueRef.dialogueData[dialogueIndex].portraitData.GetComponent<PortraitData>().portrait;
        }
        else
            portrait[index].SetActive(false);
    }

    void SetEyesAnimation(int index)
    {
        if (dialogueRef.dialogueData[dialogueIndex].portraitData == null) return;
        eyesAnim[index].runtimeAnimatorController =
        dialogueRef.dialogueData[dialogueIndex].portraitData.GetComponent<PortraitData>().eyesCharCtrller;
        eyesAnim[index].SetFloat("eyesValue", dialogueRef.dialogueData[dialogueIndex].eyesValue);
    }

    void SetMouthAnimation(int index)
    {
        if (dialogueRef.dialogueData[dialogueIndex].portraitData == null) return;
        mouthAnim[index].runtimeAnimatorController =
        dialogueRef.dialogueData[dialogueIndex].portraitData.GetComponent<PortraitData>().mouthCharCtrller;
        mouthAnim[index].SetFloat("mouthValue", dialogueRef.dialogueData[dialogueIndex].mouthValue);
    }

    void SetTalkingAnimation(bool isTalk)
    {
        if (dialogueRef.dialogueData[dialogueIndex].portraitData == null) return;
        int charOrder = (int)dialogueRef.dialogueData[dialogueIndex].charOrder;
        for (int i = 0; i < charNamePanel.Length; i++)
        {
            if (charOrder == i)
                mouthAnim[i].SetBool("isTalk", isTalk);
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
        else{
            dialoguePanel.SetActive(false);
            
        }
    }

    //Digunakan untuk melanjutkan dialog
    void NextConversation()
    {
        if(dialogueRef == null) return;
        //cek agar dialogueIndex tdk ketambah
        if (dialogueIndex >= dialogueRef.dialogueData.Capacity) return;
        dialogueIndex++;
        //mulai dialog
        StartConversation();
    }

    //skip dialog
    public void SkipDialogue()
    {
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
        if(dialogueRef.background == null) return;
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

        while (!isDone)
        {
            visibleCount = counter % (totalVisibleCharacters + 1);

            m_textMeshPro.maxVisibleCharacters = visibleCount;

            if (visibleCount >= totalVisibleCharacters)
            {

                if (isAuto)
                    StartCoroutine(AutoNextDialogue());

                isDone = true;
                doneImg.SetActive(isDone);
                SetTalkingAnimation(!isDone);
                yield return new WaitForSeconds(1.0f);
            }

            counter += 1;

            yield return new WaitForSeconds(textSpeed);
        }
    }
}
