using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarController : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator AvatarAnimator;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //
    public void ShowExcited()
    {
        AvatarAnimator.ResetTrigger("excited_trigger");
        AvatarAnimator.SetTrigger("excited_trigger");
    }

    public void ShowLose()
    {
        AvatarAnimator.ResetTrigger("lose_trigger");
        AvatarAnimator.SetTrigger("lose_trigger");
    }

    public void Dance1()
    {
        AvatarAnimator.SetTrigger("dance1_trigger");
    }

    public void Dance2()
    {
        AvatarAnimator.SetTrigger("dance2_trigger");
    }

    public void Dance3()
    {
        AvatarAnimator.SetTrigger("dance3_trigger");
    }
}
