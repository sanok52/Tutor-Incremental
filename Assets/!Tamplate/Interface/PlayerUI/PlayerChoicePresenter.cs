using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChoicePresenter : MonoBehaviour, IWaiterEventListener
{
    public int choise = -1;

    [SerializeField] private Button[] buttons;
    private bool[] canInteractButton;
    private bool isBlock;

    private void Start()
    {
        canInteractButton = new bool[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            int n = i;
            buttons[i].onClick.AddListener(() => Choise(n));
            buttons[i].gameObject.SetActive(false);
            canInteractButton[i] = true;
        }
    }

    private void Choise(int n)
    {
        choise = n;
    }

    public IEnumerator WaitPlayerChoice(string[] choices, Action<int> action, float wait = -1f)
    {
        BreakChoise();

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < choices.Length)
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = choices[i];
            }
            else
                buttons[i].gameObject.SetActive(false);
        }

        if(wait <= 0f)
            yield return new WaitUntil(() => choise >= 0);
        else
        {
            while (wait > 0 && choise >= 0)
            {
                wait -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        for (int i = 0; i < buttons.Length; i++)
            buttons[i].gameObject.SetActive(false);

        action?.Invoke(choise);
    }

    private void BreakChoise()
    {
        choise = -1;
    }

    public void SetBlock(bool isBlock)
    {
        this.isBlock = isBlock;
        ButtonsBlockUpdate();
    }

    private void ButtonsBlockUpdate()
    {
        for (int i = 0; i < buttons.Length; i++)
            if(buttons[i] != null)
                buttons[i].interactable = !isBlock && canInteractButton[i];
    }

    private void SetCanInteract(int index, bool canInteract)
    {
        if (index < 0 || index >= canInteractButton.Length)
            return;
        canInteractButton[index] = canInteract;
        buttons[index].interactable = canInteract;
    }

    void IWaiterEventListener.WaiterBlock(bool isBlock)
    {
        SetBlock(isBlock);
    }
}