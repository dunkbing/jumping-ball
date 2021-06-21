using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, ISpawn
{
    public float power = 5f;
    private Rigidbody2D _rb;
    private LineRenderer _lr;
    private ParticleSystem _ps;

    private Camera _cam;
    private Vector3 _startPoint;
    private Vector3 _endPoint;

    // on air time limit
    private float _timeLimit = Constants.OnAirTimeLimit;
    private float _chargeTime;
    private Slider _timerSlider;

    private bool _onAir = true;

    private Vector3 _originScale;

    private void Awake()
    {
        _cam = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _lr = GetComponent<LineRenderer>();
        _timerSlider = FindObjectOfType<Slider>();
        _originScale = transform.localScale;
        _ps = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        _timerSlider.maxValue = _timeLimit;
        _timerSlider.value = _timeLimit;
        transform.localScale = _originScale;
    }

    private void Update()
    {
        if (GameStats.GameIsPaused)
        {
            return;
        }

        if (_onAir)
        {
            _timeLimit -= Time.deltaTime;
        }
        _timerSlider.value = _timeLimit;
        if (_timeLimit <= 0)
        {
            _lr.positionCount = 0;
            TimeManager.StopSlowMotion();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            DragStart();
        } else if (Input.GetMouseButton(0))
        {
            Dragging();
        } else if (Input.GetMouseButtonUp(0))
        {
            DragRelease();
        }
        else
        {
            _lr.positionCount = 0;
        }
    }

    private void DragRelease()
    {
        AudioManager.Instance.Play("shoot");
        PpvUtils.Instance.ExitSlowMo();

        TimeManager.StopSlowMotion();

        _endPoint = _cam.ScreenToWorldPoint(Input.mousePosition);
        _endPoint.z = 15;

        Vector2 velocity = (_startPoint - _endPoint) * power;

        transform.up = velocity.normalized;
        transform.localScale = _originScale;

        ReduceVel(ref velocity, 15);

        _rb.velocity = velocity;
        _lr.positionCount = 0;

        _timeLimit -= _chargeTime;
        _chargeTime = 0;
    }

    private void Dragging()
    {
        _chargeTime += Time.deltaTime * 5;
        // vignette mode
        PpvUtils.Instance.EnterSlowMo();

        // slow down
        TimeManager.DoSlowMotion();

        _endPoint = _cam.ScreenToWorldPoint(Input.mousePosition);
        _endPoint.z = 15;

        Vector2 velocity = (_startPoint - _endPoint) * power;

        // set rotation toward velocity and squeeze the ball by y
        transform.up = velocity.normalized;
        if (transform.localScale.y > 0.4f)
        {
            transform.localScale -= new Vector3(0, 0.01f);
        }

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
        HandleCollision(other, true);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        HandleCollision(other);
    }

    private void HandleCollision(Collision2D other, bool stay = false)
    {
        if (!stay && !GameStats.GameIsPaused) _ps.Play();
        if (other.collider.CompareTag("PlatformSurface"))
        {
            if (!stay && !GameStats.GameIsPaused) ScoreMenu.Instance.IncreaseScore(Constants.WhitePlatScore);

            Regen(Constants.WhitePlatRegenRate);
            _onAir = false;
        } else if (other.collider.CompareTag("GreenPlatformSurface"))
        {
            if (!stay && !GameStats.GameIsPaused) ScoreMenu.Instance.IncreaseScore(Constants.GreenPlatScore);
            Regen(Constants.GreenPlatRegenRate);
            _onAir = false;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("PlatformSurface"))
        {
            _onAir = true;
        }
    }

    public void Spawn()
    {
        ResetEnergy();
    }

    public void ResetEnergy()
    {
        _timeLimit = Constants.OnAirTimeLimit;
    }

    public void Regen(float amount)
    {
        if (_timeLimit > Constants.OnAirTimeLimit)
        {
            _timeLimit = Constants.OnAirTimeLimit;
            return;
        }

        _timeLimit += amount;
    }
}
