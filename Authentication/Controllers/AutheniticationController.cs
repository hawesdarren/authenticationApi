using Authentication.Application;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text.Json;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class authenticationController : ControllerBase
    {
        private readonly ILogger<authenticationController> _logger;
        private readonly AuthenticationOptions _authenticationOptions;
        private readonly SmtpOptions _smtpOptions;
     

        public authenticationController(ILogger<authenticationController> logger, IOptions<AuthenticationOptions> authenticationOptions, IOptions<SmtpOptions> smtpOptions)
        {
            _logger = logger;
            _authenticationOptions = authenticationOptions.Value;
            _smtpOptions = smtpOptions.Value;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] Json.Requests.LoginRequest loginRequest)
        {
            LoginResponse loginResponse = new() { Success = false, Authenticated = false };
            var loginUser = new LoginUser(Microsoft.Extensions.Options.Options.Create(_authenticationOptions));
            loginResponse = loginUser.ValidatePassword(loginRequest);
            if (!loginResponse.Success) {
                _logger.LogInformation($"Login unsuccessful: {loginResponse.GetError}");
            }
            return Ok(loginResponse);
                      
  
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromBody] Json.Requests.RegisterRequest registerRequest)
        {
            RegisterResponse registerResponse = new RegisterResponse();
            RegisterUser registerUser = new RegisterUser(Microsoft.Extensions.Options.Options.Create(_authenticationOptions));
            registerResponse = registerUser.Register(registerRequest);
            if (!registerResponse.Success) {
                _logger.LogInformation($"Registration unsuccessful: {registerResponse.error}");
            }
            return Ok(registerResponse);

        }
        [Authorize(Policy = "Email")]
        [Authorize(Policy = "Authentication")]
        [HttpPost]
        [Route("change/password")]
        public IActionResult ChangePassword([FromBody] Json.Requests.ChangePasswordRequest changePasswordRequest) {
            var idendity = User.Identity as ClaimsIdentity;

            ChangePasswordResponse changePasswordResponse = new ChangePasswordResponse();
            ChangePassword changePassword = new ChangePassword(Microsoft.Extensions.Options.Options.Create(_authenticationOptions));
            changePasswordResponse = changePassword.Change(changePasswordRequest, idendity.FindFirst(ClaimTypes.Email).Value);
            if (!changePasswordResponse.Success) {
                _logger.LogInformation($"Change password unsuccessful: {changePasswordResponse.error}");
            }
            return Ok(changePasswordResponse);
        
        }

        [Authorize(Policy = "Email")]
        //[Authorize(Policy = "Authentication")] // Allow unathenticated users to register TFA
        [HttpPost]
        [Route("tfa/setup")]
        public IActionResult TfaSetup()
        {
            var idendity = User.Identity as ClaimsIdentity;
            var email = idendity?.FindFirst(ClaimTypes.Email)?.Value;
            var issuer = idendity?.FindFirst("iss")?.Value;
#pragma warning disable CS8604 // Possible null reference argument.
            Tfa tfa = new Tfa(Microsoft.Extensions.Options.Options.Create(_authenticationOptions));
            TfaSetupResponse tfaSetupResponse = tfa.CreateNewTotp(email, issuer);
            tfaSetupResponse.Authenticated = Convert.ToBoolean(idendity.FindFirst(ClaimTypes.Authentication).Value);
            if (!tfaSetupResponse.Success) {
                _logger.LogInformation($"TFA Setup unsuccessful: {tfaSetupResponse.error}");
            }
            return Ok(tfaSetupResponse);

        }

        [Authorize(Policy = "Email")]
        [HttpPost]
        [Route("tfa/validate")]
        public IActionResult TfaValidate([FromBody]Json.Requests.ValidateTfa validateTfaRequest)
        {
            var idendity = User.Identity as ClaimsIdentity;
            string email = idendity.FindFirst(ClaimTypes.Email).Value;
            Tfa tfa = new Tfa(Microsoft.Extensions.Options.Options.Create(_authenticationOptions));
            TfaValidateResponse tfaValidateResponse =  tfa.Validate(email, validateTfaRequest.tfaCode);
            if (!tfaValidateResponse.Success) {
                _logger.LogInformation($"TFA Validate unsuccessful: {tfaValidateResponse.error}");
            }
            return Ok(tfaValidateResponse);

        }

        [Authorize(Policy = "Email")]
        [HttpPost]
        [Route("tfa/enable")]
        public IActionResult EnableTfa([FromBody] Json.Requests.EnableTfa enableTfaRequest)
        {
            var idendity = User.Identity as ClaimsIdentity;
            string email = idendity.FindFirst(ClaimTypes.Email).Value;
            Tfa tfa = new Tfa(Microsoft.Extensions.Options.Options.Create(_authenticationOptions));
            EnableTfaResponse enbleTfaResponse = tfa.EnableTfa(email, enableTfaRequest.enableTfa, enableTfaRequest.tfaCode);
            if (!enbleTfaResponse.Success) {
                _logger.LogInformation($"TFA enable unsuccessful: {enbleTfaResponse.error}");
            }
            return Ok(enbleTfaResponse);

        }

        
        [Route("token/refresh")]
        [HttpPost]
        public IActionResult RefreshToken()
        { 
            // todo - need to work out what should be in the refresh token and how to store it
            //var identity = User.Identity as ClaimsIdentity;
            //string email = identity.FindFirst(ClaimTypes.Email).Value;

            var tokenGenerator = new Token(Microsoft.Extensions.Options.Options.Create(_authenticationOptions));
            string newToken = tokenGenerator.GenerateJwtToken("someone@hawes.co.nz", true, 10);
            RefreshTokenResponse refreshTokenResponse = new RefreshTokenResponse { Authenticated = true, Success = true };
            refreshTokenResponse.token = newToken;
            return Ok(refreshTokenResponse);
        }

        [HttpPost]
        [Route("forgotten/password")]
        public async Task<IActionResult> ForgottenPasswordResponse([FromBody] Json.Requests.ForgottenPasswordRequest forgottenPasswordRequest)
        {
            ForgottenPasswordResponse forgottenPasswordResponse = new ForgottenPasswordResponse { Authenticated = false, Success = false };
            ForgottenPassword forgottenPassword = new ForgottenPassword(Options.Create(_smtpOptions), Options.Create(_authenticationOptions));
            forgottenPasswordResponse = await forgottenPassword.ProcessForgottenPasswordAsync(forgottenPasswordRequest);
            if (!forgottenPasswordResponse.Success)
            {
                _logger.LogInformation($"Forgotten password unsuccessful: {forgottenPasswordResponse.error}");
            }
            return Ok(forgottenPasswordResponse);

        }

        [HttpGet]
        [Route("health")]
        public IActionResult Health()
        {
            return Ok( new { message = "I'm healthy"});
        }
    }
}
