import { Link } from "react-router-dom";
import "../css/NotFound.css";

function NotFound() {
  return (
    <div className="notfound-page">
      <h2>404 - Page Not Found</h2>
      <p>The page you're looking for doesn't exist.</p>
      <Link to="/" className="notfound-back-home-link">Go back home</Link>
    </div>
  );
}

export default NotFound;
