using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimacionesMenu : MonoBehaviour
{
    public static AnimacionesMenu instance;

    [SerializeField] GameObject TopDrawer;
    [SerializeField] GameObject BottomDrawer;

    [SerializeField] GameObject JoinFolder;
    [SerializeField] GameObject DrawerLight;

    public Transform joinCloseFolder;
    public Transform joinOpenFolder;
    public Quaternion targetRotationFolder;

    public Transform EndPoint;
    public Transform StartPoint;

    private bool isTransitioning = false;
    private Vector3 topTargetPosition;
    private Vector3 bottomTargetPosition;
    public float transitionSpeed = 2f;

    public bool openFolder;

    void Start()
    {
        DrawerLight.SetActive(false);
        openFolder = false;
        instance = this;
        TopDrawer.transform.position = new Vector3(StartPoint.position.x, TopDrawer.transform.position.y, StartPoint.position.z);
        topTargetPosition = TopDrawer.transform.position;
        BottomDrawer.transform.position = new Vector3(StartPoint.position.x, BottomDrawer.transform.position.y, StartPoint.position.z);
        bottomTargetPosition = BottomDrawer.transform.position;
    }

    void Update()
    {
        if (isTransitioning)
        {
            TopDrawer.transform.position = Vector3.Lerp(TopDrawer.transform.position, topTargetPosition, Time.deltaTime * transitionSpeed);
            BottomDrawer.transform.position = Vector3.Lerp(BottomDrawer.transform.position, bottomTargetPosition, Time.deltaTime * transitionSpeed);

            JoinFolder.transform.rotation = Quaternion.Lerp(JoinFolder.transform.rotation, targetRotationFolder, Time.deltaTime * transitionSpeed);

            if (Vector3.Distance(TopDrawer.transform.position, topTargetPosition) < 0.01f && Vector3.Distance(BottomDrawer.transform.position, bottomTargetPosition) < 0.01f
                && Quaternion.Angle(JoinFolder.transform.rotation, targetRotationFolder) < 1f)
            {
                isTransitioning = false;
            }

        }
    }

    public void OpenFolder()
    {
        openFolder = true;
        targetRotationFolder = joinOpenFolder.rotation;
        isTransitioning = true;
    }

    public void CloseFolder()
    {
        openFolder = false;
        targetRotationFolder = joinCloseFolder.rotation;
        isTransitioning = true;
    }

    public void OpenTopDrawer()
    {
        SetCameraTransform(true, EndPoint);
    }

    public void CloseTopDrawer()
    {
        SetCameraTransform(true, StartPoint);
    }

    public void OpenBottomDrawer()
    {
        SetCameraTransform(false, EndPoint);
        DrawerLight.SetActive(true);
    }

    public void CloseBottomDrawer()
    {
        SetCameraTransform(false, StartPoint);
        DrawerLight.SetActive(false);
    }

    private void SetCameraTransform(bool topDrawer, Transform camTransform)
    {
        if (topDrawer)
        {
            topTargetPosition = new Vector3(camTransform.position.x, TopDrawer.transform.position.y, camTransform.position.z);
        }
        else if (!topDrawer)
        {
            bottomTargetPosition = new Vector3(camTransform.position.x, BottomDrawer.transform.position.y, camTransform.position.z);
        }
        isTransitioning = true;
    }
}
