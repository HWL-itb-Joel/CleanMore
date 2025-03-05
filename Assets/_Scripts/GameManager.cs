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
        int waitTime = Random.Range(2, 4);
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);

        yield return new WaitForSeconds(waitTime);

        if (sceneNumber == 0)
        {
            SceneManager.LoadScene(sceneNumber, LoadSceneMode.Single);
        }
        else if (sceneNumber == 2)
        {
            SceneManager.LoadScene(sceneNumber, LoadSceneMode.Single);
            steamLobby.HostLobby();
        }
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        yield return new WaitForSeconds(4);

        SceneManager.UnloadSceneAsync(1);
    }
}
