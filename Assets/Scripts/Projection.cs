using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Projection : MonoBehaviour
{
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsFrameIterations = 100;
    [SerializeField] private Transform _bubblesParent;
    [SerializeField] private GameObject ghostPlayerPrefab;
    private Scene _simulationScene;
    private PhysicsScene2D _physicsScene;
    private readonly Dictionary<Transform, Transform> _spawnedObjects = new Dictionary<Transform, Transform>();
    private GameObject _ghostPlayer;
    private Rigidbody2D _ghostRb;

    private void Start()
    {
        CreatePhysicsScene();
    }
    private void CreatePhysicsScene()
    {
        var parameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        _simulationScene = SceneManager.CreateScene("Simulation", parameters);
        _physicsScene = _simulationScene.GetPhysicsScene2D();

        foreach (Transform obj in _bubblesParent)
        {
            var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
            if (!ghostObj.TryGetComponent<Rigidbody2D>(out var rb))
                ghostObj.AddComponent<Rigidbody2D>();

            if (!ghostObj.TryGetComponent<CircleCollider2D>(out var collider))
                ghostObj.AddComponent<CircleCollider2D>();

            if (!ghostObj.TryGetComponent<Bubble>(out var bubble))
                ghostObj.AddComponent<Bubble>();
            SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
            _spawnedObjects.Add(obj, ghostObj.transform);
        }
        if (ghostPlayerPrefab != null)
        {
            _ghostPlayer = Instantiate(ghostPlayerPrefab, transform.position, transform.rotation);

            SceneManager.MoveGameObjectToScene(_ghostPlayer, _simulationScene);

            _ghostRb = _ghostPlayer.GetComponent<Rigidbody2D>();
            if (_ghostRb == null)
                _ghostRb = _ghostPlayer.AddComponent<Rigidbody2D>();

            _ghostRb.gravityScale = GetComponent<Rigidbody2D>().gravityScale;
            _ghostRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            _ghostPlayer.SetActive(false);
        }
    }

    private void Update()
    {
        foreach (var item in _spawnedObjects)
        {
            var real = item.Key;
            var ghost = item.Value;

            if (real.gameObject.activeInHierarchy&& Time.timeScale == 0f)
            {
                if (!ghost.gameObject.activeSelf)
                    ghost.gameObject.SetActive(true);

                var ghostBubble = ghost.GetComponent<Bubble>();
                var realBubble = real.GetComponent<Bubble>();
                if (ghostBubble != null && realBubble != null)
                {
                    ghostBubble.SetBubbleForce(realBubble.GetBubbleForce());
                }

                ghost.position = real.position;
                ghost.rotation = real.rotation;
                ghost.localScale = real.localScale;
            }
            else
            {
                ghost.gameObject.SetActive(false);
            }
        }
    }

    public void SimulateTrajectory(Vector2 velocity)
    {
        if (_ghostPlayer == null || _ghostRb == null)
        {
            Debug.LogWarning("GhostPlayer was not initialized.");
            return;
        }

        _ghostPlayer.SetActive(true);
        _ghostPlayer.transform.position = transform.position;
        _ghostPlayer.transform.rotation = transform.rotation;
        _ghostPlayer.transform.localScale = transform.localScale;

        _ghostRb.linearVelocity = velocity;
        _ghostRb.angularVelocity = 0f;

        Vector3[] points = new Vector3[_maxPhysicsFrameIterations];
        for (int i = 0; i < _maxPhysicsFrameIterations; i++)
        {
            _physicsScene.Simulate(Time.fixedDeltaTime);
            points[i] = _ghostPlayer.transform.position;
        }

        _line.positionCount = _maxPhysicsFrameIterations;
        _line.SetPositions(points);

        _ghostPlayer.SetActive(false);
    }

    public void ClearLine()
    {
        _line.positionCount = 0;
    }
}
