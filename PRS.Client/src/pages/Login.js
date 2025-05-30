import { useState, useEffect } from "react";
import { useNavigate, Link } from "react-router-dom";
import { isAdmin } from "../api/auth";
import "../css/Login.css";
import { useNotification } from "../contexts/NotificationContext";

function Login({ onLogin, user }) {
    const { notify } = useNotification();
    const navigate = useNavigate();
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (user) {
            navigate(isAdmin(user) ? "/admin" : "/");
        }
    }, [user, navigate]);

    const handleLogin = async (e) => {
        e.preventDefault();

        if (loading)
            return;


        if (!username) {
            notify("Username not entered", "error");
            return;
        }

        if (!password) {
            notify("Password not entered", "error");
            return;
        }

        setLoading(true);

        try {
            const result = await onLogin(username, password);

            if (!result.success) {
                notify(result.errorMessage, "error");
                return;
            }

            const user = result.user;
            navigate(isAdmin(user) ? "/admin" : "/");
        } finally {
            setLoading(false);
        }
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
                    />
                </div>
                <div className="login-form-group">
                    <label>Password:</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </div>
                <button type="submit" className="login-button" disabled={loading}>
                    {loading ? "Logging in..." : "Login"}
                </button>
            </form>

            <div className="login-register-link">
                <p>Don't have an account? <Link to="/register">Register</Link></p>
            </div>
        </div>
    );
}

export default Login;
