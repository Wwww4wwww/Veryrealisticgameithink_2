using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnSpaceUpdated : MonoBehaviour
{
    [Header("Настройки перехода")]
    [SerializeField] private bool loadNextScene = true;
    [SerializeField] private string sceneName;

    [Header("Поведение при переходе")]
    [SerializeField] private DisableMode disableMode = DisableMode.DisableComponent;

    private enum DisableMode
    {
        DisableComponent,   // Отключить только этот скрипт
        DestroyGameObject,  // Уничтожить весь объект, на котором висит скрипт
        DoNothing           // Ничего не делать (может вызвать повторный переход)
    }

    private bool isLoading = false;

    private void Update()
    {
        if (isLoading) return; // Блокируем повторные вызовы

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isLoading = true;
            PerformDisable();
            LoadTargetScene();
        }
    }

    private void PerformDisable()
    {
        switch (disableMode)
        {
            case DisableMode.DisableComponent:
                enabled = false; // Отключаем скрипт
                break;
            case DisableMode.DestroyGameObject:
                Destroy(gameObject); // Удаляем объект (и скрипт вместе с ним)
                break;
            case DisableMode.DoNothing:
            default:
                break;
        }
    }

    private void LoadTargetScene()
    {
        if (loadNextScene)
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = currentIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                Debug.LogWarning("Нет следующей сцены в Build Settings!");
                isLoading = false;
                enabled = true; // В случае ошибки включаем обратно
            }
        }
        else
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError($"Сцена '{sceneName}' не найдена!");
                isLoading = false;
                enabled = true;
            }
        }
    }
}