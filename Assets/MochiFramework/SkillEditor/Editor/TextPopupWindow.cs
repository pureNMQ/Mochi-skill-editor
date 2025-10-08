using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TextPopupWindow : EditorWindow
{
    private string _originalText;
    private string _newText;
    private string _tip;
    
    private System.Action<string> _onRenameConfirmed;

    /// <summary>
    /// 打开重命名窗口
    /// </summary>
    /// <param name="currentText">当前轨道名称</param>
    /// <param name="onConfirm">确认重命名后的回调</param>
    public static void Open( System.Action<string> onConfirm,string title, string currentText = null, string tip = null)
    {
        // 创建或获取窗口实例
        TextPopupWindow window = GetWindow<TextPopupWindow>(true, title, true);
        window._originalText = currentText;
        window._newText = currentText;
        window._tip = tip;
        window._onRenameConfirmed = onConfirm;
        // 设置窗口大小
        window.minSize = new Vector2(300, 100);
        window.maxSize = new Vector2(300, 100);
    }
    

    private void OnGUI()
    {
        // 垂直布局
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
    
        // 文本输入框，默认显示原始名称
        EditorGUILayout.LabelField(_tip, GUILayout.MinHeight(30));
        GUIStyle style = EditorStyles.textField;
        style.fontSize = 20;
        _newText = EditorGUILayout.TextField(_newText,style, GUILayout.MinHeight(30));
    
        // 按钮布局
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // 按钮右对齐
    
        // 确认按钮
        if (GUILayout.Button("确认", GUILayout.Width(80),GUILayout.Height(25)))
        {
            // 调用回调函数并关闭窗口
            _onRenameConfirmed?.Invoke(_newText);
            Close();
        }
    
        // 取消按钮
        if (GUILayout.Button("取消", GUILayout.Width(80),GUILayout.Height(25)))
        {
            // 直接关闭窗口
            Close();
        }
    
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}