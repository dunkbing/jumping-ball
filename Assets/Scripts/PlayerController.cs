using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float power = 5f;
    private Rigidbody2D _rb;
    private LineRenderer _lr;

    private Camera _cam;
    private Vector3 _startPoint;
    private Vector3 _endPoint;

    // on air time limit
    private float _timeLimit = 3f;
    private Slider _timerSlider;

    private bool _onAir = true;

    private void Start()
    {
        _cam = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _lr = GetComponent<LineRenderer>();
        _timerSlider = FindObjectOfType<Slider>();
        _timerSlider.maxValue = _timeLimit;
        _timerSlider.value = _timeLimit;
    }

    private void Update()
    {
        if (_onAir)
        {
            _timeLimit -= Time.deltaTime;
        }
        _timerSlider.value = _timeLimit;
        if (_timeLimit <= 0)
        {
            _lr.positionCount = 0;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            DragStart();
        }

        if (Input.GetMouseButton(0))
        {
            Dragging();
        }

        if (Input.GetMouseButtonUp(0))
        {
            DragRelease();
        }
    }

    private void DragRelease()
    {
        _endPoint = _cam.ScreenToWorldPoint(Input.mousePosition);
        _endPoint.z = 15;

        Vector2 velocity = (_startPoint - _endPoint) * power;
        ReduceVel(ref velocity, 15);

        _rb.velocity = velocity;
        _lr.positionCount = 0;
    }

    private void Dragging()
    {
        _endPoint = _cam.ScreenToWorldPoint(Input.mousePosition);
        _endPoint.z = 15;

        Vector2 velocity = (_startPoint - _endPoint) * power;
        ReduceVel(ref velocity, 15);

        Vector2[] trajectory = Plot(_rb, transform.position, velocity, 200);
        _lr.positionCount = trajectory.Length;

        Vector3[] positions = new Vector3[trajectory.Length];
        for (int i = 0; i < trajectory.Length; i++)
        {
            positions[i] = trajectory[i];
        }
        _lr.SetPositions(positions);
    }

    private void DragStart()
    {
        _startPoint = _cam.ScreenToWorldPoint(Input.mousePosition);
        _startPoint.z = 15;
    }

    private Vector2[] Plot(Rigidbody2D rb, Vector2 pos, Vector2 velocity, int steps)
    {
        Vector2[] result = new Vector2[steps];

        float timeStep = Time.fixedDeltaTime / Physics2D.velocityIterations;
        Vector2 gravityAccel = Physics2D.gravity * (rb.gravityScale * timeStep * timeStep);

        float drag = 1f - timeStep * rb.drag;
        Vector2 moveStep = velocity * timeStep;

        for (int i = 0; i < steps; i++)
        {
            moveStep += gravityAccel;
            moveStep *= drag;
            pos += moveStep;
            result[i] = pos;
        }

        return result;
    }

    /// <summary>
    /// reduce the velocity magnitude if it is too large
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="maxMag"></param>
    private void ReduceVel(ref Vector2 velocity, float maxMag)
    {
        if (velocity.magnitude > maxMag)
        {
            velocity.Normalize();
            velocity *= maxMag;
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            _timeLimit = 3f;
            _onAir = false;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            _onAir = true;
        }
    }
}