using SkiDataSimulator.Models;
using SkiDataSimulator.Repositories;
using SkiDataSimulator.Simulation;
using SkidataWpf.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        _dbRepository = new DbRepository(App.DataSource);
        _simulator = new SkiSimulator(_dbRepository);
        Register.Visibility = Visibility.Hidden;
        Searchfunction.Visibility = Visibility.Hidden;
        Liftkort.Visibility = Visibility.Hidden;
       
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

    private async void btnRegister_Click(object sender, RoutedEventArgs e)
    {
        string firstname = txtFirstName.Text;
        string lastname = txtLastName.Text;
        string email = txtEmail.Text;
        string username = txtUsername.Text;
        string image = txtImageUrl.Text;

        Skier skier = new Skier
        {
            Firstname = firstname,
            Lastname = lastname,
            Email = email,
            Username = username,
            Image_url = image
        };
        bool svar = await _dbRepository.RegisterSkier(skier);
        MessageBox.Show($"du har registrerat {skier.Firstname} {skier.Lastname}");

        txtFirstName.Text = string.Empty;
        txtLastName.Text = string.Empty;
        txtEmail.Text = string.Empty;
        txtUsername.Text = string.Empty;
        txtImageUrl.Text = string.Empty;
    }
    private async void FillListbox<T>(ListBox lb, List<T> list)
    {
        lb.ItemsSource = list;
        lb.DisplayMemberPath = "FullName"; 
    }
    private async void FillCombobox<T>(ComboBox cb, List<T> list)
    {
        cb.ItemsSource = list;
        cb.DisplayMemberPath = "Name";
    }

    private async void btnSearch_Click(object sender, RoutedEventArgs e)
    {
        string search = txtSearchSkier.Text;
        List<Skier> skiers = await _dbRepository.SearchSkier(search);
        FillListbox<Skier>(lstskiers, skiers);
    }

    //källa för att använda flera sidor https://www.youtube.com/watch?v=8rUuZSFRncc 
    private void reset_grids()
    {
        Register.Visibility = Visibility.Hidden;
        Searchfunction.Visibility = Visibility.Hidden;
        Liftkort.Visibility = Visibility.Hidden;
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        reset_grids();
        Register.Visibility = Visibility.Visible;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        reset_grids();
        Searchfunction.Visibility = Visibility.Visible;
    }

    private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //nu bör vi på något sätt identifiera det valda objektet kollar i labb 2
        Skier? skier = lstskiers.SelectedItem as Skier;
        SkierDetailedSeason detailedskier = await _dbRepository.GetSkierDetailedSeason(skier.Id);
        SkierLeaderboardDetails leaderboardDetails = await _dbRepository.GetLeaderboardDetails(skier.Id);

        txtUser.Text = skier.Username;
        txtFirst.Text = skier.Firstname;
        txtLast.Text = skier.Lastname;
        if (skier.Image_url != null) 
        {
            imgSkier.Source = new BitmapImage(new Uri(skier.Image_url));
        }
        else
        {
            imgSkier.Source = new BitmapImage(new Uri("https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg"));
        }
        if (leaderboardDetails is null)
        {
            txtDrop.Text = "0";
            txtCountries.Text = "0";
        }
        else
        {
            txtDrop.Text = leaderboardDetails.TotalVerticalDrop.ToString();
            txtCountries.Text = leaderboardDetails.TotalCountries.ToString();
        }
        if (detailedskier is null)
        {
            txtSeason.Text = "ingen data för denna säsong";
            txtEndDate.Text = "ingen data för denna säsong";
            txtRuns.Text = "ingen data för denna säsong";
            txtDays.Text = "ingen data för denna säsong";
        }
        else
        {
            txtSeason.Text = detailedskier.CurrentSeason;
            txtEndDate.Text = detailedskier.Enddate.ToString();
            txtRuns.Text = detailedskier.TotalSeasonRuns.ToString();
            txtDays.Text = detailedskier.TotalSeasonDays.ToString();
        }
    }

    private async void Button_Click_2(object sender, RoutedEventArgs e)
    {
        
        Skier skier = await _dbRepository.FindSkierByUsername(txtInsertUser.Text);
        Destination? destination = cbDestination.SelectedItem as Destination;

        if (skier is Skier)
        {
            //kalender https://www.youtube.com/watch?v=gvyWHB3i930

            // För DateTime.Today https://stackoverflow.com/questions/6817266/how-to-get-the-current-date-without-the-time

            SkiPass skiPass = new SkiPass
            {
                CardNumber = txtCardNumber.Text,
                SkierId = skier.Id,
                ValidFrom = DateOnly.Parse(DateTime.Today.ToString("D")),
                ValidTo = DateOnly.Parse(calendarBox.SelectedDate.ToString("D")),
                DestinationId = destination.Id
            };

            if (skiPass.ValidTo != null)
            {
                bool svar = await _dbRepository.BuySkiPass(skiPass);
                MessageBox.Show($"{svar}");
            }
        }
        else
        {
            MessageBox.Show("Användarnamnet du angav finns inte");
            return;
        }
        txtCardNumber.Text = string.Empty;
        txtInsertUser.Text = string.Empty;

    }

    private async void btnLiftkort_Click(object sender, RoutedEventArgs e)
    {
        reset_grids();
        Liftkort.Visibility = Visibility.Visible;
        List<Destination>destinations = await _dbRepository.GetDestinations();
        FillCombobox<Destination>(cbDestination, destinations);
    }

    private async void cbDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        
    }
}
