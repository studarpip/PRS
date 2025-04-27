import { useEffect, useState } from "react";
import axios from "axios";
import "../css/CartDropdown.css";

function CartDropdown() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    axios.get("/api/cart")
      .then(res => setItems(res.data.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="cart-dropdown">
      {loading ? (
        <p className="cart-dropdown-text">Loading...</p>
      ) : items.length === 0 ? (
        <p className="cart-dropdown-text">Your cart is empty.</p>
      ) : (
        <div className="cart-dropdown-items">
          {items.map(item => (
            <div key={item.productId} className="cart-dropdown-item">
              {item.name} x{item.count}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default CartDropdown;
