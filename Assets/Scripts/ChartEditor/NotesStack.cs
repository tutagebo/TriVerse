using UnityEngine;
using UnityEngine.EventSystems;

public class NotesStack : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Transform note;          // 既存：ドラッグ対象（任意）
    [SerializeField] private Camera mainCam;          // クリック変換に使うカメラ（UIなら EventCamera）
    [SerializeField] private TimelineRenderer timeline; // ← 追加：座標→(lane,beat) 変換元

    // 既存ドラッグ用（必要最小限そのまま）
    private Plane movePlane;
    private bool dragging;

    void Awake()
    {
        if (!mainCam) mainCam = Camera.main;
        movePlane = new Plane(Vector3.forward, Vector3.zero);
    }

    // ====== クリックで (lane, beat) を取得 ======
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("NotesStack clicked");
        if (timeline == null) return;

        // スクリーン座標 → Timeline のローカル座標
        RectTransform rt = timeline.rectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out Vector2 localPos))
        {
            float canvasH = rt.rect.height;
            var (lane, beat) = timeline.LocalPosToLaneBeat(localPos, canvasH);
            Debug.Log($"Clicked lane={lane}, beat={beat:F3}");
            // ここでNotes追加やコールバック呼び出しなどを行う
            // e.g., chart.Add(new NotesItem { lane = lane, beat = (float)beat });
            //       timeline.UpdateNotes(...); timeline.Invalidate();
        }
    }

    // ====== 以下、既存のドラッグ最小実装（そのまま） ======
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!note) return;
        dragging = true;
        var worldPos = ScreenToWorld(eventData.position, note.position.z);
        note.position = worldPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || !note) return;
        var worldPos = ScreenToWorld(eventData.position, note.position.z);
        note.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData) => dragging = false;

    private Vector3 ScreenToWorld(Vector2 screen, float z)
    {
        var ray = mainCam.ScreenPointToRay(screen);
        if (movePlane.Raycast(ray, out float enter)) return ray.GetPoint(enter);

        var pos = mainCam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(mainCam.transform.position.z - z)));
        pos.z = z;
        return pos;
    }
}
