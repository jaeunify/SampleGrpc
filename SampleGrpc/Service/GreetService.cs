using Grpc.Core;
using GrpcDemo;

namespace GrpcDemo.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = $"안녕하세요, {request.Name}님!"
            });
        }
    }
}