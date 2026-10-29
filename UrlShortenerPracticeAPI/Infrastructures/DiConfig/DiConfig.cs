using UrlShortenerPractice.Common.Validators;
using UrlShortenerPractice.Infrastructures.Options;

namespace UrlShortenerPractice.API.Infrastructures.DiConfig;

public static class DiConfig
{
    //管理註冊
    public static void AddDiConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // AddSingleton : 整個程式從啟動到關閉只建立一次連線 , 適合建立成本高或需要共用的物件 ( 讀設定 , 連線工具等 )
        // Configure : 讀 appsetting 裡的設定並放進 ShortLinkOptions , 來給 Service 注入
        // AddValidatorsFromAssemblyContaining : 掃描物件裡所有繼承 AbstractValidator<T> 的類別並註冊 , AbstractValidator<T> 是指這個類別是專門驗證 <T> 的一組規則

        services.AddSingleton<TokenHelper>();
        services.AddSingleton<PasswordSecureHelper>();
        services.AddSingleton<DBConnecting>();
        services.Configure<ShortLinkOptions>(configuration.GetSection("ShortLink"));
        services.AddValidatorsFromAssemblyContaining<CreateShortLinkRequestValidator>();

        //將Service結尾且生命週期相同的物件, 統一註冊
        services.Scan(scan =>
            scan.FromAssemblyOf<Program>() // 1.遍歷Program類別所在程序集中的所有類別
                .AddClasses(classes => // 2.要自動註冊的類別,條件為Service結尾的類別
                    classes.Where(t => t.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase))
                )
                .AsImplementedInterfaces() // 3.註冊的類別有實作界面
                .WithScopedLifetime() // 4.生命週期設定為Scoped
        );
    }
}
