using Authentication.Application;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public authenticationController(ILogger<authenticationController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] Json.Requests.LoginRequest loginRequest)
        {
            LoginResponse loginResponse = new() { Success = false, Authenticated = false };
            loginResponse = LoginUser.ValidatePassword(loginRequest);
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
            registerResponse = RegisterUser.Register(registerRequest);
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
            changePasswordResponse = Application.ChangePassword.Change(changePasswordRequest, idendity.FindFirst(ClaimTypes.Email).Value);
            if (!changePasswordResponse.Success) {
                _logger.LogInformation($"Change password unsuccessful: {changePasswordResponse.error}");
            }
            return Ok(changePasswordResponse);
        
        }

        [Authorize(Policy = "Email")]
        [Authorize(Policy = "Authentication")]
        [HttpPost]
        [Route("tfa/register")]
        public IActionResult TfaRegister()
        {
            var idendity = User.Identity as ClaimsIdentity;
            TfaRegisterResponse tfaRegisterResponse = Tfa.CreateNewTotp(idendity.FindFirst(ClaimTypes.Email).Value);
            tfaRegisterResponse.Authenticated = Convert.ToBoolean(idendity.FindFirst(ClaimTypes.Authentication).Value);   
            if (!tfaRegisterResponse.Success) {
                _logger.LogInformation($"TFA Register unsuccessful: {tfaRegisterResponse.error}");
            }
            return Ok(tfaRegisterResponse);

        }

        [Authorize(Policy = "Email")]
        [HttpPost]
        [Route("tfa/validate")]
        public IActionResult TfaValidate([FromBody]Json.Requests.ValidateTfa validateTfaRequest)
        {
            var idendity = User.Identity as ClaimsIdentity;
            string email = idendity.FindFirst(ClaimTypes.Email).Value;
            TfaValidateResponse tfaValidateResponse =  Tfa.Validate(email, validateTfaRequest.tfaCode);
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
            BaseResponse enbleTfaResponse = Tfa.EnableTfa(email, enableTfaRequest.enableTfa, enableTfaRequest.tfaCode);
            if (!enbleTfaResponse.Success) {
                _logger.LogInformation($"TFA enable unsuccessful: {enbleTfaResponse.error}");
            }
            // todo
            return Ok(enbleTfaResponse);
      

        }

        [HttpGet]
        [Route("health")]
        public IActionResult Health()
        {
            return Ok( new { message = "I'm healthy"});
        }
    }
}
