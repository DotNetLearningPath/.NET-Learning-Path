import type {
    LoginRequest, //LoginDto
    LoginResponse
} from '../types/auth';

const API_URL = 'https://localhost:7083/api/auth';

export async function login(request: LoginRequest): Promise<LoginResponse> //Task<LoginResponseDto>
{
    const response = await fetch(`${API_URL}/login`, { //HttpClient
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(request),
        credentials: 'include'
    });

    if (!response.ok) {
        const problem = await response.json();

        throw new Error(
            `${problem.detail ?? problem.title} (${response.status})`
        );
    }

    return await response.json() as LoginResponse;
}

export async function refreshToken(): Promise<LoginResponse> {
    const response = await fetch(`${API_URL}/refresh`, {
        method: 'POST',
        credentials: 'include'
    });

    if (!response.ok) {
        throw new Error(`Refresh token failed: ${response.status}`);
    }

    return await response.json() as LoginResponse;
}

export async function logout(): Promise<void> {
    const response = await fetch(`${API_URL}/logout`, {
        method: 'POST',
        credentials: 'include'
    });

    if (!response.ok) {
        throw new Error(`Logout failed: ${response.status}`);
    }
}
