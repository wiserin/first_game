using UnityEngine;
using TMPro; // важно!

public class BallController : MonoBehaviour
{
    [Header("Настройки удара")]
    public LineRenderer aimLine;
    public float maxForce = 15f;
    public float minSpeedToStop = 0.05f;
    public float outOfBoundsY = -0.5f;

    [Header("Состояние")]
    public Vector3 spawnPosition;
    public bool canShoot = false;
    public bool isInHole = false;
    public bool levelCompleted = false;

    [Header("Прогресс по уровню")]
    public int shotCount = 0;
    public TextMeshProUGUI statusText;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnPosition = transform.position;
    }

    void Update()
    {
        // Проверка: мяч остановился
        bool ballIsStopped = rb.linearVelocity.magnitude < minSpeedToStop;

        if (!levelCompleted)
        {
            // Если мяч остановился и в лунке — победа!
            if (isInHole && ballIsStopped)
            {
                levelCompleted = true;
                rb.isKinematic = true;
                aimLine.enabled = false;
                Debug.Log("🎉 Мяч в лунке! Уровень пройден.");
                statusText.text = "Победа!";
                return;
            }

            // Если мяч остановился, можно бить
            canShoot = ballIsStopped;
            aimLine.enabled = canShoot;

            if (canShoot)
            {
                if (!levelCompleted)
                {
                    UpdateStatusText();
                }

                HandleAim();
            }
        }

        // Если вылетел за пределы — ресет
        if (transform.position.y < outOfBoundsY)
        {
            ResetBall();
        }
    }

    void UpdateStatusText()
    {
        if (statusText == null) return;

        if (levelCompleted)
        {
            statusText.text = $"Победа! Ударов: {shotCount}";
        }
        else if (isInHole)
        {
            statusText.text = $"Мяч в лузе... ждем остановки\nУдаров: {shotCount}";
        }
        else
        {
            statusText.text = $"Ударов: {shotCount}";
        }
    }


    void HandleAim()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Camera.main.WorldToScreenPoint(transform.position).z;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        Vector3 direction = mouseWorld - transform.position;
        direction.y = 0;

        float forcePercent = Mathf.Clamp01(direction.magnitude);
        Vector3 forceVec = -direction.normalized * forcePercent * maxForce;

        // Нарисовать стрелку
        aimLine.positionCount = 2;
        aimLine.SetPosition(0, transform.position);
        aimLine.SetPosition(1, transform.position + forceVec * 0.5f);

        // Удар
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(forceVec);
        }
    }

    void Shoot(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
        canShoot = false;
        aimLine.enabled = false;
        shotCount++;
        UpdateStatusText();
    }

    void ResetBall()
    {
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = spawnPosition;
        levelCompleted = false;
        isInHole = false;
        UpdateStatusText();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hole"))
        {
            isInHole = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hole"))
        {
            isInHole = false;
        }
    }
}
