using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowLogTool : MonoBehaviour
{
    public TextMeshProUGUI logText;
    // Start is called before the first frame update
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 示例：根据类型区分处理
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Log)
        {
            // 执行错误上报或保存堆栈
            //Debug.Log($"捕获错误: {logString}\n堆栈: {stackTrace}");
            logText.text += logString + "\n";
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
