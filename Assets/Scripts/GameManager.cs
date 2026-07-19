using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.VisualScripting;
using Unity.XR.PXR;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.Playables;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ZXing;
using ZXing.QrCode;

public class GameManager : MonoBehaviour
{

    [System.Serializable]
    public struct QrCodeTarget 
    {
        public string QrCodeContent;
        public Transform Object;
    }

    [SerializeField] 
    private List<QrCodeTarget> qrCodeTargets = new List<QrCodeTarget>();
    [SerializeField]
    private Dictionary<string, Transform> qrCodeTragetDic = new Dictionary<string, Transform>();

    [SerializeField]
    private AvatarController _boyAvatarController;
    [SerializeField]
    private AvatarController _girlAvatarController;

    private bool _leftSlotSet;
    private bool _rightSlotSet;

    public PlayableDirector danceShow;
    
    private enum GameState
    {
        None,
        Appear,
        Prepare,
        PrePerform,
        Performing,
        Judgement
    }

    private GameState _gameState;// = GameState.None;

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
        //Debug.Log("CameraAPITest CreateCameraDeviceAsync result:" + result0);
        //Debug.Log("Create Camera Capture Session");
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
        //Debug.Log("CameraAPITest CreateCameraCaptureSessionAsync result:" + result0);
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
        //Debug.Log("CameraAPITest BeginCameraCapture result:" + result0);
        isBeginCameraCapture = (result0 == PxrResult.SUCCESS);
        _gameState = GameState.Appear;        
    }

    //结束指定相机的图像捕获
    public void EndCameraCapture()
    {
        var result0 = PXR_CameraImage.EndCameraCapture(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
        Debug.Log("CameraAPITest EndCameraCapture result:" + result0);
        isBeginCameraCapture = !(result0 == PxrResult.SUCCESS);
    }

    //data member
    bool isBeginCameraCapture = false;

    private void GameInitialize()
    {
        //将待处理的二维码存入字典，后面调用
        foreach (var qrCodeTarget in qrCodeTargets)
        {
            qrCodeTragetDic.Add(qrCodeTarget.QrCodeContent, qrCodeTarget.Object);
            qrCodeTarget.Object.gameObject.SetActive(false);
        }
        _gameState = GameState.None;
        _leftSlotSet = false;
        _rightSlotSet = false;
        danceShow.stopped += (PlayableDirector pd) =>
        {
            _gameState = GameState.Judgement;
            //_boyAvatarController.SetGrabInteractableEnabled(true);
            //_girlAvatarController.SetGrabInteractableEnabled(true);
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        GameInitialize();        

        //异步创建摄像机，创建成功之后开始捕捉
        CreateCameraDeviceAsync();
    }

    private void ProcessCameraImage()
    {
        if (isBeginCameraCapture)
        {
            //ProcessCameraImageAsync();
            PxrResult acquireResult = PXR_CameraImage.AcquireCameraImage(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, 0, out ulong imageId, out Int64 captureTime);
            if (acquireResult == PxrResult.SUCCESS)
            {
                XrCameraImageDataRawBuffer imageData;
                if (PXR_CameraImage.GetCameraImageData(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, imageId, out imageData) == PxrResult.SUCCESS)
                {
                    Color32[] camPixels = ConvertRGBA32Safe(imageData);
                    var result = barcodeReader.Decode(camPixels, 1280, 960);

                    if (result != null)
                    {
                        if (qrCodeTragetDic.TryGetValue(result.Text, out Transform obj))
                        {
                            var qrCodeCenter = GetQrCodeCenter(result.ResultPoints, 960);
                            Pose pose = ConvertScreenPointToWorldPoint(qrCodeCenter);
                            obj.SetPositionAndRotation(pose.position, pose.rotation);
                            obj.gameObject.SetActive(true);
                            qrCodeTragetDic.Remove(result.Text);
                        }
                    }
                }
            }
            PXR_CameraImage.ReleaseCameraImage(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO, imageId);
        }
        //如果已经通过二维码识别到目标物体，则不再继续处理相机图像
        if (isBeginCameraCapture && qrCodeTragetDic.Count == 0)
        {
            Debug.Log("All QR codes detected, stopping camera capture.");
            PXR_CameraImage.EndCameraCapture(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
            PXR_CameraImage.DestroyCameraCaptureSession(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
            //PXR_CameraImage.DestroyCameraDevice(XrCameraIdPICO.XR_CAMERA_ID_RGB_LEFT_PICO);
            isBeginCameraCapture = false;
            _gameState = GameState.Prepare;
        }
    }

    private float _processGapTime = 1f;
    private float _processGapAccumulate = 0.0f;

    // Update is called once per frame    
    void Update()
    {
        switch (_gameState)
        {
            case GameState.None:
                break;
            case GameState.Appear:
                _processGapAccumulate += Time.deltaTime;
                if (_processGapAccumulate >= _processGapTime)
                { 
                    _processGapAccumulate -= _processGapTime;
                    ProcessCameraImage();
                }
                break;
            case GameState.Prepare:
                //两个槽位都被设置后，进入预执行状态
                if (_leftSlotSet && _rightSlotSet)
                { 
                    _gameState = GameState.PrePerform;
                }
                break;
            case GameState.PrePerform:
                //如果有任意一个槽位被清空，则回到准备状态
                if (!_leftSlotSet || !_rightSlotSet)
                {
                    _gameState = GameState.Prepare;
                }
                break;
            case GameState.Performing:
                break;
            default:
                break;
        }

    }

    private void StartPerforming()
    {
        _gameState = GameState.Performing;
        _boyAvatarController.SetGrabInteractableEnabled(false);
        _girlAvatarController.SetGrabInteractableEnabled(false);
        danceShow.Play();
    }
    
    private void OnDestroy()
    {
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
        );
        
    }

    private Pose ConvertScreenPointToWorldPoint(Vector2Int screenPoint)
    {
        Vector2 viewPoint = new Vector2(
            (float)screenPoint.x / 1280,
            (float)screenPoint.y / 960
        );

        //手动纠正视口坐标，确保其在 [0, 1] 范围内，系数0.6
        viewPoint.x = (viewPoint.x - 0.5f) * 0.6f + 0.5f;
        viewPoint.y = (viewPoint.y - 0.5f) * 0.6f + 0.5f;


        return GetHitPoseFromViewportPoint(viewPoint);
    }
    public Pose GetHitPoseFromViewportPoint(Vector2 viewportPoint)
    {
        Camera cam = Camera.main; // 或通过 XR Origin 获取主相机
        if (cam == null)
        {
            Debug.LogError("COULD NOT FOUND MAIN CAMERA");
            return Pose.identity;
        }

        // 视口坐标转射线
        Ray ray = cam.ViewportPointToRay(viewportPoint, Camera.MonoOrStereoscopicEye.Mono);
        var pos = ray.GetPoint(0.5f);
        var dir = cam.transform.position - pos;
        dir.y = 0; // 保持在水平面上
        return new Pose(pos, Quaternion.LookRotation(dir));        
    }

    //手势识别相关
    public void ShowRightThumbUp()
    {
        if (_gameState == GameState.Prepare || _gameState == GameState.Judgement)
        {
            _girlAvatarController.ShowExcited();
        }
    }

    public void ShowLeftThumbUp()
    {
        if (_gameState == GameState.Prepare || _gameState == GameState.Judgement)
        {
            _boyAvatarController.ShowExcited();
        }
    }

    public void ShowRightThumbDown()
    {
        Debug.Log("Show Right Thumb Down");
        if (_gameState == GameState.Prepare || _gameState == GameState.Judgement)
        { 
            _girlAvatarController.ShowLose();
        }
    }

    public void ShowLeftThumbDown() 
    {
        Debug.Log("Show Left Thumb Down");
        if (_gameState == GameState.Prepare || _gameState == GameState.Judgement)
        {
            _boyAvatarController.ShowLose();
        }
    }

    public void ShowLeftOK() {
        if (_gameState == GameState.PrePerform)
        {
            StartPerforming();
        }
    }

    public void ShowRightOK() {
        if (_gameState == GameState.PrePerform)
        {
            StartPerforming();
        }
    }

    public void OnLeftSlotEntered()
    {
        _leftSlotSet = true;
    }
    public void OnLeftSlotExited()
    {
        _leftSlotSet = false;
    }

    public void OnRihtSlotEntered()
    {
        _rightSlotSet = true;
    }

    public void OnRihtSlotExited()
    {
        _rightSlotSet = false;
    }

}
