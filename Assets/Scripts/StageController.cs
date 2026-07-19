using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class StageController : MonoBehaviour
{
    private XRGrabInteractable _grabInteractable;
    private XRGeneralGrabTransformer _grabTransformer;
    // Start is called before the first frame update
    private void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _grabTransformer = GetComponent<XRGeneralGrabTransformer>();
    }

    // Update is called once per frame
    public void SetGrabInteractableEnabled(bool enabled)
    {
        _grabInteractable.enabled = enabled;
        _grabTransformer.enabled = enabled;
    }
}
