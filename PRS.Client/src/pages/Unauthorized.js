import "../css/Unauthorized.css";

function Unauthorized() {
  return (
    <div className="unauthorized-container">
      <h2 className="unauthorized-title">403 - Forbidden</h2>
      <p className="unauthorized-message">You do not have permission to view this page.</p>
    </div>
  );
}

export default Unauthorized;
