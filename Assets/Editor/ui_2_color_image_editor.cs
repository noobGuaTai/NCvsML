using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;


[CustomEditor(typeof(ui_2_color_image))]
public class ui_2_color_image_editor : UnityEditor.UI.ImageEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var tar = (ui_2_color_image)target;
        tar.hover_color = EditorGUILayout.ColorField("hover_color: ", tar.hover_color);
        tar.trans = EditorGUILayout.Slider("trans: ", tar.trans, 0, 1);
    }
}