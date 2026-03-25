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
        Rides.Visibility = Visibility.Hidden;
        Delete.Visibility = Visibility.Hidden;
       
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
        Rides.Visibility = Visibility.Hidden;
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
            // För nullable DateTime https://stackoverflow.com/questions/72118892/how-to-convert-nullable-system-datetime-to-system-dateonly
            
            SkiPass skiPass = new SkiPass
            {
                CardNumber = int.Parse(txtCardNumber.Text),
                SkierId = skier.Id,
                ValidFrom = DateOnly.Parse(DateTime.Today.ToString("D")),
                ValidTo = DateOnly.Parse(calendarBox.SelectedDate?.ToString("D")),
                DestinationId = destination.Id
            };

            if (skiPass.ValidTo != null && skiPass.DestinationId != null)
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

    private async void btnRides_Click(object sender, RoutedEventArgs e)
    {
        reset_grids();
        Rides.Visibility = Visibility.Visible;
        List<Destination> destinations = await _dbRepository.GetDestinations();
        FillCombobox<Destination>(cbRunDestination, destinations);
    }

    private async void cbRunDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Destination? destination = cbRunDestination.SelectedItem as Destination;
        List<Resort> resorts = await _dbRepository.FindResortByDestinationID(destination.Id);
        FillCombobox<Resort>(cbRunResort, resorts);
    }

    private async void cbRunResort_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Resort? resort = cbRunResort.SelectedItem as Resort;
        if (resort != null)
        {
            List<Lift> lifts = await _dbRepository.FindLiftsByResortId(resort.Id);
            FillCombobox<Lift>(cbLifts, lifts);
        }
        else return;
    }

    private async void btnRegisterRide_Click(object sender, RoutedEventArgs e)
    {
        //Nu saknar vi bara kontrollerna, destinationId och att kortet är giltigt, tänker att vi gör en select och sedan typ (om det är null så kör vi inte inserten).
        Destination? destination = cbRunDestination.SelectedItem as Destination;
        Resort? resort = cbRunResort.SelectedItem as Resort;
        Lift? lift = cbLifts.SelectedItem as Lift;
        Season season = await _dbRepository.GetSeasonByTime(DateTime.Today);
        if (txtCardNumber.Text != string.Empty && destination != null && resort != null && lift != null && season != null)
        {
            SkiPass skipass = await _dbRepository.GetSkipassByCardnumber(int.Parse(txtRunCardNumber.Text));

            SkiRun skirun = new SkiRun
            {
                SkipassId = skipass.Id,
                LiftId = lift.Id,
                SeasonId = season.Id,
                Timestamp = DateTime.Now
            };

            //För datumen https://stackoverflow.com/questions/5672862/check-if-datetime-instance-falls-in-between-other-two-datetime-objects
            if (destination.Id != skipass.DestinationId && DateOnly.Parse(skirun.Timestamp.ToString("D")) < skipass.ValidTo && DateOnly.Parse(skirun.Timestamp.ToString("D")) > skipass.ValidFrom)
            {
                MessageBox.Show("Du saknar pass för denna destination");
                return;
            }
            else if (DateOnly.Parse(skirun.Timestamp.ToString("D")) > skipass.ValidTo || skipass == null)
            {
                MessageBox.Show("Du saknar giltigt liftkort");
                return;
            }
            else if (destination.Id == skipass.DestinationId && DateOnly.Parse(skirun.Timestamp.ToString("D")) < skipass.ValidTo && DateOnly.Parse(skirun.Timestamp.ToString("D")) > skipass.ValidFrom)
            {
                bool svar = await _dbRepository.RegisterRideByLiftId(skirun);

                if (svar == true) { MessageBox.Show($"Du har registrerat åk i {lift.Name}"); }

            }
        }
        else 
        {
            MessageBox.Show("Du har lämnat fält tomma");
            return;

        }

    }

    private async void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        reset_grids();
        Delete.Visibility = Visibility.Visible;
        List<Resort> resorts = await _dbRepository.GenerateResorts();
        List<Lift> lifts = await _dbRepository.GenerateLift();
        FillCombobox<Resort>(cbDeleteResorts, resorts);
        FillCombobox<Lift>(cbDeleteLifts, lifts);
    }

    private async void btnDeleteResort_Click(object sender, RoutedEventArgs e)
    {
        Resort? resort = cbDeleteResorts.SelectedItem as Resort;
        if (resort != null)
        {
            bool svar = await _dbRepository.DeleteResort(resort.Id);
            if (svar == true)
            {
                MessageBox.Show($"Du har tagit bort {resort.Name}");
            }
            else return;
        }
        else
        {
            MessageBox.Show($"Du har inte angivit ett resort");
        }
    }

    private async void btnDeleteLift_Click(object sender, RoutedEventArgs e)
    {
        Lift? lift = cbDeleteLifts.SelectedItem as Lift;
        if (lift != null)
        {
            bool svar = await _dbRepository.DeleteLift(lift.Id);
            if (svar == true)
            {
                MessageBox.Show($"Du har tagit bort {lift.Name}");
            }
            else return;
        }
        else
        {
            MessageBox.Show($"Du har inte angivit en lift");
        }
    }
}
