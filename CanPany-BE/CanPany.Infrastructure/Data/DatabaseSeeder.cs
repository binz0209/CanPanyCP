using CanPany.Domain.Entities;
using CanPany.Domain.Enums;
using CanPany.Infrastructure.Security.Hashing;
using MongoDB.Driver;
using BCrypt.Net;
using Microsoft.Extensions.Options;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Infrastructure.Data;

/// <summary>
/// Database seeder for initial data
/// </summary>
public class DatabaseSeeder
{
    private readonly MongoDbContext _context;
    private readonly HashService _hashService;

    public DatabaseSeeder(MongoDbContext context)
    {
        _context = context;
        _hashService = new HashService();
    }

    /// <summary>
    /// Create seeder with connection string directly (for console app)
    /// </summary>
    public static DatabaseSeeder Create(string connectionString, string databaseName)
    {
        var options = Options.Create(new MongoOptions
        {
            ConnectionString = connectionString,
            DatabaseName = databaseName
        });
        var context = new MongoDbContext(options);
        return new DatabaseSeeder(context);
    }

    public async Task SeedAsync()
    {
        // Seed Categories
        await SeedCategoriesAsync();

        // Seed Skills
        await SeedSkillsAsync();

        // Seed Users
        await SeedUsersAsync();

        // Seed Companies
        await SeedCompaniesAsync();

        // Seed Premium Packages
        await SeedPremiumPackagesAsync();

        // Seed Banners
        await SeedBannersAsync();

        // Seed User Profiles
        await SeedUserProfilesAsync();

        // Seed User Settings
        await SeedUserSettingsAsync();

        // Seed Wallets
        await SeedWalletsAsync();

        // Seed Jobs
        await SeedJobsAsync();

        // Seed Projects
        await SeedProjectsAsync();

        // Seed CVs
        await SeedCVsAsync();

        // Seed Applications
        await SeedApplicationsAsync();

        // Seed Messages
        await SeedMessagesAsync();

        // Seed Notifications
        await SeedNotificationsAsync();

        // Seed Payments
        await SeedPaymentsAsync();

        // Seed Wallet Transactions
        await SeedWalletTransactionsAsync();

        // Seed Job Bookmarks
        await SeedJobBookmarksAsync();

        // Seed Project Skills
        await SeedProjectSkillsAsync();

        // Seed Reviews
        await SeedReviewsAsync();

        // Seed Audit Logs
        await SeedAuditLogsAsync();

        // Seed CV Analyses
        await SeedCVAnalysesAsync();

        // Seed Candidate Alerts
        await SeedCandidateAlertsAsync();

        // Seed Contracts
        await SeedContractsAsync();

        // Seed Filter Presets
        await SeedFilterPresetsAsync();

        // Seed Job Alerts
        await SeedJobAlertsAsync();

        // Seed Proposals
        await SeedProposalsAsync();

        // Seed Reports
        await SeedReportsAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        var categoriesCollection = _context.Categories;
        var existingCount = await categoriesCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0)
        {
            return;
        }

        var categories = new List<Category>
        {
            new Category { Name = "Web Development", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Mobile Development", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Data Science", CreatedAt = DateTime.UtcNow },
            new Category { Name = "DevOps", CreatedAt = DateTime.UtcNow },
            new Category { Name = "UI/UX Design", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Backend Development", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Frontend Development", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Full Stack Development", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Machine Learning", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Cloud Computing", CreatedAt = DateTime.UtcNow }
        };

        await categoriesCollection.InsertManyAsync(categories);
    }

    private async Task SeedSkillsAsync()
    {
        var skillsCollection = _context.Skills;
        var categoriesCollection = _context.Categories;
        var existingCount = await skillsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0)
        {
            return;
        }

        var categories = await categoriesCollection.Find(_ => true).ToListAsync();
        var webDevCategory = categories.FirstOrDefault(c => c.Name == "Web Development");
        var mobileCategory = categories.FirstOrDefault(c => c.Name == "Mobile Development");
        var dataScienceCategory = categories.FirstOrDefault(c => c.Name == "Data Science");
        var devOpsCategory = categories.FirstOrDefault(c => c.Name == "DevOps");
        var uiuxCategory = categories.FirstOrDefault(c => c.Name == "UI/UX Design");
        var backendCategory = categories.FirstOrDefault(c => c.Name == "Backend Development");
        var frontendCategory = categories.FirstOrDefault(c => c.Name == "Frontend Development");
        var mlCategory = categories.FirstOrDefault(c => c.Name == "Machine Learning");

        var skills = new List<Skill>
        {
            // Web Development
            new Skill { Name = "React", CategoryId = webDevCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Vue.js", CategoryId = webDevCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Angular", CategoryId = webDevCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Next.js", CategoryId = webDevCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Node.js", CategoryId = webDevCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Express.js", CategoryId = webDevCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "TypeScript", CategoryId = webDevCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "JavaScript", CategoryId = webDevCategory?.Id, CreatedAt = DateTime.UtcNow },

            // Backend Development
            new Skill { Name = "C#", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = ".NET", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "ASP.NET Core", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Java", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Spring Boot", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Python", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Django", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Flask", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "PHP", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Laravel", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Go", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Rust", CategoryId = backendCategory?.Id, CreatedAt = DateTime.UtcNow },

            // Frontend Development
            new Skill { Name = "HTML5", CategoryId = frontendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "CSS3", CategoryId = frontendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "SASS/SCSS", CategoryId = frontendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Tailwind CSS", CategoryId = frontendCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Bootstrap", CategoryId = frontendCategory?.Id, CreatedAt = DateTime.UtcNow },

            // Mobile Development
            new Skill { Name = "React Native", CategoryId = mobileCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Flutter", CategoryId = mobileCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Swift", CategoryId = mobileCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Kotlin", CategoryId = mobileCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "iOS Development", CategoryId = mobileCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Android Development", CategoryId = mobileCategory?.Id, CreatedAt = DateTime.UtcNow },

            // Data Science
            new Skill { Name = "Pandas", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "NumPy", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "TensorFlow", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "PyTorch", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Scikit-learn", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "R", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "SQL", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "MongoDB", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "PostgreSQL", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "MySQL", CategoryId = dataScienceCategory?.Id, CreatedAt = DateTime.UtcNow },

            // DevOps
            new Skill { Name = "Docker", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Kubernetes", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "AWS", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Azure", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "GCP", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "CI/CD", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Git", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Jenkins", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "GitLab CI", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "GitHub Actions", CategoryId = devOpsCategory?.Id, CreatedAt = DateTime.UtcNow },

            // UI/UX Design
            new Skill { Name = "Figma", CategoryId = uiuxCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Adobe XD", CategoryId = uiuxCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Sketch", CategoryId = uiuxCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Photoshop", CategoryId = uiuxCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Illustrator", CategoryId = uiuxCategory?.Id, CreatedAt = DateTime.UtcNow },

            // Machine Learning
            new Skill { Name = "Deep Learning", CategoryId = mlCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Neural Networks", CategoryId = mlCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "NLP", CategoryId = mlCategory?.Id, CreatedAt = DateTime.UtcNow },
            new Skill { Name = "Computer Vision", CategoryId = mlCategory?.Id, CreatedAt = DateTime.UtcNow }
        };

        await skillsCollection.InsertManyAsync(skills);
    }

    private async Task SeedUsersAsync()
    {
        var usersCollection = _context.Users;
        var existingCount = await usersCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0)
        {
            return;
        }

        var users = new List<User>
        {
            // Admin
            new User
            {
                FullName = "Admin User",
                Email = "admin@canpany.com",
                PasswordHash = _hashService.HashPassword("Admin@123"),
                Role = "Admin",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            },
            // Candidates
            new User
            {
                FullName = "Nguyễn Văn A",
                Email = "candidate1@example.com",
                PasswordHash = _hashService.HashPassword("Candidate@123"),
                Role = "Candidate",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Trần Thị B",
                Email = "candidate2@example.com",
                PasswordHash = _hashService.HashPassword("Candidate@123"),
                Role = "Candidate",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Lê Văn C",
                Email = "candidate3@example.com",
                PasswordHash = _hashService.HashPassword("Candidate@123"),
                Role = "Candidate",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            },
            // Companies
            new User
            {
                FullName = "Tech Solutions Inc",
                Email = "company1@example.com",
                PasswordHash = _hashService.HashPassword("Company@123"),
                Role = "Company",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Digital Innovations Ltd",
                Email = "company2@example.com",
                PasswordHash = _hashService.HashPassword("Company@123"),
                Role = "Company",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Startup Hub Co",
                Email = "company3@example.com",
                PasswordHash = _hashService.HashPassword("Company@123"),
                Role = "Company",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        await usersCollection.InsertManyAsync(users);
    }

    private async Task SeedCompaniesAsync()
    {
        var companiesCollection = _context.Companies;
        var usersCollection = _context.Users;
        var existingCount = await companiesCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0)
        {
            return;
        }

        var companyUsers = await usersCollection.Find(u => u.Role == "Company").ToListAsync();

        if (!companyUsers.Any())
        {
            return;
        }

        var companies = new List<Company>
        {
            new Company
            {
                UserId = companyUsers[0].Id,
                Name = "Tech Solutions Inc",
                Description = "Leading technology solutions provider specializing in web and mobile applications.",
                Website = "https://techsolutions.com",
                Phone = "+84 123 456 789",
                Address = "123 Tech Street, Ho Chi Minh City, Vietnam",
                IsVerified = true,
                VerificationStatus = "Approved",
                VerifiedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new Company
            {
                UserId = companyUsers.Count > 1 ? companyUsers[1].Id : companyUsers[0].Id,
                Name = "Digital Innovations Ltd",
                Description = "Innovative digital solutions for modern businesses. We build cutting-edge software.",
                Website = "https://digitalinnovations.com",
                Phone = "+84 987 654 321",
                Address = "456 Innovation Avenue, Hanoi, Vietnam",
                IsVerified = true,
                VerificationStatus = "Approved",
                VerifiedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new Company
            {
                UserId = companyUsers.Count > 2 ? companyUsers[2].Id : companyUsers[0].Id,
                Name = "Startup Hub Co",
                Description = "Supporting startups with technology and talent solutions.",
                Website = "https://startuphub.com",
                Phone = "+84 555 123 456",
                Address = "789 Startup Boulevard, Da Nang, Vietnam",
                IsVerified = false,
                VerificationStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            }
        };

        await companiesCollection.InsertManyAsync(companies);
    }

    private async Task SeedPremiumPackagesAsync()
    {
        var packagesCollection = _context.PremiumPackages;
        var existingCount = await packagesCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0)
        {
            return;
        }

        var packages = new List<PremiumPackage>
        {
            // Candidate Packages
            new PremiumPackage
            {
                Name = "AI Premium - Basic",
                Description = "Basic AI features for candidates",
                UserType = "Candidate",
                PackageType = "AIPremium",
                Price = 99000, // 99,000 VND
                DurationDays = 30,
                Features = new List<string>
                {
                    "AI CV Analysis",
                    "Job Recommendations",
                    "Priority in Search Results"
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new PremiumPackage
            {
                Name = "AI Premium - Pro",
                Description = "Advanced AI features for candidates",
                UserType = "Candidate",
                PackageType = "AIPremium",
                Price = 199000, // 199,000 VND
                DurationDays = 30,
                Features = new List<string>
                {
                    "AI CV Analysis",
                    "AI CV Generation",
                    "Job Recommendations",
                    "Priority in Search Results",
                    "Unlimited Applications"
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            // Company Packages
            new PremiumPackage
            {
                Name = "Job Posting - Basic",
                Description = "Basic job posting package",
                UserType = "Company",
                PackageType = "JobPosting",
                Price = 299000, // 299,000 VND
                DurationDays = 30,
                Features = new List<string>
                {
                    "5 Job Postings",
                    "Basic Analytics",
                    "Standard Support"
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new PremiumPackage
            {
                Name = "Job Posting - Pro",
                Description = "Advanced job posting package",
                UserType = "Company",
                PackageType = "JobPosting",
                Price = 599000, // 599,000 VND
                DurationDays = 30,
                Features = new List<string>
                {
                    "Unlimited Job Postings",
                    "Advanced Analytics",
                    "Priority Support",
                    "Featured Listings"
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new PremiumPackage
            {
                Name = "AI Screening - Pro",
                Description = "AI-powered candidate screening",
                UserType = "Company",
                PackageType = "AIScreening",
                Price = 999000, // 999,000 VND
                DurationDays = 30,
                Features = new List<string>
                {
                    "AI Candidate Matching",
                    "Automated Screening",
                    "Candidate Ranking",
                    "Advanced Search Filters"
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await packagesCollection.InsertManyAsync(packages);
    }

    private async Task SeedBannersAsync()
    {
        var bannersCollection = _context.Banners;
        var existingCount = await bannersCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0)
        {
            return;
        }

        var banners = new List<Banner>
        {
            new Banner
            {
                Title = "Find Your Dream Job",
                ImageUrl = "https://via.placeholder.com/1200x400/4F46E5/FFFFFF?text=Find+Your+Dream+Job",
                LinkUrl = "/jobs",
                Order = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Banner
            {
                Title = "Hire Top Talent",
                ImageUrl = "https://via.placeholder.com/1200x400/10B981/FFFFFF?text=Hire+Top+Talent",
                LinkUrl = "/candidates",
                Order = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Banner
            {
                Title = "AI-Powered Matching",
                ImageUrl = "https://via.placeholder.com/1200x400/F59E0B/FFFFFF?text=AI-Powered+Matching",
                LinkUrl = "/features",
                Order = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await bannersCollection.InsertManyAsync(banners);
    }

    private async Task SeedUserProfilesAsync()
    {
        var profilesCollection = _context.UserProfiles;
        var usersCollection = _context.Users;
        var skillsCollection = _context.Skills;
        var existingCount = await profilesCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var candidates = await usersCollection.Find(u => u.Role == "Candidate").ToListAsync();
        var skills = await skillsCollection.Find(_ => true).Limit(20).ToListAsync();
        var skillIds = skills.Select(s => s.Id).ToList();

        if (!candidates.Any() || !skillIds.Any()) return;

        var profiles = new List<UserProfile>();
        var titles = new[] { "Senior Full Stack Developer", "Mobile App Developer", "Data Scientist", "UI/UX Designer" };
        var locations = new[] { "Ho Chi Minh City", "Hanoi", "Da Nang", "Can Tho" };

        for (int i = 0; i < candidates.Count && i < titles.Length; i++)
        {
            profiles.Add(new UserProfile
            {
                UserId = candidates[i].Id,
                Bio = $"Experienced {titles[i].ToLower()} with passion for creating innovative solutions.",
                Phone = $"+84 {900 + i}{1000 + i}{1000 + i}",
                Address = locations[i % locations.Length],
                Title = titles[i],
                Location = locations[i % locations.Length],
                HourlyRate = (i + 1) * 500000,
                SkillIds = skillIds.Take(5 + i).ToList(),
                Experience = $"{3 + i} years of professional experience in software development.",
                Education = "Bachelor's degree in Computer Science",
                LinkedInUrl = $"https://linkedin.com/in/candidate{i + 1}",
                GitHubUrl = $"https://github.com/candidate{i + 1}",
                Languages = new List<string> { "Vietnamese", "English" },
                CreatedAt = DateTime.UtcNow
            });
        }

        await profilesCollection.InsertManyAsync(profiles);
    }

    private async Task SeedUserSettingsAsync()
    {
        var settingsCollection = _context.UserSettings;
        var usersCollection = _context.Users;
        var existingCount = await settingsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var users = await usersCollection.Find(_ => true).ToListAsync();
        if (!users.Any()) return;

        var settings = users.Select(user => new UserSettings
        {
            UserId = user.Id,
            NotificationSettings = new NotificationSettings
            {
                EmailNotifications = true,
                MessageNotifications = true,
                NewProjectNotifications = true
            },
            PrivacySettings = new PrivacySettings
            {
                PublicProfile = user.Role != "Admin",
                ShowOnlineStatus = false
            },
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await settingsCollection.InsertManyAsync(settings);
    }

    private async Task SeedWalletsAsync()
    {
        var walletsCollection = _context.Wallets;
        var usersCollection = _context.Users;
        var existingCount = await walletsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var users = await usersCollection.Find(_ => true).ToListAsync();
        if (!users.Any()) return;

        var wallets = users.Select(user => new Wallet
        {
            UserId = user.Id,
            Balance = user.Role == "Company" ? 5000000 : 1000000,
            Currency = "VND",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await walletsCollection.InsertManyAsync(wallets);
    }

    private async Task SeedJobsAsync()
    {
        var jobsCollection = _context.Jobs;
        var companiesCollection = _context.Companies;
        var categoriesCollection = _context.Categories;
        var skillsCollection = _context.Skills;
        var existingCount = await jobsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var companies = await companiesCollection.Find(_ => true).ToListAsync();
        var categories = await categoriesCollection.Find(_ => true).ToListAsync();
        var skills = await skillsCollection.Find(_ => true).Limit(30).ToListAsync();

        if (!companies.Any() || !categories.Any()) return;

        var jobTitles = new[] { "Senior Full Stack Developer", "React Native Mobile Developer", "Python Data Scientist", "UI/UX Designer", "DevOps Engineer", "Backend Developer (.NET)", "Frontend Developer (React)", "Machine Learning Engineer" };
        var jobDescriptions = new[] { "We are looking for an experienced full stack developer to join our team.", "Join our mobile development team to create amazing iOS and Android apps.", "Looking for a data scientist to help us analyze large datasets.", "Creative UI/UX designer needed to design beautiful interfaces.", "DevOps engineer to manage our cloud infrastructure.", "Backend developer with .NET experience.", "Frontend developer skilled in React.", "ML engineer to develop and deploy machine learning models." };

        var jobs = new List<Job>();
        var random = new Random();

        for (int i = 0; i < Math.Min(jobTitles.Length, companies.Count * 2); i++)
        {
            var company = companies[i % companies.Count];
            var category = categories[random.Next(categories.Count)];
            var selectedSkills = skills.OrderBy(x => random.Next()).Take(3 + random.Next(5)).Select(s => s.Id).ToList();

            jobs.Add(new Job
            {
                CompanyId = company.Id,
                Title = jobTitles[i % jobTitles.Length],
                Description = jobDescriptions[i % jobDescriptions.Length],
                CategoryId = category.Id,
                SkillIds = selectedSkills,
                BudgetType = random.Next(2) == 0 ? "Fixed" : "Hourly",
                BudgetAmount = random.Next(2) == 0 ? (decimal)(10 + random.Next(40)) * 1000000 : (decimal)(500 + random.Next(1500)) * 1000,
                Level = new[] { "Junior", "Mid", "Senior", "Expert" }[random.Next(4)],
                Location = new[] { "Ho Chi Minh City", "Hanoi", "Da Nang", "Remote" }[random.Next(4)],
                IsRemote = random.Next(2) == 0,
                Deadline = DateTime.UtcNow.AddDays(30 + random.Next(60)),
                Status = "Open",
                ViewCount = random.Next(100),
                ApplicationCount = random.Next(20),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30))
            });
        }

        await jobsCollection.InsertManyAsync(jobs);
    }

    private async Task SeedProjectsAsync()
    {
        var projectsCollection = _context.Projects;
        var companiesCollection = _context.Companies;
        var categoriesCollection = _context.Categories;
        var skillsCollection = _context.Skills;
        var existingCount = await projectsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var companies = await companiesCollection.Find(_ => true).ToListAsync();
        var categories = await categoriesCollection.Find(_ => true).ToListAsync();
        var skills = await skillsCollection.Find(_ => true).Limit(30).ToListAsync();

        if (!companies.Any() || !categories.Any()) return;

        var projectTitles = new[] { "E-commerce Website Development", "Mobile App for Food Delivery", "Data Analytics Dashboard", "AI Chatbot Integration", "Cloud Migration Project" };
        var projects = new List<Project>();
        var random = new Random();

        for (int i = 0; i < Math.Min(projectTitles.Length, companies.Count * 2); i++)
        {
            var company = companies[i % companies.Count];
            var category = categories[random.Next(categories.Count)];
            var selectedSkills = skills.OrderBy(x => random.Next()).Take(3 + random.Next(5)).Select(s => s.Id).ToList();

            projects.Add(new Project
            {
                OwnerId = company.UserId,
                Title = projectTitles[i % projectTitles.Length],
                Description = $"We need an experienced developer to help us build {projectTitles[i % projectTitles.Length].ToLower()}.",
                CategoryId = category.Id,
                SkillIds = selectedSkills,
                BudgetType = random.Next(2) == 0 ? "Fixed" : "Hourly",
                BudgetAmount = random.Next(2) == 0 ? (decimal)(5 + random.Next(20)) * 1000000 : (decimal)(300 + random.Next(700)) * 1000,
                Deadline = DateTime.UtcNow.AddDays(60 + random.Next(90)),
                Status = new[] { "Open", "InProgress", "Completed" }[random.Next(3)],
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(60))
            });
        }

        await projectsCollection.InsertManyAsync(projects);
    }

    private async Task SeedCVsAsync()
    {
        var cvsCollection = _context.CVs;
        var usersCollection = _context.Users;
        var existingCount = await cvsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var candidates = await usersCollection.Find(u => u.Role == "Candidate").ToListAsync();
        if (!candidates.Any()) return;

        var cvs = candidates.Select((candidate, index) => new CV
        {
            UserId = candidate.Id,
            FileName = $"CV_{candidate.FullName.Replace(" ", "_")}.pdf",
            FileUrl = $"https://cloudinary.com/cvs/cv_{index + 1}.pdf",
            FileSize = 500000 + index * 100000,
            MimeType = "application/pdf",
            IsDefault = index == 0,
            ExtractedSkills = new List<string> { "React", "Node.js", "TypeScript", "MongoDB" },
            ExtractedContent = $"CV content for {candidate.FullName}",
            AtsScore = 75 + index * 5,
            CreatedAt = DateTime.UtcNow.AddDays(-index * 10)
        }).ToList();

        await cvsCollection.InsertManyAsync(cvs);
    }

    private async Task SeedApplicationsAsync()
    {
        var applicationsCollection = _context.Applications;
        var jobsCollection = _context.Jobs;
        var usersCollection = _context.Users;
        var cvsCollection = _context.CVs;
        var existingCount = await applicationsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var jobs = await jobsCollection.Find(_ => true).Limit(5).ToListAsync();
        var candidates = await usersCollection.Find(u => u.Role == "Candidate").ToListAsync();
        var cvs = await cvsCollection.Find(_ => true).ToListAsync();

        if (!jobs.Any() || !candidates.Any()) return;

        var applications = new List<DomainApplication>();
        var random = new Random();
        var statuses = new[] { "Pending", "Accepted", "Rejected" };

        foreach (var job in jobs)
        {
            var candidateCount = random.Next(1, Math.Min(4, candidates.Count + 1));
            var selectedCandidates = candidates.OrderBy(x => random.Next()).Take(candidateCount).ToList();

            foreach (var candidate in selectedCandidates)
            {
                var cv = cvs.FirstOrDefault(c => c.UserId == candidate.Id);
                applications.Add(new DomainApplication
                {
                    JobId = job.Id,
                    CandidateId = candidate.Id,
                    CVId = cv?.Id,
                    CoverLetter = $"I am very interested in the {job.Title} position.",
                    ExpectedSalary = job.BudgetAmount,
                    Status = statuses[random.Next(statuses.Length)],
                    MatchScore = 60 + random.Next(40),
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(10))
                });
            }
        }

        await applicationsCollection.InsertManyAsync(applications);
    }

    private async Task SeedMessagesAsync()
    {
        var messagesCollection = _context.Messages;
        var usersCollection = _context.Users;
        var projectsCollection = _context.Projects;
        var existingCount = await messagesCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var companies = await usersCollection.Find(u => u.Role == "Company").ToListAsync();
        var candidates = await usersCollection.Find(u => u.Role == "Candidate").ToListAsync();
        var projects = await projectsCollection.Find(_ => true).Limit(3).ToListAsync();

        if (!companies.Any() || !candidates.Any()) return;

        var messages = new List<Message>();
        var messageTexts = new[] { "Hello, I'm interested in your project.", "Thank you for your interest.", "I have experience with similar projects.", "That sounds great! When can we schedule a call?", "I've reviewed your proposal. It looks good." };
        var random = new Random();

        for (int i = 0; i < 10; i++)
        {
            var company = companies[random.Next(companies.Count)];
            var candidate = candidates[random.Next(candidates.Count)];
            var project = projects.Any() ? projects[random.Next(projects.Count)] : null;
            var conversationKey = $"{company.Id}_{candidate.Id}";

            messages.Add(new Message
            {
                ConversationKey = conversationKey,
                ProjectId = project?.Id,
                SenderId = i % 2 == 0 ? company.Id : candidate.Id,
                ReceiverId = i % 2 == 0 ? candidate.Id : company.Id,
                Text = messageTexts[random.Next(messageTexts.Length)],
                IsRead = i < 7,
                CreatedAt = DateTime.UtcNow.AddHours(-i * 2)
            });
        }

        await messagesCollection.InsertManyAsync(messages);
    }

    private async Task SeedNotificationsAsync()
    {
        var notificationsCollection = _context.Notifications;
        var usersCollection = _context.Users;
        var existingCount = await notificationsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var users = await usersCollection.Find(_ => true).ToListAsync();
        if (!users.Any()) return;

        var notifications = new List<Notification>();
        var notificationTypes = new[] { "NewMessage", "ApplicationAccepted", "ApplicationRejected", "NewJobMatch", "PaymentReceived" };
        var titles = new[] { "New Message", "Application Accepted", "Application Rejected", "New Job Match", "Payment Received" };
        var messages = new[] { "You have a new message", "Congratulations! Your application has been accepted", "Your application has been rejected", "We found a new job that matches your profile", "Your payment has been processed successfully" };
        var random = new Random();

        foreach (var user in users)
        {
            var count = random.Next(3, 8);
            for (int i = 0; i < count; i++)
            {
                var typeIndex = random.Next(notificationTypes.Length);
                notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Type = notificationTypes[typeIndex],
                    Title = titles[typeIndex],
                    Message = messages[typeIndex],
                    IsRead = random.Next(3) == 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(7))
                });
            }
        }

        await notificationsCollection.InsertManyAsync(notifications);
    }

    private async Task SeedPaymentsAsync()
    {
        var paymentsCollection = _context.Payments;
        var usersCollection = _context.Users;
        var walletsCollection = _context.Wallets;
        var existingCount = await paymentsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var users = await usersCollection.Find(_ => true).ToListAsync();
        var wallets = await walletsCollection.Find(_ => true).ToListAsync();

        if (!users.Any()) return;

        var payments = new List<Payment>();
        var random = new Random();
        var statuses = new[] { "Pending", "Paid", "Failed" };
        var purposes = new[] { "TopUp", "Contract" };

        foreach (var user in users.Take(5))
        {
            var wallet = wallets.FirstOrDefault(w => w.UserId == user.Id);
            var status = statuses[random.Next(statuses.Length)];
            
            payments.Add(new Payment
            {
                UserId = user.Id,
                WalletId = wallet?.Id,
                Purpose = purposes[random.Next(purposes.Length)],
                Amount = (random.Next(5, 20) * 100000),
                Currency = "VND",
                Status = status,
                PaidAt = status == "Paid" ? DateTime.UtcNow.AddDays(-random.Next(30)) : null,
                Vnp_TxnRef = $"TXN{DateTime.UtcNow.Ticks % 1000000}",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30))
            });
        }

        await paymentsCollection.InsertManyAsync(payments);
    }

    private async Task SeedWalletTransactionsAsync()
    {
        var transactionsCollection = _context.WalletTransactions;
        var walletsCollection = _context.Wallets;
        var paymentsCollection = _context.Payments;
        var existingCount = await transactionsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var wallets = await walletsCollection.Find(_ => true).ToListAsync();
        var payments = await paymentsCollection.Find(_ => true).ToListAsync();

        if (!wallets.Any()) return;

        var transactions = new List<WalletTransaction>();
        var random = new Random();

        foreach (var wallet in wallets.Take(5))
        {
            var payment = payments.FirstOrDefault(p => p.WalletId == wallet.Id);
            var balance = wallet.Balance;
            
            transactions.Add(new WalletTransaction
            {
                WalletId = wallet.Id,
                UserId = wallet.UserId,
                PaymentId = payment?.Id,
                Type = "TopUp",
                Amount = payment?.Amount ?? 1000000,
                BalanceAfter = balance,
                Note = "Initial top up",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30))
            });
        }

        await transactionsCollection.InsertManyAsync(transactions);
    }

    private async Task SeedJobBookmarksAsync()
    {
        var bookmarksCollection = _context.JobBookmarks;
        var jobsCollection = _context.Jobs;
        var usersCollection = _context.Users;
        var existingCount = await bookmarksCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var jobs = await jobsCollection.Find(_ => true).Limit(5).ToListAsync();
        var candidates = await usersCollection.Find(u => u.Role == "Candidate").ToListAsync();

        if (!jobs.Any() || !candidates.Any()) return;

        var bookmarks = new List<JobBookmark>();
        var random = new Random();

        foreach (var candidate in candidates)
        {
            var jobCount = random.Next(1, Math.Min(4, jobs.Count + 1));
            var selectedJobs = jobs.OrderBy(x => random.Next()).Take(jobCount).ToList();

            foreach (var job in selectedJobs)
            {
                bookmarks.Add(new JobBookmark
                {
                    UserId = candidate.Id,
                    JobId = job.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(10))
                });
            }
        }

        await bookmarksCollection.InsertManyAsync(bookmarks);
    }

    private async Task SeedProjectSkillsAsync()
    {
        // ProjectSkills are already handled in Project.SkillIds
        // This method is kept for future use if explicit ProjectSkill entities are needed
        await Task.CompletedTask;
    }

    private async Task SeedReviewsAsync()
    {
        var reviewsCollection = _context.Reviews;
        var projectsCollection = _context.Projects;
        var usersCollection = _context.Users;
        var existingCount = await reviewsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var projects = await projectsCollection.Find(p => p.Status == "Completed").ToListAsync();
        var companies = await usersCollection.Find(u => u.Role == "Company").ToListAsync();
        var candidates = await usersCollection.Find(u => u.Role == "Candidate").ToListAsync();

        if (!projects.Any() || !companies.Any() || !candidates.Any()) return;

        var reviews = new List<Review>();
        var random = new Random();
        var comments = new[] { "Great work! Very professional and delivered on time.", "Excellent communication and quality of work.", "Good developer, would work with again.", "Met all requirements and exceeded expectations.", "Professional and reliable. Highly recommended." };

        foreach (var project in projects.Take(3))
        {
            var companyUser = await usersCollection.Find(u => u.Id == project.OwnerId).FirstOrDefaultAsync();
            if (companyUser == null) continue;

            var company = await _context.Companies.Find(c => c.UserId == companyUser.Id).FirstOrDefaultAsync();
            var candidate = candidates[random.Next(candidates.Count)];

            if (company != null)
            {
                reviews.Add(new Review
                {
                    ProjectId = project.Id,
                    ReviewerId = candidate.Id,
                    RevieweeId = companyUser.Id,
                    Rating = 4 + random.Next(2),
                    Comment = comments[random.Next(comments.Length)],
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30))
                });
            }
        }

        if (reviews.Any())
        {
            await reviewsCollection.InsertManyAsync(reviews);
        }
    }
    private async Task SeedAuditLogsAsync()
    {
        var auditLogsCollection = _context.AuditLogs;
        var existingCount = await auditLogsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var users = await _context.Users.Find(_ => true).Limit(5).ToListAsync();
        if (!users.Any()) return;

        var auditLogs = new List<AuditLog>();
        var actions = new[] { "Login", "Logout", "CreateJob", "UpdateProfile", "ViewCandidate" };
        var random = new Random();

        foreach (var user in users)
        {
            for (int i = 0; i < 3; i++)
            {
                auditLogs.Add(new AuditLog
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    Action = actions[random.Next(actions.Length)],
                    EntityType = "User",
                    EntityId = user.Id,
                    Endpoint = "/api/v1/auth/login",
                    HttpMethod = "POST",
                    RequestPath = "/api/v1/auth/login",
                    IpAddress = "127.0.0.1",
                    UserAgent = "Mozilla/5.0",
                    ResponseStatusCode = 200,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30))
                });
            }
        }

        await auditLogsCollection.InsertManyAsync(auditLogs);
    }

    private async Task SeedCVAnalysesAsync()
    {
        var analysesCollection = _context.CVAnalyses;
        var existingCount = await analysesCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var cvs = await _context.CVs.Find(_ => true).Limit(10).ToListAsync();
        if (!cvs.Any()) return;

        var analyses = cvs.Select(cv => new CVAnalysis
        {
            CVId = cv.Id,
            CandidateId = cv.UserId,
            // CV.AtsScore is decimal? while CVAnalysis.ATSScore is double
            ATSScore = (double)(cv.AtsScore ?? 0m),
            MissingKeywords = new List<string> { "Kubernetes", "Microservices" },
            ImprovementSuggestions = new List<string> { "Add more quantifiable achievements", "Highlight leadership details" },
            AnalyzedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await analysesCollection.InsertManyAsync(analyses);
    }

    private async Task SeedCandidateAlertsAsync()
    {
        var alertsCollection = _context.CandidateAlerts;
        var existingCount = await alertsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var companies = await _context.Companies.Find(_ => true).Limit(5).ToListAsync();
        if (!companies.Any()) return;

        var alerts = companies.Select(company => new CandidateAlert
        {
            CompanyId = company.Id,
            Name = "Senior Developers Alert",
            SkillIds = new List<string>(), // Simplified for seeding
            Location = "Ho Chi Minh City",
            MinExperience = 3,
            MaxExperience = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await alertsCollection.InsertManyAsync(alerts);
    }

    private async Task SeedContractsAsync()
    {
        var contractsCollection = _context.Contracts;
        var existingCount = await contractsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var projects = await _context.Projects.Find(p => p.Status == "InProgress" || p.Status == "Completed").ToListAsync();
        var candidates = await _context.Users.Find(u => u.Role == "Candidate").ToListAsync();

        if (!projects.Any() || !candidates.Any()) return;

        var contracts = new List<Contract>();
        var random = new Random();

        foreach (var project in projects.Take(5))
        {
            var candidate = candidates[random.Next(candidates.Count)];
            contracts.Add(new Contract
            {
                ProjectId = project.Id,
                ClientId = project.OwnerId, // Note: OwnerId in Project is likely UserId of Company, verifying... yes checked User seeding
                FreelancerId = candidate.Id,
                // Project.BudgetAmount is decimal? while Contract.AgreedAmount is decimal
                AgreedAmount = project.BudgetAmount ?? 0m,
                Status = project.Status == "Completed" ? "Completed" : "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            });
        }

        await contractsCollection.InsertManyAsync(contracts);
    }

    private async Task SeedFilterPresetsAsync()
    {
        var presetsCollection = _context.FilterPresets;
        var existingCount = await presetsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var users = await _context.Users.Find(_ => true).Limit(5).ToListAsync();
        if (!users.Any()) return;

        var presets = users.Select(user => new FilterPreset
        {
            UserId = user.Id,
            Name = "My Default Search",
            FilterType = user.Role == "Candidate" ? FilterType.JobSearch : FilterType.CandidateSearch,
            Filters = new Dictionary<string, object> { { "location", "Ho Chi Minh City" } },
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await presetsCollection.InsertManyAsync(presets);
    }

    private async Task SeedJobAlertsAsync()
    {
        var alertsCollection = _context.JobAlerts;
        var existingCount = await alertsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var candidates = await _context.Users.Find(u => u.Role == "Candidate").Limit(5).ToListAsync();
        if (!candidates.Any()) return;

        var alerts = candidates.Select(candidate => new JobAlert
        {
            UserId = candidate.Id,
            Name = "Remote React Jobs",
            SkillIds = new List<string>(), // Simplified
            Location = "Remote",
            MinBudget = 1000,
            IsRemote = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await alertsCollection.InsertManyAsync(alerts);
    }

    private async Task SeedProposalsAsync()
    {
        var proposalsCollection = _context.Proposals;
        var existingCount = await proposalsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var projects = await _context.Projects.Find(_ => true).Limit(5).ToListAsync();
        var candidates = await _context.Users.Find(u => u.Role == "Candidate").ToListAsync();

        if (!projects.Any() || !candidates.Any()) return;

        var proposals = new List<Proposal>();
        var random = new Random();

        foreach (var project in projects)
        {
            var candidateCount = random.Next(1, 4);
            for (int i = 0; i < candidateCount; i++)
            {
                var candidate = candidates[random.Next(candidates.Count)];
                proposals.Add(new Proposal
                {
                    ProjectId = project.Id,
                    FreelancerId = candidate.Id,
                    CoverLetter = "I can complete this project on time.",
                    BidAmount = project.BudgetAmount,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(10))
                });
            }
        }

        await proposalsCollection.InsertManyAsync(proposals);
    }

    private async Task SeedReportsAsync()
    {
        var reportsCollection = _context.Reports;
        var existingCount = await reportsCollection.CountDocumentsAsync(_ => true);

        if (existingCount > 0) return;

        var users = await _context.Users.Find(_ => true).ToListAsync();
        if (users.Count < 2) return;

        var reports = new List<Report>();
        var random = new Random();

        for (int i = 0; i < 5; i++)
        {
            var reporter = users[random.Next(users.Count)];
            var reported = users[random.Next(users.Count)];
            
            if (reporter.Id == reported.Id) continue;

            reports.Add(new Report
            {
                ReporterId = reporter.Id,
                ReportedUserId = reported.Id,
                Reason = "Spam",
                Description = "This user is sending spam messages.",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(10))
            });
        }

        await reportsCollection.InsertManyAsync(reports);
    }
}

