var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<UserService>();

var app = builder.Build();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/login", () => Results.File("wwwroot/login.html", "text/html"));
app.MapGet("/signup", () => Results.File("wwwroot/signup.html", "text/html"));
app.MapGet("/verify", () => Results.File("wwwroot/verify.html", "text/html"));

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
    return Results.Redirect("/verify");
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
    var code = new Random().Next(100000, 999999).ToString();
    codes[email] = code;
    Console.WriteLine($"Login code for {email}: {code}");
    return Results.Redirect("/verify");
});

app.MapPost("/verify", (HttpRequest request) =>
{
    var form = request.Form;
    var code = form["code"].ToString();
    if (codes.Values.Contains(code))
    {
        return Results.Ok("Verified!");
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
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
