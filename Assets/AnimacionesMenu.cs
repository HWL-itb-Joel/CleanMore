using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimacionesMenu : MonoBehaviour
{
    [SerializeField] Animator animator;

    public void openBottomDrawer()
    {
        animator.SetBool("isOpenBottomDrawer", true);
    }
}
