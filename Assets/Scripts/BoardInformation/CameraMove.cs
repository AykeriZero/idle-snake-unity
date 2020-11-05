using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {
    public RectTransform user_interface;

    void Awake() {
        EventBus.Subscribe<BoardGeneratedEvent>(_OnBoardGenerated);
    }

    void _OnBoardGenerated(BoardGeneratedEvent e) {
        float ui_ratio = 240 / user_interface.sizeDelta.x;

        float height = BoardData.GetHeight();
        float width = BoardData.GetWidth() / (1 - ui_ratio);

        float ui_width = width - BoardData.GetWidth();

        // adjust position
        transform.position = new Vector3((width) / 2f - 0.5f - ui_ratio * width, (height) / 2f - 0.5f, transform.position.z);

        // adjust height
        Camera.main.orthographicSize = Mathf.Max(height / 2f, (width + ui_width / 2f) / (2f * Camera.main.aspect));

    }
}