// Created by Binh Bui on 06, 25, 2021

using Common;
using UnityEngine;

namespace Entities
{
    public abstract class Enemy : Entity, IFalling, IDamageable
    {
        public Rigidbody2D rigidBody;
        public float speed = 1.5f;
        public HealthBar healthBar;

        protected float Health { get; set; }
        protected string Name { get; set; }

        public void Fall()
        {
            if (GameStats.GameIsPaused) return;
            rigidBody.MovePosition(Vector3.down * (speed * Time.fixedDeltaTime) + transform.position);

            if (transform.position.y <= -4.5)
            {
                Explode();
            }
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            healthBar.SetHealth(Health);

            if (Health <= 0)
            {
                Explode();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            var gameStats = GameStats.Instance;
            var playerDmg = gameStats.CurrentWeapon.Damage;
            var dmg = Name == nameof(Virus) ? Constants.VirusDamage : Constants.StarDamage;

            switch (other.gameObject.tag)
            {
                case "Player":
                    var player = other.gameObject.GetComponent<Player>();
                    switch (gameStats.currentWeaponType)
                    {
                        case WeaponType.Sword:
                            player.TakeDamage(dmg);
                            TakeDamage(playerDmg);
                            break;
                        case WeaponType.Gun:
                        {
                            player.TakeDamage(dmg);
                            TakeDamage(playerDmg);
                            break;
                        }
                        case WeaponType.Spike:
                            player.TakeDamage(dmg);
                            TakeDamage(playerDmg);
                            break;
                        case WeaponType.None:
                            dmg = Constants.SpikePlayerHealth;
                            player.TakeDamage(dmg);
                            break;
                    }
                    break;
                case "Bullet":
                    if (other.gameObject.name.Contains("PlayerBullet"))
                    {
                        TakeDamage(playerDmg);
                    }
                    break;
            }
        }
    }
}