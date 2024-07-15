using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ui_setting_ui_all : MonoBehaviour
{

    static Vector2 size = new Vector2(1920, 1080);

    public void move_screen(Vector2 direction) {
        RectTransform rectTransform = GetComponent<RectTransform>();

        var p = rectTransform.anchoredPosition;
        p.x += direction.x * size.x;
        p.y += direction.y * size.y;
        rectTransform.anchoredPosition = p;
    }

    public void move_right() => move_screen(Vector2.right);
    public void move_left() => move_screen(Vector2.left);
    public void move_up() => move_screen(Vector2.up);
    public void move_down() => move_screen(Vector2.down);
}
