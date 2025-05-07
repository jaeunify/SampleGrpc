using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo;
using static GrpcDemo.Messenger;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("========================");
        Console.WriteLine("=어떤 기능을 선택할까요?  =");
        Console.WriteLine("=1. 채팅서버 접속        =");
        Console.WriteLine("=2. 은행서버 접속        =");
        Console.WriteLine("========================");

        int input;
        while (true)
        {
            Console.WriteLine("숫자로 입력하세요 : ");
            if (int.TryParse(Console.ReadLine(), out input)
            {
                if(input))
                break;
            }
        }


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
}