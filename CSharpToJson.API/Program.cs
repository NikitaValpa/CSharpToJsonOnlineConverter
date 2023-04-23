using CSharpToJson.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMediator(opt =>
{
    opt.Namespace = "CSharpToJson.Application";
    opt.ServiceLifetime = ServiceLifetime.Scoped;
});
builder.Services.AddScoped<ICodeAnalyzer, CSharpCodeAnalyzer>();
builder.Services.AddScoped<IJsonCodeWriter, CSharpJsonCodeWriter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
