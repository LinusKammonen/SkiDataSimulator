using SkiDataSimulator.Repositories;
using SkiDataSimulator.Simulation;
using SkidataWpf.Models;
using System.Windows;

namespace SkiDataSimulator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int SimulatedSkierCount = 20;
    private readonly DbRepository _dbRepository;
    private readonly SkiSimulator _simulator;

    public MainWindow()
    {
        InitializeComponent();
        _dbRepository = new DbRepository();
        _simulator = new SkiSimulator(_dbRepository);
    }


    private async void OnButtonSimulateDayClick(object sender, RoutedEventArgs e)
    {
        try
        {
            btnSimulateDay.IsEnabled = false;
            DateTime today = DateTime.Today;
            List<SkiPass> skiPasses = await _dbRepository.GetRandomSkiPassesAsync(SimulatedSkierCount);

            List<SkiRun> skiRuns = await _simulator.SimulateDayForAllSkipassesAsync(skiPasses, today);
            await _dbRepository.SaveSkiRunsAsync(skiRuns);

            ShowMessage("Simulering slutförd och data sparat!", "Info");

        }
        catch (Exception ex)
        {
            ShowMessage($"Ett fel inträffade: {ex.Message}", "Fel", MessageBoxImage.Error);
        }
        finally
        {
            btnSimulateDay.IsEnabled = true;
        }
    }

    private async void OnButtonSimulateSeasonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            btnSimulateSeason.IsEnabled = false;
            DateTime dayInSeason = new(2023, 1, 12); // kommer till exempel ge dig säsongen 23/24
            List<SkiPass> skiPasses = await _dbRepository.GetRandomSkiPassesAsync(SimulatedSkierCount);
            List<SkiRun> skiRuns = await _simulator.SimulateSeasonAsync(skiPasses, dayInSeason);
            await _dbRepository.SaveSkiRunsAsync(skiRuns);
            ShowMessage("Simulering slutförd och data sparat!", "Info");
        }
        catch (Exception ex)
        {
            ShowMessage($"Ett fel inträffade: {ex.Message}", "Fel", MessageBoxImage.Error);
        }
        finally
        {
            btnSimulateSeason.IsEnabled = true;
        }
    }

    private void ShowMessage(string message, string title, MessageBoxImage icon = MessageBoxImage.Information)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, icon);
    }


}
