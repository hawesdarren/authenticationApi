export function registerRequest(email){
    const request = {
        email: email,
        password: 'Testing123',
        renteredPassword: 'Testing123'
    }

    return JSON.stringify(request);
}