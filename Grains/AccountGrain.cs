public class AccountGrain : Grain, IAccountGrain
{
    private decimal _balance;

    public Task<decimal> GetBalance() => Task.FromResult(_balance);

    public Task Deposit(decimal amount)
    {
        _balance += amount;
        return Task.CompletedTask;
    }

    public Task<bool> Withdraw(decimal amount)
    {
        if (_balance >= amount)
        {
            _balance -= amount;
            return Task.FromResult(true);
        }

        return Task.FromResult(false); // 잔액 부족
    }
}