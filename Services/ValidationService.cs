using LaunderManagerClient.Entities;

namespace LaverieClient.Services
{
    public static class ValidationService
    {
        public static bool ValidateProprietor(Proprietor proprietor)
        {
            if (string.IsNullOrWhiteSpace(proprietor.Name) ||
                string.IsNullOrWhiteSpace(proprietor.Email) ||
                proprietor.Laundries == null || proprietor.Laundries.Count == 0)
            {
                return false;
            }

            foreach (var laundry in proprietor.Laundries)
            {
                if (string.IsNullOrWhiteSpace(laundry.Name) ||
                    string.IsNullOrWhiteSpace(laundry.Address) ||
                    laundry.Machines == null || laundry.Machines.Count == 0)
                {
                    return false;
                }

                foreach (var machine in laundry.Machines)
                {
                    if (string.IsNullOrWhiteSpace(machine.Type) ||
                        string.IsNullOrWhiteSpace(machine.SerialNumber) ||
                        string.IsNullOrWhiteSpace(machine.State) ||
                        machine.Cycles == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}