import http from 'k6/http';
import { check, group } from 'k6';
import { Trend } from 'k6/metrics';
import { getEmailId, getEnvironmnetData, requestParams, randomEmail, logErrorResponseDetails} from './helper.js';
import { registerRequest } from './Requests/registar.js';
import { loginRequest } from './Requests/login.js';
// Get env data
const {endpoint, emails } = getEnvironmnetData(__ENV.TARGET_ENVIRONMENT);
const emailId = getEmailId(emails);
const emailObject = emails[emailId];
const email = emailObject.email;
//console.log(`VU: ${__VU} us using email ${email}`);
//Set Trend - todo
const registerDuration = new Trend('register_duration');
const loginDuration = new Trend('login_duration');
// Set Test scenarios
export const options = {
    insecureSkipTLSVerify: true,
    thresholds: {
        http_req_failed: ["rate<0.01"],
        checks: ["rate>0.99"],
        register_duration: ['p(95)<2000'],
        login_duration: ['p(95)<2000']
    },
    scenarios: {
        ramping_requestr_rate: {
            executor: 'ramping-arrival-rate',
            startRate: 10,
            timeUnit: '1m',
            preAllocatedVUs: 10,
            maxVUs: 100,
            stages: [
                {target: 50, duration: '1m'},
                {target: 50, duration: '19m'}
            ],
            gracefulstop: '30s'
        }
        
    }
};

export default function () {
    try {
        group('AuthenticationAPI', function () {
            const url = endpoint;
            // Register query
            const registerQuery = registerRequest(randomEmail());
            const registerResponse = http.post(url + 'api/authentication/register', registerQuery, requestParams('Register Request'), 'Register request api');
            // Checks
            check(registerResponse, {
                "status was 200": (registerResponse) => registerResponse.status === 200,
                "response contains success": (registerResponse) => JSON.parse(registerResponse.body).success === true,
                "response contains token": (registerResponse) => JSON.parse(registerResponse.body).token !== null,
            })
            // Get Metrics
            registerDuration.add(registerResponse.timings.duration);

            //Login query
            const loginQuery = loginRequest(email);
            const loginResponse = http.post(url + 'api/authentication/login', loginQuery, requestParams('Login Request'), 'Login request api');
            // Checks
            check(loginResponse, {
                "status was 200": (loginResponse) => loginResponse.status === 200,
                "response contains success": (loginResponse) => JSON.parse(loginResponse.body).success === true,
                "response contains token": (loginResponse) => JSON.parse(loginResponse.body).token !== null,
            })
            // Get Metrics
            loginDuration.add(loginResponse.timings.duration);
        });
    }
    catch (error) {
        logErrorResponseDetails(registerResponse);
    }
}
