const BASE_URL = 'https://localhost:7008/api';

const api = {
    get: (path) => fetch(`${BASE_URL}${path}`).then(r => r.json()),

    post: (path, body) => fetch(`${BASE_URL}${path}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    }),

    put: (path) => fetch(`${BASE_URL}${path}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' }
    })
};