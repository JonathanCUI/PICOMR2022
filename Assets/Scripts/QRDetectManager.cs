using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.VisualScripting;
using Unity.XR.PXR;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ZXing;
using ZXing.QrCode;

public class QRDetectManager : MonoBehaviour
{

    [System.Serializable]
    public struct QrCodeTarget 
    {
        public string QrCodeContent;
        public Transform Object;
    }

    [SerializeField] private List<QrCodeTarget> qrCodeTargets = new List<QrCodeTarget>();
    private Dictionary<string, Transform> qrCodeTragetDic = new Dictionary<string, Transform>();

    private BarcodeReader barcodeReader = new BarcodeReader()
    {
        AutoRotate = true,
        Options = new ZXing.Common.DecodingOptions
        {
            TryHarder = true,
            PossibleFormats = new[] { BarcodeFormat.QR_CODE }
        }
    };

    //获取设备上可用的相机 ID 列表
    public void GetAvailableCameras()
    {
        PxrResult ret = PXR_CameraImage.GetAvailableCameras(out XrCameraIdPICO[] cameraIds);
        if (ret == PxrResult.SUCCESS)
        {
            foreach (var cameraId in cameraIds)
            {
                Debug.Log("CameraAPITest GetAvailableCameras cameraId:" + cameraId);
            }
        }
    }

    //获取指定相机支持的属性类型列表。
    public void GetCameraPropertyTypesAvailable()
    {
        PxrResult ret = PXR_CameraImage.GetCameraPropertyTypesAvailable(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO,
            out XrCameraPropertyTypePICO[] types);
        if (ret == PxrResult.SUCCESS)
        {
            foreach (var type in types)
            {
                Debug.Log("CameraAPITest GetCameraPropertyTypesAvailable type:" + type);
            }
        }
    }

    //获取相机的朝向
    public void GetCameraFacingProperties()
    {
        var ret = PXR_CameraImage.GetCameraFacingProperties(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO,
            out XrCameraFacingPICO facing);
        Debug.Log("CameraAPITest GetCameraFacingProperties ret:" + ret + " facing:" + facing);
    }

    //获取相机的位置
    public void GetCameraPositionProperties()
    {
        var ret = PXR_CameraImage.GetCameraPositionProperties(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO,
            out XrCameraPositionPICO position);
        Debug.Log("CameraAPITest GetCameraPositionProperties ret:" + ret + " position:" + position);
    }

    //获取相机的类型
    public void GetCameraCameraTypeProperties()
    {
        var ret = PXR_CameraImage.GetCameraCameraTypeProperties(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO,
            out XrCameraTypePICO cameraTypePico);
        Debug.Log("CameraAPITest GetCameraCameraTypeProperties ret:" + ret + " cameraTypePico:" + cameraTypePico);
    }

    //获取指定相机支持的能力类型列表
    public void GetCameraCapabilityAvailable()
    {
        var ret = PXR_CameraImage.GetCameraCapabilityAvailable(XrCameraIdPICO.XR_CAMERA_ID_RGB_RIGHT_PICO,
            out XrCameraCapabilityTypePICO[] capabilities);
        if (ret == PxrResult.SUCCESS)
        {
            foreach (var capability in capabilities)
            {
                Debug.Log("CameraAPITest GetCameraCapabilityAvailable capability:" + capability);
                
            }
            
        }
    }

    //异步创建指定 ID 的相机设备
    public async void CreateCameraDeviceAsync()
    {
        var result0 = await PXR_CameraImage.CreateCameraDeviceAsync(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
        Debug.Log("CameraAPITest CreateCameraDeviceAsync result:" + result0);
        Debug.Log("Create Camera Capture Session");
        CreateCameraCaptureSessionAsync();
    }

    //异步创建相机捕获会话，配置捕获参数
    public async void CreateCameraCaptureSessionAsync()
    {
        var result0 = await PXR_CameraImage.CreateCameraCaptureSessionAsync(
            XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, 
            1280, 
            960, 
            XrCameraImageFpsPICO.XR_CAMERA_IMAGE_FPS_30_PICO, 
            XrCameraImageFormatPICO.XR_CAMERA_IMAGE_FORMAT_RGBA_8888_PICO, 
            XrCameraDataTransferTypePICO.XR_CAMERA_DATA_TRANSFER_TYPE_RAW_BUFFER_PICO, 
            XrCameraModelPICO.XR_CAMERA_MODEL_PINHOLE_PICO);
        Debug.Log("CameraAPITest CreateCameraCaptureSessionAsync result:" + result0);
        if (result0 == PxrResult.SUCCESS)
        {
            // 开始捕获图像
            BeginCameraCapture();
        }
    }
    //销毁指定 ID 的相机设备。
    public void DestroyCameraDevice()
    {
        PxrResult ret = PXR_CameraImage.DestroyCameraDevice(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
        Debug.Log("CameraAPITest DestroyCameraDevice result:" + ret);
    }
    //启动指定相机的图像捕获
    public void BeginCameraCapture()
    {
        var result0 = PXR_CameraImage.BeginCameraCapture(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
        Debug.Log("CameraAPITest BeginCameraCapture result:" + result0);
        isBeginCameraCapture = (result0 == PxrResult.SUCCESS);
        //if (isBeginCameraCapture)
        //{
            // 开始捕获图像
        //    Debug.Log("Start Process Camera Image");
        //    StartCoroutine(ProcessCameraImageAsync());
        //}
    }

    //结束指定相机的图像捕获
    public void EndCameraCapture()
    {
        var result0 = PXR_CameraImage.EndCameraCapture(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
        Debug.Log("CameraAPITest EndCameraCapture result:" + result0);
        isBeginCameraCapture = !(result0 == PxrResult.SUCCESS);
    }


    private IEnumerator ProcessCameraImageAsync()
    {
        // 获取相机图像
        ulong imageId;
        PxrResult acquireResult = PXR_CameraImage.AcquireCameraImage(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, 0, out imageId, out Int64 captureTime);
        Debug.Log("111111111111");
        Debug.Log("222222222222   " + acquireResult.ToString());
        if (acquireResult == PxrResult.SUCCESS && imageId > 0)
        {
            // 获取图像的原始数据
            XrCameraImageDataRawBuffer imageData;

            if (PXR_CameraImage.GetCameraImageData(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, imageId, out imageData) == PxrResult.SUCCESS)
            {
                // 将原始图像数据渲染到 Texture2D 
                //imageData.cover
                //imageData.ConvertTo
                Debug.Log($"Get Image Data Success: {acquireResult}");
            }

            // 释放图像资源
            PXR_CameraImage.ReleaseCameraImage(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, imageId);
        }
        else if (acquireResult != PxrResult.SUCCESS)
        {
            //UpdateStatus($"获取图像失败: {acquireResult}");
            Debug.Log($"Get Image Data Failure:" + acquireResult);
        }

        yield return null;
    }


    //data member
    XrCameraIdPICO cameraId;// = XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO;
    bool isBeginCameraCapture = false;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRendererInitialized = false;
        if (lineRenderer == null)
        {
            Debug.Log("NOT FOUND LINE RENDER");
        }
        Debug.Log("Detection Start");
        //获取设备上可用的相机 ID 列表
        //GetAvailableCameras();
        //GetCameraPropertyTypesAvailable();
        //GetCameraFacingProperties();
        //GetCameraPositionProperties();
        foreach (var qrCodeTarget in qrCodeTargets)
        {
            qrCodeTragetDic.Add(qrCodeTarget.QrCodeContent, qrCodeTarget.Object);
        }
        /*
        PxrResult ret = PXR_CameraImage.GetCameraCapabilityAvailable(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, out XrCameraCapabilityTypePICO[] capabilitys);
        if (ret == PxrResult.SUCCESS)
        {
            foreach (var capabilityTypePICO in capabilitys)
            {
                Debug.Log("CameraAPITest GetCameraCapability:" + capabilityTypePICO);
            }
        }
        ret = PXR_CameraImage.GetCameraImageResolutionCapability(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, out PxrExtent2Di[] resolutions);
        if (ret == PxrResult.SUCCESS)
        {
            foreach (var resolution in resolutions)
            {
                Debug.Log("CameraAPITest GetCameraImageResolution:" + resolution.width + " " + resolution.height);
            }
        }
        */
        //异步创建指定 ID 的相机设备
        Debug.Log("Create Camera Device");
        CreateCameraDeviceAsync();
        //PXR_CameraImage.CreateCameraDeviceAsync(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO).ContinueWith(task =>
        //{
        //    if (task.Result == PxrResult.SUCCESS)
        //    {
        //        Debug.Log("Camera device created successfully.");
        //        Debug.Log("Create Camera Capture Session");
        //        CreateCameraCaptureSessionAsync();
        //    }
        //    else
        //    {
        //        Debug.LogError($"Failed to create camera device: {task.Result}");
        //    }
        //});
        //
    }

    // Update is called once per frame
    void Update()
    {
        if (isBeginCameraCapture)
        {
            //ProcessCameraImageAsync();
            PxrResult acquireResult = PXR_CameraImage.AcquireCameraImage(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, 0, out ulong imageId, out Int64 captureTime);
            //Debug.Log("AcquireCameraImage Result: " + acquireResult.ToString() + " ImageId: " + imageId + " CaptureTime: " + captureTime);
            if (acquireResult == PxrResult.SUCCESS)
            {
                //Debug.Log("Get Image Data Success:" + acquireResult.ToString() + " ImageId: " + imageId + " CaptureTime: " + captureTime);

                XrCameraImageDataRawBuffer imageData;
                if (PXR_CameraImage.GetCameraImageData(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, imageId, out imageData) == PxrResult.SUCCESS)
                {
                    // 将原始图像数据渲染到 Texture2D 
                    //Debug.Log("bytes per pixel " + imageData.bytesPerPixel);
                    //Debug.Log("image width " + imageData.width);
                    //Debug.Log("image height " + imageData.height);
                    //Debug.Log("image stride " + imageData.stride);
                    Color32[] camPixels = ConvertRGBA32Safe(imageData);
                    //cam
                    //Debug.Log("image data type" + imageData.type);
                    
                    //Debug.Log("camPixels length: " + camPixels.Length);
                    var result = barcodeReader.Decode(camPixels, 1280, 960);

                    if (result != null)
                    {
                        //Debug.Log("QR Code Detected: " + result.Text);

                        if (qrCodeTragetDic.TryGetValue(result.Text, out Transform obj))
                        {
                            //Ray ray = passthroughCameraAccess. new Ray(); //= passthroughCameraAccess.GetPassthroughCameraRayFromPixel(centerPixel);
                            //if (environmentRaycastManager.Raycast(ray, out EnvironmentRaycastHit hitInfo))
                            //{
                            //var qrCodeCenter = GetQrCodeCenter(result.ResultPoints, webCamTexture.height);
                            //Pose pose = ConvertScreenPointToWorldPoint(qrCodeCenter);
                            //obj.SetPositionAndRotation(pose.position, pose.rotation);
                            //}
                            //Debug.Log("QR Code Detected: " + result.Text + " Mapped Object: " + obj.name);

                            var qrCodeCenter = GetQrCodeCenter(result.ResultPoints, 960);
                            //Debug.Log("QR Code Center: " + qrCodeCenter.ToString());
                            Pose pose = ConvertScreenPointToWorldPoint(qrCodeCenter);
                            obj.SetPositionAndRotation(pose.position, pose.rotation);

                        }
                        //else
                        //{
                        //    Debug.Log("No target object mapped for this QR code content.");
                        //}
                    }
                }


                PXR_CameraImage.ReleaseCameraImage(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, imageId);
            }
            //else
            //{
            //    Debug.Log("Get Image Data Failure: {acquireResult}");
            //}
        }
    }

    private void OnDestroy()
    {
        PXR_CameraImage.DestroyCameraCaptureSession(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
        PXR_CameraImage.DestroyCameraDevice(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
    }

    public static Color32[] ConvertRGBA32Safe(XrCameraImageDataRawBuffer raw)
    {
        int width = (int)raw.width;
        int height = (int)raw.height;
        int stride = (int)raw.stride;
        int totalSize = (int)raw.bufferSize;

        // 将非托管数据复制到托管数组
        byte[] allData = new byte[totalSize];
        Marshal.Copy(raw.buffer, allData, 0, totalSize);

        Color32[] colors = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            int rowOffset = y * stride;
            int dstOffset = y * width;
            for (int x = 0; x < width; x++)
            {
                int srcIndex = rowOffset + x * 4;
                colors[dstOffset + x] = new Color32(
                    allData[srcIndex],     // R
                    allData[srcIndex + 1], // G
                    allData[srcIndex + 2], // B
                    allData[srcIndex + 3]  // A
                );
            }
        }
        return colors;
    }

    private Vector2Int GetQrCodeCenter(ResultPoint[] resultPoints, int textureHeight)
    {
        if (resultPoints == null || resultPoints.Length == 0)
        {
            return Vector2Int.zero;
        }

        float sumX = 0;
        float sumY = 0;

        foreach (var point in resultPoints)
        {
            sumX += point.X;
            sumY += point.Y;
        }

        float x = sumX / resultPoints.Length;
        float y = sumY / resultPoints.Length;
        return new Vector2Int(
           Mathf.RoundToInt(x),
           Mathf.RoundToInt(textureHeight - y)
           //Mathf.RoundToInt(y)
        );
        
    }

    private Pose ConvertScreenPointToWorldPoint(Vector2Int screenPoint)
    {
        Vector2 viewPoint = new Vector2(
            (float)screenPoint.x / 1280,
            (float)screenPoint.y / 960
        );
        //XRRayInteractor
        //ConvertScreenPointToWorldPoint
        //Debug.Log("ViewPoint: " + viewPoint.ToString());

        //Ray ray = PassthroughCameraUtils.ViewportPointToRay(passthroughCameraAccess, viewPoint); //passthroughCameraAccess.GetPassthroughCameraRayFromPixel(screenPoint);
        //if (environmentRaycastManager.Raycast(ray, out EnvironmentRaycastHit hitInfo))
        //{
        //    Pose pose = new Pose(hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
        //    return pose;
        //}
        return GetHitPoseFromViewportPoint(viewPoint);
    }

    private LineRenderer lineRenderer;
    bool lineRendererInitialized = false;
    public Pose GetHitPoseFromViewportPoint(Vector2 viewportPoint)
    {
        Camera cam = Camera.main; // 或通过 XR Origin 获取主相机
        if (cam == null)
        {
            Debug.LogError("COULD NOT FOUND MAIN CAMERA");
            return Pose.identity;
        }

        // 视口坐标转射线
        Ray ray = cam.ViewportPointToRay(viewportPoint);
        //cam.view
        //PXR_SceneCaptureManager.Instance.Raycast(ray, out RaycastHit hitInfo, 30f, LayerMask.GetMask("SpatialMesh"));
        //PXR_Manager.Instance.Raycast(ray, out RaycastHit hitInfo, 30f, LayerMask.GetMask("SpatialMesh"));
        //Debug.Log("333333333333333");
        //if (!lineRendererInitialized)
        //{
        //    lineRenderer.SetPosition(0, ray.origin);
        //    lineRenderer.SetPosition(1, ray.origin + ray.direction * 3f);
        //    Debug.Log("Ray Origin: " + ray.origin + " Ray Direction: " + ray.direction);
        //    lineRendererInitialized = true;
        //}
        //Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red, 0.1f);

        /*
        if (Physics.Raycast(ray, out RaycastHit hit, 30f, LayerMask.GetMask("SpatialMesh")))
        {
            Vector3 position = hit.point;
            Debug.Log("HIT WORLD");
            // 用法线作为物体的前方，适用于大多数放置需求
            Quaternion rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
            return new Pose(position, rotation);            
        }
        */
        //if (ray != null)
        {
            return new Pose(ray.GetPoint(0.5f), Quaternion.identity);
        }
        

        //return Pose.identity; // 未命中任何真实环境表面
    }


}
