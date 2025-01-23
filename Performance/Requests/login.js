export function loginRequest(email){
    const request = {
        email: email,
        password: 'Testing123'
    }

    return JSON.stringify(request);
}