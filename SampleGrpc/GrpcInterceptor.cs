using Grpc.Core;
using Grpc.Core.Interceptors;

public class GrpcInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> handle)
    {
        // 요청 출력
        Console.WriteLine($"[gRPC 인터셉터] Method: {context.Method}");
        Console.WriteLine($"[gRPC 인터셉터] 요청 타입: {typeof(TRequest).Name}");
        Console.WriteLine($"[gRPC 인터셉터] 요청 내용: {request}");

        var response = await handle(request, context);

        // 응답 출력
        Console.WriteLine($"[gRPC 인터셉터] 응답 타입: {typeof(TResponse).Name}");
        Console.WriteLine($"[gRPC 인터셉터] 응답 내용: {response}");

        return response;
    }
}