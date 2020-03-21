using System;
using UnityEngine;

public abstract class AbstractTile : MonoBehaviour
{
    public const float ACCELERATION = 0.63f;
    public const float FALL_TIME = 5f;

    public ColorBank colors;
    protected Score score;
    protected SpriteRenderer spriteRenderer;

    private SpriteRenderer highlightRenderer;
    private GameBoard gb;

    private Vector2? targetPosition;
    private Vector2? centerOfRotation;
    private float fallSpeed = 0;
    private float rotationSpeed = 0;
    private float revolutionSoFar = 0;
    private bool is120MarkPassed;
    private bool is240MarkPassed;

    public virtual void Awake() {
        score = GameObject.Find("Score").GetComponent<Score>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = colors.RandomColor();
        highlightRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    public virtual void Start() {
        gb = GameObject.Find("GameBoard(Clone)").GetComponent<GameBoard>();
    }

    public virtual void Update()
    {
        if (centerOfRotation.HasValue)
        {
            revolutionSoFar += rotationSpeed * Time.deltaTime;
            transform.RotateAround(centerOfRotation.Value, Vector3.forward, rotationSpeed * Time.deltaTime);
            if (!is120MarkPassed && Math.Abs(revolutionSoFar) > 120)
            {
                transform.position = targetPosition.Value;
                transform.rotation = Quaternion.identity;
                transform.RotateAround(centerOfRotation.Value, Vector3.forward, 120 * Mathf.Sign(rotationSpeed));
                revolutionSoFar = 120 * Mathf.Sign(rotationSpeed);
                is120MarkPassed = true;
                rotationSpeed = 0;
            }
            if (!is240MarkPassed && Math.Abs(revolutionSoFar) > 240)
            {
                transform.position = targetPosition.Value;
                transform.rotation = Quaternion.identity;
                transform.RotateAround(centerOfRotation.Value, Vector3.forward, 240 * Mathf.Sign(rotationSpeed));
                revolutionSoFar = 240 * Mathf.Sign(rotationSpeed);
                is240MarkPassed = true;
                rotationSpeed = 0;
            }
            if (Math.Abs(revolutionSoFar) > 360)
            {
                transform.position = targetPosition.Value;
                transform.rotation = Quaternion.identity;
                StopRotating();
            }
            return;
        }
        if (!targetPosition.HasValue) {
            targetPosition = transform.position;
        }
        transform.position = Vector2.MoveTowards(transform.position, targetPosition.Value, fallSpeed * Time.deltaTime);
        fallSpeed += ACCELERATION;
        if (transform.position.Equals(targetPosition.Value)) {
            fallSpeed = 0;
        }
    }

    public virtual void LateUpdate() {}
    public virtual void FixedUpdate() {}

    public void FallTo(Vector2? coordinate)
    {
        targetPosition = coordinate;
        fallSpeed += ACCELERATION;
    }

    public void RotateCWAround(Vector2 center)
    {
        centerOfRotation = center;
        rotationSpeed = 240;
    }

    public void RotateCCWAround(Vector2 center)
    {
        centerOfRotation = center;
        rotationSpeed = -240;
    }

    public void StopRotating()
    {
        transform.rotation = Quaternion.identity;
        revolutionSoFar = 0;
        rotationSpeed = 0;
        centerOfRotation = null;
        is120MarkPassed = false;
        is240MarkPassed = false;
        gb.RotationStopped();
    }

    public bool IsFalling()
    {
        return fallSpeed != 0;
    }

    public bool IsRotating()
    {
        return rotationSpeed != 0;
    }

    public void Select()
    {
        highlightRenderer.enabled = true;
    }

    public void DeSelect()
    {
        highlightRenderer.enabled = false;
    }

    public abstract void Hexplode();
}