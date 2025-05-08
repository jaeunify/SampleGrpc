using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo;
using static GrpcDemo.Messenger;

class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("========================");
        Console.WriteLine("= 어떤 기능을 선택할까요? ");
        Console.WriteLine("= 1. 채팅서버 접속 ");
        Console.WriteLine("= 2. 은행서버 접속 ");
        Console.WriteLine("========================");

        int input;
        while (true)
        {
            Console.Write("숫자 입력 : ");
            if (int.TryParse(Console.ReadLine(), out input))
            {
                switch (input)
                {
                    case 1:
                        await RunChatMode();
                        return;
                    case 2:
                        await RunBankMode();
                        return;
                    default:
                        Console.WriteLine("잘못된 입력입니다. 다시 시도하세요.");
                        break;
                }
            }
        }
    }

    public static async Task RunChatMode()
    {
        // 채널 생성 (서버는 5001 포트에서 실행 중이어야 함)
        using var channel = GrpcChannel.ForAddress("https://localhost:5001");
        var client = new MessengerClient(channel);

        // Chat 스트리밍 시작
        using var call = client.Chat();

        // 수신 Task
        var readTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    if (response.MsgCase == ResponseMessage.MsgOneofCase.Receive)
                    {
                        Console.WriteLine($"[수신] {response.Receive.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[수신 오류] {ex.Message}");
            }
        });

        // 사용자 입력 → 서버로 전송
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) break;

            var request = new RequestMessage
            {
                Send = new RequestMessage.Types.SendMessage
                {
                    Message = line
                }
            };

            await call.RequestStream.WriteAsync(request);
        }

        // 스트림 종료
        await call.RequestStream.CompleteAsync();
        await readTask;
    }

    public static async Task RunBankMode()
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:5001");
        var client = new Bank.BankClient(channel);

        while (true)
        {
            Console.WriteLine("========================");
            Console.WriteLine("= 은행 기능을 선택하세요:");
            Console.WriteLine("= 1. 입금");
            Console.WriteLine("= 2. 출금");
            Console.WriteLine("= 3. 송금");
            Console.WriteLine("= 0. 종료");
            Console.WriteLine("========================");

            Console.Write("선택: ");
            var input = Console.ReadLine();

            if (input == "0")
            {
                Console.WriteLine("은행 모드를 종료합니다.");
                break;
            }

            try
            {
                switch (input)
                {
                    case "1": // 입금
                        Console.Write("입금할 금액: ");
                        if (int.TryParse(Console.ReadLine(), out int depositAmount))
                        {
                            var result = await client.DepositAsync(new DepositRequest { Amount = depositAmount });
                            Console.WriteLine($"[입금 완료] 현재 잔액: {result.Amount}");
                        }

                        break;

                    case "2": // 출금
                        Console.Write("출금할 금액: ");
                        if (int.TryParse(Console.ReadLine(), out int withdrawAmount))
                        {
                            var result = await client.WithdrawAsync(new WithdrawRequest { Amount = withdrawAmount });
                            Console.WriteLine($"[출금 완료] 현재 잔액: {result.Amount}");
                        }

                        break;

                    case "3": // 송금
                        Console.Write("송금할 대상 이름: ");
                        var to = Console.ReadLine();
                        Console.Write("송금할 금액: ");
                        if (int.TryParse(Console.ReadLine(), out int transferAmount))
                        {
                            var result = await client.TransferAsync(new TransferRequest { Amount = transferAmount, To = to });
                            Console.WriteLine($"[송금 완료] 현재 잔액: {result.Amount}");
                        }

                        break;

                    default:
                        Console.WriteLine("잘못된 선택입니다.");
                        break;
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"[gRPC 오류] {ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[예외 발생] {ex.Message}");
            }
        }
    }
}