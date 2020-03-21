using UnityEngine;

public class ColorBank : MonoBehaviour
{
    public Color[] Colors;

    public Color RandomColor()
    {
        return Colors[Random.Range(0, Colors.Length)];
    }

    private void Start() => GenerateColors();
    private void OnValidate() => GenerateColors();

    private void GenerateColors()
    {
        int count = Colors.Length;
        float hue = 0f;
        float delta = (1f / count);
        for(int i = 0; i < count; ++i)
        {
            Colors[i] = Color.HSVToRGB(hue, 1, 1);
            hue += delta;
        }
    }
}
