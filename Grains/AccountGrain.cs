using Orleans.Concurrency;
using Orleans.Transactions.Abstractions;

[GenerateSerializer]
public record class Balance
{
    [Id(0)]
    public int Value { get; init; } = 1000;
}

[Reentrant]
public class AccountGrain : Grain, IAccountGrain
{
    private readonly ITransactionalState<Balance> _balance;

    public AccountGrain(
        [TransactionalState("balance")] ITransactionalState<Balance> balance) =>
        _balance = balance ?? throw new ArgumentNullException(nameof(balance));
    
    public Task Deposit(int amount) =>
        _balance.PerformUpdate(
            balance => balance with { Value = balance.Value + amount });
    
    public Task<int> GetBalance() =>
        _balance.PerformRead(balance => balance.Value);
}