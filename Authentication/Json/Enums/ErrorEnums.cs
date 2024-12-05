namespace Authentication.Json.Enums
{
    public class ErrorEnums
    {
        public enum Error
        {
            INVALID,
            PASSWORD_TEMP_BLOCK,
            TEMP_PASSWORD_EXPIRED,
            INVALID_EMAIL,
            EMAIL_ALREADY_REGISTERED,
            PASSWORD_COMPLEXITY,
            COMMON_PASSWORD,
            REGISTRATION_FAILURE,
            PASSWORDS_DONT_MATCH
        }
    }
}
