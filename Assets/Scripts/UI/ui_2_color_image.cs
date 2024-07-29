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
