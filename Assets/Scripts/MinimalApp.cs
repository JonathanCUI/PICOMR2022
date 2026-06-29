using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Unity.XR.PXR;
using Unity.XR.PXR.SecureMR;

public class MinimalApp : MonoBehaviour
{
    public TextAsset helmetGltfAsset;
    public int vstWidth = 1024;
    public int vstHeight = 1024;

    private Provider provider;
    private Pipeline pipeline;
    private Tensor gltfTensor;
    private Tensor gltfPlaceholderTensor;
    private void Awake()
    {
        PXR_Manager.EnableVideoSeeThrough = true;
    }

    private void Start()
    {
        CreateProvider();
        CreateGlobals();
        CreatePipeline();
    }

    private void Update()
    {
        RunPipeline();
    }

    private void CreateProvider()
    {
        provider = new Provider(vstWidth, vstHeight);
    }

    private void CreateGlobals()
    {
        // Create GLTF tensor
        gltfTensor = provider.CreateTensor<Gltf>(helmetGltfAsset.bytes);

    }

    private void CreatePipeline()
    {
        pipeline = provider.CreatePipeline();

        // Create transform matrix tensor
        int[] transformDim = { 4, 4 };
        var transformShape = new TensorShape(transformDim);
        float[] transformData = {
                0.5f, 0.0f, 0.0f, 0.0f,
                0.0f, 0.5f, 0.0f, 0.25f,
                0.0f, 0.0f, 0.5f, -1.5f,
                0.0f, 0.0f, 0.0f, 1.0f
            };
        var poseTensor = pipeline.CreateTensor<float, Matrix>(1, transformShape, transformData);

        // Create GLTF tensor placeholder
        gltfPlaceholderTensor = pipeline.CreateTensorReference<Gltf>();

        // Create render GLTF operator
        var renderGltfOperator = pipeline.CreateOperator<SwitchGltfRenderStatusOperator>();
        renderGltfOperator.SetOperand("gltf", gltfPlaceholderTensor);
        renderGltfOperator.SetOperand("world pose", poseTensor);
    }

    private void RunPipeline()
    {
        Debug.Log("Running pipeline...");

        var tensorMapping = new TensorMapping();

        tensorMapping.Set(gltfPlaceholderTensor, gltfTensor);

        pipeline.Execute(tensorMapping);
    }

}
