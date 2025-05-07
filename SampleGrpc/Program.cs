using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 포트 명시적 설정
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
        listenOptions.UseHttps(); // 인증서 자동 사용 (개발용)
    });
});

builder.Services.AddSingleton<StreamManager>();
builder.Services.AddGrpc(options =>                
        options.Interceptors.Add<GrpcInterceptor>()
    );


var app = builder.Build();

// gRPC 엔드포인트 매핑
app.MapGrpcService<GrpcDemo.Services.BankService>();
app.MapGrpcService<GrpcDemo.Services.MessengerService>();
app.MapGet("/", () => "gRPC 클라이언트로 접속해야 합니다.");

app.Run();