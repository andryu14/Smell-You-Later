using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomerScript : MonoBehaviour {
    enum CustomerState {
        Shopping,
        ShoppingStopped,
        Alerted,
        Investigating,
        Chasing,
        Stunned,
        Smelling
    }

    [SerializeField] private Transform[] pathPoints;
    [SerializeField] private Transform[] stoppingPoints;
    public Rigidbody2D rb;
    public Animation animator;

    private CustomerState currentState;
    private GameObject player;
    private int currentPathIndex;
    private bool hasLineOfSight = false;
    private Transform lastStoppingPoint;

    [Header("Speeds")]
    [SerializeField] private float shoppingSpeed = 5f;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animation>();
        player = GameObject.FindGameObjectWithTag("Player");
        currentState = CustomerState.Shopping;
        StartCoroutine(FSM());
    }

    void FixedUpdate() {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, player.transform.position - transform.position);
        if (ray) {
            hasLineOfSight = ray.collider.CompareTag("Player");

            if (hasLineOfSight) {
                Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.green);
            } else {
                Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.red);
            }
        }
    }

    IEnumerator FSM() {
        while (true) {
            switch (currentState) {
                case CustomerState.Shopping:
                    Shopping();
                    break;
                case CustomerState.ShoppingStopped:
                    yield return ShoppingStopped();
                    break;
                case CustomerState.Alerted:
                    animator.Play();
                    yield return new WaitForSeconds(0.5f);
                    currentState = CustomerState.Investigating;
                    break;
                case CustomerState.Investigating:
                    Investigating();
                    break;
                case CustomerState.Chasing:
                    Chasing();
                    break;
                case CustomerState.Stunned:
                    Stunned();
                    break;
                case CustomerState.Smelling:
                    Smelling();
                    break;
            }
            yield return null;
        }
    }

    void Shopping() {
        if (pathPoints.Length == 0) return; // If path has no points for whatever reason

        Transform targetPoint = pathPoints[currentPathIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, shoppingSpeed * Time.deltaTime);

        Vector2 direction = (targetPoint.position - transform.position).normalized;

        if (Vector2.Distance(transform.position, player.transform.position) < 3) { // If customer detects player's smell, trigger Alerted state
            currentState = CustomerState.Alerted;
            return;
        }


        if (direction != Vector2.zero) { // Rotate sprite along path
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        }

        /* foreach (Transform point in stoppingPoints) {
            if (point != null) {
                if (Vector2.Distance(transform.position, point.position) < 0.1f && lastStoppingPoint != point) { // If customer reaches stopping point
                lastStoppingPoint = point;
                    if (UnityEngine.Random.value <= 1f) {
                        Debug.Log("WOO");
                        currentState = CustomerState.ShoppingStopped;
                    }
                }
            }
        } */

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.01f) { // Target the next point in path
            currentPathIndex = (currentPathIndex + 1) % pathPoints.Length;
        }
    }

    IEnumerator ShoppingStopped() {
        Debug.Log("Function entered");
        transform.Rotate(0, 0, -90f);
        bool lookingLeft = true;
        bool searchFinished = false;
        float degreesTurned = 0;

        while (!searchFinished) {
            while (lookingLeft) {
                if (degreesTurned < 45) {
                    transform.Rotate(0, 0, 0.1f);
                    degreesTurned += 0.1f;
                    yield return null;
                } else {
                    lookingLeft = false;
                    degreesTurned = 0;
                }
                
            }
            while (!lookingLeft) {
                if (degreesTurned < 90) {
                    transform.Rotate(0, 0, -0.1f);
                    degreesTurned += 0.1f;
                    yield return null;
                } else {
                    degreesTurned = 0;
                    searchFinished = true;
                    break;
                }
            }
        }

        while (degreesTurned < 45) {
            transform.Rotate(0, 0, 0.1f);
            degreesTurned += 0.1f;
            yield return null;
        }
        currentState = CustomerState.Shopping;
    }

    void Investigating() {
        Debug.Log("Investigation started");
        return;
    }

    void Chasing() {
        return;
    }

    void Stunned() {
        return;
    }

    void Smelling() {
        return;
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < pathPoints.Length; i++) {
            Gizmos.DrawWireSphere(pathPoints[i].transform.position, 0.5f);
            Gizmos.DrawLine(pathPoints[i].transform.position, pathPoints[(i + 1) % pathPoints.Length].transform.position);
        }
    }
}
