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
            var app = new CommandLineApplication();
            app.HelpOption("-h|--help");

            var optionUrl = app.Option("-u|--url <localhost>", "EventStore Http URL", CommandOptionType.SingleValue);
            var optionPort = app.Option<int>("-p|--port <2113>", "Port", CommandOptionType.SingleValue);
            var optionLogin = app.Option("-l|--login <admin>", "Login to Change Password", CommandOptionType.SingleValue);
            var optionOldPasswort = app.Option("-o|--old <changeit>", "Old Password", CommandOptionType.SingleValue);
            var optionNewPasswort = app.Option("-n|--new <changed>", "New Password", CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                var url = optionUrl.HasValue() ? optionUrl.Value() : "localhost";
                var port = optionPort.HasValue() ? optionPort.ParsedValue : 2113;
                var login = optionLogin.HasValue() ? optionLogin.Value() : "admin";
                var oldp = optionOldPasswort.HasValue() ? optionOldPasswort.Value() : "changeit";
                var newp = optionNewPasswort.HasValue() ? optionNewPasswort.Value() : "changed";

                await ChangePasswordAsync(url, port, login, oldp, newp);
            });

            return app.Execute(args);
        }

        private static async Task ChangePasswordAsync(string url, int port, string login, string oldPassword,
            string newPassword)
        {
            var host = Dns.GetHostEntry(url);
            var ip = host.AddressList.First(x => x.ToString() != "::1");

            Console.WriteLine("ChangePasswordAsync {0} {1} {2}", ip, port, login);

            var man = new UsersManager(new ConsoleLogger(),
                new IPEndPoint(ip, port),
                TimeSpan.FromSeconds(1));

            try
            {
                Console.WriteLine("Trying new Password");
                await man.GetUserAsync(login, new UserCredentials(login, newPassword));
                Console.WriteLine("New Password already set");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot access with new Password");
                var e1 = e.InnerException ?? e;
                Console.WriteLine(e1.Message);
            }

            Console.WriteLine("Try change password");
            await man.ChangePasswordAsync(login, oldPassword, newPassword);
            Console.WriteLine("Password changed");

            //await man.ResetPasswordAsync(user,
            //    newPassword,
            //    new UserCredentials(user, currentPassword));

        }
    }
}
