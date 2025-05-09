using Orleans.Concurrency;
using Orleans.Transactions.Abstractions;

[GenerateSerializer]
public class Balance
{
    [Id(0)] public int Value { get; set; } = 1000;
}

[Reentrant]
public class AccountGrain : Grain, IAccountGrain
{
    private readonly ITransactionalState<Balance> _balance;

    public AccountGrain([TransactionalState("balance")] ITransactionalState<Balance> balance)
    {
        _balance = balance ?? throw new ArgumentNullException(nameof(balance));
    }
    
    public Task<int> GetBalance()
    {
        return _balance.PerformRead(balance => balance.Value);
    }

    public Task Deposit(int amount)
    {
        return _balance.PerformUpdate(b => b.Value += amount);
    }

    public Task Withdraw(int amount)
    {
        return _balance.PerformUpdate(b => b.Value -= amount);
    }
}