namespace CashReportApi.Domain
{
    public class Error
    {
        protected Error(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public static Error ContactAdministrator { get; } = new Error(1000, "An error occurred. Please contact the administrator.");
        public static Error BalanceNotFound { get; } = new Error(1001, "Balance was not found");

        #region Properties 

        public int Code { get; }
        public string Message { get; }
        public object? AdditionalInfo { get; init; }

        #endregion Properties
    }

    public class BalanceResponse
    {
        public DateOnly Date { get; init; }
        public decimal Amount { get; init; }
    }
}
