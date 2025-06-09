var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<UserService>();

var app = builder.Build();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
app.MapGet("/", () => Results.File(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "login.html"), "text/html"));
app.MapGet("/login", () => Results.File(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "login.html"), "text/html"));
app.MapGet("/signup", () => Results.File(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "signup.html"), "text/html"));
app.MapGet("/verify", () => Results.File(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "verify.html"), "text/html"));
app.MapGet("/dashboard", () => Results.File(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "dashboard.html"), "text/html"));

var codes = new Dictionary<string, string>();

app.MapPost("/signup", (HttpRequest request, UserService users) =>
{
    var form = request.Form;
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    if (!users.AddUser(email, password))
    {
        return Results.BadRequest("User already exists");
    }
    var code = new Random().Next(100000, 999999).ToString();
    codes[email] = code;
    Console.WriteLine($"Verification code for {email}: {code}");
    return Results.Redirect($"/verify?email={Uri.EscapeDataString(email)}&code={code}");
});

app.MapPost("/login", (HttpRequest request, UserService users) =>
{
    var form = request.Form;
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    if (!users.ValidateCredentials(email, password))
    {
        return Results.BadRequest("Invalid credentials");
    }
    if (!users.IsVerified(email))
    {
        var code = new Random().Next(100000, 999999).ToString();
        codes[email] = code;
        Console.WriteLine($"Login code for {email}: {code}");
        return Results.Redirect($"/verify?email={Uri.EscapeDataString(email)}&code={code}");
    }
    return Results.Redirect("/dashboard");
});

app.MapPost("/verify", (HttpRequest request, UserService users) =>
{
    var form = request.Form;
    var code = form["code"].ToString();
    var email = form["email"].ToString();
    if (codes.TryGetValue(email, out var stored) && stored == code)
    {
        users.MarkVerified(email);
        codes.Remove(email);
        return Results.Redirect("/dashboard");
    }
    return Results.BadRequest("Invalid code");
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
