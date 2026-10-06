using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBounds2D : MonoBehaviour
{
    public static CameraBounds2D I { get; private set; }
    private Camera cam;

    private void Awake()
    {
        I = this;
        cam = GetComponent<Camera>();
    }

    public Rect GetWorldRect(float extra = 0f)
    {
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        Vector2 center = (Vector2)cam.transform.position;
        return new Rect(center - new Vector2(width / 2f, height / 2f) - Vector2.one * extra,
                        new Vector2(width + extra * 2f, height + extra * 2f));
    }

    public Vector2 ClampInside(Vector2 pos, float margin = 0f)
    {
        var r = GetWorldRect(margin);
        pos.x = Mathf.Clamp(pos.x, r.xMin, r.xMax);
        pos.y = Mathf.Clamp(pos.y, r.yMin, r.yMax);
        return pos;
    }
}
