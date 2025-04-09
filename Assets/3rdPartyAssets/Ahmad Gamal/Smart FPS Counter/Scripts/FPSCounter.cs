using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Nano.Managers;
public class FPSCounter : MonoBehaviour
{
    [HideInInspector] public Transform advancedTransform;
    [HideInInspector] public TextType GraphicsType;
    [HideInInspector] public MemorySizeUnit memorySizeUnit;
    [HideInInspector] public Text framerateText;
    [HideInInspector] public Text avgFramerateText;
    [HideInInspector] public Text maxFramerateText;
    [HideInInspector] public Text minFrameRateText;
    [Space]

    [HideInInspector] public Text OSText;
    [HideInInspector] public Text CPUText;
    [HideInInspector] public Text GPUText;
    [HideInInspector] public Text VRAMText;
    [HideInInspector] public Text RAMText;
    [HideInInspector] public Text ScreenText;
    [Space]

    [HideInInspector] public TextMeshProUGUI framerateTextMPro;
    [HideInInspector] public TextMeshProUGUI avgFramerateTextMPro;
    [HideInInspector] public TextMeshProUGUI maxFramerateTextMPro;
    [HideInInspector] public TextMeshProUGUI minFrameRateTextMPro;
    [Space]

    [HideInInspector] public TextMeshProUGUI OSTextMPro;
    [HideInInspector] public TextMeshProUGUI CPUTextMPro;
    [HideInInspector] public TextMeshProUGUI GPUTextMPro;
    [HideInInspector] public TextMeshProUGUI VRAMTextMPro;
    [HideInInspector] public TextMeshProUGUI RAMTextMPro;
    [HideInInspector] public TextMeshProUGUI ScreenTextMPro;
    [Space]


    [Space]
    [HideInInspector] public float updateInterval = 10;
    [HideInInspector] public bool useDynamicColors;
    [HideInInspector] public Color highFPSColor = Color.green;
    [HideInInspector] public Color mediumFPSColor = Color.yellow;
    [HideInInspector] public Color lowFPSColor = Color.red;
    [HideInInspector] public Color staticInfoColor = Color.white;
    [HideInInspector] public bool useAdvanced;

    private float framerate;
    private int averageFramerate;
    private float maxFramerate;
    private float minFramerate;
    private string OS;
    private string CPU;
    private string GPU;
    private string VRAM;
    private string RAM;
    private string Screen;
    private Color framerateColor;
    private Color avgFramerateColor;
    private Color maxFramerateColor;
    private Color minFramerateColor;

    private double memorySizeMulipler;
    private string memorySizeName;
    const float FPSMeasurePeriod = 0.5f;
    private int FPSAccumulator = 0;
    private float FPSPeriod = 0;
    private float nextUpdate;

    float TargetFPS;

    private List<float> FPSHistory = new List<float>();

    private void Start()
    {
        TargetFPS = GameManager.Instance.GetTargetFPS;

        FPSHistory.Add(TargetFPS);

        if(memorySizeUnit == MemorySizeUnit.GB)
        {
            memorySizeMulipler = 0.001;
            memorySizeName = "GB";
        }

        if (memorySizeUnit == MemorySizeUnit.MG)
        {
            memorySizeMulipler = 1;
            memorySizeName = "MB";
        }

        if (memorySizeUnit == MemorySizeUnit.KB)
        {
            memorySizeMulipler = 1000;
            memorySizeName = "KB";
        }

        OS = $"OS: {SystemInfo.operatingSystem}";
        CPU = $"CPU: {SystemInfo.processorType} [{SystemInfo.processorCount}] Cores";
        GPU =  $"GPU: {SystemInfo.graphicsDeviceName}";
        VRAM = $"VRAM: {(int)(SystemInfo.graphicsMemorySize * memorySizeMulipler) + memorySizeName}";
        RAM = $"RAM {(int)(SystemInfo.systemMemorySize * memorySizeMulipler) + memorySizeName}";
        Screen = $"Screen: {UnityEngine.Screen.currentResolution} [DPI: {UnityEngine.Screen.dpi}]";

        FPSPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;

        if(advancedTransform)
            advancedTransform.gameObject.SetActive(useAdvanced);
    }

    private void Update()
    {
        FPSAccumulator++;
        if (Time.realtimeSinceStartup > FPSPeriod)
        {
            framerate = (int)(FPSAccumulator / FPSMeasurePeriod);
            FPSAccumulator = 0;
            FPSPeriod += FPSMeasurePeriod;
        }

        if(Time.time >= nextUpdate)
        {
            nextUpdate = Time.time + 1 / updateInterval;
            UpdateUI();
        }


        float average = Time.frameCount / Time.unscaledTime;
        averageFramerate = (int)average;

        if(framerate > 0)
        FPSHistory.Add(framerate);

        if (FPSHistory.ToArray().Length > 1000) FPSHistory.RemoveRange(1, 999);

        CalculateMinAndMaxFPS();
    }

    private void UpdateUI()
    {
        if(!useDynamicColors)
        {
            if (framerate >= TargetFPS) framerateColor = highFPSColor;
            if (framerate < TargetFPS && framerate > TargetFPS / 2) framerateColor = mediumFPSColor;
            if (framerate < TargetFPS / 2) framerateColor = lowFPSColor;

            if (averageFramerate >= TargetFPS) avgFramerateColor = highFPSColor;
            if (averageFramerate < TargetFPS && averageFramerate > TargetFPS / 2) avgFramerateColor = mediumFPSColor;
            if (averageFramerate < TargetFPS / 2) avgFramerateColor = lowFPSColor;

            if (maxFramerate >= TargetFPS) maxFramerateColor = highFPSColor;
            if (maxFramerate < TargetFPS && maxFramerate > TargetFPS / 2) maxFramerateColor = mediumFPSColor;
            if (maxFramerate < TargetFPS / 2) maxFramerateColor = lowFPSColor;

            if (minFramerate >= TargetFPS) minFramerateColor = highFPSColor;
            if (minFramerate < TargetFPS && minFramerate > TargetFPS / 2) minFramerateColor = mediumFPSColor;
            if (minFramerate < TargetFPS / 2) minFramerateColor = lowFPSColor;
        }

        if (useDynamicColors)
        {
            framerateColor = staticInfoColor;
            avgFramerateColor = staticInfoColor;
            maxFramerateColor = staticInfoColor;
            minFramerateColor = staticInfoColor;
        }


        if (GraphicsType == TextType.Text)
        {
            framerateText.text = $"FPS: {framerate}";
            avgFramerateText.text = $"Average FPS: {averageFramerate}";
            maxFramerateText.text = $"Max FPS: {maxFramerate}";
            minFrameRateText.text = $"Min FPS: {minFramerate}";

            framerateText.color = framerateColor;
            avgFramerateText.color = avgFramerateColor;
            maxFramerateText.color = maxFramerateColor;
            minFrameRateText.color = minFramerateColor;

            if(useAdvanced)
            {
                OSText.text = OS;
                CPUText.text = CPU;
                GPUText.text = GPU;
                VRAMText.text = VRAM;
                RAMText.text = RAM;
                ScreenText.text = Screen;
            }
        }

        if (GraphicsType == TextType.TextMeshPro)
        {
            framerateTextMPro.text = $"FPS: {framerate}";
            avgFramerateTextMPro.text = $"Average FPS: {averageFramerate}";
            maxFramerateTextMPro.text = $"Max FPS: {maxFramerate}";
            minFrameRateTextMPro.text = $"Min FPS: {minFramerate}";

            framerateTextMPro.color = framerateColor;
            avgFramerateTextMPro.color = avgFramerateColor;
            maxFramerateTextMPro.color = maxFramerateColor;
            minFrameRateTextMPro.color = minFramerateColor;

            if(useAdvanced)
            {
                //OSTextMPro.text = OS;
                //CPUTextMPro.text = CPU;
                //GPUTextMPro.text = GPU;
                //VRAMTextMPro.text = VRAM;
                //RAMTextMPro.text = RAM;
                //ScreenTextMPro.text = Screen;
            }
        }
    }

    private void CalculateMinAndMaxFPS()
    {
        float max = FPSHistory[0];
        float min = FPSHistory[0];

        int index = 0;

        for (int i = 1; i < FPSHistory.Count; i++)
        {
            if (FPSHistory[i] < min)
            {
                min = FPSHistory[i];

                index = i;
            }
        }

        for (int i = 1; i < FPSHistory.Count; i++)
        {
            if (FPSHistory[i] > max)
            {
                max = FPSHistory[i];

                index = i;
            }
        }

        maxFramerate = max;
        minFramerate = min;
    }
}

public enum TextType
{
    Text,
    TextMeshPro
}

public enum MemorySizeUnit
{
    GB,
    MG,
    KB
}
