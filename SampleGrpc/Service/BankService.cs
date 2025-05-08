using Grpc.Core;
using GrpcDemo;
using Orleans;
using GrpcDemo.Services;

namespace GrpcDemo.Services
{
    public class BankService : Bank.BankBase
    {
        private readonly IClusterClient _orleansClient;
        private readonly ITransactionClient _transactionClient;

        public BankService(IClusterClient orleansClient, ITransactionClient transactionClient)
        {
            _orleansClient = orleansClient;
            _transactionClient = transactionClient;
        }
        
        // 입금
        public override async Task<Account> Deposit(DepositRequest request, ServerCallContext context)
        {
            var userId = GetUserId(context); // 예: 요청자 ID
            var grain = _orleansClient.GetGrain<IAccountGrain>(userId);

            int balance = 0;

            await _transactionClient.RunTransaction(TransactionOption.Create, async () =>
            {
                await grain.Deposit(request.Amount);
            });
            
            balance = await grain.GetBalance(); // 이 호출도 트랜잭션 내에서 해야 함
            return new Account { Amount = balance };
        }

        // 출금
        // public override async Task<Account> Withdraw(WithdrawRequest request, ServerCallContext context)
        // {
        //     var userId = GetUserId(context);
        //     var grain = _orleansClient.GetGrain<IAccountGrain>(userId);
        //
        //     await grain.Withdraw(request.Amount); // 실패 여부는 무시
        //     var balance = await grain.GetBalance();
        //
        //     return new Account { Amount = balance };
        // }

        // 송금
        // public override async Task<Account> Transfer(TransferRequest request, ServerCallContext context)
        // {
        //     var fromId = GetUserId(context);
        //     var toId = request.To;
        //
        //     var fromGrain = _orleansClient.GetGrain<IAccountGrain>(fromId);
        //     var toGrain = _orleansClient.GetGrain<IAccountGrain>(toId);
        //
        //     if (await fromGrain.Withdraw(request.Amount))
        //     {
        //         await toGrain.Deposit(request.Amount);
        //     }
        //
        //     var balance = await fromGrain.GetBalance();
        //     return new Account { Amount = balance };
        // }
        
        // 유저 ID를 context에서 얻는 임시 메서드 (실제로는 메타데이터 또는 JWT 사용)
        private string GetUserId(ServerCallContext context)
        {
            return "default"; // 간단히 하드코딩. 필요시 context.RequestHeaders 활용
        }
    }
}