using System;
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

    public CameraTransform[] cameraViews; // Deber?an ser 4
    public float transitionSpeed = 2f;

    public int currentIndex = 4;
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
        GoToView(0);
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
            if (currentIndex == 3 || currentIndex == 2)
            {
                GoToView(0);
            }
            else if (currentIndex == 1 && !AnimacionesMenu.instance.openFolder)
            {
                AnimacionesMenu.instance.CloseBottomDrawer();
                AnimacionesMenu.instance.CloseTopDrawer();
                GoToView(2);
            }
            else if (AnimacionesMenu.instance.openFolder)
            {
                AnimacionesMenu.instance.CloseFolder();
            }
            else
            {
                QuitGame();
            }
        }

    }

    public void GoToView(int index)
    {
        currentIndex = index;
        SetCameraTransform(cameraViews[index]);
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

        // Esto es ?til para que funcione tambi?n en el editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

    }
}