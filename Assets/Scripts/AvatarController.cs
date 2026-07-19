using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class AvatarController : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator _avatarAnimator;
    private XRGrabInteractable _grabInteractable;
    private XRGeneralGrabTransformer _grabTransformer;
    private void Awake()
    {
        _avatarAnimator = GetComponent<Animator>();
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _grabTransformer = GetComponent<XRGeneralGrabTransformer>();
    }

    public void SetGrabInteractableEnabled(bool enabled)
    {
        _grabInteractable.enabled = enabled;
        _grabTransformer.enabled = enabled;
    }



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
        _avatarAnimator.ResetTrigger("excited_trigger");
        _avatarAnimator.SetTrigger("excited_trigger");
    }

    public void ShowLose()
    {
        _avatarAnimator.ResetTrigger("lose_trigger");
        _avatarAnimator.SetTrigger("lose_trigger");
    }

    public void Dance1()
    {
        _avatarAnimator.SetTrigger("dance1_trigger");
    }

    public void Dance2()
    {
        _avatarAnimator.SetTrigger("dance2_trigger");
    }

    public void Dance3()
    {
        _avatarAnimator.SetTrigger("dance3_trigger");
    }
}
