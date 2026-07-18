using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class ThingSpiker : OutSpikerViewSpecific
{
    [SerializeField] private Animator animator;
    [SerializeField] private string[] triggersAnim = { "a", "aa", "aaa" };
    [SerializeField] private string triggerAnimStop = "Stop";

    [Space]
    [SerializeField] private TextTyper textTyper;
    [SerializeField] private char[] animChar = new char[]
    {
        'à', 'å', '¸', 'è', 'î', 'ó', 'û', 'ý', 'þ', 'ÿ'
    };

    [SerializeField] private float durationCoef = 0.2f;
    [SerializeField] private float animantionDuration = 0.2f;
    [SerializeField] private float waitToReed = 1.5f;

    [Space]
    [SerializeField] private AudioSource sourceTalk;
    [SerializeField] private bool IsTyperVoice;
    [SerializeField] private float typeTalkDuration;

    private void Awake()
    {
        StartCoroutine(VoiceRoutine());
    }

    public override IEnumerator Play(OutDialogElement element)
    {
        var spiker = DialogGlobalData.GetSpiker(element.IDSpiker);
        AudioDataPlay audioVoice = null;
        if (spiker.voiceType.clip != null)
            audioVoice = spiker.voiceType;

        if(audioVoice != null && !IsTyperVoice)        
           sourceTalk.Play(audioVoice);

        string text = element.Text;
        if(textTyper != null)
            text = textTyper.Parse(element.Text).cleanText;
        int length = text.Length;

        if (IsTyperVoice)
            isPlayVoice = true;

        float wait = (length * durationCoef) + waitToReed;
        float step = length * durationCoef;

        int currentAfter = 0;
        while (wait > 0)
        {
            wait -= animantionDuration;

            currentAfter++;
            if (currentAfter >= length - 1)
                currentAfter = 0;

            PlayChar(text, currentAfter);
            yield return new WaitForSeconds(animantionDuration);
        }

        if (IsTyperVoice)
            isPlayVoice = false;

        if (gameObject == null || gameObject.activeInHierarchy == false)
            yield break;

        sourceTalk.Stop();
        animator.SetTrigger(triggerAnimStop);
    }

    bool isPlayVoice;
    private IEnumerator VoiceRoutine()
    {
        yield return new WaitForEndOfFrame();

        var spiker = DialogGlobalData.GetSpiker(IDSpiker);
        AudioDataPlay audioVoice = null;
        if (spiker.voiceType.clip != null)
            audioVoice = spiker.voiceType;

        while (true)
        {
            if(isPlayVoice && IsTyperVoice)
               sourceTalk.PlayOneShot(audioVoice);
            yield return new WaitForSeconds(typeTalkDuration);
        }
    }

    private void PlayChar(string text, int current)
    {
        string ch = text.Substring(current, 1);
        if (animator == null)
            return;

        if (ch.Any(x => animChar.Contains(x)))
            animator.SetTrigger(triggersAnim.RandomElement());
        else
            animator.SetTrigger(triggerAnimStop);
    }

    public override void Break()
    {
        StopCoroutine("Play");
        //StopAllCoroutines();
        sourceTalk.Stop();
        animator.SetTrigger(triggerAnimStop);
        isPlayVoice = false;
    }
}
