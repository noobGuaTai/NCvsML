using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ui_2_color_button : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
{
    public bool is_hover = false;
    public UnityEvent on_click;
    Tween tween;
    ui_2_color_image image;

    public void OnPointerClick(PointerEventData eventData) {
        if (!is_hover) return;
        on_click.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        is_hover = true;
        tween.playMode = Tween.PlayMode.NORMAL;
        tween.Play();
    }

    public void OnPointerExit(PointerEventData eventData) {
        is_hover = false;
        tween.playMode = Tween.PlayMode.REVERSE;
        tween.Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        image = gameObject.GetComponent<ui_2_color_image>();
        tween = gameObject.AddComponent<Tween>();
        tween.AddTween((float a) => {
            image.trans = a;
        }, 0, 1f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
