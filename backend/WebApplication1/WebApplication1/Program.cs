using System.Text.Json.Serialization;
using WebApplication1;
using WebApplication1.ServiceDefaults;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy(name: MyAllowSpecificOrigins,
		policy =>
		{
			policy.WithOrigins("http://localhost:3000",
				"http://localhost:8000",
				"http://localhost:8080")
				.AllowAnyHeader()
				.AllowAnyMethod();
		});
});

builder.AddServiceDefaults();

builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddAuthentication()
	.AddJwtBearer();

builder.Services.AddAuthorizationBuilder()
	.AddPolicy("reports-realm", policy =>
		policy
			.RequireRole("reports-realm")
	)
	;

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

var sampleTodos = new Todo[]
{
	new(1, "Walk the dog"),
	new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
	new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
	new(4, "Clean the bathroom"),
	new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/reports");
todosApi.MapGet("/", () => sampleTodos)
	//.RequireAuthorization("prothetic_user")
	;

app.Run();

namespace WebApplication1
{
	public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

	[JsonSerializable(typeof(Todo[]))]
	internal partial class AppJsonSerializerContext : JsonSerializerContext
	{
	}
}