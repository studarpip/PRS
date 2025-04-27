import { useState, useEffect } from "react";
import { useNavigate, Link } from "react-router-dom";
import { isAdmin } from "../api/auth";
import "../css/Login.css";

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
        navigate(isAdmin(user) ? "/admin" : "/");
    };

    return (
        <div className="login-page">
            <form onSubmit={handleLogin} className="login-form">
                <div className="login-form-group">
                    <label>Username:</label>
                    <input
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>
                <div className="login-form-group">
                    <label>Password:</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                <button type="submit" className="login-button">
                    Login
                </button>
            </form>

            <div className="login-register-link">
                <p>Don't have an account? <Link to="/register">Register</Link></p>
            </div>
        </div>
    );
}

export default Login;
