using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using static UnityEngine.GraphicsBuffer;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody rb;
    
    [HideInInspector] public Animator animator;
    
    #region Joystick Input
    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private Vector2 joystickSize = new Vector2(300, 300);
    
    private Finger movementFinger;
    private Vector2 movementAmount;


    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleFingerUp;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleFingerUp;
        ETouch.Touch.onFingerMove -= HandleFingerMove;
    }
    private void HandleFingerDown(Finger touchedFinger)
    {
        if (movementFinger == null)
        {
            movementFinger = touchedFinger;
            movementAmount = Vector2.zero;
            joystick.gameObject.SetActive(true);
            joystick.GetComponent<RectTransform>().sizeDelta = joystickSize;
            joystick.GetComponent<RectTransform>().anchoredPosition = ClampStartPosition(touchedFinger.screenPosition);
        }
    }
    private void HandleFingerUp(Finger lostFinger)
    {
        if (lostFinger == movementFinger)
        {
            movementFinger = null;
            joystick.knob.anchoredPosition = Vector2.zero;
            joystick.gameObject.SetActive(false);
            movementAmount = Vector2.zero;
        }
    }
    private void HandleFingerMove(Finger movedFinger)
    {
        if (movedFinger == movementFinger && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 knobPosition;
            float maxMovement = joystickSize.x / 2f;
            ETouch.Touch currentTouch = movedFinger.currentTouch;

            if (Vector2.Distance(
                currentTouch.screenPosition,
                joystick.GetComponent<RectTransform>().anchoredPosition
            ) > maxMovement)
            {
                knobPosition = (
                                   currentTouch.screenPosition - joystick.GetComponent<RectTransform>().anchoredPosition
                               ).normalized
                               * maxMovement;
            }
            else
            {
                knobPosition = currentTouch.screenPosition - joystick.GetComponent<RectTransform>().anchoredPosition;
            }

            joystick.knob.anchoredPosition = knobPosition;
            movementAmount = knobPosition / maxMovement;
        }
    }
    private Vector2 ClampStartPosition(Vector2 startPosition)
    {
        if (startPosition.x < joystickSize.x / 2)
        {
            startPosition.x = joystickSize.x / 2;
        }

        if (startPosition.y < joystickSize.y / 2)
        {
            startPosition.y = joystickSize.y / 2;
        }
        else if (startPosition.y > Screen.height - joystickSize.y / 2)
        {
            startPosition.y = Screen.height - joystickSize.y / 2;
        }

        return startPosition;
    }
    

    #endregion

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 moveDirection = new Vector3(movementAmount.x, 0, movementAmount.y);

        rb.velocity = moveDirection * speed * Time.fixedDeltaTime;

        if (rb.velocity != Vector3.zero)
        {
            Vector3 relativePos = rb.velocity.normalized;
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = rotation;
        }

        if (rb.velocity == Vector3.zero)
        {
            animator.SetBool("isRunning", false);
        }
        else
        {
            animator.SetBool("isRunning", true);
        }
    }

    public void LookAt(Transform target)
    {
        Vector3 relativePos = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = rotation;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ResourceArea"))
        {
            if (rb.velocity == Vector3.zero)
            {
                animator.SetBool("isMining", true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ResourceArea"))
        {
            animator.SetBool("isMining", false);
        }
    }
}
