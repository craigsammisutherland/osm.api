using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Auxano.Osm.Api;
using Newtonsoft.Json;

namespace Auxano.Osm.TestHarness
{
    internal class Program
    {
        private static IEnumerable<Badge> ListBadges(Manager manager, Section section, TermArray terms, int badgeType)
        {
            var badges = manager.Badge.ListForSectionAsync(section, badgeType, terms.Current).Result;
            foreach (var badge in badges)
            {
                Console.WriteLine("Badge: " + badge.Name);
            }

            var first = badges.FirstOrDefault();
            if (first == null) return badges;

            var progress = manager.Badge.ListProgressForBadgeAsync(section, first, terms.Current).Result;
            foreach (var item in progress)
            {
                Console.WriteLine(item.Member.FamilyName + ", " + item.Member.FirstName + " -> " + (item.IsAwarded ? "Awarded" : string.Empty));
            }

            return badges;
        }

        private static void Main(string[] args)
        {
            try
            {
                Manager manager;
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "settings.json")))
                {
                    Console.WriteLine("Loading settings from file");
                    manager = ReadSettings();
                }
                else
                {
                    Console.WriteLine("Unable to find settings file, prompting");
                    manager = PromptForSettings();
                }

                if (manager != null)
                {
                    Console.WriteLine("Starting...");
                    var groups = manager.Group.LoadForCurrentUserAsync().Result;
                    foreach (var group in groups)
                    {
                        Console.WriteLine("Group: " + group.Name);
                        foreach (var section in group.Sections)
                        {
                            Console.WriteLine("Section: " + section.Name);
                            var terms = manager.Term.ListForSectionAsync(section).Result;
                            foreach (var term in terms)
                            {
                                var isActive = terms.Current.Id == term.Id;
                                Console.WriteLine("Term: "
                                    + term.Name
                                    + " [" + term.StartDate.ToShortDateString() + "->" + term.EndDate.ToShortDateString() + "]"
                                    + (isActive ? " ** ACTIVE TERM **" : string.Empty));
                            }

                            var members = manager.Member.ListForSectionAsync(section, terms.Current).Result;
                            foreach (var member in members)
                            {
                                Console.WriteLine("Member: " + member.FamilyName + ", " + member.FirstName);
                            }

                            ListBadges(manager, section, terms, 1);
                            ListBadges(manager, section, terms, 2);
                        }
                    }
                    Console.WriteLine("...finished");
                }
            }
            catch (AggregateException error)
            {
                foreach (var inner in error.Flatten().InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + inner.Message);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("ERROR: " + error.Message);
            }

            if (Debugger.IsAttached) Console.ReadKey(true);
        }

        private static string PromptForInput(string prompt)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            return input;
        }

        private static Manager PromptForSettings()
        {
            var apiId = PromptForInput("What is API ID? ");
            var token = PromptForInput("What is your token? ");
            var user = PromptForInput("What is your email address? ");
            var password = PromptForInput("What is your password? ");

            var manager = new Manager(apiId, token);
            Console.WriteLine("Authorising...");
            if (manager.Authorise(user, password).Result)
            {
                Console.WriteLine("...authorise successful!");
                var settings = new Settings
                {
                    ApiId = apiId,
                    Token = token,
                    UserId = manager.Authorisation.UserId,
                    Secret = manager.Authorisation.Secret
                };
                var json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "settings.json"), json);
            }
            else
            {
                Console.WriteLine("...authorise failed!");
                return null;
            }

            return manager;
        }

        private static Manager ReadSettings()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "settings.json"));
            var setings = JsonConvert.DeserializeObject<Settings>(json);
            var manager = new Manager(setings.ApiId, setings.Token, setings.UserId, setings.Secret);
            return manager;
        }
    }
}