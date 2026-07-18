using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TextTyper : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp;
    [SerializeField] private float defaultSpeed = 30f;
    [SerializeField] private float defaultShake = 0f;
    [SerializeField] private bool isDebugging = false;

    // Эффекты
    private List<ShakeChar> shakingChars = new List<ShakeChar>();
    private List<WaveChar> waveChars = new List<WaveChar>();
    private bool effectsActive = false;
    private Coroutine effectsCoroutine = null;

    [Space]
    [SerializeField] private AudioSource sourceType;
    [SerializeField] private int countCharType = 1; // по умолчанию каждый символ
    [SerializeField] private AudioDataPlay dataPlayType;
    private int typedCharsCount = 0;

    // Синхронизация операций
    private Coroutine currentOperation = null;

    public event Action<char> OnTypeChar;

    private struct ShakeChar
    {
        public int index;
        public float amplitude;
        public Vector3[] originalVertices;
    }

    private struct WaveChar
    {
        public int index;
        public float amplitude;
        public float frequency;
        public Vector3[] originalVertices;
    }

    private void Awake()
    {
        if (tmp == null) tmp = GetComponent<TMP_Text>();
        if (isDebugging) Debug.Log("[TextTyper] Awake");
    }

    // ======================== Публичные методы (обёртки с синхронизацией) ========================

    public IEnumerator TypeText(string rawMessage)
    {
        // Останавливаем текущую операцию, если она есть
        if (currentOperation != null)
        {
            if (isDebugging) Debug.Log("[TextTyper] Interrupting previous operation");
            StopCoroutine(currentOperation);
            StopAllEffects();       // полная очистка эффектов
            currentOperation = null;
        }
        currentOperation = StartCoroutine(TypeTextInternal(rawMessage));
        yield return currentOperation;
        currentOperation = null;
    }

    public void ClearText(float speedRemove = 50f)
    {
        StartCoroutine(PlayClearText(speedRemove));
    }

    public void ClearImmidiatly()
    {
        if(currentOperation != null)
            StopCoroutine(currentOperation);
        StopAllEffects();
        currentOperation = null;
        tmp.text = "";   
    }

    public IEnumerator PlayClearText(float speedRemove = 50f)
    {
        if (currentOperation != null)
        {
            if (isDebugging) Debug.Log("[TextTyper] Interrupting previous operation for clear");
            StopCoroutine(currentOperation);
            StopAllEffects();
            currentOperation = null;
        }
        currentOperation = StartCoroutine(PlayClearTextInternal(speedRemove));
        yield return currentOperation;
        currentOperation = null;
    }

    public IEnumerator ClearAndTypeText(string message, float speedRemove = 50f)
    {
        if (currentOperation != null)
        {
            if (isDebugging) Debug.Log("[TextTyper] Interrupting previous operation for clear&type");
            StopCoroutine(currentOperation);
            StopAllEffects();
            currentOperation = null;
        }
        currentOperation = StartCoroutine(ClearAndTypeTextInternal(message, speedRemove));
        yield return currentOperation;
        currentOperation = null;
    }

    // ======================== Внутренние реализации ========================

    private void StopAllEffects()
    {
        if (effectsCoroutine != null) StopCoroutine(effectsCoroutine);
        effectsActive = false;
        shakingChars.Clear();
        waveChars.Clear();
        tmp.SetVerticesDirty();
        tmp.maxVisibleCharacters = 0;
        if (isDebugging) 
            Debug.Log("[TextTyper] All effects stopped");
        typedCharsCount = 0;
    }

    private IEnumerator TypeTextInternal(string rawMessage)
    {
        StopAllEffects();
        if (isDebugging) Debug.Log($"[TextTyper] TypeTextInternal: {rawMessage}");

        ParseResult result = Parse(rawMessage);
        if (string.IsNullOrEmpty(result.cleanText)) yield break;

        tmp.text = result.cleanText;
        tmp.maxVisibleCharacters = 0;
        tmp.ForceMeshUpdate();

        var commands = result.commands;
        int cmdIdx = 0;
        float currentSpeed = defaultSpeed;
        float currentShake = defaultShake;
        float currentWaveAmp = 0f;
        float currentWaveFreq = 0f;
        Stack<float> speedStack = new Stack<float>();
        Stack<float> shakeStack = new Stack<float>();
        Stack<float> waveAmpStack = new Stack<float>();
        Stack<float> waveFreqStack = new Stack<float>();

        int totalChars = tmp.textInfo.characterCount; // видимые символы
        if (isDebugging) Debug.Log($"[TextTyper] Visible chars: {totalChars}");

        effectsActive = true;
        effectsCoroutine = StartCoroutine(EffectsUpdate());

        for (int visible = 0; visible <= totalChars; visible++)
        {
            // Применяем команды, запланированные на эту позицию
            while (cmdIdx < commands.Length && commands[cmdIdx].position == visible)
            {
                var cmd = commands[cmdIdx];
                if (!cmd.isClose)
                {
                    switch (cmd.type)
                    {
                        case CmdType.Speed:
                            speedStack.Push(currentSpeed);
                            currentSpeed = cmd.value;
                            if (isDebugging) Debug.Log($"Speed changed to {currentSpeed}");
                            break;
                        case CmdType.Shake:
                            shakeStack.Push(currentShake);
                            currentShake = cmd.value;
                            break;
                        case CmdType.Wave:
                            waveAmpStack.Push(currentWaveAmp);
                            waveFreqStack.Push(currentWaveFreq);
                            currentWaveAmp = cmd.value;
                            currentWaveFreq = cmd.freq;
                            break;
                        case CmdType.Pause:
                            yield return new WaitForSeconds(cmd.value);
                            break;
                    }
                }
                else
                {
                    switch (cmd.type)
                    {
                        case CmdType.Speed:
                            if (speedStack.Count > 0) currentSpeed = speedStack.Pop();
                            break;
                        case CmdType.Shake:
                            if (shakeStack.Count > 0) currentShake = shakeStack.Pop();
                            if (currentShake <= 0.01f) RemoveShakesFrom(visible);
                            break;
                        case CmdType.Wave:
                            if (waveAmpStack.Count > 0) currentWaveAmp = waveAmpStack.Pop();
                            if (waveFreqStack.Count > 0) currentWaveFreq = waveFreqStack.Pop();
                            if (currentWaveAmp <= 0.01f) RemoveWavesFrom(visible);
                            break;
                    }
                }
                cmdIdx++;
            }

            if (visible < totalChars)
            {
                tmp.maxVisibleCharacters = visible + 1;
                tmp.ForceMeshUpdate();
                yield return null; // даём время на обновление меша

                if (currentShake > 0.01f) AddShakingChar(visible, currentShake);
                if (currentWaveAmp > 0.01f) AddWaveChar(visible, currentWaveAmp, currentWaveFreq);

                float delay = 1f / Mathf.Max(0.01f, currentSpeed);
                yield return new WaitForSeconds(delay); 
                
                typedCharsCount++;
                if (sourceType != null && countCharType > 0 && typedCharsCount % countCharType == 0)
                {
                     sourceType.PlayOneShot(dataPlayType);
                }

                if (tmp.text.Length > visible + 1)
                {
                    char[] chars = tmp.text.Substring(visible, 1).ToCharArray();
                    if (chars.Length > 0)
                        OnTypeChar?.Invoke(chars[chars.Length - 1]);
                }
            }
        }
        if (isDebugging) Debug.Log("[TextTyper] Typing finished – effects continue");
    }

    private IEnumerator PlayClearTextInternal(float speedRemove)
    {
        StopAllEffects();
        tmp.ForceMeshUpdate();
        int currentVisible = Mathf.Min(tmp.maxVisibleCharacters, tmp.textInfo.characterCount);
        if (currentVisible <= 0) yield break;

        float delay = 1f / Mathf.Max(0.01f, speedRemove);
        for (int visible = currentVisible; visible > 0; visible--)
        {
            tmp.maxVisibleCharacters = visible;
            yield return new WaitForSeconds(delay);
        }
        if (isDebugging) Debug.Log("[TextTyper] Clear finished");
    }

    private IEnumerator ClearAndTypeTextInternal(string message, float speedRemove)
    {
        yield return PlayClearTextInternal(speedRemove);
        yield return TypeTextInternal(message);
    }

    // ======================== Методы управления эффектами ========================

    private void AddShakingChar(int idx, float amp)
    {
        if (tmp.textInfo == null || idx >= tmp.textInfo.characterCount) return;
        var info = tmp.textInfo;
        var c = info.characterInfo[idx];
        if (!c.isVisible) return;

        int mat = c.materialReferenceIndex;
        int vtx = c.vertexIndex;
        Vector3[] vertices = info.meshInfo[mat].vertices;
        Vector3[] orig = new Vector3[4];
        for (int i = 0; i < 4; i++) orig[i] = vertices[vtx + i];

        shakingChars.Add(new ShakeChar { index = idx, amplitude = amp, originalVertices = orig });
        if (isDebugging) Debug.Log($"[TextTyper] Add shake char {idx}, amp {amp}");
    }

    private void RemoveShakesFrom(int startIdx)
    {
        int removed = shakingChars.RemoveAll(sc => sc.index >= startIdx);
        if (isDebugging && removed > 0) Debug.Log($"[TextTyper] Removed {removed} shakes from {startIdx}");
    }

    private void AddWaveChar(int idx, float amp, float freq)
    {
        if (tmp.textInfo == null || idx >= tmp.textInfo.characterCount) return;
        var info = tmp.textInfo;
        var c = info.characterInfo[idx];
        if (!c.isVisible) return;

        int mat = c.materialReferenceIndex;
        int vtx = c.vertexIndex;
        Vector3[] vertices = info.meshInfo[mat].vertices;
        Vector3[] orig = new Vector3[4];
        for (int i = 0; i < 4; i++) orig[i] = vertices[vtx + i];

        waveChars.Add(new WaveChar { index = idx, amplitude = amp, frequency = freq, originalVertices = orig });
        if (isDebugging) Debug.Log($"[TextTyper] Add wave char {idx}, amp {amp}, freq {freq}");
    }

    private void RemoveWavesFrom(int startIdx)
    {
        int removed = waveChars.RemoveAll(w => w.index >= startIdx);
        if (isDebugging && removed > 0) Debug.Log($"[TextTyper] Removed {removed} waves from {startIdx}");
    }

    private IEnumerator EffectsUpdate()
    {
        while (effectsActive)
        {
            if (tmp.textInfo != null)
            {
                var info = tmp.textInfo;
                bool meshChanged = false;

                // Дрожание
                foreach (var sc in shakingChars)
                {
                    if (sc.index >= info.characterCount) continue;
                    var c = info.characterInfo[sc.index];
                    if (!c.isVisible) continue;

                    int mat = c.materialReferenceIndex;
                    int vtx = c.vertexIndex;
                    Vector3[] vertices = info.meshInfo[mat].vertices;

                    Vector3 offset = UnityEngine.Random.insideUnitSphere * sc.amplitude;
                    offset.z = 0;

                    vertices[vtx + 0] = sc.originalVertices[0] + offset;
                    vertices[vtx + 1] = sc.originalVertices[1] + offset;
                    vertices[vtx + 2] = sc.originalVertices[2] + offset;
                    vertices[vtx + 3] = sc.originalVertices[3] + offset;
                    meshChanged = true;
                }

                // Волны
                foreach (var wc in waveChars)
                {
                    if (wc.index >= info.characterCount) continue;
                    var c = info.characterInfo[wc.index];
                    if (!c.isVisible) continue;

                    int mat = c.materialReferenceIndex;
                    int vtx = c.vertexIndex;
                    Vector3[] vertices = info.meshInfo[mat].vertices;

                    float offsetY = wc.amplitude * Mathf.Sin(Time.time * wc.frequency * Mathf.PI * 2f + wc.index);
                    Vector3 offset = new Vector3(0, offsetY, 0);

                    vertices[vtx + 0] = wc.originalVertices[0] + offset;
                    vertices[vtx + 1] = wc.originalVertices[1] + offset;
                    vertices[vtx + 2] = wc.originalVertices[2] + offset;
                    vertices[vtx + 3] = wc.originalVertices[3] + offset;
                    meshChanged = true;
                }

                if (meshChanged)
                {
                    for (int i = 0; i < info.meshInfo.Length; i++)
                    {
                        if (info.meshInfo[i].mesh != null)
                        {
                            info.meshInfo[i].mesh.vertices = info.meshInfo[i].vertices;
                            tmp.UpdateGeometry(info.meshInfo[i].mesh, i);
                        }
                    }
                }
            }
            yield return null;
        }
    }

    // ======================== Парсер команд ========================

    public enum CmdType { Speed, Shake, Pause, Unknown, Wave }

    public struct ParsedCmd
    {
        public int position;
        public CmdType type;
        public float value;
        public float freq;
        public bool isClose;
    }

    public struct ParseResult
    {
        public string cleanText;
        public ParsedCmd[] commands;
    }

    public ParseResult Parse(string raw)
    {
        var cmds = new List<ParsedCmd>();
        var clean = new StringBuilder();
        int visibleCount = 0;
        int i = 0;
        int len = raw.Length;

        while (i < len)
        {
            if (raw[i] == '<')
            {
                int end = raw.IndexOf('>', i);
                if (end != -1)
                {
                    string tag = raw.Substring(i, end - i + 1);
                    // Извлекаем внутренность тега
                    string inner = tag;
                    bool hasBrace = inner.StartsWith("<{");
                    if (hasBrace)
                        inner = inner.Substring(2, inner.Length - 3);
                    else
                        inner = inner.Substring(1, inner.Length - 2);
                    if (inner.EndsWith("}"))
                        inner = inner.Substring(0, inner.Length - 1);

                    bool isClose = inner.StartsWith("/");
                    string cmdPart = isClose ? inner.Substring(1) : inner;
                    string cmdName = cmdPart.Split('=')[0].Trim().ToLower();

                    bool isTyperCommand = cmdName == "sp" || cmdName == "shake" || cmdName == "pause" || cmdName == "wave";

                    if (isTyperCommand)
                    {
                        string paramStr = cmdPart.Contains("=") ? cmdPart.Split('=')[1] : "";
                        CmdType type = CmdType.Unknown;
                        float val = 0f, freq = 0f;

                        switch (cmdName)
                        {
                            case "sp": type = CmdType.Speed; break;
                            case "shake": type = CmdType.Shake; break;
                            case "pause": type = CmdType.Pause; break;
                            case "wave": type = CmdType.Wave; break;
                        }

                        if (type != CmdType.Unknown && !string.IsNullOrEmpty(paramStr))
                        {
                            if (type == CmdType.Wave)
                            {
                                string[] wav = paramStr.Split(',');
                                if (wav.Length >= 1) float.TryParse(wav[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out val);
                                if (wav.Length >= 2) float.TryParse(wav[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out freq);
                            }
                            else
                            {
                                float.TryParse(paramStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out val);
                            }
                        }

                        cmds.Add(new ParsedCmd
                        {
                            position = visibleCount,
                            type = type,
                            value = val,
                            freq = freq,
                            isClose = isClose
                        });
                        i = end + 1;
                        continue;
                    }
                    else
                    {
                        // Обычный RichText-тег – оставляем в тексте, visibleCount не меняем
                        clean.Append(tag);
                        i = end + 1;
                        continue;
                    }
                }
            }
            // Обычный символ
            clean.Append(raw[i]);
            visibleCount++;
            i++;
        }

        if (isDebugging)
        {
            Debug.Log($"[Parse] Clean text: '{clean}'");
            foreach (var c in cmds)
                Debug.Log($"[Parse] Cmd: pos={c.position}, type={c.type}, val={c.value}, freq={c.freq}, close={c.isClose}");
        }

        return new ParseResult { cleanText = clean.ToString(), commands = cmds.ToArray() };
    }
}