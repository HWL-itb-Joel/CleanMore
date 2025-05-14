using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMainMenu : MonoBehaviour
{
    [System.Serializable]
    public struct CameraTransform
    {
        public Transform position;
    }

    public CameraTransform[] cameraViews; // Deberían ser 4
    public float transitionSpeed = 2f;

    private int currentIndex = 0;
    private bool isTransitioning = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        if (cameraViews.Length > 0)
        {
            SetCameraTransform(cameraViews[currentIndex]);
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }

    }

    void Update()
    {
        if (isTransitioning)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f &&
                Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isTransitioning = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentIndex > 0)
            {
                GoToView(currentIndex - 1);
            }
            else
            {
                QuitGame();
            }
        }

    }

    public void GoToView(int index)
    {
        if (index < 0 || index >= cameraViews.Length)
        {
            Debug.LogWarning("Índice fuera de rango: " + index);
            return;
        }

        currentIndex = index;
        SetCameraTransform(cameraViews[currentIndex]);
    }


    private void SetCameraTransform(CameraTransform camTransform)
    {
        targetPosition = camTransform.position.position;
        targetRotation = camTransform.position.rotation;
        isTransitioning = true;
    }

    private void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

        // Esto es útil para que funcione también en el editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

    }
}