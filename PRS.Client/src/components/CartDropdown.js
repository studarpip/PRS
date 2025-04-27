import { useEffect, useState } from "react";
import axios from "axios";
import "../css/CartDropdown.css";

function CartDropdown() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    axios.get("/api/cart")
      .then(res => setItems(res.data.data))
      .catch(() => { window.location.href = "/login"; })
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="mini-cart-dropdown">
      {loading ? (
        <p className="mini-cart-dropdown-text">Loading...</p>
      ) : items.length === 0 ? (
        <p className="mini-cart-dropdown-text">Your cart is empty.</p>
      ) : (
        <div className="mini-cart-dropdown-items">
          {items.map(item => (
            <div key={item.productId} className="mini-cart-dropdown-item">
              <div className="mini-cart-item-left">
                {item.image ? (
                  <img
                    src={`data:image/jpeg;base64,${item.image}`}
                    alt={item.name}
                    className="mini-cart-item-image"
                  />
                ) : (
                  <div className="mini-cart-item-placeholder">No Image</div>
                )}
              </div>
              <div className="mini-cart-item-info">
                <span className="mini-cart-item-name">{item.name}</span>
                <span className="mini-cart-item-count">x{item.count}</span>
              </div>
              <div className="mini-cart-item-price">
                €{(item.price * item.count).toFixed(2)}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default CartDropdown;
