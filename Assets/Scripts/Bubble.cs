using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] private bool _isFixed;
    [SerializeField] private bool _isConnected;

    [SerializeField] private BubbleColor _bubbleColor;

    [SerializeField] private float lifeTime;
    [SerializeField] private float _neighborDetectionRange = 0.7f;

    private Animator _animator;

    public bool IsFixed { set => _isFixed = value; }
    public bool IsConnected { set => _isConnected = value; get { return _isConnected; } }
    public BubbleColor BubbleColor { set => _bubbleColor = value; get { return _bubbleColor; } }

    private void Awake()
    {
        _isFixed = true;
        _isConnected = true;
        _animator = this.gameObject.GetComponent<Animator>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Bubble>() != null)
        {
            if (collision.gameObject.GetComponent<Bubble>()._isFixed && !_isFixed)
            {
                HasCollided();
            }
        }
        else if (collision.gameObject.GetComponent<RoofTrigger>() != null)
        {
            HasCollided();
        }
    }

    private void Update()
    {
        var normalizedPos = Camera.main.WorldToViewportPoint(this.transform.position);
        if (normalizedPos.y < 0 || normalizedPos.x < 0 || normalizedPos.x > 1)
        {
            if (this.gameObject != null) GameLevelManager.instance.RecycleBubble(this.gameObject.transform);
        }
    }

    private void HasCollided()
    {
        var rb = GetComponent<Rigidbody2D>();
        Destroy(rb);
        _isFixed = true;
        GameLevelManager.instance.SetAsBubbleAreaChild(transform);
        GameManager.instance.ProcessTurn(transform);
    }

    public List<Transform> GetNeighbors()
    {
        List<Transform> neighbors = new List<Transform>();
        var hits = Physics2D.OverlapCircleAll(transform.position, _neighborDetectionRange);

        foreach(var hit in hits)
        {
            if (hit != null)
                if (hit.transform.gameObject.GetComponent<Bubble>() != null)
                    if (hit.transform.gameObject.GetComponent<Bubble>()._isFixed)
                        neighbors.Add(hit.transform);
        }

        return neighbors;
    }

    public IEnumerator SetStartTimer()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!this._isFixed)
        {
            GameManager.instance.ProcessTurn(transform);
            GameLevelManager.instance.RecycleBubble(this.gameObject.transform);
        }
    }

    public void OnDieAnimFinish()
    {
        Destroy(this.gameObject);
    }

    public void OnRecycle()
    {
        this.gameObject.GetComponent<CircleCollider2D>().enabled = false;
        _animator.Play("Disappear");
    }
}
