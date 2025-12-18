namespace Authentication.Json.Enums
{
    public class ErrorEnums
    {
        public enum Error
        {
            // Login Errors
            INVALID,
            PASSWORD_TEMP_BLOCK,
            TEMP_PASSWORD_EXPIRED,
            INVALID_EMAIL,
            // Registration Errors
            EMAIL_ALREADY_REGISTERED,
            PASSWORD_COMPLEXITY,
            COMMON_PASSWORD,
            REGISTRATION_FAILURE,
            PASSWORDS_DONT_MATCH,
            // Two-Factor Authentication Errors
            EMAIL_NOT_REGISTERED_FOR_TFA,
            MULTIPLE_TFA_REGISTERED,
            TFA_NOT_ENABLED,
            TFA_ERROR,
            TFA_CODE_INVALID,
            // Forgotton Password Errors
            EMAIL_NOT_FOUND,
            DATABASE_ERROR,
            EMAIL_SENDING_FAILED
        }
    }
}
