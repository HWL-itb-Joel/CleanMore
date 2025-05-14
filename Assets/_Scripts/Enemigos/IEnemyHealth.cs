public interface IEnemyHealth
{
    int Health { get; set; }

    void FlashOnHit();
    void TakeDamage(int i);
    void OnHealthChanged(int oldValue, int newValue);
}