namespace MainWebApplication.Methods
{
    public static class CorrectSymbols
    {
        public static string CorrectMethod(string phoneNumber)
        {
            var notCorrectSymbols = new string[] { "(", ")", "-" };
            foreach (var item in notCorrectSymbols)
            {
                phoneNumber = phoneNumber.Replace(item, string.Empty);
            }
            return phoneNumber;
        }
    }
}
