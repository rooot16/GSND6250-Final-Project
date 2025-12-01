using UnityEngine;
using UnityEngine.UI;

public class TemperatureUI : MonoBehaviour
{
    [Header("References")]
    public PlayerTemperature playerTemperature; // 拖入玩家物体
    public Slider tempSlider;                   // 拖入 UI 上的 Slider 组件
    public Image fillImage;                     // 拖入 Slider 内部的 "Fill" 图片

    [Header("Color Settings")]
    public Color coldColor = Color.green;       // 34-36 度 (绿色)
    public Color normalColor = Color.yellow;    // 36-37 度 (黄色)
    public Color dangerColor = Color.red;       // 37-38 度 (红色)

    [Header("Blink Settings")]
    public float blinkSpeed = 10f;              // 闪烁速度

    void Start()
    {
        if (tempSlider != null)
        {
            // 设置 Slider 的范围对应体温范围
            tempSlider.minValue = 34f;
            tempSlider.maxValue = 38f;
        }
    }

    void Update()
    {
        if (playerTemperature == null || tempSlider == null || fillImage == null) return;

        // 1. 获取当前体温
        float currentTemp = playerTemperature.GetCurrentTemperature();

        // 2. 更新 Slider 的进度
        tempSlider.value = currentTemp;

        // 3. 颜色和闪烁逻辑
        if (currentTemp < 36f)
        {
            // --- 安全/低温区 (绿色) ---
            fillImage.color = coldColor;
        }
        else if (currentTemp >= 36f && currentTemp < 37f)
        {
            // --- 警戒区 (黄色) ---
            fillImage.color = normalColor;
        }
        else
        {
            // --- 危险区 (红色 + 闪烁) ---

            // 使用 PingPong 函数在 0 和 1 之间往复，模拟透明度变化
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);

            // 保持红色，但是改变透明度 (Alpha)
            // 如果你想让它在红色和暗红色之间闪烁，也可以用 Color.Lerp
            Color blinkingColor = dangerColor;

            // 这里让它在 红色(100%不透明) 和 红色(20%不透明) 之间闪烁
            blinkingColor.a = Mathf.Lerp(0.2f, 1f, alpha);

            fillImage.color = blinkingColor;
        }
    }
}