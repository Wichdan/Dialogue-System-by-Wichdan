using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PortraitManager : MonoBehaviour
{
    [Header("Speaker")]
    [SerializeField] List<Speaker> speakerReference;
    [SerializeField] List<string> twoOrMoreSpeakerName;


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

    //Mengambil charOrder index sekarang
    public void GetCharOrder(Dialogue dialogueRef, int conversationIndex)
    {
        int charOrder = (int)dialogueRef.dialogueData[conversationIndex].speakerOrder;

        string charName = dialogueRef.dialogueData[conversationIndex].charName;

        PortraitData portraitData = null;
        if (dialogueRef.dialogueData[conversationIndex].portraitData != null)
            portraitData = dialogueRef.dialogueData[conversationIndex].portraitData.GetComponent<PortraitData>();
        //isi dan hidupkan nama panel
        for (int i = 0; i < speakerReference.Capacity; i++)
        {
            //menghidupkan / mematikan CharOrder nama sesuai dengan ordernya
            if (charOrder == i)
            {
                SetCharName(i, charName, true);
                SetPortraitMask(i, true);

                if (portraitData != null)
                {
                    SetSinglePortraitData(i, portraitData);
                    CheckPortraitData(dialogueRef, conversationIndex, i);
                }
            }
            else
            {
                SetCharName(i, charName, false);
                SetPortraitMask(i, false);
            }

            for (int j = 0; j < twoOrMoreSpeakerName.Capacity; j++)
            {
                if (dialogueRef.dialogueData[conversationIndex].charName == twoOrMoreSpeakerName[j])
                {
                    speakerReference[i].mask.SetActive(false);
                    SetAllPortraitData(dialogueRef, conversationIndex, portraitData);
                }
            }
        }
    }

    void SetCharName(int speakerRefIndex, string charName, bool condition)
    {
        speakerReference[speakerRefIndex].charNamePanel.SetActive(condition);
        speakerReference[speakerRefIndex].charNameTxt.text = charName;

        if (charName == "")
            speakerReference[speakerRefIndex].charNamePanel.SetActive(false);
    }

    void SetPortraitMask(int index, bool condition)
    {
        speakerReference[index].mask.SetActive(!condition);
    }

    void CheckPortraitSprite(int index)
    {
        if (speakerReference[index].portrait.sprite != null)
            speakerReference[index].portrait.gameObject.SetActive(true);
        else
            speakerReference[index].portrait.gameObject.SetActive(false);
    }

    void PlayEyesAnimation(float eyesValue, int speakerRefIndex)
    {
        if (speakerReference[speakerRefIndex].eyesAnim.runtimeAnimatorController != null)
        {
            speakerReference[speakerRefIndex].eyesAnim.gameObject.SetActive(true);
            speakerReference[speakerRefIndex].eyesAnim.SetFloat("eyesValue", eyesValue);
        }
        else
            speakerReference[speakerRefIndex].eyesAnim.gameObject.SetActive(false);

    }

    void PlayMouthAnimation(float mouthValue, int speakerRefIndex)
    {
        if (speakerReference[speakerRefIndex].mouthAnim.runtimeAnimatorController != null)
        {
            speakerReference[speakerRefIndex].mouthAnim.gameObject.SetActive(true);
            speakerReference[speakerRefIndex].mouthAnim.SetFloat("mouthValue", mouthValue);
        }
        else
            speakerReference[speakerRefIndex].mouthAnim.gameObject.SetActive(false);
    }

    void SetSinglePortraitData(int speakerIndex, PortraitData portraitData)
    {
        if (portraitData.speakerData.Capacity == 1)
            SetPortraitReference(speakerIndex, 0, portraitData);
    }

    void SetPortraitReference(int speakerIndex, int speakerData, PortraitData portraitData)
    {
        speakerReference[speakerIndex].portrait.sprite =
        portraitData.speakerData[speakerData].portrait;

        speakerReference[speakerIndex].eyesAnim.runtimeAnimatorController =
        portraitData.speakerData[speakerData].eyesCharCtrller;

        speakerReference[speakerIndex].mouthAnim.runtimeAnimatorController =
        portraitData.speakerData[speakerData].mouthCharCtrller;
    }

    void CheckPortraitData(Dialogue dialogueRef, int conversationIndex, int index)
    {
        float mouthValue = dialogueRef.dialogueData[conversationIndex].mouthValue;
        float eyesValue = dialogueRef.dialogueData[conversationIndex].eyesValue;
        float gestureValue = dialogueRef.dialogueData[conversationIndex].gestureValue;

        CheckPortraitSprite(index);
        PlayEyesAnimation(eyesValue, index);
        PlayMouthAnimation(mouthValue, index);
        PlayGestureAnimation(gestureValue, index);
    }

    void SetAllPortraitData(Dialogue dialogueRef, int conversationIndex, PortraitData portraitData)
    {
        if (portraitData.speakerData.Capacity > 1)
        {
            for (int i = 0; i < portraitData.speakerData.Capacity; i++)
            {
                if (portraitData.speakerData[i].speakerName != "")
                {
                    SetPortraitReference(i, i, portraitData);
                    CheckPortraitData(dialogueRef, conversationIndex, i);
                }
                else
                    speakerReference[i].mask.SetActive(true);
            }
        }
    }

    public void CheckTalkingAnimation(Dialogue dialogueRef, int conversationIndex, bool isTalk)
    {
        int charOrder = (int)dialogueRef.dialogueData[conversationIndex].speakerOrder;
        for (int i = 0; i < speakerReference.Capacity; i++)
        {
            if (charOrder == i)
                PlayTalkingAnimation(i, isTalk);

            for (int j = 0; j < twoOrMoreSpeakerName.Capacity; j++)
            {
                if (dialogueRef.dialogueData[conversationIndex].charName == twoOrMoreSpeakerName[j])
                    PlayTalkingAnimation(i, isTalk);
            }
        }

        PortraitData portraitData = null;
        if (dialogueRef.dialogueData[conversationIndex].portraitData != null)
            portraitData = dialogueRef.dialogueData[conversationIndex].portraitData.GetComponent<PortraitData>();
        else
            return;

        for (int i = 0; i < portraitData.speakerData.Capacity; i++)
        {
            if (portraitData.speakerData[i].speakerName == "")
                PlayTalkingAnimation(i, false);
        }
    }

    void PlayTalkingAnimation(int index, bool isTalk)
    {
        if (speakerReference[index].portrait.gameObject.activeSelf)
            speakerReference[index].mouthAnim.SetBool("isTalk", isTalk);
    }

    void PlayGestureAnimation(float gestureValue, int index)
    {
        if (speakerReference[index].portrait.gameObject.activeSelf)
            speakerReference[index].gestureAnim.SetFloat("gestureValue", gestureValue);
    }

    public void ResetSpeaker()
    {
        for (int i = 0; i < speakerReference.Capacity; i++)
        {
            speakerReference[i].portrait.gameObject.SetActive(false);
            speakerReference[i].portrait.sprite = null;
            speakerReference[i].eyesAnim.runtimeAnimatorController = null;
            speakerReference[i].mouthAnim.runtimeAnimatorController = null;
        }
    }
}
