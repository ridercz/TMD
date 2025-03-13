# Test for various code blocks
- - -
The following is indented code block:

	var builder = WebApplication.CreateBuilder(args);
	var app = builder.Build();
	app.MapGet("/", () => "Hello, world!");
	app.Run();
- - -
The following is fenced code block (and should look exactly the same):

```
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Hello, world!");
app.Run();
```
- - -
The following is a code block with edit tag and shows what is added and removed:

```edit
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
- app.MapGet("/", () => "Hello, world!");
+ app.MapGet("/", () => "Hello, world!");
app.Run();
```
