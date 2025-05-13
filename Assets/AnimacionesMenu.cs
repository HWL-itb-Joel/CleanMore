using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimacionesMenu : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void openBottomDrawer()
    {
        animator.SetBool("isOpenBottomDrawer", true);
    }
}
