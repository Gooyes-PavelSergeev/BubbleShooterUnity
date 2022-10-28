using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelCreator : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    [SerializeField] private Transform _bubbleArea;
    [SerializeField] private List<GameObject> _bubblePrefabs;
    private GameObject _currentBubble;
    [SerializeField] private Shooter _shooter;
    

    private void Start()
    {
        _currentBubble = _bubblePrefabs[0];
        _shooter.gameObject.SetActive(false);
    }

    private void Update()
    {
        ProcessInputBubbleType();
        ProcessInputBubbleFormation();
        ProcessInputCameraPos();
    }

    private void ProcessInputBubbleType()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _currentBubble = _bubblePrefabs[0];
            Debug.Log(_currentBubble.GetComponent<Bubble>().BubbleColor.ToString());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _currentBubble = _bubblePrefabs[1];
            Debug.Log(_currentBubble.GetComponent<Bubble>().BubbleColor.ToString());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _currentBubble = _bubblePrefabs[2];
            Debug.Log(_currentBubble.GetComponent<Bubble>().BubbleColor.ToString());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _currentBubble = _bubblePrefabs[3];
            Debug.Log(_currentBubble.GetComponent<Bubble>().BubbleColor.ToString());
        }
    }

    private void ProcessInputBubbleFormation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var bubble = _currentBubble;
            var bubbleToCreate = Instantiate(bubble, _bubbleArea);
            bubbleToCreate.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = _grid.WorldToCell(bubbleToCreate.transform.position);
            bubbleToCreate.transform.position = _grid.GetCellCenterWorld(cellPosition);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var mouseCell = _grid.WorldToCell(mousePos);
            foreach (Transform bubbleTransform in _bubbleArea)
            {
                var bubbleCell = _grid.WorldToCell(bubbleTransform.position);
                if (mouseCell == bubbleCell)
                {
                    Destroy(bubbleTransform.gameObject);
                    break;
                }
            }
        }
    }

    private void ProcessInputCameraPos()
    {
        var camera = Camera.main;
        if (Input.GetKey(KeyCode.W))
        {
            camera.transform.position += new Vector3(0, 0.02f, 0);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            camera.transform.position += new Vector3(0, -0.02f, 0);
        }
    }
}
