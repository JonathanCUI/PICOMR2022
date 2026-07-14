using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;


public class SpatialMeshStarter : MonoBehaviour
{
    public PXR_SpatialMeshManager spatialMeshManager;
    // Start is called before the first frame update
    void Start()
    {
        //spatialMeshManager.
    }

    // Update is called once per frame
    void Update()
    {
        if (spatialMeshManager.meshPrefab != null)
        {
            spatialMeshManager.meshPrefab.layer = LayerMask.NameToLayer("SpatialMesh");
            //Debug.Log("SpatialMesh layer set to: " + LayerMask.LayerToName(spatialMeshManager.meshPrefab.layer));
        }
    }
}
