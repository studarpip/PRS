import {
  BrowserRouter,
  Routes,
  Route,
  Link,
  Navigate,
  useLocation,
  useNavigate,
} from "react-router-dom";
import { useEffect, useState } from "react";
import Welcome from "./pages/Welcome";
import Home from "./pages/Home";
import Login from "./pages/Login";
import Registration from "./pages/Registration";
import Admin from "./pages/Admin";
import Unauthorized from "./pages/Unauthorized";
import NotFound from "./pages/NotFound";
import { login, logout, getCurrentUser, isAdmin } from "./api/auth";

function App() {
  const [user, setUser] = useState(null);
  const [checkingAuth, setCheckingAuth] = useState(true);
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    getCurrentUser()
      .then(setUser)
      .catch(() => setUser(null))
      .finally(() => setCheckingAuth(false));
  }, [location]);

  useEffect(() => {
    if (!user && !checkingAuth) {
      const securePaths = ["/home", "/admin"];
      if (securePaths.includes(location.pathname)) {
        navigate("/");
      }
    }
  }, [user, checkingAuth, location.pathname, navigate]);

  const handleLogin = async (username, password) => {
    const ok = await login(username, password);
    if (ok) {
      const user = await getCurrentUser();
      setUser(user);
      return user;
    }
    return null;
  };

  const handleLogout = async () => {
    await logout();
    setUser(null);
  };

  if (checkingAuth) return <p>Loading...</p>;

  return (
    <>
      <nav className="nav">
        <div className="nav-left">
          {user ? <Link to="/home">Home</Link> : <Link to="/">Welcome</Link>}
          {isAdmin(user) && <Link to="/admin">Admin</Link>}
        </div>

        <div className="nav-right">
          {!user && <Link to="/login">Login</Link>}
          {user && <button onClick={handleLogout}>Logout</button>}
        </div>
      </nav>

      <div className="container">
        <Routes>
          <Route path="/" element={user ? <Navigate to="/home" /> : <Welcome />} />
          <Route path="/home" element={user ? <Home /> : <Navigate to="/login" />} />
          <Route path="/login" element={<Login onLogin={handleLogin} user={user} />} />
          <Route path="/register" element={<Registration user={user} />} />
          <Route path="/admin" element={isAdmin(user) ? <Admin /> : <Navigate to="/unauthorized" />} />
          <Route path="/unauthorized" element={<Unauthorized />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </div>
    </>
  );
}

export default function AppWrapper() {
  return (
    <BrowserRouter>
      <App />
    </BrowserRouter>
  );
}
