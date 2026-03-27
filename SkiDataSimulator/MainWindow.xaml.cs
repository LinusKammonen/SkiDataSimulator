using Npgsql;
using SkiDataSimulator.Models;
using SkiDataSimulator.Repositories;
using SkiDataSimulator.Simulation;
using SkidataWpf.Models;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static System.Net.WebRequestMethods;

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
        reset_grids();
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

            string[] display = new string[skiRuns.Count];
 
                for (int i = 0; i < skiRuns.Count; i++)
                {
                    display[i] += "Ski pass id (" + skiRuns[i].SkipassId.ToString() + ") ";
                    display[i] += "tid (" + skiRuns[i].Timestamp.ToString() + ") ";
                    display[i] += "lift id (" + skiRuns[i].LiftId.ToString() + ")";
                }

            lstDayResults.ItemsSource = display;
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
            string[] display = new string[skiRuns.Count];

            for (int i = 0; i < skiRuns.Count; i++)
            {
                display[i] += "Ski pass id (" + skiRuns[i].SkipassId.ToString() + ") ";
                display[i] += "tid (" + skiRuns[i].Timestamp.ToString() + ") ";
                display[i] += "lift id (" + skiRuns[i].LiftId.ToString() + ")";
            }

            lstSeasonResults.ItemsSource = display;
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
        try
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
        catch (Exception exception)
        {
            MessageBox.Show($"{exception.Message}");
        }
        
        
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
        Delete.Visibility = Visibility.Hidden;
        Simulator.Visibility = Visibility.Hidden;
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
        try
        {
            
            //nu bör vi på något sätt identifiera det valda objektet kollar i labb 2

            Skier? skier = lstskiers.SelectedItem as Skier;

            if (skier is not Skier)
            {
                return;
            }

            SkierDetailedSeason? detailedskier = await _dbRepository.GetSkierDetailedSeason(skier.Id);
            SkierLeaderboardDetails leaderboardDetails = await _dbRepository.GetLeaderboardDetails(skier.Id);

            

            txtUser.Text = skier.Username;
            txtFirst.Text = skier.Firstname;
            txtLast.Text = skier.Lastname;

            //https://stackoverflow.com/questions/11082804/detecting-image-url-in-c-net - för att kolla om det är en URL och inte bara en sträng

            bool IsImageUrl(string URL)
            {
                var req = (HttpWebRequest)HttpWebRequest.Create(URL);
                req.Method = "HEAD";
                using (var resp = req.GetResponse())
                {
                    return resp.ContentType.ToLower(CultureInfo.InvariantCulture)
                               .StartsWith("image/");
                }
            }
            if (skier.Image_url != string.Empty && skier.Image_url != null)
            {
                if (IsImageUrl(skier.Image_url) == true)
                {
                    imgSkier.Source = new BitmapImage(new Uri(skier.Image_url));
                }
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
        catch (Exception exception)
        {
            MessageBox.Show($"{exception.Message}");
        }
        
    }

    private async void Button_Click_2(object sender, RoutedEventArgs e)
    {
        try
        {
            //https://www.tutorialspoint.com/article/chash-datetime-to-add-days-to-the-current-date
            //kalender https://www.youtube.com/watch?v=gvyWHB3i930

            // För DateTime.Today https://stackoverflow.com/questions/6817266/how-to-get-the-current-date-without-the-time
            // För nullable DateTime https://stackoverflow.com/questions/72118892/how-to-convert-nullable-system-datetime-to-system-dateonly
            //Tryparse https://stackoverflow.com/questions/45030/how-to-parse-a-string-into-a-nullable-int

            Skier skier = await _dbRepository.FindSkierByUsername(txtInsertUser.Text);
            Destination? destination = cbDestination.SelectedItem as Destination;
            Season season = await _dbRepository.GetSeasonByDateAsync(DateTime.Today);
            Liftkort? liftkort = cbPasses.SelectedItem as Liftkort;
           

            if (destination == null)
            {
                MessageBox.Show("Du har inte valt en destination");
                return;
            }
            if (calendarBox.SelectedDate == null)
            {
                MessageBox.Show("Du har inte fyllt i ett startdatum");
                return;
            }
            if (cbPasses.SelectedItem == null)
            {
                MessageBox.Show("Du har inte valt ett pass");
                return;
            }
            
            if (skier is Skier && liftkort?.Name != "Säsong" && txtCardNumber.Text != string.Empty)
            {
                SkiPass skiPass = new SkiPass
                {
                    CardNumber = int.Parse(txtCardNumber.Text),
                    SkierId = skier.Id,
                    ValidFrom = DateOnly.Parse(calendarBox.SelectedDate?.ToString("D")),
                    ValidTo = DateOnly.Parse(calendarBox.SelectedDate?.ToString("D")).AddDays(liftkort.Time),
                    DestinationId = destination.Id
                };

                if (skiPass.ValidTo != null && skiPass.DestinationId != null)
                {
                    if (skiPass.ValidTo <= DateOnly.Parse(season.EndDate.ToString("D")) && skiPass.ValidFrom >= DateOnly.Parse(season.StartDate.ToString("D")) && skiPass.ValidFrom >= DateOnly.Parse(DateTime.Now.ToString("D")))
                    {
                        bool svar = await _dbRepository.BuySkiPass(skiPass);
                        if (svar == true)
                        {
                            MessageBox.Show($"Tack för ditt köp {skier.Firstname}, ditt liftkort gäller mellan {skiPass.ValidFrom} & {skiPass.ValidTo}.");
                            txtCardNumber.Text = string.Empty;
                            txtInsertUser.Text = string.Empty;
                        }
                    }
                    else if (skiPass.ValidFrom <= DateOnly.Parse(DateTime.Now.ToString("D")))
                    {
                         MessageBox.Show("Du kan inte välja ett datum som redan varit");
                    }
                    else
                    {
                        MessageBox.Show("Du har valt datum utanför säsongen");
                    }
                }
                
            }
            else if(skier is Skier && liftkort?.Name == "Säsong" && txtCardNumber.Text != string.Empty)
            {
                SkiPass skiPass = new SkiPass
                {
                    CardNumber = int.Parse(txtCardNumber.Text),
                    SkierId = skier.Id,
                    ValidFrom = DateOnly.Parse(season.StartDate.ToString("D")),
                    ValidTo = DateOnly.Parse(season.EndDate.ToString("D")),
                    DestinationId = destination.Id
                };

                if (skiPass.ValidTo != null && skiPass.DestinationId != null)
                {
                    bool svar = await _dbRepository.BuySkiPass(skiPass);
                    if (svar == true)
                    {
                        MessageBox.Show($"Tack för ditt köp {skier.Firstname}, ditt liftkort gäller mellan {skiPass.ValidFrom} & {skiPass.ValidTo}.");
                        txtCardNumber.Text = string.Empty;
                        txtInsertUser.Text = string.Empty;
                    }
                }
            }
            else if (txtCardNumber.Text == string.Empty && skier is Skier)
            {
                MessageBox.Show($"Ange kortnummer");
            }
            else if (skier is not Skier)
            {
                MessageBox.Show("Ange ett användarnamn som existerar");
            }
            
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }

    private async void btnLiftkort_Click(object sender, RoutedEventArgs e)
    {
        
        reset_grids();
        Liftkort.Visibility = Visibility.Visible;
        List<Destination>destinations = await _dbRepository.GetDestinations();
        FillCombobox<Destination>(cbDestination, destinations);

        Liftkort liftkort = new Liftkort();
        List<Liftkort> passes = liftkort.GetPasses();
        FillCombobox<Liftkort>(cbPasses, passes);
    }

    private async void cbDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        
    }

    private async void btnRides_Click(object sender, RoutedEventArgs e)
    {
        cbRunDestination.ItemsSource = null;
        cbRunResort.ItemsSource = null;
        cbLifts.ItemsSource = null;
        reset_grids();
        Rides.Visibility = Visibility.Visible;
        List<Destination> destinations = await _dbRepository.GetDestinations();
        FillCombobox<Destination>(cbRunDestination, destinations);
    }

    private async void cbRunDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Destination? destination = cbRunDestination.SelectedItem as Destination;
        if (destination is not null)
        {
            List<Resort> resorts = await _dbRepository.FindResortByDestinationID(destination.Id);
            FillCombobox<Resort>(cbRunResort, resorts);
        }
        
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
        
        try
        {
            //Nu saknar vi bara kontrollerna, destinationId och att kortet är giltigt, tänker att vi gör en select och sedan typ (om det är null så kör vi inte inserten).
            Destination? destination = cbRunDestination.SelectedItem as Destination;
            Resort? resort = cbRunResort.SelectedItem as Resort;
            Lift? lift = cbLifts.SelectedItem as Lift;
            Season season = await _dbRepository.GetSeasonByTime(DateTime.Today);
            if (txtRunCardNumber.Text != string.Empty && destination != null && resort != null && lift != null && season != null)
            {
                SkiPass? skipass = await _dbRepository.GetSkipassByCardnumber(int.Parse(txtRunCardNumber.Text));
                if (skipass is SkiPass)
                {
                    if (destination.Id != skipass.DestinationId && DateOnly.Parse(DateTime.Now.ToString("D")) <= skipass.ValidTo && DateOnly.Parse(DateTime.Now.ToString("D")) >= skipass.ValidFrom)
                    {
                        MessageBox.Show("Du saknar pass för denna destination");
                        return;
                    }
                    else if (DateOnly.Parse(DateTime.Now.ToString("D")) > skipass.ValidTo || skipass == null)
                    {
                        MessageBox.Show("Du saknar giltigt liftkort");
                        return;
                    }
                    
                    else if (destination.Id == skipass.DestinationId && DateOnly.Parse(DateTime.Now.ToString("D")) <= skipass.ValidTo && DateOnly.Parse(DateTime.Now.ToString("D")) >= skipass.ValidFrom)
                    {
                        bool svar = await _dbRepository.RegisterRideByLift(skipass.Id, lift.Id, season.Id, DateTime.Now); 

                        if (svar == true) { MessageBox.Show($"Du har registrerat åk i {lift.Name}"); }
                    }
                }
                else
                {
                    MessageBox.Show("Liftkortsnumret du angav tillhör ingen skidåkare.");
                    return;
                }

                //För datumen https://stackoverflow.com/questions/5672862/check-if-datetime-instance-falls-in-between-other-two-datetime-objects

            }
            else if (txtRunCardNumber.Text != string.Empty && destination != null && resort != null && lift != null && season == null)
            {
                MessageBox.Show("Liftarna har stängt för säsongen");
                return;
            }
            else
            {
                MessageBox.Show("Du har lämnat obligatoriska fält tomma");
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
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
        try
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
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
        
    }

    private async void btnDeleteLift_Click(object sender, RoutedEventArgs e)
    {
        try
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
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
        
    }

    private void btnSimulator_Click(object sender, RoutedEventArgs e)
    {
        reset_grids();
        Simulator.Visibility = Visibility.Visible;
    }
}
