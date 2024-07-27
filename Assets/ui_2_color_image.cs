using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ui_2_color_image : Image
{
    [SerializeField]
    public Color _hover_color = Color.white;

    [SerializeField]
    float _trans = 0;

    public float trans
    {
        set
        {
            _trans = value;
            material.SetFloat("_trans", value);
        }
        get { return _trans; }
    }

    public Color hover_color
    {
        set
        {
            _hover_color = value;
            material.SetColor("_hover_color", value);
        }
        get { return _hover_color; }
    }

    protected override void Start()
    {
        base.Start();
        material = new Material(material);
        material.SetColor("_hover_color", hover_color);
    }
}

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