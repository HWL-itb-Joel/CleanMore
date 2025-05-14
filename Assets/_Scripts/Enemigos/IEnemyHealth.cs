public interface IEnemyHealth
{
    public int Health { get; set; }

    public void TakeDamage(int i)
    {
        Health -= i;
    }
}