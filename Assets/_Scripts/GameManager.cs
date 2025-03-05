using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    private SteamLobby steamLobby;

    private void Awake()
    {
        if (GameManager.gameManager != null && GameManager.gameManager != this)
        {
            Destroy(gameObject);
        }
        else
        {
            GameManager.gameManager = this;
            DontDestroyOnLoad(gameObject);
        }
        steamLobby = FindAnyObjectByType<SteamLobby>();
    }

    public void LoadScene(int sceneNumber)
    {
        StartCoroutine(ChangeScene(sceneNumber));
    }

    private IEnumerator ChangeScene(int sceneNumber)
    {
        AsyncOperation loadAsync = null;
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);

        if (sceneNumber == 0)
        {
            loadAsync = SceneManager.LoadSceneAsync(sceneNumber, LoadSceneMode.Additive);
            loadAsync.priority = -1;
        }
        else if (sceneNumber == 2)
        {
            loadAsync = SceneManager.LoadSceneAsync(sceneNumber, LoadSceneMode.Additive);
            loadAsync.priority = -1;
            steamLobby.HostLobby();
        }

        yield return new WaitWhile(() => !loadAsync.isDone);

        SceneManager.UnloadSceneAsync(1);
    }

    public void OpenMatches()
    {
        SteamFriends.ActivateGameOverlay("Friends");
    }
}
