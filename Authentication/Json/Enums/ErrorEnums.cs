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
            PASSWORDS_DONT_MATCH,
            EMAIL_NOT_REGISTERED_FOR_TFA,
            MULTIPLE_TFA_REGISTERED,
            TFA_NOT_ENABLED,
            TFA_ERROR,
            TFA_CODE_INVALID
        }
    }
}
