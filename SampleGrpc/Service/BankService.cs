using Grpc.Core;
using GrpcDemo;
using Orleans;
using GrpcDemo.Services;
using Orleans.Transactions;
using SampleGrpc;

namespace GrpcDemo.Services
{
    public class BankService : Bank.BankBase
    {
        private readonly IClusterClient _orleansClient;
        private readonly ITransactionClient _transactionClient;

        public BankService(IClusterClient orleansClient, ITransactionClient transactionClient, ScopeManager s)
        {
            _orleansClient = orleansClient;
            _transactionClient = transactionClient;
            Console.WriteLine("scope id는 : " + s.Id);
        }
        
        public override async Task<Account> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            var grain = _orleansClient.GetGrain<IAccountGrain>(request.Id);
            var balance = await grain.GetBalance();
            return new Account { Amount = balance };
        }
        
        public override async Task<Account> Deposit(DepositRequest request, ServerCallContext context)
        {
            var userId = GetUserId(context);
            var grain = _orleansClient.GetGrain<IAccountGrain>(userId);

            await _transactionClient.RunTransaction(TransactionOption.Create, async () =>
            {
                await grain.Deposit(request.Amount);
            });
            
            var balance = await grain.GetBalance();
            
            return new Account { Amount = balance };
        }

        public override async Task<Account> Transfer(TransferRequest request, ServerCallContext context)
        {
            var userId = GetUserId(context);
            
            var grain = _orleansClient.GetGrain<IAccountGrain>(userId);
            var targetGrain = _orleansClient.GetGrain<IAccountGrain>(request.To);
            
            await _transactionClient.RunTransaction(TransactionOption.Create, async () =>
            {
                Console.WriteLine("Service: " + TransactionContext.GetRequiredTransactionInfo());
                await grain.Withdraw(request.Amount);
                await targetGrain.Deposit(request.Amount);
            });
            
            var balance = await grain.GetBalance();
            return new Account { Amount = balance };
        }

        private string GetUserId(ServerCallContext context)
        {
            return "2"; // temp
        }
    }
}