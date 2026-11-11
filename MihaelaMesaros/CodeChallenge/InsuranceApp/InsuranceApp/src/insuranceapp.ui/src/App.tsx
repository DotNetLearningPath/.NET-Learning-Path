import { login, refreshToken, logout } from './api/authApi';
import type { LoginResponse } from './types/auth';
import { useEffect, useState } from 'react';

function App() {

    const [username, setUsername] = useState<string>('');
    const [password, setPassword] = useState<string>('');

    const [authData, setAuthData] = useState<LoginResponse | null>(null);
    const [error, setError] = useState<string>('');
    const [loading, setLoading] = useState<boolean>(false);

    const [message, setMessage] = useState<string>('');

    const [secondsUntilRefresh, setSecondsUntilRefresh] = useState<number>(0);

    useEffect(() => {
        if (!authData) return;

        const expiresAt = new Date(authData.expiresAt).getTime();
        const refreshBefore = 40 * 1000;

        const delay = Math.max(expiresAt - Date.now() - refreshBefore, 0);

        console.log('Access Token expires:', new Date(expiresAt).toLocaleString());
        console.log('Current time:', new Date().toLocaleString());
        console.log('Refresh scheduled in:', delay / 1000, 'seconds');

        const timer = setTimeout(async () => {
            try {
                const response = await refreshToken();

                setAuthData(response);
                setError('');
                setMessage('Token refreshed automatically.');
            }
            catch (error) {
                setError(error instanceof Error ? error.message : 'Automatic token refresh failed');

                setAuthData(null);
            }
        }, delay);

        return () => clearTimeout(timer);

    }, [authData]);


    useEffect(() => {
        if (!authData) return;

        const refreshAt = new Date(authData.expiresAt).getTime() - 40 * 1000;

        const updateCountdown = () => {
            const remaining = Math.max(0, Math.ceil((refreshAt - Date.now()) / 1000));

            setSecondsUntilRefresh(remaining);
        };

        updateCountdown();

        const interval = setInterval(updateCountdown, 1000);

        return () => clearInterval(interval);

    }, [authData]);


    async function handleLogin() {

        setMessage('');
        setError('');
        setAuthData(null);
        setLoading(true);

        try {
            const response = await login({
                username,
                password
            });

            setAuthData(response);
            setPassword('');
        }
        catch (error) {
            setError(error instanceof Error ? error.message : 'Unexpected error');
        }
        finally {
            setLoading(false);
        }
    }


    // async function handleRefreshToken() {
    //     if (!authData) return;

    //     setError('');
    //     setMessage('');
    //     setLoading(true);

    //     try {
    //         const response = await refreshToken({
    //             refreshToken: authData.refreshToken
    //         });


    //         const tokenChanged = authData.refreshToken !== response.refreshToken;

    //         setMessage(tokenChanged ? 'Refresh Token rotated successfully.' : 'Refresh Token did not change!');

    //         setAuthData(response);
    //     }
    //     catch (error) {
    //         setError(error instanceof Error ? error.message : 'Unexpected error');
    //     }
    //     finally {
    //         setLoading(false);
    //     }
    // }

    async function handleLogout() {
        if (!authData) return;

        setError('');
        setMessage('');
        setLoading(true);

        try {
            await logout();

            setAuthData(null);
            setMessage('Logged out successfully.');
        }
        catch (error) {
            setError(error instanceof Error ? error.message : 'Unexpected error');
        }
        finally {
            setLoading(false);
        }
    }


    return (
        <div>
            <h1>InsuranceApp</h1>
            <h2>Authentication Demo</h2>
            <br />
            <div>
                <label>Username: </label>
                <input type="text" value={username} onChange={(e) => setUsername(e.target.value)} />
            </div>

            <br />

            <div>
                <label>Password: </label>
                <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
            </div>

            <br />

            <button type="button" onClick={handleLogin} disabled={loading}>
                {loading ? 'Logging in...' : 'Login'}
            </button>

            {authData && (
                <div>
                    <h3>Login successful!</h3>
                    <p>Access Token expires: {authData.expiresAt}</p>

                    <p>
                        Next automatic refresh in: {' '}
                        {String(Math.floor(secondsUntilRefresh / 60)).padStart(2, '0')}:
                        {String(secondsUntilRefresh % 60).padStart(2, '0')}
                    </p>
                </div>
            )}


            {/* <br /><br />
            <button type="button" onClick={handleRefreshToken} disabled={!authData || loading}>
                Refresh Token
            </button> */}

            <br /><br />
            <button type="button" onClick={handleLogout} disabled={!authData || loading}>
                Logout
            </button>
            <br /><br />

            {message && <p>{message}</p>}

            {error && <p>{error}</p>}
        </div>
    );
}

export default App;