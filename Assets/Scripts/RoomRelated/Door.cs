using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#region Event args
public class OnDoorInteractArgs
{
    public PlayerController Player { get; }
    public OnDoorInteractArgs(PlayerController player) { Player = player; }
}

#endregion

[Serializable]
public class Door : MonoBehaviour, Interactable
{
    [SerializeField] public UnityEvent onEnter;

    public delegate void OnDoorInteractHandler(object sender, OnDoorInteractArgs args);
    public event OnDoorInteractHandler OnDoorInteract;

    [SerializeField] private Transform hinge;
    // The point outside a door where the player waits before entering
    [SerializeField] private Transform waitPoint1;
    [SerializeField] private Transform waitPoint2;

    private Vector3 otherWaitPoint;
    private PlayerController player;
    private float rotateDegrees;

    void Awake()
    {
        player = null;
        rotateDegrees = 90;
    }

    public void Interact(PlayerController player)
    {
        OnDoorInteract?.Invoke(this, new OnDoorInteractArgs(player));

        // DEBUG
        StartCoroutine(DEBUG(player));
    }

    public IEnumerator DEBUG(PlayerController player)
    {
        PlanStageStart(player);
        yield return new WaitForSeconds(3);
        EnterDoor();
        yield return new WaitForSeconds(3);
        CloseDoor();
    }

    /// <summary>
    /// The method called on this door when the player enters the planning stage
    /// </summary>
    /// <param name="player"></param>
    public void PlanStageStart(PlayerController player)
    {
        this.player = player;

        // We know that the door only has two waitpoints
        // Check which is closer and set the player's position to that one
        bool lowerDistCheck = Vector3.Distance(waitPoint1.position, player.transform.position) <
            Vector3.Distance(waitPoint2.position, player.transform.position);
        (Vector3, Vector3) waitPts = lowerDistCheck ? 
            (waitPoint1.position, waitPoint2.position) : 
            (waitPoint2.position, waitPoint1.position);
        player.transform.position = waitPts.Item1;
        otherWaitPoint = waitPts.Item2;
        // Make sure that the door opens in the right direction
        rotateDegrees *= lowerDistCheck ? -1 : 1;
    }

    public void EnterDoor()
    {
        transform.RotateAround(hinge.position, Vector3.up, rotateDegrees);
        player.transform.position = otherWaitPoint;
        player = null;
        onEnter?.Invoke();
    }

    public void CloseDoor()
    {
        transform.RotateAround(hinge.position, Vector3.up, -rotateDegrees);
    }

    private IEnumerable AnimateDoor()
    {
        return null;
    }
}