
namespace CV_3_project.Models
{
    public enum AccountType
    {
        Manager,
        Worker
    }
    public class AccountFactory
    {
        public Account CreateAccount(AccountType type, AccountCreationArgs args)
        {
            Account account;

            switch (type)
            {
                case AccountType.Manager:
                    account = new Manager(args.Login, args.Name, args.Surname, args.Contacts);
                    account.SetPassword(args.Password);
                    account.Settings.EmailNotificationsEnabled = true;
                    account.Settings.Theme = "Light";
                    break;

                case AccountType.Worker:
                    if (string.IsNullOrWhiteSpace(args.Position))
                    {
                        throw new ArgumentException("Position is required for a Worker.");
                    }

                    var worker = new Worker(args.Login, args.Name, args.Surname, args.Contacts, args.Position);
                    worker.SetPassword(args.Password);
                    worker.Settings.EmailNotificationsEnabled = false;
                    worker.Settings.Theme = "Dark";

                    account = worker;
                    break;

                default:
                    throw new ArgumentException("Invalid account type specified.");
            }

            return account;
        }
    }
}