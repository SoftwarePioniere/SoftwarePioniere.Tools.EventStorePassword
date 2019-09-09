using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.UserManagement;
using McMaster.Extensions.CommandLineUtils;

namespace SoftwarePioniere.Tools.EventStorePassword
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "eventstore-password",
                Description = "Tool to set the Event Store Password",
                LongVersionGetter = () => typeof(Program).Assembly.GetName().Version.ToString()
            };

            app.HelpOption("-h|--help", inherited: true);
            app.Command("test",
                cmd =>
                {
                    cmd.FullName = "Test Password";
                    cmd.Description = "Test a Login with Password";

                    var argLogin = cmd.Argument<string>("login", "Login Name");
                    argLogin.IsRequired(errorMessage: "login empty");

                    var argPassword = cmd.Argument<string>("password", "Password to Test");
                    argPassword.IsRequired(errorMessage: "password empty");

                    var optionUrl = cmd.Option("-u|--url", "EventStore Http URL [localhost]", CommandOptionType.SingleValue);
                    var optionPort = cmd.Option<int>("-p|--port", "Port [2113]", CommandOptionType.SingleValue);

                    cmd.OnExecute(async () =>
                    {
                        var url = optionUrl.HasValue() ? optionUrl.Value() : "localhost";
                        var port = optionPort.HasValue() ? optionPort.ParsedValue : 2113;
                        try
                        {
                            await TestPasswordAsync(url, port, argLogin.Value, argPassword.Value);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            return 1;
                        }
                    });
                });

            app.Command("change", cmd =>
            {
                cmd.FullName = "Change the Password";
                cmd.Description = "Change Login Password";

                var argLogin = cmd.Argument<string>("login", "Login Name");
                argLogin.IsRequired(errorMessage: "login empty");

                var argOldPassword = cmd.Argument<string>("old_password", "Old Password");
                argOldPassword.IsRequired(errorMessage: "old_password empty");

                var argNewPassword = cmd.Argument<string>("new_password", "New Password");
                argNewPassword.IsRequired(errorMessage: "new_password empty");

                var optionUrl = cmd.Option("-u|--url", "EventStore Http URL [localhost]", CommandOptionType.SingleValue);
                var optionPort = cmd.Option<int>("-p|--port", "Port [2113]", CommandOptionType.SingleValue);

                cmd.OnExecute(async () =>
                {
                    var url = optionUrl.HasValue() ? optionUrl.Value() : "localhost";
                    var port = optionPort.HasValue() ? optionPort.ParsedValue : 2113;

                    try
                    {
                        var test = await TestPasswordAsync(url, port, argLogin.Value, argNewPassword.Value);

                        if (!test)
                        {
                            await ChangePasswordAsync(url,
                                port,
                                argLogin.Value,
                                argOldPassword.Value,
                                argNewPassword.Value);
                        }

                        return 0;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return 1;
                    }
                });
            });


            var optionVersion = app.Option("-v|--version", "Shows the Version", CommandOptionType.NoValue);


            app.OnExecute(() =>
            {
                if (optionVersion.HasValue())
                {
                    app.ShowVersion();
                    return 0;
                }

                app.ShowHelp();
                return 1;

            });

            return app.Execute(args);
        }


        private static async Task<bool> TestPasswordAsync(string url, int port, string login, string password)
        {
            var man = CreateUsersManager(url, port);

            Console.WriteLine("Test Password for Login {0}", login);

            try
            {
                Console.WriteLine("Trying Password");
                await man.GetUserAsync(login, new UserCredentials(login, password));
                Console.WriteLine("Password works");

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot access with Password");
                var e1 = e.InnerException ?? e;
                Console.WriteLine(e1.Message);
            }

            return false;
        }

        private static UsersManager CreateUsersManager(string url, int port)
        {
            if (IPAddress.TryParse(url, out var ip) == false)
            {
                Console.WriteLine("Trying Dns GetHostEntry {0}", url);
                var host = Dns.GetHostEntry(url);
                ip = host.AddressList.First(x => x.ToString() != "::1");
            }
            
            Console.WriteLine("URL / IP: {0} {1}", url, ip);

            var man = new UsersManager(new ConsoleLogger(),
                new IPEndPoint(ip, port),
                TimeSpan.FromSeconds(1));

            return man;
        }

        private static async Task ChangePasswordAsync(string url, int port, string login, string oldPassword,
            string newPassword)
        {
            var man = CreateUsersManager(url, port);

            Console.WriteLine("Change Password for Login {0}", login);

            Console.WriteLine("Try change password");
            await man.ChangePasswordAsync(login, oldPassword, newPassword);
            Console.WriteLine("Password changed");

            //await man.ResetPasswordAsync(user,
            //    newPassword,
            //    new UserCredentials(user, currentPassword));

        }
    }
}
