using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotPointer : MonoBehaviour
{
    private Vector2 _startPoint;
    private Vector2 _endPoint;

    public Vector2 StartPoint { get { return _startPoint; } }
    public Vector2 EndPoint { get { return _endPoint; } }

    private void Start()
    {
        this.gameObject.transform.position = _startPoint;
    }

    public void Init(Vector2 start)
    {
        start += new Vector2(0, 0.5f);
        this.gameObject.transform.position = start;
        _startPoint = start;
    }

    public void PointerUpdate(Vector3 lookDirection)
    {
        var lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        var hit = Physics2D.Raycast(_startPoint, lookDirection);
        var pointerLength = hit.distance;

        this.gameObject.transform.localScale = new Vector3(0.2f, pointerLength, 1);
        this.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
    }
}
