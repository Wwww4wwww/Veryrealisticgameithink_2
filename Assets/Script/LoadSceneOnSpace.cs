using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnSpace : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private bool loadNextScene = true;   // Если true — загружает следующую по порядку сцену
    [SerializeField] private string sceneName;             // Если loadNextScene = false, загружает сцену с этим именем

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (loadNextScene)
            {
                LoadNextScene();
            }
            else
            {
                LoadSceneByName(sceneName);
            }
        }
    }

    private void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("Нет следующей сцены в Build Settings! Это последняя сцена.");
        }
    }

    private void LoadSceneByName(string name)
    {
        if (Application.CanStreamedLevelBeLoaded(name))
        {
            SceneManager.LoadScene(name);
        }
        else
        {
            Debug.LogError($"Сцена с именем '{name}' не найдена в Build Settings!");
        }
    }
}