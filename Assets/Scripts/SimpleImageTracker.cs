using System.Collections.Generic;
using Unity.XR.PXR;
using Unity.XR.PXR.SecureMR;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class SimpleImageTracker : MonoBehaviour
{
    /*
    [Tooltip("特征库文件名（放在StreamingAssets下）")]
    public string trackingDataFileName = "ImageLibrary.ptrack";

    [Tooltip("每个追踪图像显示的预制体")]
    public GameObject trackedObjectPrefab;

    private Unity.XR.PXR.SecureMR.PxrImageTrackerManager  imageTracker;
    private Dictionary<int, GameObject> trackedObjects = new Dictionary<int, GameObject>();

    void Start()
    {
        // 获取图像追踪器实例
        imageTracker = PxrImageTracker.Instance;
        if (imageTracker == null)
        {
            Debug.LogError("PxrImageTracker 未初始化！请确保已添加 PXR_Manager 并启用 MR 功能。");
            return;
        }

        // 设置特征库路径（StreamingAssets）
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, trackingDataFileName);
        bool success = imageTracker.SetDataBasePath(path);
        if (!success)
        {
            Debug.LogError("设置特征库路径失败: " + path);
            return;
        }

        // 开启图像追踪
        imageTracker.StartTrack();
        Debug.Log("图像追踪已启动");
    }

    void Update()
    {
        if (imageTracker == null || !imageTracker.IsTrackRunning)
            return;

        // 获取追踪的图像数量（特征库中的图像索引范围）
        int imageCount = imageTracker.GetDataBaseImageCount();
        for (int i = 0; i < imageCount; i++)
        {
            var state = imageTracker.GetImageTrackingState(i);
            if (state == null) continue;

            bool tracked = (state.trackingState == TrackingState.Tracked);
            if (tracked)
            {
                if (!trackedObjects.ContainsKey(i))
                {
                    // 首次识别，创建对象
                    GameObject obj = Instantiate(trackedObjectPrefab);
                    trackedObjects[i] = obj;
                }
                // 更新位置和旋转
                trackedObjects[i].transform.position = state.position;
                trackedObjects[i].transform.rotation = state.rotation;
                trackedObjects[i].SetActive(true);
            }
            else
            {
                // 未追踪到，隐藏对象
                if (trackedObjects.ContainsKey(i))
                {
                    trackedObjects[i].SetActive(false);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (imageTracker != null && imageTracker.IsTrackRunning)
        {
            imageTracker.StopTrack();
        }
    }*/
}