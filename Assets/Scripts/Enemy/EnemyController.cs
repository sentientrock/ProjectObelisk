using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Chase, Attack, Stunned }
public class EnemyController : MonoBehaviour
{
    public UnityEventEnemy onEnemyDeath;

    private EnemyState currState;
    private NavMeshAgent agent;
    private AmmoDictionary ammo;
    private float stun;

    
    [SerializeField] private float distToAttack;
    [SerializeField] private Weapon weapon; public Weapon EquippedWeapon {get => weapon;}
    [SerializeField] private Transform equipPos;

    [SerializeField] private Transform _target;
    public Transform Target
    {
        set
        {
            _target = value;
            //agent.enabled = true;
        }
    }

    void Awake()
    {
        currState = EnemyState.Idle;
        agent = transform.GetComponent<NavMeshAgent>();
        weapon.PickUpWeapon(gameObject, equipPos);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Paused) return;
        switch (currState)
        {
            case EnemyState.Idle:
                if (_target != null) currState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                Chase();
                if (Vector3.Distance(_target.position, transform.position) < distToAttack)
                    currState = EnemyState.Attack;
                break;

            case EnemyState.Attack:
                Attack();
                if (Vector3.Distance(_target.position, transform.position) > distToAttack)
                    currState = EnemyState.Chase;
                break;
            case EnemyState.Stunned:
                Stunned(stun);
                stun -= Time.deltaTime;
                if (stun <= 0.0f) {
                    currState = EnemyState.Idle;
                }
                break;
        }
    }

    #region Machine States

    private void Chase()
    {
        agent.isStopped = false;
        transform.LookAt(_target);
        agent.SetDestination(_target.position);
    }

    private void Attack()
    {
        agent.isStopped = true;
        transform.LookAt(_target);
        weapon.Fire1();
        weapon.Fire1Stop();
    }

    public void Stunned(float stunTime) 
    {
        if (currState != EnemyState.Stunned) {
            stun = stunTime;
            currState = EnemyState.Stunned;
        }
        agent.isStopped = true;
    }
    #endregion

    /// <summary>
    /// Handles this enemy dieing
    /// </summary>
    public void Die()
    {
        weapon.DropWeapon();
        onEnemyDeath?.Invoke(this);

        // Temp, can make ragdoll here instead of destroy
        Destroy(gameObject);
    }
}
