using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shooter : MonoBehaviour
{
    private GameObject _currentBubble;
    private GameObject _nextBubble;

    [SerializeField] private float _shootingForce = 20f;

    [SerializeField] private float _swapSpeed = 0.02f;
    private bool _isSwaping = false;
    private bool _ableToShoot;

    [SerializeField] private float _allowedShotAngle = 50f;
    private float _lookAngle;

    [SerializeField] private GameObject pointerPrefab;
    private GameObject _pointer;

    [SerializeField] private Transform _shotTransform;
    [SerializeField] private Transform nextBubbleTransform;

    [SerializeField] private GameObject _bubbleGlow;

    public bool AbleToShoot { get { return _ableToShoot; } set { _ableToShoot = value; } }

    public void ShooterStart()
    {
        _ableToShoot = true;
        _isSwaping = false;
        if (_currentBubble != null) Destroy(_currentBubble.gameObject);
        if (_nextBubble != null) Destroy(_nextBubble.gameObject);
        StartCoroutine(CreateNextBubbleWithDelay());
    }

    private void Update()
    {
        if (GameManager.instance.ShotsLeft > 0) ProcessInput();
        if (GameManager.instance.ShotsLeft > 1) SwapBubbles();
    }

    private void ProcessInput()
    {
        if (Input.GetMouseButtonDown(0) && _ableToShoot && !EventSystem.current.IsPointerOverGameObject() && _currentBubble != null)
        {
            _pointer = Instantiate(pointerPrefab, this.gameObject.transform);
            _pointer.GetComponent<ShotPointer>().Init(_shotTransform.position);
        }

        if (Input.GetMouseButton(0) && _pointer != null)
        {
            var lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            var lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            this._lookAngle = lookAngle;

            if (Mathf.Abs(lookAngle - 90) > _allowedShotAngle)
            {
                Destroy(_pointer);
            }

            _pointer.GetComponent<ShotPointer>().PointerUpdate(lookDirection);
        }

        if (Input.GetMouseButtonUp(0) && _pointer != null)
        {
            Shoot();
            Destroy(_pointer);
        }
    }

    public void OnBubbleCollided()
    {
        if (GameManager.instance.ShotsLeft == 0)
        {
            GameManager.instance.OnGameEnd();
        }

        _ableToShoot = true;
        _isSwaping = false;

        if (GameManager.instance.ShotsLeft > 0 && GameLevelManager.instance.BubblesLeft > 0)
        {
            _nextBubble.transform.position = _shotTransform.position;
            _currentBubble = _nextBubble;
            _bubbleGlow.SetActive(true);
            CheckCurrentBubbleColor();
            _nextBubble = null;
        }

        if (GameManager.instance.ShotsLeft > 1 && GameLevelManager.instance.BubblesLeft > 0)
        {
            CreateNextBubble();
        }
    }

    private void SwapBubbles()
    {
        if (_isSwaping && _nextBubble != null)
        {
            _nextBubble.transform.position = Vector2.Lerp(_nextBubble.transform.position, _shotTransform.position, _swapSpeed);

            if (Vector2.Distance(_nextBubble.transform.position, _shotTransform.position) <= 0.1f)
            {
                _nextBubble.transform.position = _shotTransform.position;
                _isSwaping = false;
            }
        }
    }

    public void Shoot()
    {
        _ableToShoot = false;
        _isSwaping = true;
        _bubbleGlow.SetActive(false);
        GameManager.instance.OnShoot();

        _currentBubble.transform.SetParent(null);
        _currentBubble.transform.rotation = Quaternion.Euler(0f, 0f, _lookAngle - 90f);
        _currentBubble.GetComponent<CircleCollider2D>().enabled = true;
        _currentBubble.GetComponent<Rigidbody2D>().AddForce(_currentBubble.transform.up * _shootingForce, ForceMode2D.Impulse);
        StartCoroutine(_currentBubble.GetComponent<Bubble>().SetStartTimer());
        _lookAngle = 90f;
        _currentBubble = null;
    }

    public void CreateNextBubble()
    {
        List<GameObject> bubblesInScene = GameLevelManager.instance.BubblesInScene;
        List<BubbleColor> colors = GameLevelManager.instance.ColorsInScene;
        if (bubblesInScene == null || colors == null) return;

        if (_nextBubble == null)
        {
            _nextBubble = InstantiateNewBubble(bubblesInScene);
            if (!colors.Contains(_nextBubble.GetComponent<Bubble>().BubbleColor) && colors.Count > 0)
            {
                Destroy(_nextBubble);
                CreateNextBubble();
            }
        }

        if (_currentBubble == null)
        {
            _currentBubble = _nextBubble;
            _currentBubble.transform.position = _shotTransform.position;
            _nextBubble = InstantiateNewBubble(bubblesInScene);
        }
    }

    private void CheckCurrentBubbleColor()
    {
        List<GameObject> bubblesInScene = GameLevelManager.instance.BubblesInScene;
        List<BubbleColor> colors = GameLevelManager.instance.ColorsInScene;

        if (!colors.Contains(_currentBubble.GetComponent<Bubble>().BubbleColor))
        {
            Destroy(_currentBubble);
            _currentBubble = InstantiateNewBubble(bubblesInScene);
            _currentBubble.transform.position = _shotTransform.position;
            CheckCurrentBubbleColor();
        }
    }

    private GameObject InstantiateNewBubble(List<GameObject> bubblesInScene)
    {
        GameObject newBubble = Instantiate(bubblesInScene[Random.Range(0, bubblesInScene.Count)], this.gameObject.transform);
        newBubble.transform.position = nextBubbleTransform.position;
        newBubble.GetComponent<Bubble>().IsFixed = false;
        newBubble.GetComponent<CircleCollider2D>().enabled = false;
        Rigidbody2D rb2d = newBubble.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
        rb2d.gravityScale = 0f;

        return newBubble;
    }

    private IEnumerator CreateNextBubbleWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        CreateNextBubble();
        _bubbleGlow.SetActive(true);
    }
}
