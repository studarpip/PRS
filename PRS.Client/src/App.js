import { useEffect, useState } from "react";
import {
  BrowserRouter,
  Routes,
  Route,
  Link,
  Navigate,
  useLocation,
  useNavigate,
} from "react-router-dom";
import Welcome from "./pages/Welcome";
import Home from "./pages/Home";
import Login from "./pages/Login";
import Registration from "./pages/Registration";
import Admin from "./pages/Admin";
import Product from "./pages/Product";
import Cart from "./pages/Cart";
import Unauthorized from "./pages/Unauthorized";
import NotFound from "./pages/NotFound";
import CartDropdown from "./components/CartDropdown";
import RecommendationSettings from "./components/RecommendationSettings";
import { login, logout, getCurrentUser, isAdmin } from "./api/auth";
import { useCart, CartProvider } from "./contexts/CartContext";
import { Home as HomeIcon, LogIn, LogOut, ShoppingCart, UserCog, Settings as SettingsIcon } from "lucide-react";
import "./css/App.css";

function App() {
  const [user, setUser] = useState(null);
  const [checkingAuth, setCheckingAuth] = useState(true);
  const [cartOpen, setCartOpen] = useState(false);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [animateBadge, setAnimateBadge] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();
  const { cartCount, fetchCartCount, clearCartCount } = useCart();

  useEffect(() => {
    getCurrentUser()
      .then(setUser)
      .catch(() => setUser(null))
      .finally(() => {
        setCheckingAuth(false);
        fetchCartCount();
      });
  }, [location]);

  useEffect(() => {
    if (!user && !checkingAuth) {
      const securePaths = ["/home", "/admin"];
      if (securePaths.includes(location.pathname)) {
        navigate("/");
      }
    }
  }, [user, checkingAuth, location.pathname, navigate]);

  useEffect(() => {
    if (cartCount > 0) {
      setAnimateBadge(true);
      const timer = setTimeout(() => setAnimateBadge(false), 500);
      return () => clearTimeout(timer);
    }
  }, [cartCount]);

  const handleLogin = async (username, password) => {
    const ok = await login(username, password);
    if (ok) {
      const user = await getCurrentUser();
      fetchCartCount();
      setUser(user);
      return user;
    }
    return null;
  };

  const handleLogout = async () => {
    await logout();
    clearCartCount();
    setUser(null);
  };

  if (checkingAuth) return <p>Loading...</p>;

  return (
    <>
      <nav className="app-nav">
        <div className="app-nav-left">
          {user ? (
            <Link
              to="/home"
              className={location.pathname.startsWith("/home") ? "active-link" : ""}
            >
              <HomeIcon size={18} /> Home
            </Link>
          ) : (
            <Link
              to="/"
              className={location.pathname === "/" ? "active-link" : ""}
            >
              <HomeIcon size={18} /> Home
            </Link>
          )}

          {isAdmin(user) && (
            <Link
              to="/admin"
              className={location.pathname.startsWith("/admin") ? "active-link" : ""}
            >
              <UserCog size={18} /> Admin
            </Link>
          )}
        </div>

        <div className="app-nav-right">
          {user && (
            <>
              <div
                className="app-cart-link-wrapper"
                onMouseEnter={() => setCartOpen(true)}
                onMouseLeave={() => setCartOpen(false)}
              >
                <Link
                  to="/cart"
                  className={`app-cart-link ${location.pathname.startsWith("/cart") ? "active-link" : ""}`}
                >
                  <ShoppingCart size={18} />
                  {cartCount > 0 && (
                    <span className={`app-cart-badge ${animateBadge ? "app-bounce" : ""}`}>
                      {cartCount}
                    </span>
                  )}
                </Link>
                {cartOpen && <CartDropdown />}
              </div>

              <button onClick={() => setSettingsOpen(true)} className="app-settings-button">
                <SettingsIcon size={18} />
              </button>
            </>
          )}

          {!user && (
            <Link
              to="/login"
              className={location.pathname.startsWith("/login") ? "active-link" : ""}
            >
              <LogIn size={18} /> Login
            </Link>
          )}

          {user && (
            <button onClick={handleLogout}>
              <LogOut size={18} /> Logout
            </button>
          )}
        </div>
      </nav>

      <div className="app-container">
        <Routes>
          <Route path="/" element={user ? <Navigate to="/home" /> : <Welcome />} />
          <Route path="/home" element={user ? <Home /> : <Navigate to="/login" />} />
          <Route path="/login" element={<Login onLogin={handleLogin} user={user} />} />
          <Route path="/register" element={<Registration user={user} />} />
          <Route path="/admin" element={isAdmin(user) ? <Admin /> : <Navigate to="/unauthorized" />} />
          <Route path="/product/:id" element={user ? <Product /> : <Navigate to="/login" />} />
          <Route path="/cart" element={user ? <Cart /> : <Navigate to="/login" />} />
          <Route path="/unauthorized" element={<Unauthorized />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </div>

      {settingsOpen && (
        <div className="app-modal-overlay">
          <div className="app-modal-content">
            <RecommendationSettings onClose={() => setSettingsOpen(false)} />
          </div>
        </div>
      )}
    </>
  );
}

export default function AppWrapper() {
  return (
    <BrowserRouter>
      <CartProvider>
        <App />
      </CartProvider>
    </BrowserRouter>
  );
}
