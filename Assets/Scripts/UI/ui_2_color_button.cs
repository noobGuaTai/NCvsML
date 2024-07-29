using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ui_2_color_button : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
{
    public bool is_hover = false;
    public bool isActive = true;
    public bool isActiveButton = false;// 是否是激活类型的按钮
    public float maxColorValue = 1f;
    public float maxFontSizeValue = 150f;
    public UnityEvent on_click;
    Tween tween;
    ui_2_color_image image;
    TextMeshProUGUI text;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!is_hover) return;
        isActive = !isActive;
        on_click.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        is_hover = true;
        if (!isActiveButton || !isActive)
        {
            tween.playMode = Tween.PlayMode.NORMAL;
            tween.Play();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        is_hover = false;
        if (!isActiveButton || !isActive)
        {
            tween.playMode = Tween.PlayMode.REVERSE;
            tween.Play();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        image = gameObject.GetComponent<ui_2_color_image>();
        tween = gameObject.AddComponent<Tween>();
        text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        float fontSize = text.fontSize;
        if (isActiveButton)
        {
            tween.AddTween((float a) =>
            {
                image.trans = a;
            }, 0, maxColorValue, 0.25f, Tween.TransitionType.QUART, Tween.EaseType.OUT);
        }
        else
        {
            tween.AddTween((float a) =>
            {
                text.fontSize = a;
            }, fontSize, maxFontSizeValue, 0.25f, Tween.TransitionType.QUART, Tween.EaseType.OUT);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
