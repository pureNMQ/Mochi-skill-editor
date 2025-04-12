using UnityEngine;

public static class ColorHelper
{
    /// <summary>
    /// 将16进制字符串转换为UnityEngine.Color（扩展方法）
    /// 支持格式：#RGB、#RRGGBB、#RGBA、#RRGGBBAA（带或不带#前缀）
    /// </summary>
    public static Color HexToColor(string hex)
    {
        hex = hex?.Trim().Replace("#", "") ?? "";
        
        switch (hex.Length)
        {
            case 3: // RGB → RRGGBB
                return new Color(
                    ParseComponent(hex[0], hex[0]) / 255f,
                    ParseComponent(hex[1], hex[1]) / 255f,
                    ParseComponent(hex[2], hex[2]) / 255f
                );

            case 4: // RGBA → RRGGBBAA
                return new Color(
                    ParseComponent(hex[0], hex[0]) / 255f,
                    ParseComponent(hex[1], hex[1]) / 255f,
                    ParseComponent(hex[2], hex[2]) / 255f,
                    ParseComponent(hex[3], hex[3]) / 255f
                );

            case 6: // RRGGBB
                return new Color(
                    ParseComponent(hex[0], hex[1]) / 255f,
                    ParseComponent(hex[2], hex[3]) / 255f,
                    ParseComponent(hex[4], hex[5]) / 255f
                );

            case 8: // RRGGBBAA
                return new Color(
                    ParseComponent(hex[0], hex[1]) / 255f,
                    ParseComponent(hex[2], hex[3]) / 255f,
                    ParseComponent(hex[4], hex[5]) / 255f,
                    ParseComponent(hex[6], hex[7]) / 255f
                );

            default:
                Debug.LogError($"HexToColor failed: '{hex}'");
                return Color.magenta; //返回一个明显错误的颜色
        }

    }

    // 高性能解析单个字符或字符对
    private static byte ParseComponent(char c1, char c2)
    {
        return (byte)((HexValue(c1) << 4) | HexValue(c2));
    }

    // 将单个字符转换为16进制值（无字符串操作）
    private static byte HexValue(char c)
    {
        if (c >= '0' && c <= '9') return (byte)(c - '0');
        if (c >= 'a' && c <= 'f') return (byte)(c - 'a' + 10);
        if (c >= 'A' && c <= 'F') return (byte)(c - 'A' + 10);
        throw new System.FormatException($"Invalid hex character: {c}");
    }
}
