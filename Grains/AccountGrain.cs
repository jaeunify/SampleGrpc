using Orleans.Transactions.Abstractions;

[GenerateSerializer]
public record class Balance
{
    [Id(0)]
    public int Value { get; init; } = 1_000;
}

public class AccountGrain : Grain, IAccountGrain
{
    private readonly ITransactionalState<Balance> _balance;
    
    public AccountGrain([TransactionalState("balance")] ITransactionalState<Balance> balance)
    {
        _balance = balance ?? throw new ArgumentNullException(nameof(balance));
    }
    
    public async Task<int> GetBalance() 
        => await _balance.PerformRead(balance => balance.Value);

    public async Task Deposit(int amount)
    {
        await _balance.PerformUpdate(balance => balance with { Value = balance.Value + amount });
    }
    
    // public async Task<bool> Withdraw(int amount)
    // {
    //     try
    //     {
    //         await _balance.PerformUpdate(balance =>
    //         {
    //             if (balance.Value < amount)
    //             {
    //                 throw new InvalidOperationException("잔액 부족");
    //             }
    //
    //             return Task.FromResult(balance with { Value = balance.Value - amount });
    //         });
    //         
    //         return true;
    //     }
    //     catch (InvalidOperationException)
    //     {
    //         return false;
    //     }
    // }

}