using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.PXR;
using UnityEditor;
using UnityEngine;

public class SpatialAnchorTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //PlayerPrefs.DeleteAll();
    }

    // Update is called once per frame
    
    bool reloadFromAnchor = false;
    async void Update()
    {
        if (!PlayerPrefs.HasKey("SpatialAnchorTest"))
        {
            return;
        }

        if (!reloadFromAnchor && GameManager.Instance.SpatialAnchorIsStarted)
        {
            //加载已经存储的空间锚点
            string uuidString = PlayerPrefs.GetString("SpatialAnchorTest", "");
            if (!Guid.TryParse(uuidString, out Guid anchorUuid))
            {
                Debug.LogError("UUID 格式错误");
            }

            var result2 = await PXR_MixedReality.QuerySpatialAnchorAsync(new Guid[] { anchorUuid });
            if (result2.result == PxrResult.SUCCESS)
            {
                Debug.Log("QuerySpatialAnchorAsync success");
                //获得第一个元素
                //如果有历史的空间锚点，将物体定位到历史空间锚点的位置
                PxrResult res = PXR_MixedReality.LocateAnchor(result2.anchorHandleList[0], out var position, out var rotation);
                if (res == PxrResult.SUCCESS)
                {
                    Debug.Log("LocateAnchor Success:");
                    transform.SetPositionAndRotation(position, rotation);
                }
                else
                {
                    Debug.Log("LocateAnchor Fail: " + res);
                    //Destroy(gameObject);
                }
                reloadFromAnchor = true;
            }
            else
            {
                Debug.LogError("QuerySpatialAnchorAsync failed: " + result2);
            }
        }
    }

    //离开选择时，创建空间锚点并保存
    public async void OnDisSelect()
    {
        Debug.Log("Dis Select Spatial Anchor");
        //确保空间锚点已经开始
        if (GameManager.Instance.SpatialAnchorIsStarted)
        {
            ////如果有历史的空间锚点，将物体定位到历史空间锚点的位置
            //if (PlayerPrefs.HasKey("SpatialAnchorTest"))
            //{
            //    var anchorHandle = ulong.Parse(PlayerPrefs.GetString("SpatialAnchorTest"));
            //    PxrResult res = PXR_MixedReality.LocateAnchor(anchorHandle, out var position, out var rotation);
            //    if (res == PxrResult.SUCCESS)
            //    {
            //        Debug.Log("LocateAnchor Success:");
            //        transform.SetPositionAndRotation(position, rotation);
            //    }
            //    else
            //    {
            //        Debug.Log("LocateAnchor Fail: ");
            //        //Destroy(gameObject);
            //    }
            //}

            //如果没有创建空间锚点，则将当前值创建并保存
            var result = await PXR_MixedReality.CreateSpatialAnchorAsync(transform.position, transform.rotation);
            if (result.result == PxrResult.SUCCESS)
            {
                Debug.Log("CreateSpatialAnchor Success anchor handle: " + result.anchorHandle);
                Debug.Log("CreateSpatialAnchor Success uuid: " + result.uuid);
                PlayerPrefs.SetString("SpatialAnchorTest", result.uuid.ToString());
                reloadFromAnchor = true; //视同为已经加载过历史锚点，避免重复加载

                //将锚点持久化
                _ = PXR_MixedReality.UnPersistSpatialAnchorAsync(result.anchorHandle);
                var r = await PXR_MixedReality.PersistSpatialAnchorAsync(result.anchorHandle);
                //PXRSample_SpatialAnchorManager.Instance.SetLogInfo("PersistSpatialAnchorAsync:" + result.ToString());
                if (r == PxrResult.SUCCESS)
                {
                    // 如果成功，显示保存图标
                    Debug.Log("PersistSpatialAnchorAsync Success: " + result.anchorHandle);
                }
            }
            else
            {
                Debug.Log("LocateAnchor Fail: " + result);
            }

        }

    }

    private void OnDisable()
    {
        PXR_MixedReality.StopSenseDataProvider(PxrSenseDataProviderType.SpatialAnchor);
    }
}
