using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class CustomerScript : MonoBehaviour {
    enum CustomerState {
        Shopping,
        ShoppingStopped,
        Alerted,
        Investigating,
        Returning,
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
    private int currentChasePathIndex;
    private List<Vector3> pathVectorList;
    private bool hasLineOfSight = false;
    private Transform lastStoppingPoint;
    private Transform investigationPoint;

    [Header("Speeds")]
    [SerializeField] private float shoppingSpeed = 5f;
    [SerializeField] private float investigateSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.1f;
    [SerializeField] private float returnSpeed = 2f;

    void Start() {
        rb = transform.GetChild(0).gameObject.GetComponent<Rigidbody2D>();
        animator = transform.GetChild(0).gameObject.GetComponent<Animation>();
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
                    SetTargetPosition(player.transform.position);
                    yield return new WaitForSeconds(0.5f);
                    currentState = CustomerState.Investigating;
                    break;
                case CustomerState.Investigating:
                    Investigating();
                    break;
                case CustomerState.Returning:
                    //yield return new WaitForSeconds(2f);
                    SetTargetPosition(pathPoints[0].position);
                    Returning();
                    break;
                case CustomerState.Chasing:
                Debug.Log("Chasing");
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

        if (Vector2.Distance(transform.position, player.transform.position) < 3f) { // If customer detects player
            if (hasLineOfSight) {
                currentState = CustomerState.Chasing;
            } else {
                currentState = CustomerState.Alerted;
            }
            
            return;
        }


        //if (direction != Vector2.zero) { // Rotate sprite along path
        //    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //    transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        //}

        /* foreach (Transform point in stoppingPoints) {
            if (point != null) {
                if (Vector2.Distance(transform.position, point.position) < 0.1f && lastStoppingPoint != point) { // If customer reaches stopping point
                lastStoppingPoint = point;
                    if (UnityEngine.Random.value <= 1f) {
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

        if (Vector2.Distance(transform.position, player.transform.position) < 3 && hasLineOfSight) {
            currentState = CustomerState.Chasing;
             return;
        }

        if (pathVectorList != null) {
            Vector3 targetPosition = pathVectorList[currentChasePathIndex];
            if (Vector3.Distance(transform.position, targetPosition) > .5f) {
                Vector3 moveDir = (targetPosition - transform.position).normalized;

                transform.position = transform.position + moveDir * investigateSpeed * Time.deltaTime;
            } else {
                currentChasePathIndex++;
                if (currentChasePathIndex >= pathVectorList.Count) {
                    pathVectorList = null;
                    if (Vector2.Distance(transform.position, player.transform.position) > 2.5f || !hasLineOfSight) { // They reach the last point and player isn't around
                        currentState = CustomerState.Returning;
                    }
                }
            }
        }
    }

    void Returning() {
        Debug.Log("Returning");

        if (Vector2.Distance(transform.position, player.transform.position) < 3 && hasLineOfSight) {
            currentState = CustomerState.Chasing;
             return;
        }

        if (pathVectorList != null) {
            Vector3 targetPosition = pathVectorList[currentChasePathIndex];
            if (Vector3.Distance(transform.position, targetPosition) > .5f) {
                Vector3 moveDir = (targetPosition - transform.position).normalized;

                transform.position = transform.position + moveDir * returnSpeed * Time.deltaTime;
            } else {
                currentChasePathIndex++;
                if (currentChasePathIndex >= pathVectorList.Count) {
                    pathVectorList = null;
                    currentPathIndex = 0;
                    currentState = CustomerState.Shopping;
                }
            }
        }
    }

    void Chasing() {
        Vector3 moveDir = (player.transform.position - transform.position).normalized;

        if (Vector2.Distance(transform.position, player.transform.position) < 3 && hasLineOfSight) {
            transform.position = transform.position + moveDir * chaseSpeed * Time.deltaTime;
        } else if (!hasLineOfSight) {
            SetTargetPosition(player.transform.position);
            currentState = CustomerState.Investigating;
        }
    }

    void Stunned() {
        return;
    }

    void Smelling() {
        return;
    }

    public void SetTargetPosition(Vector3 targetPosition) {
        currentChasePathIndex = 0;
        pathVectorList = Pathfinder.Instance.FindPath(transform.position, targetPosition);

        if (pathVectorList != null) {
            for (int i = 0; i < pathVectorList.Count - 1; i++) {
                Debug.DrawLine(new Vector3(pathVectorList[i].x, pathVectorList[i].y), new Vector3(pathVectorList[i + 1].x, pathVectorList[i + 1].y), Color.green, 5f);
            }
        }

        if (pathVectorList != null && pathVectorList.Count > 1) {
            pathVectorList.RemoveAt(0);
        }
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < pathPoints.Length; i++) {
            Gizmos.DrawWireSphere(pathPoints[i].transform.position, 0.5f);
            Gizmos.DrawLine(pathPoints[i].transform.position, pathPoints[(i + 1) % pathPoints.Length].transform.position);
        }
    }
}
