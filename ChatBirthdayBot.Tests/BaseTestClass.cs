using System;
using System.Globalization;
using ChatBirthdayBot.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatBirthdayBot.Tests;

public class BaseTestClass : IDisposable
{
	private readonly SqliteConnection _connection;
	private readonly DbContextOptions<DataContext> _contextOptions;

	protected BaseTestClass()
	{
		// Create and open a connection. This creates the SQLite in-memory database, which will persist until the connection is closed
		// at the end of the test (see Dispose below).
		_connection = new SqliteConnection("Filename=:memory:");
		_connection.Open();

		// These options will be used by the context instances in this test suite, including the connection opened above.
		_contextOptions = new DbContextOptionsBuilder<DataContext>()
			.UseSqlite(_connection)
			.Options;

		// Create the schema and seed some data
		using var context = new DataContext(_contextOptions);
		context.Database.EnsureCreated();

		context.SaveChanges();

		CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
	}

	protected DataContext CreateContext() => new(_contextOptions);

	protected ServiceProvider CreateServiceProvider(DataContext context)
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped(_ => context);
		return serviceCollection.BuildServiceProvider();
	}

	public void Dispose() => _connection.Dispose();
}
