namespace GarageV3.Services
{
    public class GarageFeeService
    {
        public decimal CalculateFee(DateTime arrivalTime, DateTime departureTime)
        {
            decimal totalFee = 0m;

            var currentTime = arrivalTime;

            while (currentTime < departureTime)
            {
                if (currentTime.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (currentTime.Hour >= 6 && currentTime.Hour < 20)
                    {
                        totalFee += 20m / 60m;
                    }
                    else
                    {
                        totalFee += 2m / 60m;
                    }
                }

                currentTime = currentTime.AddMinutes(1);
            }

            return Math.Round(totalFee, 2);
        }
    }
}