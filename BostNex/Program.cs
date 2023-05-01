using BostNex.Areas.Identity;
using BostNex.Data;
using BostNex.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.AzureAppServices;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddTransient<IAesService, AesService>();   // AES�Í�
builder.Services.AddTransient<IChatService, ChatService>();
builder.Services.AddTransient<IHelperService, HelperService>();
builder.Services.AddTransient<IChatFormatService, ChatFormatService>();
builder.Services.AddScoped<IOpenAiService, OpenAiService>();                // Scoped�Ƃ��邱�ƂŃ����[�h������Z�b�V�����؂��悤�ɂ���
builder.Services.AddTransient<ISummaryService, SummaryService>();
builder.Services.Configure<AesOption>(builder.Configuration.GetSection("AesSettings"));
builder.Services.Configure<OpenAiOption>(builder.Configuration.GetSection("OpenAiSettings"));
builder.Services.Configure<ChatOption>(builder.Configuration.GetSection("ChatSettings"));
builder.Services.AddHotKeys2(); // �L�[�{�[�h�V���[�g�J�b�g������
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//app.UseStaticFiles();
// ��������ǉ�
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".dat"] = "text/plain";
provider.Mappings[".data"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".br"] = "application/octet-stream";   // .br �t�@�C���ɃA�N�Z�X�ł���悤�ɒǉ�
provider.Mappings[".js"] = "application/javascript";     // ��̕ϊ��ׂ̈ɒǉ�

app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider,
    OnPrepareResponse = context =>
    {
        var path = context.Context.Request.Path.Value;
        var extension = Path.GetExtension(path);

        // �u.gz�v�u.br�v�t�@�C���ɃA�N�Z�X�����ꍇ�� Content-Type �� Content-Encoding ��ݒ肷��
        if (extension == ".gz" || extension == ".br")
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path) ?? "";
            if (provider.TryGetContentType(fileNameWithoutExtension, out string? contentType))
            {
                context.Context.Response.ContentType = contentType;
                context.Context.Response.Headers.Add("Content-Encoding", extension == ".gz" ? "gzip" : "br");
            }
        }
    },
});
app.UseStaticFiles();
// �����܂Œǉ�

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// �}�C�O���[�V�������Ă��炤
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
