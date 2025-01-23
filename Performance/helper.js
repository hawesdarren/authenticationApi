import { SharedArray } from 'k6/data';

export function randomEmail(){
    let result = '';
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    const charactersLength = characters.length;
    let counter = 0;
    while (counter < 9) {
      result += characters.charAt(Math.floor(Math.random() * charactersLength));
      counter += 1;
    }
    return result + "@hawes.co.nz";
}

export function getEnvironmnetData(envName){
    const envData = JSON.parse(
        open(`./TestData/${envName}.json`)
    );

    const endpoint = `${envData.baseURL}`
    const emails = new SharedArray('emails', function (){
        return envData.emails;
    });

    return {
        endpoint,
        emails
    }
}

export function getEmailId(array){
    // Function to allocate test user to each thread/vu in a sequential order
    return (__VU % array.length)
}

export function requestParams(tag){
    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
        tags: {
            RequestTag: tag
        },
    }

    return params;
}

export function logErrorResponseDetails(res) {
    if (res.status !== 200) {
        console.log(`Failed response: ${res.url}`);
        console.error(`Error: HTTP ${res.status} ${res.statusText}`);
        console.log(`Error: Headers ${JSON.stringify(res.headers)}`);
        console.error(`Error: Body ${res.body}`);
        
    }
    else if(JSON.parse(res.body).data === null){
        console.error(`Error Request: ${res.request.body}`)
        console.error(`Error Body: ${res.body}`);
    }
}