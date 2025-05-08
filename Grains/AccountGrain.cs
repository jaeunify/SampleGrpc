public class AccountGrain : Grain, IAccountGrain
{
    private int _balance;

    public Task<int> GetBalance() => Task.FromResult(_balance);

    public Task Deposit(int amount)
    {
        _balance += amount;
        return Task.CompletedTask;
    }

    public Task<bool> Withdraw(int amount)
    {
        if (_balance >= amount)
        {
            _balance -= amount;
            return Task.FromResult(true);
        }

        return Task.FromResult(false); // 잔액 부족
    }
}