using System.Collections.Generic;
using UnityEngine;

public class PortraitData : MonoBehaviour
{
    public List<SpeakerData> speakerData;
    public AudioClip talkSfx;

    [System.Serializable]
    public struct SpeakerData
    {
        public string speakerName;
        public Sprite portrait;
        public RuntimeAnimatorController eyesCharCtrller;
        public RuntimeAnimatorController mouthCharCtrller;
    }
}
