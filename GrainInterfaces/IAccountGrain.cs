public interface IAccountGrain : IGrainWithStringKey
{
    Task<int> GetBalance();
    Task Deposit(int amount);
    Task<bool> Withdraw(int amount);
}