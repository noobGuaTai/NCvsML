using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ui_game_select_bar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Tween tween;
    public void Start() {
        tween = gameObject.AddComponent<Tween>();
        tween.AddTween((float a) => {
            transform.localScale = new Vector3(a, a, a);
        }, 1, 1.1f, 0.2f, Tween.TransitionType.QUAD, Tween.EaseType.OUT);
    }
    public void OnPointerEnter(PointerEventData eventData) {
        tween.playMode = Tween.PlayMode.NORMAL;
        tween.Play();
    }

    public void OnPointerExit(PointerEventData eventData) {
        tween.playMode = Tween.PlayMode.REVERSE;
        tween.Play();
    }
}
