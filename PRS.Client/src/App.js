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
import { NotificationProvider } from "./contexts/NotificationContext";
import {
  Home as HomeIcon,
  LogIn,
  LogOut,
  ShoppingCart,
  UserCog,
  Settings as SettingsIcon,
} from "lucide-react";
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
    const result = await login(username, password);
    if (result.success) {
      const user = await getCurrentUser();
      fetchCartCount();
      setUser(user);
      return { success: true, user };
    } else {
      return { success: false, errorMessage: result.errorMessage };
    }
  };

  const handleLogout = async () => {
    await logout();
    setUser(null);
    navigate("/login");
    clearCartCount();
  };

  if (checkingAuth) return <p>Loading...</p>;

  return (
    <>
      <nav className="app-nav">
        <div className="app-nav-left">
          {user && !isAdmin(user) && (
            <Link
              to="/home"
              className={location.pathname.startsWith("/home") ? "active-link" : ""}
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
          {user && !isAdmin(user) && (
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
          <Route
            path="/"
            element={
              user ? (
                <Navigate to={isAdmin(user) ? "/admin" : "/home"} />
              ) : (
                <Navigate to="/login" />
              )
            }
          />
          <Route
            path="/home"
            element={
              !user ? (
                <Navigate to="/login" />
              ) : isAdmin(user) ? (
                <Navigate to="/unauthorized" />
              ) : (
                <Home />
              )
            }
          />
          <Route
            path="/cart"
            element={
              !user ? (
                <Navigate to="/login" />
              ) : isAdmin(user) ? (
                <Navigate to="/unauthorized" />
              ) : (
                <Cart />
              )
            }
          />
          <Route
            path="/admin"
            element={
              !user ? (
                <Navigate to="/login" />
              ) : isAdmin(user) ? (
                <Admin />
              ) : (
                <Navigate to="/unauthorized" />
              )
            }
          />
          <Route
            path="/product/:id"
            element={
              !user ? (
                <Navigate to="/login" />
              ) : isAdmin(user) ? (
                <Navigate to="/unauthorized" />
              ) : (
                <Product />
              )
            }
          />
          <Route
            path="/login"
            element={
              user ? (
                <Navigate to={isAdmin(user) ? "/admin" : "/home"} />
              ) : (
                <Login onLogin={handleLogin} user={user} />
              )
            }
          />
          <Route
            path="/register"
            element={
              user ? (
                <Navigate to={isAdmin(user) ? "/admin" : "/home"} />
              ) : (
                <Registration user={user} />
              )
            }
          />
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
      <NotificationProvider>
        <CartProvider>
          <App />
        </CartProvider>
      </NotificationProvider>
    </BrowserRouter>
  );
}
