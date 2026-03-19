using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SkiDataSimulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static NpgsqlDataSource DataSource { get; private set; } = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var config = new ConfigurationBuilder()
                              .AddUserSecrets<App>()
                              .Build();

            string connectionstring = config.GetConnectionString("DefaultConnection");

            DataSource = NpgsqlDataSource.Create(connectionstring);
        }
    }

}
