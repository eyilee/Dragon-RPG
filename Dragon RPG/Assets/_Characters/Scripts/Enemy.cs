﻿using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealthPoint = 100f;
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float chaseRadius = 10f;

    [Tooltip("Damage per hit")]
    [SerializeField] private float damagePerShot = 10f;

    [Tooltip("Attack times per second")]
    [SerializeField] private float attackFrequency = 1f;

    [SerializeField] private GameObject projectileToUse;
    [SerializeField] private GameObject projectileSocket;
    [SerializeField] Vector3 aimOffset = new Vector3(0, 1f, 0);

    private GameObject player;
    private AICharacterControl aiCharacterControl;

    private float currentHealthPoint = 100f;
    private bool isAttacking = false;
    private Coroutine attackCoroutine = null;

    public float HealthAsPercentage { get { return currentHealthPoint / maxHealthPoint; } }

    private void Start()
    {
        aiCharacterControl = GetComponent<AICharacterControl>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        if (distanceToPlayer <= attackRadius && !isAttacking)
        {
            isAttacking = true;
            attackCoroutine = StartCoroutine(AttackToPlayer());
        }

        if (distanceToPlayer > attackRadius && isAttacking)
        {
            isAttacking = false;
            StopCoroutine(attackCoroutine);
        }

        if (distanceToPlayer <= chaseRadius)
        {
            aiCharacterControl.SetTarget(player.transform);
        }
        else
        {
            aiCharacterControl.SetTarget(transform);
        }
    }

    private IEnumerator AttackToPlayer()
    {
        while (true)
        {
            FireProjectile();
            yield return new WaitForSeconds(attackFrequency);
        }
    }

    void FireProjectile()
    {
        GameObject newProjectile = Instantiate(projectileToUse, projectileSocket.transform.position, Quaternion.identity);
        Projectile projectileComponent = newProjectile.GetComponent<Projectile>();
        projectileComponent.SetDamage(damagePerShot);
        projectileComponent.SetShooter(gameObject);

        Vector3 unitVectorToPlayer = (player.transform.position + aimOffset - projectileSocket.transform.position).normalized;
        float projectileSpeed = projectileComponent.GetDefaultLaunchSpeed();
        newProjectile.GetComponent<Rigidbody>().velocity = unitVectorToPlayer * projectileSpeed;
    }

    public void TakeDamage(float damage)
    {
        currentHealthPoint = Mathf.Clamp(currentHealthPoint - damage, 0f, currentHealthPoint);

        if (currentHealthPoint <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }
}
