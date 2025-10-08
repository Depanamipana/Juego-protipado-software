using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameOnClick : MonoBehaviour
{
    public string firstSceneName;

    private void Start()
    {
        // Asegurar que el tiempo siempre comience normal (no en pausa)
        Time.timeScale = 1f;

        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => LoadSceneAsync(firstSceneName));
    }

    private void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }
}
