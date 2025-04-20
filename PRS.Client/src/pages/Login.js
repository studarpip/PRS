import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { isAdmin } from "../api/auth";

function Login({ onLogin, user }) {
    const navigate = useNavigate();
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    useEffect(() => {
        if (user) {
            navigate(isAdmin(user) ? "/admin" : "/");
        }
    }, [user, navigate]);

    const handleLogin = async (e) => {
        e.preventDefault();
        const user = await onLogin(username, password);
        if (!user) {
            alert("Login failed");
            return;
        }
        ;
        navigate(isAdmin(user) ? "/admin" : "/");
    };

    return (
        <div>
            <h2>Login</h2>
            <form onSubmit={handleLogin}>
                <div>
                    <label>Username:</label>
                    <input
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>
                <div style={{ marginTop: "10px" }}>
                    <label>Password:</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                <button type="submit" style={{ marginTop: "15px" }}>
                    Login
                </button>
            </form>
        </div>
    );
}

export default Login;
