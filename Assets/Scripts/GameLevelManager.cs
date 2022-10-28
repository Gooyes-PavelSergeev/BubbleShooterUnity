using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLevelManager : MonoBehaviour
{
    [SerializeField] private Grid _grid;

    [SerializeField] private int _rowNum;
    [SerializeField] private int _minBubbleHeightIndex;

    [SerializeField] private Transform _bubblesArea;
    [SerializeField] private List<GameObject> _bubblePrefabs;
    [SerializeField] private Transform _roof;
    [SerializeField] private EdgeCollider2D _edgeCollider;
    [SerializeField] private PhysicsMaterial2D _edgeColliderMaterial;

    private float _fieldHorExtent;
    private float _fieldVertExtent;

    private Vector2 _edgeLeftBottom;
    private Vector2 _edgeRightBottom;

    private Camera _camera;
    private bool _cameraIsMoving;

    private List<Vector3Int> _cellsInt;
    private List<Transform> _bubblesTransform;

    private List<GameObject> _bubblesInScene;
    private List<BubbleColor> _colorsInScene;

    private Vector3 _newCameraPosition;
    [SerializeField] private float _cameraTransitionSpeed = 0.05f;

    [SerializeField] private List<GameObject> _levelPrefabs;


    public List<Vector3Int> CellsInt { get { return _cellsInt; } }
    public List<GameObject> BubblesInScene { get { return _bubblesInScene; } }
    public List<BubbleColor> ColorsInScene { get { return _colorsInScene; } }
    public List<Transform> BubblesTransform { get { return _bubblesTransform; } }
    public Transform BubbleArea { get { return _bubblesArea; } }
    public Vector3 HighestPos { get { return GetHighestBubblePos(); } }
    public int BubblesLeft { get { return _bubblesTransform.Count; } }

    #region Singleton
    public static GameLevelManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion

    public void Start()
    {
        _cellsInt = new List<Vector3Int>();
        _bubblesTransform = new List<Transform>();
        _bubblesInScene = new List<GameObject>();
        _colorsInScene = new List<BubbleColor>();
        _camera = Camera.main;
        _camera.transform.position = Vector3.zero;
        _newCameraPosition = _camera.transform.position;
        _cameraIsMoving = false;
        _minBubbleHeightIndex = 5;

        _fieldVertExtent = _camera.orthographicSize;
        _fieldHorExtent = _fieldVertExtent * Screen.width / Screen.height;

        _edgeLeftBottom = (Vector2)_camera.ScreenToWorldPoint(new Vector3(0, 0, _camera.nearClipPlane));
        _edgeRightBottom = (Vector2)_camera.ScreenToWorldPoint(new Vector3(_camera.pixelWidth, 0, _camera.nearClipPlane));
        _edgeCollider.sharedMaterial = _edgeColliderMaterial;

        _roof.localScale = new Vector3 (_fieldHorExtent * 2, 0.3f, 0);
        _roof.position = new Vector3 (0, _fieldVertExtent - 0.5f, 0);

        StartCoroutine(ChangePosWithDelay());

        GetStartData();
    }

    private void GetStartData()
    {
        if (DataKeeper.instance != null)
        {
            if (DataKeeper.instance.IsRandomLevel)
            {
                _rowNum = 5 + 5 * DataKeeper.instance.RandomLevelDifficulty;
                RandomLevelStart();
            }
            else
            {
                var levelNum = DataKeeper.instance.CampaignLevelNum;
                levelNum--;
                CampaignLevelStart(levelNum);
            }
        }
    }

    private void RandomLevelStart()
    {
        CreateRows(_rowNum);
        UpdateScene();
        GameManager.instance.GameStart();
    }

    private void CampaignLevelStart(int levelNum)
    {
        FillFieldFromPrefab(_levelPrefabs[levelNum]);
        if (levelNum == 0)
        {
            GameManager.instance.ShotsAvailable = 40;
        }
        else if (levelNum == 1)
        {
            GameManager.instance.ShotsAvailable = 40;
        }
        else if (levelNum == 2)
        {
            GameManager.instance.ShotsAvailable = 60;
        }
        UpdateScene();
        GameManager.instance.GameStart();
    }

    private void Update()
    {
        if (_camera != null && _newCameraPosition != null && _cameraIsMoving)
        {
            _camera.transform.position = Vector2.Lerp(_camera.transform.position, _newCameraPosition, _cameraTransitionSpeed);
            if (Vector2.Distance(_camera.transform.position, _newCameraPosition) <= 0.2f)
            {
                _cameraIsMoving = false;
            }
        }
    }

    private void FillFieldFromPrefab(GameObject levelPrefab)
    {
        foreach (Transform bubbleTransform in levelPrefab.transform)
        {
            var bubble = Instantiate(bubbleTransform, _bubblesArea);
            _bubblesTransform.Add(bubble.transform);
        }
    }

    private void CreateRows(int rowNum)
    {
        for (int i = _minBubbleHeightIndex; i < rowNum + _minBubbleHeightIndex; i++)
        {
            var firstCell = GetFirstCellInRowInt(i);
            int cellNum;
            if (_grid.CellToWorld(firstCell).x % 2 != 0) cellNum = Mathf.Abs(firstCell.x * 2);
            else cellNum = Mathf.Abs(firstCell.x * 2) + 1;

            for (int j = 0; j < cellNum; j++)
            {
                _cellsInt.Add(new Vector3Int(j + firstCell.x, firstCell.y, 0));
            }
        }

        foreach (var cell in _cellsInt)
        {
            var bubble = Instantiate(_bubblePrefabs[Random.Range(0, _bubblePrefabs.Count)], _bubblesArea);
            bubble.transform.position = _grid.CellToWorld(cell);
            _bubblesTransform.Add(bubble.transform);
        }
    }

    private Vector3Int GetFirstCellInRowInt(int currentRowNum)
    {
        var i = _minBubbleHeightIndex;
        while (true)
        {
            if (i >= currentRowNum) break;
            if (i > 50) break;
            i++;
        }

        var gridPosInt = new Vector3Int((int)(-_fieldHorExtent / _grid.cellSize.x), i, 0);
        return gridPosInt;
    }

    public void UpdateScene()
    {
        List<BubbleColor> colors = new List<BubbleColor>();
        List<GameObject> listOfBubbles = new List<GameObject>();

        foreach (Transform bubbleTransform in _bubblesTransform)
        {
            Bubble bubble = bubbleTransform.GetComponent<Bubble>();
            if (colors.Count < _bubblePrefabs.Count && !colors.Contains(bubble.BubbleColor))
            {
                var color = bubble.BubbleColor;
                colors.Add(color);

                foreach (GameObject bubblePrefab in _bubblePrefabs)
                {
                    if (color.Equals(bubblePrefab.GetComponent<Bubble>().BubbleColor))
                    {
                        listOfBubbles.Add(bubblePrefab);
                    }
                }
            }
        }
        _colorsInScene = colors;
        _bubblesInScene = listOfBubbles;

        AdjustCamera();
    }

    private void SnapToNearestGripPosition(Transform bubbleTransform)
    {
        Vector3Int cellPosition = _grid.WorldToCell(bubbleTransform.position);
        bubbleTransform.position = _grid.GetCellCenterWorld(cellPosition);
    }

    public void SetAsBubbleAreaChild(Transform bubbleTransform)
    {
        SnapToNearestGripPosition(bubbleTransform);
        bubbleTransform.SetParent(_bubblesArea);
        _bubblesTransform.Add(bubbleTransform);
    }

    public void RecycleBubble(Transform bubbleTransform)
    {
        if (_bubblesTransform.Contains(bubbleTransform))
        {
            RemoveBubbleTransform(bubbleTransform);
            bubbleTransform.gameObject.GetComponent<Bubble>().OnRecycle();
        }
        else if (bubbleTransform.gameObject != null)
        {
            bubbleTransform.gameObject.GetComponent<Bubble>().OnRecycle();
        }
    }

    private void AdjustCamera()
    {
        var highestPoint = GetHighestBubblePos();

        var highestCellIndex = _grid.WorldToCell(highestPoint).y;
        var lowestCellIndex = _grid.WorldToCell(GetLowestBubblePos()).y;
        var centerPosWorld = _camera.ScreenToWorldPoint(new Vector2(Screen.width / 2, Screen.height / 2));
        var cameraCenterCellIndex = _grid.WorldToCell(centerPosWorld).y;

        if (highestCellIndex < _grid.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y - 1)
        {
            var differenceIndex = _grid.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y - 1 - highestCellIndex;
            var newPos = _grid.CellToWorld(new Vector3Int(0, cameraCenterCellIndex - differenceIndex, 0));
            var differenceHeight = newPos.y - _camera.transform.position.y;
            var adjustVector = new Vector3(0, differenceHeight, 0);

            _newCameraPosition += adjustVector;
            _roof.position += adjustVector;
            _minBubbleHeightIndex -= differenceIndex;
            _cameraIsMoving = true;
        }

        else if (lowestCellIndex > _minBubbleHeightIndex)
        {
            var differenceIndex = lowestCellIndex - _minBubbleHeightIndex;

            if (highestCellIndex - differenceIndex <= _grid.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y + 1)
            {
                differenceIndex = highestCellIndex - _grid.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y + 1;
            }

            var newPos = _grid.CellToWorld(new Vector3Int(0, cameraCenterCellIndex + differenceIndex, 0));
            var differenceHeight = newPos.y - _camera.transform.position.y;
            var adjustVector = new Vector3(0, differenceHeight, 0);

            _newCameraPosition += adjustVector;
            _roof.position += adjustVector;
            _minBubbleHeightIndex += differenceIndex;
            _cameraIsMoving = true;
        }
        else if (lowestCellIndex < _minBubbleHeightIndex)
        {
            var differenceIndex = _minBubbleHeightIndex - lowestCellIndex;
            var newPos = _grid.CellToWorld(new Vector3Int(0, cameraCenterCellIndex - differenceIndex, 0));
            var differenceHeight = newPos.y - _camera.transform.position.y;
            var adjustVector = new Vector3(0, differenceHeight, 0);

            _newCameraPosition += adjustVector;
            _roof.position += adjustVector;
            _minBubbleHeightIndex -= differenceIndex;
            _cameraIsMoving = true;
        }

        var leftTop = (Vector2)_camera.ScreenToWorldPoint(new Vector3(0, _camera.pixelHeight, _camera.nearClipPlane)) + new Vector2(0, highestPoint.y - _fieldVertExtent + 0.5f);
        var rightTop = (Vector2)_camera.ScreenToWorldPoint(new Vector3(_camera.pixelWidth, _camera.pixelHeight, _camera.nearClipPlane)) + new Vector2(0, highestPoint.y - _fieldVertExtent + 0.5f);

        var edgePoints = new[] { _edgeLeftBottom, leftTop, rightTop, _edgeRightBottom };

        _edgeCollider.points = edgePoints;
    }

    private Vector3 GetHighestBubblePos()
    {
        var lastBubbleIndexVert = _grid.WorldToCell(_bubblesTransform[0].position);
        Vector3Int maxBubbleIndexVert;
        for (int i = 0; i < _bubblesTransform.Count; i++)
        {
            var bubbleTransform = _bubblesTransform[i];
            maxBubbleIndexVert = _grid.WorldToCell(bubbleTransform.position);
            if (maxBubbleIndexVert.y > lastBubbleIndexVert.y) lastBubbleIndexVert = maxBubbleIndexVert;
        }
        lastBubbleIndexVert += new Vector3Int(-10, 0, 0);

        return _grid.CellToWorld(lastBubbleIndexVert);
    }

    private Vector3 GetLowestBubblePos()
    {
        var lastBubbleIndexVert = _grid.WorldToCell(_bubblesTransform[0].position);
        Vector3Int minBubbleIndexVert;
        for (int i = 0; i < _bubblesTransform.Count; i++)
        {
            var bubbleTransform = _bubblesTransform[i];
            minBubbleIndexVert = _grid.WorldToCell(bubbleTransform.position);
            if (minBubbleIndexVert.y < lastBubbleIndexVert.y) lastBubbleIndexVert = minBubbleIndexVert;
        }
        lastBubbleIndexVert += new Vector3Int(-10, 0, 0);

        return _grid.CellToWorld(lastBubbleIndexVert);
    }

    public void RemoveBubbleTransform(Transform bubbleTransform)
    {
        if (_bubblesTransform.Contains(bubbleTransform))
        {
            _bubblesTransform.Remove(bubbleTransform);
        }
        if (_bubblesTransform.Count <= 0)
        {
            GameManager.instance.OnGameEnd();
        }
    }

    private IEnumerator ChangePosWithDelay()
    {
        yield return new WaitForSeconds(1f);
        _camera.transform.position = Vector3.zero;
    }
}

public enum BubbleColor
{
    Blue,
    Yellow,
    Red,
    Green
}
