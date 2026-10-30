using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // Добавляем пространство имён для новой системы ввода

public class GameNameTypewriter : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text textComponent;

    [Header("Dialogue Settings")]
    [SerializeField] private string[] sentences;
    [SerializeField] private float delayBetweenChars = 0.05f;

    [Header("Behaviour")]
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private bool allowSkipTyping = true;

    private int currentSentenceIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool sentenceFullyDisplayed = false;
    public AudioSource audioSource;
    // Ссылка на мышь из новой системы ввода

    private void Awake()
    {
        // Получаем текущее устройство мыши
    }
    private void Start()
    {
        if (textComponent == null)
        {
            Debug.LogError("DialogueTypewriter: не назначен компонент TMP_Text!");
            enabled = false;
            return;
        }

        if (sentences.Length == 0)
        {
            Debug.LogWarning("DialogueTypewriter: массив предложений пуст.");
            return;
        }

        if (startAutomatically)
        {
            StartTypingCurrentSentence();
        }
    }

    private void Update()
    {

    }

    private void HandleClick()
    {
        if (sentences.Length == 0) return;

        if (isTyping && allowSkipTyping)
        {
            FinishTyping();
            return;
        }

        if (!isTyping && sentenceFullyDisplayed)
        {
            NextSentence();
        }
    }

    private void StartTypingCurrentSentence()
    {
        if (currentSentenceIndex >= sentences.Length)
        {
            Debug.Log("Диалог завершён.");
            textComponent.text = "";
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(sentences[currentSentenceIndex]));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        sentenceFullyDisplayed = false;

        textComponent.text = sentence;
        textComponent.maxVisibleCharacters = 0;

        for (int i = 0; i <= sentence.Length; i++)
        {
            textComponent.maxVisibleCharacters = i;
            audioSource.Play();
            yield return new WaitForSeconds(delayBetweenChars);
        }

        isTyping = false;
        sentenceFullyDisplayed = true;
        typingCoroutine = null;
    }

    private void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
        isTyping = false;
        sentenceFullyDisplayed = true;
    }

    private void NextSentence()
    {
        currentSentenceIndex++;

        if (currentSentenceIndex < sentences.Length)
        {
            StartTypingCurrentSentence();
        }
        else
        {
            Debug.Log("Все предложения показаны.");
            textComponent.text = "";
            sentenceFullyDisplayed = false;
        }
    }

    public void StartDialogueFromIndex(int index)
    {
        if (index >= 0 && index < sentences.Length)
        {
            currentSentenceIndex = index;
            FinishTyping();
            StartTypingCurrentSentence();
        }
    }

    public void SetSentences(string[] newSentences, bool startImmediately = true)
    {
        sentences = newSentences;
        currentSentenceIndex = 0;
        if (startImmediately && sentences.Length > 0)
        {
            StartTypingCurrentSentence();
        }
    }
}