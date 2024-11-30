using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using LigaNOS.Helpers;
using System.Linq;
using LigaNOS.Data.Repositories;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;



namespace LigaNOS.Data.Entities
{
    public class Seed
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private Random _random;
        private Club club1;
        private Club club2;
        private Club club3;
        private Club club4;
        private IServiceProvider _serviceProvider;
        private object userAdmin;

        public Seed(DataContext context, IUserHelper userHelper, IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userHelper = userHelper ?? throw new ArgumentNullException(nameof(userHelper)); ;
            _random = new Random();
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)); ;
        }


        public async Task SeedAsync()
        {
            

            if (_context == null) throw new InvalidOperationException("DataContext is null.");
            if (_userHelper == null) throw new InvalidOperationException("UserHelper is null.");
            if (_serviceProvider == null) throw new InvalidOperationException("ServiceProvider is null.");
            if (_context.Clubs == null) throw new InvalidOperationException("Clubs collection is null.");
            await _context.Database.EnsureCreatedAsync();

            

            await EnsureRoleExistsAsync("Admin");
            await EnsureRoleExistsAsync("Emplo");
            await EnsureRoleExistsAsync("Club");


            var userAdmin = await CreateUserAndAssignRoleAsync("miguens.rp@gmail.com", "Rita", "Miguens", "Admin", "123456");
            var userEmplo = await CreateUserAndAssignRoleAsync("miguel@yopmail.com", "Miguel", "Miguens", "Emplo", "123456");
            var userClub = await CreateUserAndAssignRoleAsync("maria@yopmail.com", "Maria", "Miguens", "Club", "123456");
            var clubRepository = _serviceProvider.GetRequiredService<IClubRepository>();


            if (!_context.Clubs.Any())
            {
               /* var club1 = new Club { Name = "Sport Lisboa e Benfica", Coach = "Jorge Jesus", Stadium = "Estádio do Benfica", User = userAdmin };
                var club2 = new Club { Name = "Ericeirense", Coach = "Jorge Deus", Stadium = "Estádio do Ericeira", User = userAdmin };
                var club3 = new Club { Name = "FC Porto", Coach = "Vitor Bruno", Stadium = "Estádio do Dragão", User = userAdmin };
                var club4 = new Club { Name = "Estrela da Amadora", Coach = "Bernardo Cruz", Stadium = "Estádio da Amadora", User = userAdmin };*/
                AddClub("Sport Lisboa e Benfica", "Oitxenxa y ocho", "Estádio do Benfica", userAdmin);
                AddClub("Ericeirense", "Jorge Deus", "Estádio do Ericeira", userAdmin);
                AddClub("FC Porto", "Vitor Bruno", "Estádio do Dragão", userAdmin);
                AddClub("Estrela da Amadora", "Bernardo Cruz", "Estádio da Amadora", userAdmin);
              //  _context.Clubs.AddRange(club1, club2, club3, club4);
                await _context.SaveChangesAsync();

                var clubs = _context.Clubs.ToList();
                if (clubs.Any(c => c.Id == 0))
                    throw new InvalidOperationException("One or more clubs were not assigned an ID.");



                // Download images from GitHub and save them
                await SaveImageFromUrlAsync(clubRepository, club1.Id, "https://github.com/luukhopman/football-logos/blob/master/logos/Portugal%20-%20Liga%20Portugal/SL%20Benfica.png?raw=true");
                await SaveImageFromUrlAsync(clubRepository, club2.Id, "https://www.wikisporting.com/images/a/a9/GDU_Ericeirense_R%C3%A2guebi.png");
                await SaveImageFromUrlAsync(clubRepository, club3.Id, "https://github.com/luukhopman/football-logos/blob/master/logos/Portugal%20-%20Liga%20Portugal/FC%20Porto.png?raw=true");
                await SaveImageFromUrlAsync(clubRepository, club4.Id, "https://github.com/luukhopman/football-logos/blob/master/logos/Portugal%20-%20Liga%20Portugal/CF%20Estrela%20Amadora.png?raw=true");




                await _context.SaveChangesAsync();
                clubs = _context.Clubs.ToList();

                if (clubs.Count < 2)
                {
                    throw new InvalidOperationException("Not enough clubs to create matches.");
                }
                if (clubs.Count < 2)
                {
                    throw new InvalidOperationException("Not enough clubs to create matches.");
                }


                if (!_context.Players.Any())
                {
                    foreach (var club in clubs)
                    {
                        AddPlayer(club, userAdmin);
                        AddPlayer(club, userAdmin);
                    }
                    await _context.SaveChangesAsync();
                }


                if (!_context.Players.Any())
                {
                    foreach (var club in clubs)
                    {
                        AddPlayer(club, userAdmin);
                        AddPlayer(club, userAdmin);
                    }
                    await _context.SaveChangesAsync();
                }

            }

            static async Task SaveImageFromUrlAsync(IClubRepository clubRepository, int clubId, string imageUrl)
            {
                byte[] imageData;
                string imageType;
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        // Download the image data
                        imageData = await client.GetByteArrayAsync(imageUrl);

                        // Determine the MIME type from the file extension
                        imageType = "image/" + Path.GetExtension(imageUrl).TrimStart('.').ToLower();

                        // Save the image using the repository method
                       
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error downloading image for ClubId {clubId}: {ex.Message}");
                    imageData = File.ReadAllBytes("wwwroot/images/noimage.png"); // Path to your default image
                    imageType = "image/png"; // Adjust MIME type based on the default image
                }
                try
                {
                    await clubRepository.SaveImageAsync(clubId, imageData, imageType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving image for ClubId {clubId}: {ex.Message}");
                }
            }
            


            if (!_context.Matches.Any())
            {
                AddMatch(userAdmin);
                await _context.SaveChangesAsync();
            }

            if (!_context.Employees.Any())
            {
                AddEmployee("Miguel Miguens", userEmplo);
                AddEmployee("Maria Miguens", userEmplo);
                AddEmployee("Francisco Miguens", userEmplo);
                await _context.SaveChangesAsync();
            }
        }

        private void AddEmployee(string name, User userEmplo)
        {
            _context.Employees.Add(new Employee
            {
                Name = name,
                Address = GenerateRandomAddress(),
                Phone = GenerateRandomNumbers(9),
                Email = name.Replace(" ", "_") + "@cinel.pt",
                Role = GenerateRandomRole(),
                User = userEmplo,
            });
        }

        private string GenerateRandomAddress()
        {
            string[] city = { "Lisboa", "Coimbra", "Aveiro" };
            string[] country = { "Portugal" };

            return $"{city[_random.Next(city.Length)]}, {country[_random.Next(country.Length)]}";

        }

        private string GenerateRandomRole()
        {
            string[] names = { "Admin", "Clubs", "Emplo" };
            string roleName = names[_random.Next(names.Length)];

            return roleName;
        }
        private string GenerateRandomNumbers(int value)
        {
            string phoneNumber = "";
            for (int i = 0; i < value; i++)
            {
                phoneNumber += _random.Next(10).ToString();
            }
            return phoneNumber;
        }

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            var roleExists = await _userHelper.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _userHelper.CheckRoleAsync(roleName);
            }
        }

        private async Task<User> CreateUserAndAssignRoleAsync(string email, string firstName, string lastName, string role, string password)
        {

            var user = await _userHelper.GetUserByEmailAsync(email);
            
            if (user == null)
            {
                user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    UserName = email
                };

                var result = await _userHelper.AddUserAsync(user, password);
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException($"Could not create the {role} user.");
                }
            }

            if (!await _userHelper.IsUserInRoleAsync(user, role))
            {
                await _userHelper.AddUserToRoleAsync(user, role);
            }

            return user;
        }

        public async Task AddClub(string name, string coach, string stadium, User user)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Club name cannot be null or empty.");
            var club = new Club
            {
                Name = name,
                Coach = coach,
                Stadium = stadium,
                User = user

            };

            //Assign the club to the private variables
            if (club1 == null)
                club1 = club;
            else if (club2 == null)
                club2 = club;
            else if (club3 == null)
                club3 = club;
            else if (club4 == null)
                club4 = club;
            _context.Clubs.Add(club);
            if (!_context.Clubs.Any())
            {
                AddClub("Sport Lisboa e Benfica", "Jorge Jesus", "Estádio do Benfica", userAdmin);
                AddClub("Ericeirense", "Jorge Deus", "Estádio do Ericeira", userAdmin);
                AddClub("FC Porto", "Sérgio Conceição", "Estádio do Dragão", userAdmin);
                AddClub("Estrela da Amadora", "Bernardo Cruz", "Estádio da Amadora", userAdmin);

                // Save changes to assign IDs
                await _context.SaveChangesAsync();

                Console.WriteLine($"Adding club: {name} - Coach: {coach} - Stadium: {stadium}");
                if (club1 == null || club2 == null || club3 == null || club4 == null)
                {
                    throw new InvalidOperationException("One or more club objects are null.");
                }
            }
        }

        private void AddClub(string v1, string v2, string v3, object userAdmin)
        {
            throw new NotImplementedException();
        }

        private void AddPlayer(Club club, User user)
        {
            _context.Players.Add(new Player
            {
                Name = GenerateRandomPlayerName(),
                DateOfBirth = GenerateRandomDateOfBirth(),
                Position = GenerateRandomPosition(),
                ClubId = club.Id,
                User = user
            });
        }

        private void AddMatch(User user)
        {
            /*var clubs = _context.Clubs.ToList();
            if (clubs.Count < 2)
            {
                throw new InvalidOperationException("Not enough clubs to create a match.");
            }

            var homeClub = clubs[_random.Next(clubs.Count)];
            Club awayClub;

            do
            {
                awayClub = clubs[_random.Next(clubs.Count)];
            } while (awayClub.Id == homeClub.Id);

            _context.Matches.Add(new Match
            {
                MatchDay = GenerateRandomMatchDay(),
                MatchTime = GenerateRandomMatchTime(),
                HomeClub = homeClub,
                AwayClub = awayClub,
                Stadium = homeClub.Stadium,
                User = user*/
            
                var clubs = _context.Clubs.ToList();
                if (clubs.Count < 2)
                {
                    throw new InvalidOperationException("Not enough clubs to create a match.");
                }

            var homeClub = clubs[_random.Next(clubs.Count)];
            Club awayClub;
            do
            {
                awayClub = clubs[_random.Next(clubs.Count)];
            } while (awayClub.Id == homeClub.Id); // Ensure they are not the same club

            _context.Matches.Add(new Match
                {
                    MatchDay = GenerateRandomMatchDay(),
                    MatchTime = GenerateRandomMatchTime(),
                    HomeClubId = homeClub.Id,
                    AwayClubId = awayClub.Id,
                    Stadium = homeClub.Stadium,
                    User = user
                });
               

        
            Console.WriteLine($"HomeClub ID: {homeClub.Id}, AwayClub ID: {awayClub.Id}");

        }

        private DateTime GenerateRandomDateOfBirth()
        {
            int daysToSubtract = _random.Next(18 * 365, 40 * 365);
            return DateTime.Today.AddDays(-daysToSubtract);
        }

        private string GenerateRandomPlayerName()
        {
            string[] playerNames = { "DiMaria", "Pepe", "CR7", "Moreira", "Mantorras", "Figo" };
            return playerNames[_random.Next(playerNames.Length)];
        }

        private string GenerateRandomPosition()
        {
            string[] positions = { "Forward", "Midfielder", "Defender", "Goalkeeper" };
            return positions[_random.Next(positions.Length)];
        }

        private string GenerateRandomMatchTime()
        {
            string[] matchTimes = { "19:15", "19:30", "19:45", "20:00", "20:15", "20:30", "20:45", "21:00", "21:15" };
            return matchTimes[_random.Next(matchTimes.Length)];
        }

        private DateTime GenerateRandomMatchDay()
        {
            int daysToAdd = _random.Next(0, 31);
            return DateTime.Today.AddDays(daysToAdd);
        }
    }
}
   
 
